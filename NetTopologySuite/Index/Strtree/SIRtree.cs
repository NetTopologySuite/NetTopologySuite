using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
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
    public class SirTree<TItem> : AbstractStrTree<Interval, TItem>
    {
        private class SirTreeNode : AbstractNode<Interval>
        {
            public SirTreeNode(Int32 nodeCapacity) : base(nodeCapacity) {}

            protected override Interval ComputeBounds()
            {
                Interval? bounds = null;
                foreach (IBoundable<Interval> childBoundable in ChildBoundables)
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
        }

        /// <summary> 
        /// Constructs an SIRtree with the default (10) node capacity.
        /// </summary>
        public SirTree() : this(10) {}

        /// <summary> 
        /// Constructs an SIRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public SirTree(Int32 nodeCapacity) : base(nodeCapacity) {}

        protected override AbstractNode<Interval> CreateNode(Int32 level)
        {
            return new SirTreeNode(level);
        }

        /// <summary> 
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        public void Insert(Double x1, Double x2, TItem item)
        {
            Insert(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)), item);
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
            return Query(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)));
        }

        protected override Func<Interval, Interval, Boolean> IntersectsOp
        {
            get
            {
                return delegate(Interval left, Interval right)
                       {
                           return left.Overlaps(right);
                       };
            }
        }

        protected override Comparison<IBoundable<Interval>> CompareOp
        {
            get
            {
                return delegate(IBoundable<Interval> left, IBoundable<Interval> right)
                {
                    return left.Bounds.Center.CompareTo(right.Bounds.Center);
                };
            }
        }
    }
}