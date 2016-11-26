using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Operation.Buffer;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// Builds the buffer geometry for a given input geometry and precision model.
    /// Allows setting the level of approximation for circular arcs,
    /// and the precision model in which to carry out the computation.
    ///</summary>
    /// <remarks>
    /// When computing buffers in floating point double-precision 
    /// it can happen that the process of iterated noding can fail to converge (terminate).
    /// In this case a <see cref="TopologyException"/> will be thrown.
    /// Retrying the computation in a fixed precision
    /// can produce more robust results.
    /// </remarks>
    internal class BufferBuilder
    {
        ///<summary>Compute the change in depth as an edge is crossed from R to L</summary>
        private static int DepthDelta(Label label)
        {
            var lLoc = label.GetLocation(0, Positions.Left);
            var rLoc = label.GetLocation(0, Positions.Right);
            if (lLoc == Location.Interior && rLoc == Location.Exterior)
                return 1;
            if (lLoc == Location.Exterior && rLoc == Location.Interior)
                return -1;
            return 0;
        }
        private readonly IBufferParameters _bufParams;

        private IPrecisionModel _workingPrecisionModel;
        private INoder _workingNoder;
        private IGeometryFactory _geomFact;
        private PlanarGraph _graph;
        private readonly EdgeList _edgeList = new EdgeList();

        ///<summary>Creates a new BufferBuilder</summary>
        public BufferBuilder(IBufferParameters bufParams)
        {
            _bufParams = bufParams;
        }

        ///<summary>
        /// Sets the precision model to use during the curve computation and noding, 
        /// if it is different to the precision model of the Geometry.
        ///</summary>
        ///<remarks>
        /// If the precision model is less than the precision of the Geometry precision model,
        /// the Geometry must have previously been rounded to that precision.
        ///</remarks>
        public IPrecisionModel WorkingPrecisionModel
        {
            get { return _workingPrecisionModel; }
            set { _workingPrecisionModel = value; }
        }

        ///<summary>
        /// Sets the <see cref="INoder"/> to use during noding.
        /// This allows choosing fast but non-robust noding, or slower
        /// but robust noding.
        ///</summary>
        public INoder Noder
        {
            get { return _workingNoder; }
            set { _workingNoder = value; }
        }

        public IGeometry Buffer(IGeometry g, double distance)
        {
            IPrecisionModel precisionModel = _workingPrecisionModel;
            if (precisionModel == null)
                precisionModel = g.PrecisionModel;

            // factory must be the same as the one used by the input
            _geomFact = g.Factory;

            OffsetCurveBuilder curveBuilder = new OffsetCurveBuilder(precisionModel, _bufParams);

            OffsetCurveSetBuilder curveSetBuilder = new OffsetCurveSetBuilder(g, distance, curveBuilder);

            var bufferSegStrList = curveSetBuilder.GetCurves();

            // short-circuit test
            if (bufferSegStrList.Count <= 0)
            {
                return CreateEmptyResultGeometry();
            }

            ComputeNodedEdges(bufferSegStrList, precisionModel);
            _graph = new PlanarGraph(new OverlayNodeFactory());
            _graph.AddEdges(_edgeList.Edges);

            IEnumerable<BufferSubgraph> subgraphList = CreateSubgraphs(_graph);
            PolygonBuilder polyBuilder = new PolygonBuilder(_geomFact);
            BuildSubgraphs(subgraphList, polyBuilder);
            var resultPolyList = polyBuilder.Polygons;

            // just in case...
            if (resultPolyList.Count <= 0)
            {
                return CreateEmptyResultGeometry();
            }

            IGeometry resultGeom = _geomFact.BuildGeometry(resultPolyList);
            return resultGeom;
        }

        private INoder GetNoder(IPrecisionModel precisionModel)
        {
            if (_workingNoder != null) return _workingNoder;

            // otherwise use a fast (but non-robust) noder
            var noder = new MCIndexNoder(new IntersectionAdder(new RobustLineIntersector { PrecisionModel = precisionModel}));
            
            //var li = new RobustLineIntersector();
            //li.PrecisionModel = precisionModel;
            //noder.SegmentIntersector = new IntersectionAdder(li);
            //    Noder noder = new IteratedNoder(precisionModel);
            return noder;
            //    Noder noder = new SimpleSnapRounder(precisionModel);
            //    Noder noder = new MCIndexSnapRounder(precisionModel);
            //    Noder noder = new ScaledNoder(new MCIndexSnapRounder(new PrecisionModel(1.0)),
            //                                  precisionModel.getScale());
        }

        private void ComputeNodedEdges(IList<ISegmentString> bufferSegStrList, IPrecisionModel precisionModel)
        {
            var noder = GetNoder(precisionModel);
            noder.ComputeNodes(bufferSegStrList);
            var nodedSegStrings = noder.GetNodedSubstrings();
            // DEBUGGING ONLY
            //BufferDebug.saveEdges(nodedEdges, "run" + BufferDebug.runCount + "_nodedEdges");

            foreach (var segStr in nodedSegStrings)
            {
                /**
                 * Discard edges which have zero length, 
                 * since they carry no information and cause problems with topology building
                 */
                var pts = segStr.Coordinates;
                if (pts.Length == 2 && pts[0].Equals2D(pts[1]))
                    continue;

                var oldLabel = (Label)segStr.Context;
                var edge = new Edge(segStr.Coordinates, new Label(oldLabel));

                InsertUniqueEdge(edge);
            }
            //saveEdges(edgeList.getEdges(), "run" + runCount + "_collapsedEdges");
        }


        /// <summary>
        /// Inserted edges are checked to see if an identical edge already exists.
        /// If so, the edge is not inserted, but its label is merged
        /// with the existing edge.
        /// </summary>
        protected void InsertUniqueEdge(Edge e)
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
                e.DepthDelta = DepthDelta(e.Label);
            }
        }

        private static IEnumerable<BufferSubgraph> CreateSubgraphs(PlanarGraph graph)
        {
            var subgraphList = new List<BufferSubgraph>();
            foreach (Node node in graph.Nodes)
            {
                if (!node.IsVisited)
                {
                    var subgraph = new BufferSubgraph();
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
        /// Completes the building of the input subgraphs by depth-labelling them,
        /// and adds them to the PolygonBuilder.
        /// </summary>
        /// <remarks>
        /// The subgraph list must be sorted in rightmost-coordinate order.
        /// </remarks>
        /// <param name="subgraphList"> the subgraphs to build</param>
        /// <param name="polyBuilder"> the PolygonBuilder which will build the final polygons</param>
        private static void BuildSubgraphs(IEnumerable<BufferSubgraph> subgraphList, PolygonBuilder polyBuilder)
        {
            var processedGraphs = new List<BufferSubgraph>();
            foreach (var subgraph in subgraphList)
            {
                Coordinate p = subgraph.RightMostCoordinate;
                //      int outsideDepth = 0;
                //      if (polyBuilder.containsPoint(p))
                //        outsideDepth = 1;
                var locater = new SubgraphDepthLocater(processedGraphs);
                int outsideDepth = locater.GetDepth(p);
                //      try {
                subgraph.ComputeDepth(outsideDepth);
                //      }
                //      catch (RuntimeException ex) {
                //        // debugging only
                //        //subgraph.saveDirEdges();
                //        throw ex;
                //      }
                subgraph.FindResultEdges();
                processedGraphs.Add(subgraph);
                polyBuilder.Add(new List<EdgeEnd>(
                    Utilities.Caster.Upcast<DirectedEdge, EdgeEnd>(subgraph.DirectedEdges)), subgraph.Nodes);
            }
        }

        private static IGeometry ConvertSegStrings(IEnumerator<ISegmentString> it)
        {
            var fact = new GeometryFactory();
            var lines = new List<IGeometry>();
            while (it.MoveNext())
            {
                ISegmentString ss = it.Current;
                ILineString line = fact.CreateLineString(ss.Coordinates);
                lines.Add(line);
            }
            return fact.BuildGeometry(lines);
        }

        ///<summary>
        /// Gets the standard result for an empty buffer.
        /// Since buffer always returns a polygonal result, this is chosen to be an empty polygon.
        ///</summary>
        /// <returns>The empty result geometry</returns>
        private IGeometry CreateEmptyResultGeometry()
        {
            IGeometry emptyGeom = _geomFact.CreatePolygon(null, null);
            return emptyGeom;
        }
    }
}
