using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A connected subset of the graph of
    /// <c>DirectedEdges</c> and <c>Node</c>s.
    /// Its edges will generate either
    /// a single polygon in the complete buffer, with zero or more holes, or
    /// one or more connected holes.
    /// </summary>
    internal class BufferSubgraph : IComparable
    {
        private readonly RightmostEdgeFinder _finder;
        private readonly List<DirectedEdge> _dirEdgeList  = new List<DirectedEdge>();
        private readonly List<Node> _nodes        = new List<Node>();
        private Coordinate _rightMostCoord;

        /// <summary>
        ///
        /// </summary>
        public BufferSubgraph()
        {
            _finder = new RightmostEdgeFinder();
        }

        /// <summary>
        ///
        /// </summary>
        public IList<DirectedEdge> DirectedEdges => _dirEdgeList;

        /// <summary>
        ///
        /// </summary>
        public IList<Node> Nodes => _nodes;

        /// <summary>
        /// Gets the rightmost coordinate in the edges of the subgraph.
        /// </summary>
        public Coordinate RightMostCoordinate => _rightMostCoord;

        /// <summary>
        /// Creates the subgraph consisting of all edges reachable from this node.
        /// Finds the edges in the graph and the rightmost coordinate.
        /// </summary>
        /// <param name="node">A node to start the graph traversal from.</param>
        public void Create(Node node)
        {
            AddReachable(node);
            _finder.FindEdge(_dirEdgeList);
            _rightMostCoord = _finder.Coordinate;
        }

        /// <summary>
        /// Adds all nodes and edges reachable from this node to the subgraph.
        /// Uses an explicit stack to avoid a large depth of recursion.
        /// </summary>
        /// <param name="startNode">A node known to be in the subgraph.</param>
        private void AddReachable(Node startNode)
        {
            var nodeStack = new Stack<Node>();
            nodeStack.Push(startNode);
            while (nodeStack.Count != 0)
            {
                var node = nodeStack.Pop();
                Add(node, nodeStack);
            }
        }

        /// <summary>
        /// Adds the argument node and all its out edges to the subgraph
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <param name="nodeStack">The current set of nodes being traversed.</param>
        private void Add(Node node, Stack<Node> nodeStack)
        {
            node.Visited = true;
            _nodes.Add(node);
            foreach (DirectedEdge de in (DirectedEdgeStar)node.Edges)
            {
                _dirEdgeList.Add(de);
                var sym = de.Sym;
                var symNode = sym.Node;
                /*
                * NOTE: this is a depth-first traversal of the graph.
                * This will cause a large depth of recursion.
                * It might be better to do a breadth-first traversal.
                */
                if (! symNode.IsVisited)
                    nodeStack.Push(symNode);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void ClearVisitedEdges()
        {
            foreach (var de in _dirEdgeList)
                de.Visited = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outsideDepth"></param>
        public void ComputeDepth(int outsideDepth)
        {
            ClearVisitedEdges();
            // find an outside edge to assign depth to
            var de = _finder.Edge;
            // right side of line returned by finder is on the outside
            de.SetEdgeDepths(Position.Right, outsideDepth);
            CopySymDepths(de);
            ComputeDepths(de);
        }

        /// <summary>
        /// Compute depths for all dirEdges via breadth-first traversal of nodes in graph.
        /// </summary>
        /// <param name="startEdge">Edge to start processing with.</param>
        // <FIX> MD - use iteration & queue rather than recursion, for speed and robustness
        private static void ComputeDepths(DirectedEdge startEdge)
        {
            var nodesVisited = new HashSet<Node>();
            var nodeQueue = new Queue<Node>();
            var startNode = startEdge.Node;
            nodeQueue.Enqueue(startNode);
            nodesVisited.Add(startNode);
            startEdge.Visited = true;
            while (nodeQueue.Count != 0)
            {
                var n = nodeQueue.Dequeue();
                nodesVisited.Add(n);
                // compute depths around node, starting at this edge since it has depths assigned
                ComputeNodeDepth(n);
                // add all adjacent nodes to process queue, unless the node has been visited already
                foreach (DirectedEdge de in (DirectedEdgeStar)n.Edges)
                {
                    var sym = de.Sym;
                    if (sym.IsVisited) continue;
                    var adjNode = sym.Node;
                    if (!(nodesVisited.Contains(adjNode)))
                    {
                        nodeQueue.Enqueue(adjNode);
                        nodesVisited.Add(adjNode);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        private static void ComputeNodeDepth(Node n)
        {
            // find a visited dirEdge to start at
            DirectedEdge startEdge = null;
            foreach (DirectedEdge de in (DirectedEdgeStar)n.Edges)
            {
                if (de.IsVisited || de.Sym.IsVisited)
                {
                    startEdge = de;
                    break;
                }
            }

            // MD - testing  Result: breaks algorithm
            // only compute string append if assertion would fail
            if (startEdge == null)
                //Assert.IsTrue(false, "unable to find edge to compute depths at " + n.Coordinate);
                throw new TopologyException("unable to find edge to compute depths at " + n.Coordinate);

            ((DirectedEdgeStar) n.Edges).ComputeDepths(startEdge);

            // copy depths to sym edges
            foreach (DirectedEdge de in (DirectedEdgeStar)n.Edges)
            {
                de.Visited = true;
                CopySymDepths(de);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        private static void CopySymDepths(DirectedEdge de)
        {
            var sym = de.Sym;
            sym.SetDepth(Position.Left, de.GetDepth(Position.Right));
            sym.SetDepth(Position.Right, de.GetDepth(Position.Left));
        }

        /// <summary>
        /// Find all edges whose depths indicates that they are in the result area(s).
        /// Since we want polygon shells to be
        /// oriented CW, choose dirEdges with the interior of the result on the RHS.
        /// Mark them as being in the result.
        /// Interior Area edges are the result of dimensional collapses.
        /// They do not form part of the result area boundary.
        /// </summary>
        public void FindResultEdges()
        {
            foreach (var de in _dirEdgeList)
            {
                /*
                * Select edges which have an interior depth on the RHS
                * and an exterior depth on the LHS.
                * Note that because of weird rounding effects there may be
                * edges which have negative depths!  Negative depths
                * count as "outside".
                */
                // <FIX> - handle negative depths
                if (de.GetDepth(Position.Right) >= 1 && de.GetDepth(Position.Left) <= 0 && !de.IsInteriorAreaEdge)
                    de.InResult = true;
            }
        }

        /// <summary>
        /// BufferSubgraphs are compared on the x-value of their rightmost Coordinate.
        /// This defines a partial ordering on the graphs such that:
        /// g1 >= g2 - Ring(g2) does not contain Ring(g1)
        /// where Polygon(g) is the buffer polygon that is built from g.
        /// This relationship is used to sort the BufferSubgraphs so that shells are guaranteed to
        /// be built before holes.
        /// </summary>
        public int CompareTo(object o)
        {
            var graph = (BufferSubgraph) o;
            if (RightMostCoordinate.X < graph.RightMostCoordinate.X)
                return -1;
            if (RightMostCoordinate.X > graph.RightMostCoordinate.X)
                return 1;
            return 0;
        }
    }
}
