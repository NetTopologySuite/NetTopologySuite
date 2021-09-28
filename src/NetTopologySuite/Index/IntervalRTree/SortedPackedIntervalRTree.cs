using System;
using System.Collections.Generic;

#if NTSREPORT
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
#endif

namespace NetTopologySuite.Index.IntervalRTree
{
    /// <summary>
    /// A static index on a set of 1-dimensional intervals,
    /// using an R-Tree packed based on the order of the interval midpoints.
    /// </summary>
    /// <remarks>
    /// It supports range searching,
    /// where the range is an interval of the real line (which may be a single point).
    /// A common use is to index 1-dimensional intervals which
    /// are the projection of 2-D objects onto an axis of the coordinate system.
    /// <para>
    /// This index structure is <i>static</i>
    /// - items cannot be added or removed once the first query has been made.
    /// The advantage of this characteristic is that the index performance
    /// can be optimized based on a fixed set of items.
    /// </para>
    /// <author>Martin Davis</author>
    /// </remarks>
    public class SortedPackedIntervalRTree<T>
    {
        private readonly object _leavesLock = new object();
        private readonly List<IntervalRTreeNode<T>> _leaves = new List<IntervalRTreeNode<T>>();

        /// <summary>
        /// If root is null that indicates
        /// that the tree has not yet been built,
        /// OR nothing has been added to the tree.
        /// In both cases, the tree is still open for insertions.
        /// </summary>
        private volatile IntervalRTreeNode<T> _root;

        /// <summary>
        /// Adds an item to the index which is associated with the given interval
        /// </summary>
        /// <param name="min">The lower bound of the item interval</param>
        /// <param name="max">The upper bound of the item interval</param>
        /// <param name="item">The item to insert</param>
        /// <exception cref="InvalidOperationException">if the index has already been queried</exception>
        public void Insert(double min, double max, T item)
        {
            if (_root != null)
                throw new InvalidOperationException("Index cannot be added to once it has been queried");
            _leaves.Add(new IntervalRTreeLeafNode<T>(min, max, item));
        }

        private void Init()
        {
            // already built
            if (_root != null) return;

            lock (_leavesLock)
            {
                /*
                 * if leaves is empty then nothing has been inserted.
                 * In this case it is safe to leave the tree in an open state
                 */
                if (_leaves.Count == 0) return;

                //2nd check
                if (_root == null)
                {
                    _root = BuildTree();
                }
            }
        }

        private IntervalRTreeNode<T> BuildTree()
        {
            // sort the leaf nodes
            _leaves.Sort(IntervalRTreeNode<T>.NodeComparator.Instance);

            // now group nodes into blocks of two and build tree up recursively
            var src = _leaves;
            var dest = new List<IntervalRTreeNode<T>>();

            int level = 0;
            while (true)
            {
                BuildLevel(src, dest, ref level);
                if (dest.Count == 1)
                    return dest[0];

                var temp = src;
                src = dest;
                dest = temp;
            }
        }

        private static void BuildLevel(List<IntervalRTreeNode<T>> src, List<IntervalRTreeNode<T>> dest, ref int level)
        {
            level++;
            dest.Clear();
            for (int i = 0; i < src.Count; i += 2)
            {
                var n1 = src[i];
                var n2 = (i + 1 < src.Count) ? src[i] : null;
                if (n2 == null)
                {
                    dest.Add(n1);
                }
                else
                {
                    var node = new IntervalRTreeBranchNode<T>(src[i], src[i + 1]);
#if NTSREPORT
                    //PrintNode(node, level);
#endif
                    dest.Add(node);
                }
            }
        }

#if NTSREPORT
        private void PrintNode(IntervalRTreeNode<T> node, int level)
        {
            Console.WriteLine(WKTWriter.ToLineString(new Coordinate(node.Min, level), new Coordinate(node.Max, level)));
        }
#endif

        /// <summary>
        /// Search for intervals in the index which intersect the given closed interval
        /// and apply the visitor to them.
        /// </summary>
        /// <param name="min">The lower bound of the query interval</param>
        /// <param name="max">The upper bound of the query interval</param>
        /// <param name="visitor">The visitor to pass any matched items to</param>
        public void Query(double min, double max, IItemVisitor<T> visitor)
        {
            Init();

            // if root is null tree must be empty
            if (_root == null) return;

            _root.Query(min, max, visitor);
        }

    }
}
