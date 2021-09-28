using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Represents a planar graph of edges that can be used to compute a
    /// polygonization, and implements the algorithms to compute the
    /// <see cref="EdgeRing"/>s formed by the graph.
    /// The marked flag on DirectedEdges is used to indicate that a directed edge
    /// has be logically deleted from the graph.
    /// </summary>
    public class PolygonizeGraph : PlanarGraph
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static int GetDegreeNonDeleted(Node node)
        {
            var edges = node.OutEdges.Edges;
            int degree = 0;
            foreach (PolygonizeDirectedEdge de in edges)
            {
                if (! de.IsMarked)
                    degree++;
            }
            return degree;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private static int GetDegree(Node node, long label)
        {
            var edges = node.OutEdges.Edges;
            int degree = 0;
            foreach (PolygonizeDirectedEdge de in edges)
            {
                if (de.Label == label)
                    degree++;
            }
            return degree;
        }

        /// <summary>
        /// Deletes all edges at a node.
        /// </summary>
        /// <param name="node"></param>
        public static void DeleteAllEdges(Node node)
        {
            var edges = node.OutEdges.Edges;
            foreach (PolygonizeDirectedEdge de in edges)
            {
                de.Marked = true;
                var sym = (PolygonizeDirectedEdge) de.Sym;
                if (sym != null) sym.Marked = true;
            }
        }

        private readonly GeometryFactory _factory;

        /// <summary>
        /// Create a new polygonization graph.
        /// </summary>
        /// <param name="factory"></param>
        public PolygonizeGraph(GeometryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Add a <c>LineString</c> forming an edge of the polygon graph.
        /// </summary>
        /// <param name="line">The line to add.</param>
        public void AddEdge(LineString line)
        {
            if (line.IsEmpty)
                return;

            var linePts = CoordinateArrays.RemoveRepeatedPoints(line.Coordinates);
            if (linePts.Length < 2)
                return; // see #87

            var startPt = linePts[0];
            var endPt = linePts[linePts.Length - 1];

            var nStart = GetNode(startPt);
            var nEnd = GetNode(endPt);

            var de0 = new PolygonizeDirectedEdge(nStart, nEnd, linePts[1], true);
            var de1 = new PolygonizeDirectedEdge(nEnd, nStart, linePts[linePts.Length - 2], false);
            var edge = new PolygonizeEdge(line);
            edge.SetDirectedEdges(de0, de1);
            Add(edge);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Node GetNode(Coordinate pt)
        {
            var node = FindNode(pt);
            if (node == null)
            {
                node = new Node(pt);
                // ensure node is only added once to graph
                Add(node);
            }
            return node;
        }

        /// <summary>
        ///
        /// </summary>
        private void ComputeNextCWEdges()
        {
            // set the next pointers for the edges around each node
            foreach (var node in Nodes)
                ComputeNextCWEdges(node);
        }

        /// <summary>
        /// Convert the maximal edge rings found by the initial graph traversal
        /// into the minimal edge rings required by NTS polygon topology rules.
        /// </summary>
        /// <param name="ringEdges">The list of start edges for the edgeRings to convert.</param>
        private static void ConvertMaximalToMinimalEdgeRings(IEnumerable<DirectedEdge> ringEdges)
        {
            foreach (PolygonizeDirectedEdge de in ringEdges)
            {
                long label = de.Label;
                var intNodes = FindIntersectionNodes(de, label);

                if (intNodes == null)
                    continue;

                // flip the next pointers on the intersection nodes to create minimal edge rings
                foreach (var node in intNodes)
                {
                    ComputeNextCCWEdges(node, label);
                }
            }
        }

        /// <summary>
        /// Finds all nodes in a maximal edgering which are self-intersection nodes
        /// </summary>
        /// <param name="startDE"></param>
        /// <param name="label"></param>
        /// <returns>
        /// The list of intersection nodes found,
        /// or null if no intersection nodes were found.
        /// </returns>
        private static IEnumerable<Node> FindIntersectionNodes(PolygonizeDirectedEdge startDE, long label)
        {
            var de = startDE;
            IList<Node> intNodes = null;
            do
            {
                var node = de.FromNode;
                if (GetDegree(node, label) > 1)
                {
                    if (intNodes == null)
                        intNodes = new List<Node>();
                    intNodes.Add(node);
                }

                de = de.Next;
                Assert.IsTrue(de != null, "found null DE in ring");
                Assert.IsTrue(de == startDE || ! de.IsInRing, "found DE already in ring");
            }
            while (de != startDE);
            return intNodes;
        }

        /// <summary>
        /// Computes the minimal EdgeRings formed by the edges in this graph.
        /// </summary>
        /// <returns>A list of the{EdgeRings found by the polygonization process.</returns>
        public IList<EdgeRing> GetEdgeRings()
        {
            // maybe could optimize this, since most of these pointers should be set correctly already by deleteCutEdges()
            ComputeNextCWEdges();
            // clear labels of all edges in graph
            Label(dirEdges, -1);
            var maximalRings = FindLabeledEdgeRings(dirEdges);
            ConvertMaximalToMinimalEdgeRings(maximalRings);

            // find all edgerings (which will now be minimal ones, as required)
            var edgeRingList = new List<EdgeRing>();
            foreach (PolygonizeDirectedEdge de in dirEdges)
            {
                if (de.IsMarked) continue;
                if (de.IsInRing) continue;

                var er = FindEdgeRing(de);
                edgeRingList.Add(er);
            }
            return edgeRingList;
        }

        /// <summary>
        /// Finds and labels all edgerings in the graph.
        /// The edge rings are labeling with unique integers.
        /// The labeling allows detecting cut edges.
        /// </summary>
        /// <param name="dirEdges">A List of the DirectedEdges in the graph.</param>
        /// <returns>A List of DirectedEdges, one for each edge ring found.</returns>
        private static IList<DirectedEdge> FindLabeledEdgeRings(IEnumerable<DirectedEdge> dirEdges)
        {
            var edgeRingStarts = new List<DirectedEdge>();
            // label the edge rings formed
            long currLabel = 1;
            foreach (PolygonizeDirectedEdge de in dirEdges)
            {
                if (de.IsMarked) continue;
                if (de.Label >= 0) continue;

                edgeRingStarts.Add(de);
                var edges = EdgeRing.FindDirEdgesInRing(de);

                Label(edges, currLabel);
                currLabel++;
            }
            return edgeRingStarts;
        }

        /// <summary>
        /// Finds and removes all cut edges from the graph.
        /// </summary>
        /// <returns>A list of the <c>LineString</c>s forming the removed cut edges.</returns>
        public IList<LineString> DeleteCutEdges()
        {
            ComputeNextCWEdges();
            // label the current set of edgerings
            FindLabeledEdgeRings(dirEdges);
            /*
            * Cut Edges are edges where both dirEdges have the same label.
            * Delete them, and record them
            */
            var cutLines = new List<LineString>();
            foreach (PolygonizeDirectedEdge de in dirEdges)
            {
                if (de.IsMarked) continue;

                var sym = (PolygonizeDirectedEdge) de.Sym;
                if (de.Label == sym.Label)
                {
                    de.Marked = true;
                    sym.Marked = true;

                    // save the line as a cut edge
                    var e = (PolygonizeEdge) de.Edge;
                    cutLines.Add(e.Line);
                }
            }
            return cutLines;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dirEdges"></param>
        /// <param name="label"></param>
        private static void Label(IEnumerable<DirectedEdge> dirEdges, long label)
        {
            foreach (PolygonizeDirectedEdge de in dirEdges)
                de.Label = label;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        private static void ComputeNextCWEdges(Node node)
        {
            var deStar = node.OutEdges;
            PolygonizeDirectedEdge startDE = null;
            PolygonizeDirectedEdge prevDE = null;

            // the edges are stored in CCW order around the star
            foreach (PolygonizeDirectedEdge outDE in deStar.Edges)
            {
                if (outDE.IsMarked) continue;

                if (startDE == null)
                    startDE = outDE;

                if (prevDE != null)
                {
                    var sym = (PolygonizeDirectedEdge) prevDE.Sym;
                    sym.Next = outDE;
                }
                prevDE = outDE;
            }
            if (prevDE != null)
            {
                var sym = (PolygonizeDirectedEdge) prevDE.Sym;
                sym.Next = startDE;
            }
        }

        /// <summary>
        /// Computes the next edge pointers going CCW around the given node, for the
        /// given edgering label.
        /// This algorithm has the effect of converting maximal edgerings into minimal edgerings
        /// </summary>
        /// <param name="node"></param>
        /// <param name="label"></param>
        private static void ComputeNextCCWEdges(Node node, long label)
        {
            var deStar = node.OutEdges;
            //PolyDirectedEdge lastInDE = null;
            PolygonizeDirectedEdge firstOutDE = null;
            PolygonizeDirectedEdge prevInDE = null;

            // the edges are stored in CCW order around the star
            var edges = deStar.Edges;
            //for (IEnumerator i = deStar.Edges.GetEnumerator(); i.MoveNext(); ) {
            for (int i = edges.Count - 1; i >= 0; i--)
            {
                var de = (PolygonizeDirectedEdge) edges[i];
                var sym = (PolygonizeDirectedEdge) de.Sym;

                PolygonizeDirectedEdge outDE = null;
                if (de.Label == label) outDE = de;

                PolygonizeDirectedEdge inDE = null;
                if (sym.Label == label) inDE =  sym;

                if (outDE == null && inDE == null) continue;  // this edge is not in edgering

                if (inDE != null)
                    prevInDE = inDE;

                if (outDE != null)
                {
                    if (prevInDE != null)
                    {
                        prevInDE.Next = outDE;
                        prevInDE = null;
                    }
                    if (firstOutDE == null)
                        firstOutDE = outDE;
                }
            }
            if (prevInDE != null)
            {
                Assert.IsTrue(firstOutDE != null);
                prevInDE.Next = firstOutDE;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="startDE"></param>
        /// <returns></returns>
        private EdgeRing FindEdgeRing(PolygonizeDirectedEdge startDE)
        {
            var er = new EdgeRing(_factory);
            er.Build(startDE);
            return er;
        }

        /// <summary>
        /// Marks all edges from the graph which are "dangles".
        /// Dangles are which are incident on a node with degree 1.
        /// This process is recursive, since removing a dangling edge
        /// may result in another edge becoming a dangle.
        /// In order to handle large recursion depths efficiently,
        /// an explicit recursion stack is used.
        /// </summary>
        /// <returns>A List containing the <see cref="LineString"/>s that formed dangles.</returns>
        public ICollection<LineString> DeleteDangles()
        {
            var nodesToRemove = FindNodesOfDegree(1);
            var dangleLines = new HashSet<LineString>();
            var nodeStack = new Stack<Node>();
            foreach (var node in nodesToRemove)
                nodeStack.Push(node);

            while (nodeStack.Count != 0)
            {
                var node = nodeStack.Pop();

                DeleteAllEdges(node);
                var nodeOutEdges = node.OutEdges.Edges;
                foreach (PolygonizeDirectedEdge de in nodeOutEdges)
                {
                    // delete this edge and its sym
                    de.Marked = true;
                    var sym = (PolygonizeDirectedEdge) de.Sym;
                    if (sym != null) sym.Marked = true;

                    // save the line as a dangle
                    var e = (PolygonizeEdge) de.Edge;
                    dangleLines.Add(e.Line);

                    var toNode = de.ToNode;
                    // add the toNode to the list to be processed, if it is now a dangle
                    if (GetDegreeNonDeleted(toNode) == 1)
                        nodeStack.Push(toNode);
                }
            }

            var dangleArray = new LineString[dangleLines.Count];
            dangleLines.CopyTo(dangleArray, 0);
            return new ReadOnlyCollection<LineString>(dangleArray);
                //new ArrayList(dangleLines.CastPlatform());
        }

        /// <summary>
        /// Traverses the polygonized edge rings in the graph
        /// and computes the depth parity (odd or even)
        /// relative to the exterior of the graph.
        ///
        /// If the client has requested that the output
        /// be polygonally valid, only odd polygons will be constructed.
        /// </summary>
        public void ComputeDepthParity()
        {
            while (true)
            {
                PolygonizeDirectedEdge de = null; //findLowestDirEdge();
                if (de == null)
                    return;
                ComputeDepthParity(de);
            }
        }

        /// <summary>
        /// Traverses all connected edges, computing the depth parity of the associated polygons.
        /// </summary>
        /// <param name="de"></param>
        private void ComputeDepthParity(PolygonizeDirectedEdge de)
        {

        }

    }
}
