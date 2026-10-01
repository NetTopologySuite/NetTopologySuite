using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.KdTree
{
    public partial class KdTree<T>
    {
        /// <summary>
        /// Finds the nearest node in the tree to the given query point.
        /// </summary>
        /// <param name="query">The query point</param>
        /// <returns>The nearest node, or <c>null</c> if the tree is empty</returns>
        public KdNode<T> NearestNeighbor(Coordinate query)
        {
            if (_root == null)
                return null;

            KdNode<T> bestNode = null;
            double bestDistSq = double.PositiveInfinity;

            var stack = new Stack<NNFrame>();
            stack.Push(new NNFrame(_root, isAxisX: true));

            while (stack.Count > 0)
            {
                var frame = stack.Pop();
                var node = frame.Node;
                if (node == null) continue;

                //-- 1. visit this node
                double dx = query.X - node.X;
                double dy = query.Y - node.Y;
                double dSq = dx * dx + dy * dy;
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    bestNode = node;
                    if (dSq == 0)
                        break; // perfect hit
                }

                //-- 2. decide which child to explore first
                bool axisIsX = frame.IsAxisX;
                double diff = axisIsX ? dx : dy;

                var nearChild = (diff < 0) ? node.Left : node.Right;
                var farChild = (diff < 0) ? node.Right : node.Left;

                //-- 3. depth-first: push far side only if it can still win
                if (farChild != null && diff * diff < bestDistSq)
                {
                    stack.Push(new NNFrame(farChild, isAxisX: !axisIsX));
                }
                if (nearChild != null)
                {
                    stack.Push(new NNFrame(nearChild, isAxisX: !axisIsX));
                }
            }
            return bestNode;
        }

        /// <summary>
        /// Finds the nearest <paramref name="k"/> nodes in the tree to the given query point.
        /// </summary>
        /// <param name="query">The query point</param>
        /// <param name="k">The number of nearest nodes to find</param>
        /// <returns>A list of the nearest nodes, sorted by distance (closest first),
        /// or an empty list if the tree is empty or <paramref name="k"/> is non-positive.</returns>
        public IList<KdNode<T>> NearestNeighbors(Coordinate query, int k)
        {
            if (_root == null || k <= 0)
                return new List<KdNode<T>>();

            //-- Max-heap (Neighbor.CompareTo inverts ordering) so Peek() / Poll()
            //-- return the worst (farthest) kept candidate.
            var heap = new PriorityQueue<Neighbor>();
            double worstDistSq = double.PositiveInfinity; // updated when heap full

            //-- depth-first search with an explicit stack
            var stack = new Stack<NNStackFrame>();
            var node = _root;
            bool axisIsX = true;

            while (node != null || stack.Count > 0)
            {
                //-- a) descend
                if (node != null)
                {
                    //-- visit the current node
                    double dx = query.X - node.X;
                    double dy = query.Y - node.Y;
                    double distSq = dx * dx + dy * dy;

                    if (heap.Size < k) // not full yet
                    {
                        heap.Add(new Neighbor(node, distSq));
                        if (heap.Size == k)
                            worstDistSq = heap.Peek().DistSq;
                    }
                    else if (distSq < worstDistSq) // better than worst
                    {
                        heap.Poll(); // discard worst
                        heap.Add(new Neighbor(node, distSq));
                        worstDistSq = heap.Peek().DistSq;
                    }

                    //-- choose near / far child
                    double split = axisIsX ? node.X : node.Y;
                    double diff = axisIsX ? dx : dy;

                    var nearChild = (diff < 0) ? node.Left : node.Right;
                    var farChild = (diff < 0) ? node.Right : node.Left;

                    //-- push the far branch (if it exists) together with split info
                    if (farChild != null)
                    {
                        stack.Push(new NNStackFrame(farChild, axisIsX, split, !axisIsX));
                    }

                    //-- tail-recurse into the near branch
                    node = nearChild;
                    axisIsX = !axisIsX;
                }
                //-- b) backtrack
                else
                {
                    var sf = stack.Pop();
                    double diff = sf.ParentSplitAxis
                        ? query.X - sf.ParentSplitValue
                        : query.Y - sf.ParentSplitValue;
                    double diffSq = diff * diff;

                    if (heap.Size < k || diffSq < worstDistSq)
                    {
                        node = sf.Node;
                        axisIsX = sf.NodeAxisX;
                    }
                    else
                    {
                        node = null; // prune whole subtree
                    }
                }
            }

            //-- Drain the max-heap to an array worst-first, then reverse for best-first.
            int count = heap.Size;
            var result = new KdNode<T>[count];
            for (int i = count - 1; i >= 0; i--)
            {
                result[i] = heap.Poll().Node;
            }
            return result;
        }

        private readonly struct NNFrame
        {
            public NNFrame(KdNode<T> node, bool isAxisX)
            {
                Node = node;
                IsAxisX = isAxisX;
            }

            public KdNode<T> Node { get; }
            public bool IsAxisX { get; }
        }

        private readonly struct NNStackFrame
        {
            public NNStackFrame(KdNode<T> node, bool parentSplitAxis, double parentSplitValue, bool nodeAxisX)
            {
                Node = node;
                ParentSplitAxis = parentSplitAxis;
                ParentSplitValue = parentSplitValue;
                NodeAxisX = nodeAxisX;
            }

            public KdNode<T> Node { get; }
            public bool ParentSplitAxis { get; }
            public double ParentSplitValue { get; }
            public bool NodeAxisX { get; }
        }

        /// <summary>
        /// Heap entry for k-NN search. <see cref="CompareTo"/> reverses natural ordering
        /// so <see cref="PriorityQueue{T}"/> behaves as a max-heap (Peek/Poll yield the worst-so-far).
        /// </summary>
        private sealed class Neighbor : IComparable<Neighbor>
        {
            public Neighbor(KdNode<T> node, double distSq)
            {
                Node = node;
                DistSq = distSq;
            }

            public KdNode<T> Node { get; }
            public double DistSq { get; }

            public int CompareTo(Neighbor other)
            {
                if (other == null) return -1;
                return other.DistSq.CompareTo(DistSq);
            }
        }
    }
}
