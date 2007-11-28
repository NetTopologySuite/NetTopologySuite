using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary> 
    /// Builds the buffer point for a given input point and precision model.
    /// Allows setting the level of approximation for circular arcs,
    /// and the precision model in which to carry out the computation.
    /// When computing buffers in floating point Double-precision
    /// it can happen that the process of iterated noding can fail to converge (terminate).
    /// In this case a TopologyException will be thrown.
    /// Retrying the computation in a fixed precision
    /// can produce more robust results.    
    /// </summary>
    public class BufferBuilder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Compute the change in depth as an edge is crossed from R to L.
        /// </summary>
        private static Int32 DepthDelta(Label label)
        {
            Locations lLoc = label.GetLocation(0, Positions.Left);
            Locations rLoc = label.GetLocation(0, Positions.Right);
            if (lLoc == Locations.Interior && rLoc == Locations.Exterior)
            {
                return 1;
            }
            else if (lLoc == Locations.Exterior && rLoc == Locations.Interior)
            {
                return -1;
            }
            return 0;
        }

        private Int32 _quadrantSegments = OffsetCurveBuilder<TCoordinate>.DefaultQuadrantSegments;
        private BufferStyle _endCapStyle = BufferStyle.CapRound;

        private IPrecisionModel<TCoordinate> _workingPrecisionModel = null;
        private INoder<TCoordinate> _workingNoder = null;
        private IGeometryFactory<TCoordinate> _geometryFactory = null;
        private PlanarGraph<TCoordinate> _graph = null;
        private readonly EdgeList<TCoordinate> _edgeList = new EdgeList<TCoordinate>();

        /// <summary>
        /// Gets or sets the number of segments used to approximate a angle fillet.
        /// </summary>
        public Int32 QuadrantSegments
        {
            get { return _quadrantSegments; }
            set { _quadrantSegments = value; }
        }

        /// <summary>
        /// Gets or sets the precision model to use during the curve computation and noding,
        /// if it is different to the precision model of the Geometry.
        /// If the precision model is less than the precision of the Geometry precision model,
        /// the Geometry must have previously been rounded to that precision.
        /// </summary>
        public IPrecisionModel<TCoordinate> WorkingPrecisionModel
        {
            get { return _workingPrecisionModel; }
            set { _workingPrecisionModel = value; }
        }

        public BufferStyle EndCapStyle
        {
            get { return _endCapStyle; }
            set { _endCapStyle = value; }
        }

        public INoder<TCoordinate> Noder
        {
            get { return _workingNoder; }
            set { _workingNoder = value; }
        }

        public IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g, Double distance)
        {
            IPrecisionModel<TCoordinate> precisionModel = _workingPrecisionModel;

            if (precisionModel == null)
            {
                precisionModel = g.PrecisionModel;
            }

            // factory must be the same as the one used by the input
            _geometryFactory = g.Factory;

            OffsetCurveBuilder<TCoordinate> curveBuilder 
                = new OffsetCurveBuilder<TCoordinate>(precisionModel, _quadrantSegments);
            curveBuilder.EndCapStyle = _endCapStyle;
            OffsetCurveSetBuilder<TCoordinate> curveSetBuilder 
                = new OffsetCurveSetBuilder<TCoordinate>(g, distance, curveBuilder);

            IEnumerable<SegmentString<TCoordinate>> bufferSegStrList = curveSetBuilder.GetCurves();

            // short-circuit test
            if (!Slice.CountGreaterThan(0, bufferSegStrList))
            {
                IGeometry<TCoordinate> emptyGeom = _geometryFactory.CreateGeometryCollection(new IGeometry<TCoordinate>[0]);
                return emptyGeom;
            }

            computeNodedEdges(bufferSegStrList, precisionModel);
            _graph = new PlanarGraph<TCoordinate>(new OverlayNodeFactory<TCoordinate>());
            _graph.AddEdges(_edgeList.Edges);

            IList subgraphList = CreateSubgraphs(_graph);
            PolygonBuilder<TCoordinate> polyBuilder = new PolygonBuilder<TCoordinate>(_geometryFactory);
            BuildSubgraphs(subgraphList, polyBuilder);
            IList resultPolyList = polyBuilder.Polygons;

            IGeometry resultGeom = _geometryFactory.BuildGeometry(resultPolyList);
            return resultGeom;
        }

        private INoder<TCoordinate> getNoder(IPrecisionModel<TCoordinate> precisionModel)
        {
            if (_workingNoder != null)
            {
                return _workingNoder;
            }

            // otherwise use a fast (but non-robust) noder
            LineIntersector<TCoordinate> li = new RobustLineIntersector<TCoordinate>();
            li.PrecisionModel = precisionModel;
            MCIndexNoder noder = new MCIndexNoder(new IntersectionAdder(li));
            return noder;
        }

        private void computeNodedEdges(IEnumerable<SegmentString<TCoordinate>> bufferSegStrList, IPrecisionModel<TCoordinate> precisionModel)
        {
            INoder noder = getNoder(precisionModel);
            noder.ComputeNodes(bufferSegStrList);
            IList nodedSegStrings = noder.GetNodedSubstrings();

            foreach (object obj in nodedSegStrings)
            {
                SegmentString segStr = (SegmentString) obj;
                Label oldLabel = (Label) segStr.Data;
                Edge edge = new Edge(segStr.Coordinates, new Label(oldLabel));
                InsertEdge(edge);
            }
        }

        /// <summary>
        /// Inserted edges are checked to see if an identical edge already exists.
        /// If so, the edge is not inserted, but its label is merged
        /// with the existing edge.
        /// </summary>
        protected void InsertEdge(Edge e)
        {
            //<FIX> MD 8 Oct 03  speed up identical edge lookup
            // fast lookup
            Edge existingEdge = _edgeList.FindEqualEdge(e);

            // If an identical edge already exists, simply update its label
            if (existingEdge != null)
            {
                Label existingLabel = existingEdge.Label;

                Label labelToMerge = e.Label;
                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = new Label(e.Label);
                    labelToMerge.Flip();
                }
                existingLabel.Merge(labelToMerge);

                // compute new depth delta of sum of edges
                Int32 mergeDelta = DepthDelta(labelToMerge);
                Int32 existingDelta = existingEdge.DepthDelta;
                Int32 newDelta = existingDelta + mergeDelta;
                existingEdge.DepthDelta = newDelta;
            }
            else
            {
                // no matching existing edge was found
                // add this new edge to the list of edges in this graph
                //e.setName(name + edges.size());
                _edgeList.Add(e);
                e.DepthDelta = DepthDelta(e.Label);
            }
        }

        private IList CreateSubgraphs(PlanarGraph<TCoordinate> graph)
        {
            ArrayList subgraphList = new ArrayList();
            foreach (Node<TCoordinate> obj in graph.Nodes)
            {
                if (!node.IsVisited)
                {
                    BufferSubgraph subgraph = new BufferSubgraph();
                    subgraph.Create(node);
                    subgraphList.Add(subgraph);
                }
            }

            /*
             * Sort the subgraphs in descending order of their rightmost coordinate.
             * This ensures that when the Polygons for the subgraphs are built,
             * subgraphs for shells will have been built before the subgraphs for
             * any holes they contain.
             */
            subgraphList.Sort();
            subgraphList.Reverse();
            return subgraphList;
        }

        /// <summary>
        /// Completes the building of the input subgraphs by depth-labeling them,
        /// and adds them to the <see cref="PolygonBuilder" />.
        /// The subgraph list must be sorted in rightmost-coordinate order.
        /// </summary>
        /// <param name="subgraphList">The subgraphs to build.</param>
        /// <param name="polyBuilder">The PolygonBuilder which will build the final polygons.</param>
        private void BuildSubgraphs(IList subgraphList, PolygonBuilder polyBuilder)
        {
            IList processedGraphs = new ArrayList();
            foreach (object obj in subgraphList)
            {
                BufferSubgraph subgraph = (BufferSubgraph) obj;
                ICoordinate p = subgraph.RightMostCoordinate;
                SubgraphDepthLocater locater = new SubgraphDepthLocater(processedGraphs);
                Int32 outsideDepth = locater.GetDepth(p);
                subgraph.ComputeDepth(outsideDepth);
                subgraph.FindResultEdges();
                processedGraphs.Add(subgraph);
                polyBuilder.Add(subgraph.DirectedEdges, subgraph.Nodes);
            }
        }
    }
}
