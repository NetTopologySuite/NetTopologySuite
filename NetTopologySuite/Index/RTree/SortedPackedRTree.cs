using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.RTree
{
    ///<summary>
    /// A static index on a set of 1-dimensional intervals, using an R-Tree packed based on the order of the interval midpoints.
    /// It supports range searching, where the range is an interval of the real line (which may be a single point).
    /// A common use is to index 1-dimensional intervals which are the projection of 2-D objects onto an axis of the coordinate system.
    /// This index structure is <i>static</i>
    /// - items cannot be added or removed once the first query has been made.
    /// The advantage of this characteristic is that the index performance can be optimized based on a fixed set of items.
    ///</summary>
    public class SortedPackedRTree<TBounds, TItem> : ISpatialIndex<TBounds, TItem>
        where TItem : IBoundable<TBounds>
        where TBounds : IContainable<TBounds>, IIntersectable<TBounds>, IComparable<TBounds>
    {
        private readonly List<RTreeNode<TBounds, TItem>> _leaves;

        private int _level;
        private RTreeNode<TBounds, TItem> _root;

        ///<summary>
        /// constructor of this class
        ///</summary>
        public SortedPackedRTree(IBoundsFactory<TBounds> boundsFactory)
        {
            BoundsFactory = boundsFactory;
            _leaves = new List<RTreeNode<TBounds, TItem>>();
        }

        protected IBoundsFactory<TBounds> BoundsFactory { get; set; }

        protected Comparison<RTreeNode<TBounds, TItem>> CompareOp
        {
            get
            {
                return
                    delegate(RTreeNode<TBounds, TItem> left, RTreeNode<TBounds, TItem> right) { return left.Bounds.CompareTo(right.Bounds); };
            }
        }

        #region ISpatialIndex<TBounds,TItem> Members

        ///<summary>
        /// Search for intervals in the index which intersect the given closed interval
        ///</summary>
        ///<param name="interval">query interval</param>
        ///<returns>enumerable of {TItems}</returns>
        public IEnumerable<TItem> Query(TBounds interval)
        {
            Init();
            return _root.Query(interval);
        }

        public void Insert(TItem item)
        {
            Insert(item.Bounds, item);
        }

        public void InsertRange(IEnumerable<TItem> items)
        {
            foreach (TItem item in items)
            {
                Insert(item);
            }
        }

        public bool Remove(TItem item)
        {
            checkNotBuilt();
            return
                _leaves.RemoveAll(delegate(RTreeNode<TBounds, TItem> o) { return Enumerable.Contains(o.Items, item); }) >
                0;
        }

        public IEnumerable<TItem> Query(TBounds bounds, Predicate<TItem> predicate)
        {
            foreach (TItem item in Query(bounds))
            {
                if (predicate(item))
                    yield return item;
            }
        }

        public IEnumerable<TResult> Query<TResult>(TBounds bounds, Func<TItem, TResult> selector)
        {
            foreach (TItem item in Query(bounds))
            {
                yield return selector(item);
            }
        }

        public TBounds Bounds
        {
            get { return _root.Bounds; }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ISpatialIndexNode<TBounds, TItem> CreateNode(int level)
        {
            throw new NotImplementedException();
        }

        #endregion

        ///<summary>
        ///</summary>
        ///<param name="interval"></param>
        ///<param name="item"></param>
        ///<exception cref="InvalidOperationException"></exception>
        public void Insert(TBounds interval, TItem item)
        {
            checkNotBuilt();
            _leaves.Add(new RTreeLeafNode<TBounds, TItem>(BoundsFactory, item));
        }

        private void checkNotBuilt()
        {
            if (_root != null)
                throw new InvalidOperationException("Index cannot be added to once it has been queried");
        }

        private void Init()
        {
            if (_root != null) return;
            _root = BuildTree();
        }

        private RTreeNode<TBounds, TItem> BuildTree()
        {
            _leaves.Sort(CompareOp);

            // now group nodes into blocks of two and build tree up recursively
            List<RTreeNode<TBounds, TItem>> src = _leaves;
            List<RTreeNode<TBounds, TItem>> dest = new List<RTreeNode<TBounds, TItem>>();

            while (true)
            {
                BuildLevel(src, dest);
                if (dest.Count == 1)
                    return dest[0];

                List<RTreeNode<TBounds, TItem>> temp = null;
                temp = src;
                src = dest;
                dest = temp;
            }
        }

        private void BuildLevel(List<RTreeNode<TBounds, TItem>> src, List<RTreeNode<TBounds, TItem>> dest)
        {
            _level++;
            dest.Clear();
            for (int i = 0; i < src.Count; i += 2)
            {
                RTreeNode<TBounds, TItem> n1 = src[i];
                RTreeNode<TBounds, TItem> n2 = (i + 1 < src.Count)
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
                    RTreeNode<TBounds, TItem> node = new RTreeBranchNode<TBounds, TItem>(BoundsFactory,
                                                                                         _level, n1, src[i + 1]);
                    dest.Add(node);
                }
            }
        }
    }
}