using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Diagnostics;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A connected subset of the graph of
    /// <see cref="DirectedEdge{TCoordinate}"/>s 
    /// and <see cref="Node{TCoordinate}"/>s.
    /// Its edges will generate either
    /// a single polygon in the complete buffer, with zero or more holes, or
    /// one or more connected holes.
    /// </summary>
    public class BufferSubgraph<TCoordinate> : IComparable<BufferSubgraph<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly List<DirectedEdge<TCoordinate>> _dirEdgeList = new List<DirectedEdge<TCoordinate>>();
        private readonly RightmostEdgeFinder<TCoordinate> _finder = new RightmostEdgeFinder<TCoordinate>();
        private readonly List<Node<TCoordinate>> _nodes = new List<Node<TCoordinate>>();
        private TCoordinate rightMostCoord;

        public IList<DirectedEdge<TCoordinate>> DirectedEdges
        {
            get { return _dirEdgeList; }
        }

        public IList<Node<TCoordinate>> Nodes
        {
            get { return _nodes; }
        }

        /// <summary>
        /// Gets the rightmost coordinate in the edges of the subgraph.
        /// </summary>
        public TCoordinate RightMostCoordinate
        {
            get { return rightMostCoord; }
        }

        #region IComparable<BufferSubgraph<TCoordinate>> Members

        /// <summary>
        /// BufferSubgraphs are compared on the x-value of their rightmost Coordinate.
        /// This defines a partial ordering on the graphs such that:
        /// g1 >= g2 - Ring(g2) does not contain Ring(g1)
        /// where Polygon(g) is the buffer polygon that is built from g.
        /// This relationship is used to sort the BufferSubgraphs so that shells are guaranteed to
        /// be built before holes.
        /// </summary>
        public Int32 CompareTo(BufferSubgraph<TCoordinate> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (RightMostCoordinate[Ordinates.X] < other.RightMostCoordinate[Ordinates.X])
            {
                return -1;
            }

            if (RightMostCoordinate[Ordinates.X] > other.RightMostCoordinate[Ordinates.X])
            {
                return 1;
            }

            return 0;
        }

        #endregion

        /// <summary>
        /// Creates the subgraph consisting of all edges reachable from this node.
        /// Finds the edges in the graph and the rightmost coordinate.
        /// </summary>
        /// <param name="node">A node to start the graph traversal from.</param>
        public void Create(Node<TCoordinate> node)
        {
            addReachable(node);
            _finder.FindEdge(_dirEdgeList);
            rightMostCoord = _finder.Coordinate;
        }

        public void ComputeDepth(Int32 outsideDepth)
        {
            clearVisitedEdges();

            // find an outside edge to assign depth to
            DirectedEdge<TCoordinate> de = _finder.Edge;

            // right side of line returned by finder is on the outside
            de.SetEdgeDepths(Positions.Right, outsideDepth);
            copySymDepths(de);
            computeDepths(de);
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
            foreach (DirectedEdge<TCoordinate> de in _dirEdgeList)
            {
                /*
                * Select edges which have an interior depth on the RHS
                * and an exterior depth on the LHS.
                * Note that because of weird rounding effects there may be
                * edges which have negative depths!  Negative depths
                * count as "outside".
                */
                // <FIX> - handle negative depths
                if (de.GetDepth(Positions.Right) >= 1 && de.GetDepth(Positions.Left) <= 0 && !de.IsInteriorAreaEdge)
                {
                    de.IsInResult = true;
                }
            }
        }

        /// <summary>
        /// Adds all nodes and edges reachable from this node to the subgraph.
        /// Uses an explicit stack to avoid a large depth of recursion.
        /// </summary>
        /// <param name="startNode">A node known to be in the subgraph.</param>
        private void addReachable(Node<TCoordinate> startNode)
        {
            Stack<Node<TCoordinate>> nodeStack = new Stack<Node<TCoordinate>>();
            nodeStack.Push(startNode);

            while (nodeStack.Count != 0)
            {
                Node<TCoordinate> node = nodeStack.Pop();
                add(node, nodeStack);
            }
        }

        /// <summary>
        /// Adds the argument node and all its out edges to the subgraph
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <param name="nodeStack">The current set of nodes being traversed.</param>
        private void add(Node<TCoordinate> node, Stack<Node<TCoordinate>> nodeStack)
        {
            node.Visited = true;
            _nodes.Add(node);

            foreach (DirectedEdge<TCoordinate> de in node.Edges)
            {
                _dirEdgeList.Add(de);
                DirectedEdge<TCoordinate> sym = de.Sym;
                Node<TCoordinate> symNode = sym.Node;
                Debug.Assert(symNode != null);

                /*
                * NOTE: this is a depth-first traversal of the graph.
                * This will cause a large depth of recursion.
                * It might be better to do a breadth-first traversal.
                */
                if (!symNode.IsVisited)
                {
                    nodeStack.Push(symNode);
                }
            }
        }

        private void clearVisitedEdges()
        {
            _dirEdgeList.ForEach(delegate(DirectedEdge<TCoordinate> item) { item.IsVisited = false; });
        }

        /// <summary>
        /// Compute depths for all dirEdges via breadth-first traversal of nodes in graph.
        /// </summary>
        /// <param name="startEdge">Edge to start processing with.</param>
        // <FIX> MD - use iteration & queue rather than recursion, for speed and robustness
        private void computeDepths(DirectedEdge<TCoordinate> startEdge)
        {
            ISet<Node<TCoordinate>> nodesVisited = new HashedSet<Node<TCoordinate>>();
            Queue<Node<TCoordinate>> nodeQueue = new Queue<Node<TCoordinate>>();
            Node<TCoordinate> startNode = startEdge.Node;
            nodeQueue.Enqueue(startNode);
            nodesVisited.Add(startNode);
            startEdge.IsVisited = true;

            while (nodeQueue.Count != 0)
            {
                Node<TCoordinate> n = nodeQueue.Dequeue();
                nodesVisited.Add(n);

                // compute depths around node, starting at this edge since it has depths assigned
                computeNodeDepth(n);

                // add all adjacent nodes to process queue, unless the node has been visited already

                DirectedEdgeStar<TCoordinate> edges = n.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(edges != null);

                foreach (DirectedEdge<TCoordinate> de in edges)
                {
                    DirectedEdge<TCoordinate> sym = de.Sym;
                    Debug.Assert(sym != null);

                    if (sym.IsVisited)
                    {
                        continue;
                    }

                    Node<TCoordinate> adjNode = sym.Node;

                    if (!(nodesVisited.Contains(adjNode)))
                    {
                        nodeQueue.Enqueue(adjNode);
                        nodesVisited.Add(adjNode);
                    }
                }
            }
        }

        private void computeNodeDepth(Node<TCoordinate> n)
        {
            // find a visited dirEdge to start at
            DirectedEdge<TCoordinate> startEdge = null;
            DirectedEdgeStar<TCoordinate> edges = n.Edges as DirectedEdgeStar<TCoordinate>;
            Debug.Assert(edges != null);

            foreach (DirectedEdge<TCoordinate> de in edges)
            {
                if (de.IsVisited || de.Sym.IsVisited)
                {
                    startEdge = de;
                    break;
                }
            }


            // MD - testing  Result: breaks algorithm
            Assert.IsTrue(startEdge != null, "unable to find edge to compute depths at " + n.Coordinate);
            edges.ComputeDepths(startEdge);

            // copy depths to sym edges
            foreach (DirectedEdge<TCoordinate> de in edges)
            {
                de.IsVisited = true;
                copySymDepths(de);
            }
        }

        private void copySymDepths(DirectedEdge<TCoordinate> de)
        {
            DirectedEdge<TCoordinate> sym = de.Sym;
            Debug.Assert(sym != null);
            sym.SetDepth(Positions.Left, de.GetDepth(Positions.Right));
            sym.SetDepth(Positions.Right, de.GetDepth(Positions.Left));
        }
    }
}