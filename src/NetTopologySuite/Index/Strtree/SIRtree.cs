
using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// One-dimensional version of an STR-packed R-tree. SIR stands for
    /// "Sort-Interval-Recursive". STR-packed R-trees are described in:
    /// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </summary>
    public class SIRtree<TItem> : AbstractSTRtree<Interval, TItem>
    {
        private class AnnonymousComparerImpl : IComparer<IBoundable<Interval, TItem>>
        {
            public int Compare(IBoundable<Interval, TItem> o1, IBoundable<Interval, TItem> o2)
            {
                double c1 = o1.Bounds.Centre;
                double c2 = o2.Bounds.Centre;
                return c1.CompareTo(c2);

                /*
                return CompareDoubles(((Interval)((IBoundable)o1).Bounds).Centre,
                                      ((Interval)((IBoundable)o2).Bounds).Centre);
                 */
            }
        }

        private class AnonymousIntersectsOpImpl : IIntersectsOp
        {
            public bool Intersects(Interval aBounds, Interval bBounds)
            {
                return aBounds.Intersects(bBounds);
            }
        }

        private class AnonymousAbstractNodeImpl : AbstractNode<Interval, TItem>
        {
            /// <summary>
            ///
            /// </summary>
            /// <param name="nodeCapacity"></param>
            public AnonymousAbstractNodeImpl(int nodeCapacity) : base(nodeCapacity) { }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
            protected override Interval ComputeBounds()
            {
                Interval bounds = null;
                //var bounds = Interval.Create();
                foreach (var childBoundable in ChildBoundables)
                {
                    if (bounds == null)
                         bounds = new Interval(childBoundable.Bounds);
                    else
                        bounds.ExpandToInclude(childBoundable.Bounds);
                    //bounds = bounds.ExpandedByInterval((Interval) childBoundable.Bounds);
                }
                return bounds;
            }
        }

        private static readonly IComparer<IBoundable<Interval, TItem>> Comparator = new AnnonymousComparerImpl();

        private static readonly IIntersectsOp IntersectsOperation = new AnonymousIntersectsOpImpl();

        /// <summary>
        /// Constructs an SIRtree with the default (10) node capacity.
        /// </summary>
        public SIRtree() : this(10) { }

        /// <summary>
        /// Constructs an SIRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public SIRtree(int nodeCapacity) : base(nodeCapacity) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected override AbstractNode<Interval, TItem> CreateNode(int level)
        {
            return new AnonymousAbstractNodeImpl(level);
        }

        /// <summary>
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="item"></param>
        public void Insert(double x1, double x2, TItem item)
        {
            Insert(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)), item);
            //Insert(Interval.Create(x1, x2), item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given value.
        /// </summary>
        /// <param name="x"></param>
        public IList<TItem> Query(double x)
        {
            return Query(x, x);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given bounds.
        /// </summary>
        /// <param name="x1">Possibly equal to x2.</param>
        /// <param name="x2">Possibly equal to x1.</param>
        public IList<TItem> Query(double x1, double x2)
        {
            return Query(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)));
            //return Query(Interval.Create(x1, x2));
        }

        /// <summary>
        ///
        /// </summary>
        protected override IIntersectsOp IntersectsOp => IntersectsOperation;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override IComparer<IBoundable<Interval, TItem>> GetComparer()
        {
            return Comparator;
        }
    }
}
