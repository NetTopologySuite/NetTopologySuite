using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using NPack.Interfaces;
using GeoAPI.DataStructures;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary> 
    /// Builds the buffer point for a given input point and precision model.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate.</typeparam>
    /// <remarks>
    /// Allows setting the level of approximation for circular arcs,
    /// and the precision model in which to carry out the computation.
    /// When computing buffers in floating point Double-precision
    /// it can happen that the process of iterated noding can fail to converge (terminate).
    /// In this case a <see cref="TopologyException"/> will be thrown.
    /// Retrying the computation in a fixed precision
    /// can produce more robust results.    
    /// </remarks>
    public class BufferBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private Int32 _quadrantSegments = OffsetCurveBuilder<TCoordinate>.DefaultQuadrantSegments;
        private BufferStyle _endCapStyle = BufferStyle.Round;

        private IPrecisionModel<TCoordinate> _workingPrecisionModel;
        private INoder<TCoordinate> _workingNoder;
        private IGeometryFactory<TCoordinate> _geoFactory;
        private PlanarGraph<TCoordinate> _graph;
        private readonly EdgeList<TCoordinate> _edgeList;

        /// <summary>
        /// Constructs a new <see cref="BufferBuilder{TCoordinate}"/>
        /// with the given <paramref name="geoFactory"/>.
        /// </summary>
        /// <param name="geoFactory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> to use 
        /// to construct geometries.
        /// </param>
        public BufferBuilder(IGeometryFactory<TCoordinate> geoFactory)
        {
            _edgeList = new EdgeList<TCoordinate>(geoFactory);
        }

        /// <summary>
        /// Gets or sets the number of segments used to approximate a angle fillet.
        /// </summary>
        public Int32 QuadrantSegments
        {
            get { return _quadrantSegments; }
            set { _quadrantSegments = value; }
        }

        /// <summary>
        /// Gets or sets the precision model to use during the curve 
        /// computation and noding, if it is different to the precision model 
        /// of the Geometry.
        /// If the precision model is less than the precision of the Geometry 
        /// precision model, the Geometry must have previously been rounded to 
        /// that precision.
        /// </summary>
        public IPrecisionModel<TCoordinate> WorkingPrecisionModel
        {
            get { return _workingPrecisionModel; }
            set { _workingPrecisionModel = value; }
        }

        /// <summary>
        /// Gets or sets the style of the buffer's end.
        /// </summary>
        public BufferStyle EndCapStyle
        {
            get { return _endCapStyle; }
            set { _endCapStyle = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="INoder{TCoordinate}"/>
        /// used to create nodes for intersections in the buffer graph.
        /// </summary>
        public INoder<TCoordinate> Noder
        {
            get { return _workingNoder; }
            set { _workingNoder = value; }
        }

        /// <summary>
        /// Generates a new <see cref="IGeometry{TCoordinate}"/>
        /// which is the buffer of the given geometry, <paramref name="g"/>.
        /// </summary>
        /// <param name="g">The input geometry to compute the buffer for.</param>
        /// <param name="distance">
        /// The distance from the input geometry's border at which 
        /// to construct the new geometry.
        /// </param>
        /// <returns>
        /// A newly constructed geometry whose border is <paramref name="distance"/>
        /// from the input geometry's border.
        /// </returns>
        public IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g, Double distance)
        {
            IPrecisionModel<TCoordinate> precisionModel = _workingPrecisionModel;

            if (precisionModel == null)
            {
                precisionModel = g.PrecisionModel;
            }

            // factory must be the same as the one used by the input
            _geoFactory = g.Factory;

            OffsetCurveBuilder<TCoordinate> curveBuilder 
                = new OffsetCurveBuilder<TCoordinate>(_geoFactory, 
                                                      precisionModel, 
                                                      _quadrantSegments);

            curveBuilder.EndCapStyle = _endCapStyle;

            OffsetCurveSetBuilder<TCoordinate> curveSetBuilder
                = new OffsetCurveSetBuilder<TCoordinate>(g, distance, curveBuilder);

            IEnumerable<NodedSegmentString<TCoordinate>> bufferSegStrList 
                = curveSetBuilder.GetCurves();

            // short-circuit test
            if (!Slice.CountGreaterThan(bufferSegStrList, 0))
            {
                IGeometry<TCoordinate> emptyGeom 
                    = _geoFactory.CreateGeometryCollection();
                return emptyGeom;
            }

            computeNodedEdges(bufferSegStrList, precisionModel);
            _graph = new PlanarGraph<TCoordinate>(new OverlayNodeFactory<TCoordinate>());
            _graph.AddEdges(_edgeList);

            IEnumerable<BufferSubgraph<TCoordinate>> subgraphs = createSubgraphs(_graph);
            PolygonBuilder<TCoordinate> polyBuilder 
                = new PolygonBuilder<TCoordinate>(_geoFactory);
            buildSubgraphs(subgraphs, polyBuilder);

            IEnumerable<IGeometry<TCoordinate>> resultPolyList
                = Enumerable.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(polyBuilder.Polygons);

            IGeometry<TCoordinate> resultGeom = _geoFactory.BuildGeometry(resultPolyList);
            return resultGeom;
        }

        /// <summary>
        /// Inserted edges are checked to see if an identical edge already exists.
        /// If so, the edge is not inserted, but its label is merged
        /// with the existing edge.
        /// </summary>
        protected void InsertEdge(Edge<TCoordinate> e)
        {
            //<FIX> MD 8 Oct 03  speed up identical edge lookup
            // fast lookup
            Edge<TCoordinate> existingEdge = _edgeList.FindEqualEdge(e);

            Debug.Assert(e.Label.HasValue);

            // If an identical edge already exists, simply update its label
            if (existingEdge != null)
            {
                Debug.Assert(existingEdge.Label.HasValue);

                Label existingLabel = existingEdge.Label.Value;
                Label labelToMerge = e.Label.Value;

                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = e.Label.Value.Flip();
                }

                existingEdge.Label = existingLabel.Merge(labelToMerge);

                // compute new depth delta of sum of edges
                Int32 mergeDelta = depthDelta(labelToMerge);
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
                e.DepthDelta = depthDelta(e.Label.Value);
            }
        }

        #region Private helper routines
        private INoder<TCoordinate> getNoder(IPrecisionModel<TCoordinate> precisionModel)
        {
            if (_workingNoder != null)
            {
                return _workingNoder;
            }

            // otherwise use a fast (but non-robust) noder
            LineIntersector<TCoordinate> li
                = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(_geoFactory);

            li.PrecisionModel = precisionModel;

            MonotoneChainIndexNoder<TCoordinate> noder
                = new MonotoneChainIndexNoder<TCoordinate>(_geoFactory,
                    new IntersectionAdder<TCoordinate>(li));

            return noder;
        }

        private void computeNodedEdges(IEnumerable<NodedSegmentString<TCoordinate>> bufferSegStrList, 
                                       IPrecisionModel<TCoordinate> precisionModel)
        {
            INoder<TCoordinate> noder = getNoder(precisionModel);
            IEnumerable<NodedSegmentString<TCoordinate>> nodedSegStrings = noder.Node(bufferSegStrList);

            foreach (NodedSegmentString<TCoordinate> segStr in nodedSegStrings)
            {
                Label oldLabel = (Label)segStr.Context;
                Edge<TCoordinate> edge = new Edge<TCoordinate>(
                    _geoFactory, segStr.Coordinates, oldLabel);
                InsertEdge(edge);
            }
        }

        /// <summary>
        /// Completes the building of the input subgraphs by depth-labeling them,
        /// and adds them to the <see cref="PolygonBuilder{TCoordinate}" />.
        /// The subgraph list must be sorted in rightmost-coordinate order.
        /// </summary>
        /// <param name="subgraphList">The subgraphs to build.</param>
        /// <param name="polyBuilder">The PolygonBuilder which will build the final polygons.</param>
        private static void buildSubgraphs(IEnumerable<BufferSubgraph<TCoordinate>> subgraphList, 
                                           PolygonBuilder<TCoordinate> polyBuilder)
        {
            List<BufferSubgraph<TCoordinate>> processedGraphs = new List<BufferSubgraph<TCoordinate>>();

            foreach (BufferSubgraph<TCoordinate> subgraph in subgraphList)
            {
                TCoordinate p = subgraph.RightMostCoordinate;
                SubgraphDepthLocater<TCoordinate> locater = new SubgraphDepthLocater<TCoordinate>(processedGraphs);
                Int32 outsideDepth = locater.GetDepth(p);
                subgraph.ComputeDepth(outsideDepth);
                subgraph.FindResultEdges();
                processedGraphs.Add(subgraph);
                IEnumerable<EdgeEnd<TCoordinate>> edges =
                    Enumerable.Upcast<EdgeEnd<TCoordinate>, DirectedEdge<TCoordinate>>(subgraph.DirectedEdges);
                polyBuilder.Add(edges, subgraph.Nodes);
            }
        }

        /// <summary>
        /// Compute the change in depth as an edge is crossed from R to L.
        /// </summary>
        private static Int32 depthDelta(Label label)
        {
            Locations left = label[0, Positions.Left];
            Locations right = label[0, Positions.Right];

            if (left == Locations.Interior && right == Locations.Exterior)
            {
                return 1;
            }
            
            if (left == Locations.Exterior && right == Locations.Interior)
            {
                return -1;
            }

            return 0;
        }

        private static IEnumerable<BufferSubgraph<TCoordinate>> createSubgraphs(PlanarGraph<TCoordinate> graph)
        {
            List<BufferSubgraph<TCoordinate>> subgraphList 
                = new List<BufferSubgraph<TCoordinate>>();

            foreach (Node<TCoordinate> node in graph.Nodes)
            {
                if (!node.IsVisited)
                {
                    BufferSubgraph<TCoordinate> subgraph 
                        = new BufferSubgraph<TCoordinate>();

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
        #endregion
    }
}
