using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.IntervalRTree
{

    ///<summary>
    /// A static index on a set of 1-dimensional intervals, using an R-Tree packed based on the order of the interval midpoints.
    /// It supports range searching, where the range is an interval of the real line (which may be a single point).
    /// A common use is to index 1-dimensional intervals which are the projection of 2-D objects onto an axis of the coordinate system.
    /// This index structure is <i>static</i>
    /// - items cannot be added or removed once the first query has been made.
    /// The advantage of this characteristic is that the index performance can be optimized based on a fixed set of items.
    ///</summary>
    public class SortedPackedIntervalRTree<TItem>
        where TItem : IBoundable<Interval>
    {
        private List<IntervalRTreeNode<TItem>> _leaves;
        private IntervalRTreeNode<TItem> _root;

        private int _level = 0;

        ///<summary>
        /// constructor of this class
        ///</summary>
        public SortedPackedIntervalRTree()
        {
            _leaves = new List<IntervalRTreeNode<TItem>>();
        }

        ///<summary>
        ///</summary>
        ///<param name="interval"></param>
        ///<param name="item"></param>
        ///<exception cref="InvalidOperationException"></exception>
        public void Insert(Interval interval, TItem item )
        {
            if ( _root != null )
                throw new InvalidOperationException("Index cannot be added to once it has been queried");
            _leaves.Add(new IntervalRTreeLeafNode<TItem>(item));
        }

        private void Init()
        {
            if (_root != null) return;
            _root = BuildTree();
        }

        private IntervalRTreeNode<TItem> BuildTree()
        {
            _leaves.Sort( CompareOp );

            // now group nodes into blocks of two and build tree up recursively
            List<IntervalRTreeNode<TItem>> src = _leaves;
            List<IntervalRTreeNode<TItem>> dest = new List<IntervalRTreeNode<TItem>>();

            while (true) {

                BuildLevel(src, dest);
                if (dest.Count == 1)
                    return dest[0];

                List<IntervalRTreeNode<TItem>> temp = null;
                temp = src;
                src = dest;
                dest = temp;
            }
        }

        private void BuildLevel(List<IntervalRTreeNode<TItem>> src, List<IntervalRTreeNode<TItem>> dest)
        {
            _level++;
            dest.Clear();
            for (int i = 0; i < src.Count; i += 2)
            {

                IntervalRTreeNode<TItem> n1 = src[i];
                IntervalRTreeNode<TItem> n2 = (i + 1 < src.Count)
                                                ?
                                                    src[i]
                                                :
                                                    null;

                if (n2 == null)
                {
                    dest.Add(n1);
                }
                else
                {
                    IntervalRTreeNode<TItem> node = new IntervalRTreeBranchNode<TItem>(
                        _level, n1, src[i + 1] );
                    dest.Add(node);
                }
            }
        }

        ///<summary>
        /// Search for intervals in the index which intersect the given closed interval
        ///</summary>
        ///<param name="interval">query interval</param>
        ///<returns>enumerable of {TItems}</returns>
        public IEnumerable<TItem> Query(Interval interval)
        {
            Init();
            return _root.Query(interval);
        }

        protected Comparison<IntervalRTreeNode<TItem>> CompareOp
        {
            get
            {
                return delegate(IntervalRTreeNode<TItem> left, IntervalRTreeNode<TItem> right)
                {
                    return left.Bounds.CompareTo(right.Bounds);
                };
            }
        }
    }
}
