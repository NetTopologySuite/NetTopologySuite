using System;
using System.Collections;
using GeoAPI.DataStructures;

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
    public class SirTree : AbstractStrTree
    {
        // DESIGN_NOTE: Implement as delegate
        private class AnnonymousComparerImpl : IComparer
        {
            public Int32 Compare(object o1, object o2)
            {
                return new SirTree().CompareDoubles(((Interval) ((IBoundable) o1).Bounds).Center,
                                                    ((Interval) ((IBoundable) o2).Bounds).Center);
            }
        }

        private class AnonymousIntersectsOpImpl : IIntersectsOp
        {
            public Boolean Intersects(object aBounds, object bBounds)
            {
                return ((Interval) aBounds).Overlaps((Interval) bBounds);
            }
        }

        private class AnonymousAbstractNodeImpl : AbstractNode
        {
            public AnonymousAbstractNodeImpl(Int32 nodeCapacity) : base(nodeCapacity) {}

            protected override object ComputeBounds()
            {
                Interval? bounds = null;
                for (IEnumerator i = ChildBoundables.GetEnumerator(); i.MoveNext();)
                {
                    IBoundable childBoundable = (IBoundable) i.Current;
                    
                    if (bounds == null)
                    {
                        bounds = new Interval((Interval) childBoundable.Bounds);
                    }
                    else
                    {
                        bounds.Value.ExpandToInclude((Interval) childBoundable.Bounds);
                    }
                }

                return bounds;
            }
        }

        private IComparer comparator = new AnnonymousComparerImpl();
        private IIntersectsOp intersectsOp = new AnonymousIntersectsOpImpl();

        /// <summary> 
        /// Constructs an SIRtree with the default (10) node capacity.
        /// </summary>
        public SirTree() : this(10) {}

        /// <summary> 
        /// Constructs an SIRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public SirTree(Int32 nodeCapacity) : base(nodeCapacity) {}

        protected override AbstractNode CreateNode(Int32 level)
        {
            return new AnonymousAbstractNodeImpl(level);
        }

        /// <summary> 
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        public void Insert(Double x1, Double x2, object item)
        {
            Insert(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)), item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given value.
        /// </summary>
        public IList Query(Double x)
        {
            return Query(x, x);
        }

        /// <summary> 
        /// Returns items whose bounds intersect the given bounds.
        /// </summary>
        /// <param name="x1">Possibly equal to x2.</param>
        /// <param name="x2">Possibly equal to x1.</param>
        public IList Query(Double x1, Double x2)
        {
            return Query(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)));
        }

        protected override IIntersectsOp IntersectsOp
        {
            get { return intersectsOp; }
        }

        protected override IComparer GetComparer()
        {
            return comparator;
        }
    }
}