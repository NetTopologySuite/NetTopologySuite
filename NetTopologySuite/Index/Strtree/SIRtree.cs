
using System;
using IList = System.Collections.Generic.IList<object>;
using System.Collections.Generic;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// One-dimensional version of an STR-packed R-tree. SIR stands for
    /// "Sort-Interval-Recursive". STR-packed R-trees are described in:
    /// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </summary>
    public class SIRtree : AbstractSTRtree 
    {
        private class AnnonymousComparerImpl : IComparer<object>
        {    
            public int Compare(object o1, object o2)
            {
                var c1 = ((Interval) ((IBoundable) o1).Bounds).Centre;
                var c2 = ((Interval) ((IBoundable) o2).Bounds).Centre;
                return c1.CompareTo(c2);

                /*
                return CompareDoubles(((Interval)((IBoundable)o1).Bounds).Centre, 
                                      ((Interval)((IBoundable)o2).Bounds).Centre);
                 */
            }
        }        

        private class AnonymousIntersectsOpImpl : IIntersectsOp
        {
            public bool Intersects(object aBounds, object bBounds) 
            {
                return ((Interval)aBounds).Intersects((Interval)bBounds);
            }
        }

        private class AnonymousAbstractNodeImpl : AbstractNode
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
            protected override object ComputeBounds()
            {
                Interval bounds = null;
                //var bounds = Interval.Create();
                foreach (object i in ChildBoundables)
                {
                    var childBoundable = (IBoundable)i;
                    if (bounds == null)
                         bounds = new Interval((Interval)childBoundable.Bounds);
                    else 
                        bounds.ExpandToInclude((Interval)childBoundable.Bounds);
                    //bounds = bounds.ExpandedByInterval((Interval) childBoundable.Bounds);
                }
                return bounds;
            }
        }       

        private static readonly IComparer<object> Comparator = new AnnonymousComparerImpl(); 

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
        protected override AbstractNode CreateNode(int level) 
        {                
            return new AnonymousAbstractNodeImpl(level);
        }

        /// <summary> 
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="item"></param>
        public void Insert(double x1, double x2, object item) 
        {
            Insert(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)), item);
            //Insert(Interval.Create(x1, x2), item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given value.
        /// </summary>
        /// <param name="x"></param>
        public IList Query(double x) 
        {
            return Query(x, x);
        }

        /// <summary> 
        /// Returns items whose bounds intersect the given bounds.
        /// </summary>
        /// <param name="x1">Possibly equal to x2.</param>
        /// <param name="x2">Possibly equal to x1.</param>
        public IList Query(double x1, double x2) 
        {
            return Query(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)));
            //return Query(Interval.Create(x1, x2));
        }

        /// <summary>
        /// 
        /// </summary>
        protected override IIntersectsOp IntersectsOp
        {
            get
            {
                return IntersectsOperation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IComparer<object> GetComparer() 
        {
            return Comparator;
        }
    }
}
