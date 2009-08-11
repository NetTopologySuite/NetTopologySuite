using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// Builds the buffer geometry for a given input geometry and precision model.
    /// Allows setting the level of approximation for circular arcs,
    /// and the precision model in which to carry out the computation.
    /// 
    /// When computing buffers in floating point double-precision
    /// it can happen that the process of iterated noding can fail to converge (terminate).
    /// In this case a TopologyException will be thrown.
    /// Retrying the computation in a fixed precision
    /// can produce more robust results.
    ///</summary>
    public class BufferBuilder_110<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {

        ///<summary>
        /// Compute the change in depth as an edge is crossed from R to L
        ///</summary>
        private static Int32 DepthDelta(Label label)
        {
            Locations lLoc = label[0, Positions.Left];
            Locations rLoc = label[0, Positions.Right];
            if (lLoc == Locations.Interior && rLoc == Locations.Exterior)
                return 1;
            if (lLoc == Locations.Exterior && rLoc == Locations.Interior)
                return -1;

            return 0;
        }

        private BufferParameters _bufParams;

        private IPrecisionModel<TCoordinate> _workingPrecisionModel;
        private INoder<TCoordinate> _workingNoder;
        private IGeometryFactory<TCoordinate> _geomFact;
        private PlanarGraph<TCoordinate> _graph;
        private EdgeList<TCoordinate> _edgeList;

        ///<summary>
        /// Creates a new BufferBuilder
        ///</summary>
        ///<param name="bufParams"></param>
        public BufferBuilder_110(IGeometryFactory<TCoordinate> geomFact, BufferParameters bufParams)
        {
            _geomFact = geomFact;
            _edgeList = new EdgeList<TCoordinate>(_geomFact);
            _bufParams = bufParams;
        }

        ///<summary>
        /// Gets/Sets the precision model to use during the curve computation and noding,
        /// if it is different to the precision model of the Geometry.
        /// If the precision model is less than the precision of the Geometry precision model,
        /// the Geometry must have previously been rounded to that precision.
        ///</summary>
        public IPrecisionModel<TCoordinate> WorkingPrecisionModel
        {
            get
            { return _workingPrecisionModel; }
            set
            {
                _workingPrecisionModel = value;
            }
        }

        ///<summary>
        /// Gets/Sets the {@link Noder} to use during noding.
        /// This allows choosing fast but non-robust noding, or slower
        /// but robust noding.
        ///</summary>
        public INoder<TCoordinate> WorkingNoder
        {
            get { return _workingNoder; }
            set { _workingNoder = value; }
        }

        public IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> g, Double distance)
        {
            IPrecisionModel<TCoordinate> precisionModel = _workingPrecisionModel;
            if (precisionModel == null)
                precisionModel = g.Coordinates.CoordinateFactory.PrecisionModel;

            // factory must be the same as the one used by the input
            _geomFact = g.Factory;

            OffsetCurveBuilder_110<TCoordinate> curveBuilder = new OffsetCurveBuilder_110<TCoordinate>(_geomFact, precisionModel, _bufParams);
            OffsetCurveSetBuilder_110<TCoordinate> curveSetBuilder = new OffsetCurveSetBuilder_110<TCoordinate>(g, distance, curveBuilder);

            IEnumerable<NodedSegmentString<TCoordinate>> bufferSegStrList
                = curveSetBuilder.GetCurves();

            // short-circuit test
            if (!Slice.CountGreaterThan(bufferSegStrList, 0))
            {
                return _geomFact.CreateGeometryCollection();
            }

            //BufferDebug.runCount++;
            //String filename = "run" + BufferDebug.runCount + "_curves";
            //System.out.println("saving " + filename);
            //BufferDebug.saveEdges(bufferEdgeList, filename);
            // DEBUGGING ONLY
            //WKTWriter wktWriter = new WKTWriter();
            //Debug.println("Rings: " + wktWriter.write(convertSegStrings(bufferSegStrList.iterator())));
            //wktWriter.setMaxCoordinatesPerLine(10);
            //System.out.println(wktWriter.writeFormatted(convertSegStrings(bufferSegStrList.iterator())));

            ComputeNodedEdges(bufferSegStrList, precisionModel);
            _graph = new PlanarGraph<TCoordinate>(new OverlayNodeFactory<TCoordinate>());
            _graph.AddEdges(_edgeList);

            IEnumerable<BufferSubgraph<TCoordinate>> subgraphList = CreateSubgraphs(_graph);
            PolygonBuilder<TCoordinate> polyBuilder = new PolygonBuilder<TCoordinate>(_geomFact);
            BuildSubgraphs(subgraphList, polyBuilder);
            IEnumerable<IGeometry<TCoordinate>> resultPolyList = 
                Caster.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(polyBuilder.Polygons);

            // just in case...
            if (!Slice.CountGreaterThan(resultPolyList,0))
            {
                return CreateEmptyResultGeometry();
            }

            return _geomFact.BuildGeometry(resultPolyList); ;
        }

        private INoder<TCoordinate> GetNoder(IPrecisionModel<TCoordinate> precisionModel)
        {
            if (_workingNoder != null)
                return _workingNoder;

            // otherwise use a fast (but non-robust) noder
            LineIntersector<TCoordinate> li = new RobustLineIntersector<TCoordinate>(_geomFact);
            li.PrecisionModel = precisionModel;
            MonotoneChainIndexNoder<TCoordinate> noder = 
                new MonotoneChainIndexNoder<TCoordinate>(_geomFact, new IntersectionAdder<TCoordinate>(li));

            //    Noder noder = new IteratedNoder(precisionModel);
            return noder;
            //    Noder noder = new SimpleSnapRounder(precisionModel);
            //    Noder noder = new MCIndexSnapRounder(precisionModel);
            //    Noder noder = new ScaledNoder(new MCIndexSnapRounder(new PrecisionModel(1.0)),
            //                                  precisionModel.getScale());
        }

        private void ComputeNodedEdges(IEnumerable<NodedSegmentString<TCoordinate>> bufferSegStrList, IPrecisionModel<TCoordinate> precisionModel)
        {
            INoder<TCoordinate> noder = GetNoder(precisionModel);
            foreach (NodedSegmentString<TCoordinate> segStr in noder.Node(bufferSegStrList))
            {
                Label oldLabel = (Label) segStr.Context;
                Edge<TCoordinate> edge = new Edge<TCoordinate>(
                    _geomFact, segStr.Coordinates, oldLabel );//new Label(oldLabel[0], oldLabel[1]));
                InsertUniqueEdge(edge);
            }
        }


        /**
         * Inserted edges are checked to see if an identical edge already exists.
         * If so, the edge is not inserted, but its label is merged
         * with the existing edge.
         */
        protected void InsertUniqueEdge(Edge<TCoordinate> e)
        {
            //<FIX> MD 8 Oct 03  speed up identical edge lookup
            // fast lookup
            Edge<TCoordinate> existingEdge = _edgeList.FindEqualEdge(e);

            // If an identical edge already exists, simply update its label
            if (existingEdge != null)
            {
                Label existingLabel = existingEdge.Label.Value;

                Label labelToMerge = e.Label.Value;

                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = labelToMerge.Flip();
                }
                existingLabel.Merge(labelToMerge);

                // compute new depth delta of sum of edges
                int mergeDelta = DepthDelta(labelToMerge);
                int existingDelta = existingEdge.DepthDelta;
                int newDelta = existingDelta + mergeDelta;
                existingEdge.DepthDelta = newDelta;
            }
            else
            {   // no matching existing edge was found
                // add this new edge to the list of edges in this graph
                //e.setName(name + edges.size());
                _edgeList.Add(e);
                e.DepthDelta = DepthDelta(e.Label.Value);
            }
        }

        private IEnumerable<BufferSubgraph<TCoordinate>> CreateSubgraphs(PlanarGraph<TCoordinate> graph)
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

        /**
         * Completes the building of the input subgraphs by depth-labelling them,
         * and adds them to the PolygonBuilder.
         * The subgraph list must be sorted in rightmost-coordinate order.
         *
         * @param subgraphList the subgraphs to build
         * @param polyBuilder the PolygonBuilder which will build the final polygons
         */
        private void BuildSubgraphs(IEnumerable<BufferSubgraph<TCoordinate>> subgraphList,
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
                    Caster.Upcast<EdgeEnd<TCoordinate>, DirectedEdge<TCoordinate>>(subgraph.DirectedEdges);
                polyBuilder.Add(edges, subgraph.Nodes);
            }
        }

        //private static IGeometry<TCoordinate> convertSegStrings(Iterator it)
        //{
        //    GeometryFactory fact = new GeometryFactory();
        //    List lines = new ArrayList();
        //    while (it.hasNext())
        //    {
        //        SegmentString ss = (SegmentString)it.next();
        //        LineString line = fact.createLineString(ss.getCoordinates());
        //        lines.add(line);
        //    }
        //    return fact.buildGeometry(lines);
        //}

        /**
         * Gets the standard result for an empty buffer.
         * Since buffer always returns a polygonal result,
         * this is chosen to be an empty polygon.
         * 
         * @return the empty result geometry
         */
        private IGeometry<TCoordinate> CreateEmptyResultGeometry()
        {
            IGeometry<TCoordinate> emptyGeom = _geomFact.CreatePolygon();
            return emptyGeom;
        }
    }
}
