using System.Collections;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary> 
    /// Builds the buffer point for a given input point and precision model.
    /// Allows setting the level of approximation for circular arcs,
    /// and the precision model in which to carry out the computation.
    /// When computing buffers in floating point double-precision
    /// it can happen that the process of iterated noding can fail to converge (terminate).
    /// In this case a TopologyException will be thrown.
    /// Retrying the computation in a fixed precision
    /// can produce more robust results.    
    /// </summary>
    public class BufferBuilder
    {
        /// <summary>
        /// Compute the change in depth as an edge is crossed from R to L.
        /// </summary>
        /// <param name="label"></param>
        private static int DepthDelta(Label label)
        {
            var lLoc = label.GetLocation(0, Positions.Left);
            var rLoc = label.GetLocation(0, Positions.Right);
            if (lLoc == Locations.Interior && rLoc == Locations.Exterior)
                return 1;
            else if (lLoc == Locations.Exterior && rLoc == Locations.Interior)
                return -1;
            return 0;
        }
        
        private int quadrantSegments = OffsetCurveBuilder.DefaultQuadrantSegments;
        private BufferStyle endCapStyle = BufferStyle.CapRound;
        
        private IPrecisionModel workingPrecisionModel;
        private INoder workingNoder;
        private IGeometryFactory geomFact;
        private PlanarGraph graph;
        private readonly EdgeList edgeList = new EdgeList();        

        /// <summary>
        /// Gets/Sets the number of segments used to approximate a angle fillet.
        /// </summary>
        public int QuadrantSegments
        {
            get { return quadrantSegments;  }
            set { quadrantSegments = value; }
        }

        /// <summary>
        /// Gets/Sets the precision model to use during the curve computation and noding,
        /// if it is different to the precision model of the Geometry.
        /// If the precision model is less than the precision of the Geometry precision model,
        /// the Geometry must have previously been rounded to that precision.
        /// </summary>
        public IPrecisionModel WorkingPrecisionModel
        {
            get { return workingPrecisionModel;  }
            set { workingPrecisionModel = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public BufferStyle EndCapStyle
        {
            get { return endCapStyle;  }
            set { endCapStyle = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public INoder Noder
        {
            get { return workingNoder; }
            set { workingNoder = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public IGeometry Buffer(IGeometry g, double distance)
        {
            var precisionModel = workingPrecisionModel ?? g.PrecisionModel;

            // factory must be the same as the one used by the input
            geomFact = g.Factory;

            var curveBuilder = new OffsetCurveBuilder(precisionModel, quadrantSegments) {EndCapStyle = endCapStyle};
            var curveSetBuilder = new OffsetCurveSetBuilder(g, distance, curveBuilder);

            var bufferSegStrList = curveSetBuilder.GetCurves();

            // short-circuit test
            if (bufferSegStrList.Count <= 0)
            {
                IGeometry emptyGeom = geomFact.CreateGeometryCollection(new IGeometry[0]);
                return emptyGeom;
            }

            ComputeNodedEdges(bufferSegStrList, precisionModel);
            graph = new PlanarGraph(new OverlayNodeFactory());
            graph.AddEdges(edgeList.Edges);

            var subgraphList = CreateSubgraphs(graph);
            var polyBuilder = new PolygonBuilder(geomFact);
            BuildSubgraphs(subgraphList, polyBuilder);
            var resultPolyList = polyBuilder.Polygons;

            var resultGeom = geomFact.BuildGeometry(resultPolyList);
            return resultGeom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="precisionModel"></param>
        /// <returns></returns>
        private INoder GetNoder(IPrecisionModel precisionModel)
        {
            if (workingNoder != null) 
                return workingNoder;

            // otherwise use a fast (but non-robust) noder
            var li = new RobustLineIntersector {PrecisionModel = precisionModel};
            var noder = new MCIndexNoder(new IntersectionAdder(li));                     
            return noder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferSegStrList"></param>
        /// <param name="precisionModel"></param>
        private void ComputeNodedEdges(IList bufferSegStrList, IPrecisionModel precisionModel)
        {
            var noder = GetNoder(precisionModel);
            noder.ComputeNodes(bufferSegStrList);
            var nodedSegStrings = noder.GetNodedSubstrings();
            
            foreach (var obj in nodedSegStrings)
            {
                var segStr = (SegmentString) obj;
                var oldLabel = (Label) segStr.Data;
                var edge = new Edge(segStr.Coordinates, new Label(oldLabel));
                InsertEdge(edge);
            }
        }

        /// <summary>
        /// Inserted edges are checked to see if an identical edge already exists.
        /// If so, the edge is not inserted, but its label is merged
        /// with the existing edge.
        /// </summary>
        /// <param name="e"></param>
        protected void InsertEdge(Edge e)
        {
            //<FIX> MD 8 Oct 03  speed up identical edge lookup
            // fast lookup
            var existingEdge = edgeList.FindEqualEdge(e);

            // If an identical edge already exists, simply update its label
            if (existingEdge != null)
            {
                var existingLabel = existingEdge.Label;

                var labelToMerge = e.Label;
                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = new Label(e.Label);
                    labelToMerge.Flip();
                }
                existingLabel.Merge(labelToMerge);

                // compute new depth delta of sum of edges
                var mergeDelta = DepthDelta(labelToMerge);
                var existingDelta = existingEdge.DepthDelta;
                var newDelta = existingDelta + mergeDelta;
                existingEdge.DepthDelta = newDelta;
            }
            else
            {   // no matching existing edge was found
                // add this new edge to the list of edges in this graph
                //e.setName(name + edges.size());
                edgeList.Add(e);
                e.DepthDelta = DepthDelta(e.Label);
            }
        }

        private IList CreateSubgraphs(PlanarGraph graph)
        {
            var subgraphList = new ArrayList();
            foreach(var obj in graph.Nodes)
            {
                var node = (Node)obj;
                if (node.IsVisited)
                    continue;
                var subgraph = new BufferSubgraph();
                subgraph.Create(node);
                subgraphList.Add(subgraph);
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
        /// and adds them to the <see cref="PolygonBuilder" />.
        /// The subgraph list must be sorted in rightmost-coordinate order.
        /// </summary>
        /// <param name="subgraphList">The subgraphs to build.</param>
        /// <param name="polyBuilder">The PolygonBuilder which will build the final polygons.</param>
        private void BuildSubgraphs(IList subgraphList, PolygonBuilder polyBuilder)
        {
            IList processedGraphs = new ArrayList();
            foreach(var obj in subgraphList)
            {
                var subgraph = (BufferSubgraph)obj;
                var p = subgraph.RightMostCoordinate;
                var locater = new SubgraphDepthLocater(processedGraphs);
                var outsideDepth = locater.GetDepth(p);
                subgraph.ComputeDepth(outsideDepth);
                subgraph.FindResultEdges();
                processedGraphs.Add(subgraph);
                polyBuilder.Add(subgraph.DirectedEdges, subgraph.Nodes);
            }
        }

    }
}
