using System;
using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Index.IntervalRTree
{
    /**
     * A static index on a set of 1-dimensional intervals,
     * using an R-Tree packed based on the order of the interval midpoints.
     * It supports range searching,
     * where the range is an interval of the real line (which may be a single point).
     * A common use is to index 1-dimensional intervals which 
     * are the projection of 2-D objects onto an axis of the coordinate system.
     * <p>
     * This index structure is <i>static</i> 
     * - items cannot be added or removed once the first query has been made.
     * The advantage of this characteristic is that the index performance 
     * can be optimized based on a fixed set of items.
     * 
     * @author Martin Davis
     */
    public class SortedPackedIntervalRTree
    {
        private readonly List<IntervalRTreeNode> _leaves = new List<IntervalRTreeNode>();
        private IntervalRTreeNode _root;

        /**
         * Adds an item to the index which is associated with the given interval
         * 
         * @param min the lower bound of the item interval
         * @param max the upper bound of the item interval
         * @param item the item to insert
         * 
         * @throw IllegalStateException if the index has already been queried
         */
        public void Insert(double min, double max, Object item)
        {
            if (_root != null)
                throw new InvalidOperationException("Index cannot be added to once it has been queried");
            _leaves.Add(new IntervalRTreeLeafNode(min, max, item));
        }

        private void Init()
        {
            if (_root != null) return;
            _root = BuildTree();
        }

        private IntervalRTreeNode BuildTree()
        {
            // sort the leaf nodes
            _leaves.Sort(IntervalRTreeNode.NodeComparator.Instance);

            // now group nodes into blocks of two and build tree up recursively
            var src = _leaves;
            List<IntervalRTreeNode> temp;
            List<IntervalRTreeNode> dest = new List<IntervalRTreeNode>();

            while (true)
            {
                BuildLevel(src, dest);
                if (dest.Count == 1)
                    return dest[0];

                temp = src;
                src = dest;
                dest = temp;
            }
        }

        private int _level;

        private void BuildLevel(List<IntervalRTreeNode> src, List<IntervalRTreeNode> dest)
        {
            _level++;
            dest.Clear();
            for (int i = 0; i < src.Count; i += 2)
            {
                IntervalRTreeNode n1 = src[i];
                IntervalRTreeNode n2 = (i + 1 < src.Count) ? src[i] : null;
                if (n2 == null)
                {
                    dest.Add(n1);
                }
                else
                {
                    IntervalRTreeNode node =
                        new IntervalRTreeBranchNode(src[i], src[i + 1]);
                    //        printNode(node);
                    //				System.out.println(node);
                    dest.Add(node);
                }
            }
        }

        private void PrintNode(IntervalRTreeNode node)
        {
            Console.WriteLine(WKTWriter.ToLineString(new Coordinate(node.Min, _level), new Coordinate(node.Max, _level)));
        }

        /**
         * Search for intervals in the index which intersect the given closed interval
         * and apply the visitor to them.
         * 
         * @param min the lower bound of the query interval
         * @param max the upper bound of the query interval
         * @param visitor the visitor to pass any matched items to
         */
        public void Query(double min, double max, IItemVisitor visitor)
        {
            Init();

            _root.Query(min, max, visitor);
        }

    }
}