using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// One-dimensional version of an STR-packed R-tree.
    /// </summary>
    /// <remarks>
    /// SIR stands for
    /// "Sort-Interval-Recursive". STR-packed R-trees are described in:
    /// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </remarks>
    public class SirTree<TItem> : AbstractStrTree<Interval, ItemBoundable<Interval, TItem>>
    {
        /// <summary> 
        /// Constructs an SIRtree with the default (10) node capacity.
        /// </summary>
        public SirTree() : this(10)
        {
        }

        /// <summary> 
        /// Constructs an SIRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public SirTree(Int32 nodeCapacity) : base(nodeCapacity)
        {
        }

        protected override Comparison<IBoundable<Interval>> CompareOp
        {
            get
            {
                return
                    delegate(IBoundable<Interval> left, IBoundable<Interval> right) { return left.Bounds.Center.CompareTo(right.Bounds.Center); };
            }
        }

        protected override ISpatialIndexNode<Interval, ItemBoundable<Interval, TItem>> CreateNode(Int32 level)
        {
            return new SirTreeNode(level);
        }

        /// <summary> 
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        public void Insert(Double x1, Double x2, TItem item)
        {
            Interval bounds = new Interval(Math.Min(x1, x2), Math.Max(x1, x2));
            Insert(new SirTreeItemBoundable(bounds, item));
        }

        /// <summary>
        /// Returns items whose bounds intersect the given value.
        /// </summary>
        public IEnumerable<TItem> Query(Double x)
        {
            return Query(x, x);
        }

        /// <summary> 
        /// Returns items whose bounds intersect the given bounds.
        /// </summary>
        /// <param name="x1">Possibly equal to x2.</param>
        /// <param name="x2">Possibly equal to x1.</param>
        public IEnumerable<TItem> Query(Double x1, Double x2)
        {
            IEnumerable<ItemBoundable<Interval, TItem>> boundedItems
                = Query(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)));

            foreach (ItemBoundable<Interval, TItem> boundable in boundedItems)
            {
                yield return boundable.Item;
            }
        }

        #region Nested type: SirTreeItemBoundable

        private class SirTreeItemBoundable : ItemBoundable<Interval, TItem>
        {
            public SirTreeItemBoundable(Interval bounds, TItem item)
                : base(bounds, item)
            {
            }

            public override Boolean Intersects(Interval bounds)
            {
                return Bounds.Overlaps(bounds);
            }
        }

        #endregion

        #region Nested type: SirTreeNode

        private class SirTreeNode : AbstractNode<Interval, ItemBoundable<Interval, TItem>>
        {
            public SirTreeNode(Int32 nodeCapacity) : base(nodeCapacity)
            {
            }

            protected override Interval ComputeBounds()
            {
                Interval? bounds = null;

                foreach (IBoundable<Interval> childBoundable in SubNodes)
                {
                    if (bounds == null)
                    {
                        bounds = childBoundable.Bounds;
                    }
                    else
                    {
                        bounds.Value.ExpandToInclude(childBoundable.Bounds);
                    }
                }

                Debug.Assert(bounds != null);
                return bounds.Value;
            }

            public override Boolean Intersects(Interval bounds)
            {
                return Bounds.Overlaps(bounds);
            }

            protected override Boolean IsSearchMatch(Interval query)
            {
                return query.Overlaps(Bounds);
            }
        }

        #endregion

        //protected override Func<Interval, Interval, Boolean> IntersectsOp
        //{
        //    get
        //    {
        //        return delegate(Interval left, Interval right)
        //               {
        //                   return left.Overlaps(right);
        //               };
        //    }
        //}

        //protected override IBoundable<Interval> CreateItemBoundable(Interval bounds, ItemBoundable<Interval, TItem> item)
        //{
        //    return new SirTreeItemBoundable(bounds, item);
        //}
    }
}