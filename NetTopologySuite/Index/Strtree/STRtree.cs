using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;
using GeoAPI.Diagnostics;
using GeoAPI.DataStructures;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>  
    /// A query-only R-tree created using the Sort-Tile-Recursive (STR) algorithm.
    /// For two-dimensional spatial data. 
    /// </summary>
    /// <remarks>
    /// The STR packed R-tree is simple to implement and maximizes space
    /// utilization; that is, as many leaves as possible are filled to capacity.
    /// Overlap between nodes is far less than in a basic R-tree. However, once the
    /// tree has been built (explicitly or on the first call to <see cref="Query"/>), 
    /// items may not be added or removed. 
    /// Described in: P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </remarks>
    public class StrTree<TCoordinate, TItem> : AbstractStrTree<IExtents<TCoordinate>, TItem>, ISpatialIndex<IExtents<TCoordinate>, TItem>
        where TItem : IBoundable<IExtents<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<Double, TCoordinate>,
            IConvertible
    {
        #region Nested types

        //// TODO: Make this a delegate
        //private class AnonymousXComparerImpl : IComparer
        //{
        //    private StrTree<TCoordinate, TItem> container = null;

        //    public AnonymousXComparerImpl(StrTree<TCoordinate, TItem> container)
        //    {
        //        this.container = container;
        //    }

        //    public Int32 Compare(object o1, object o2)
        //    {
        //        return container.CompareDoubles(container.getCenterX((IExtents) ((IBoundable) o1).Bounds),
        //                                        container.getCenterX((IExtents) ((IBoundable) o2).Bounds));
        //    }
        //}

        //// TODO: Make this a delegate
        //private class AnonymousYComparerImpl : IComparer
        //{
        //    private StrTree<TCoordinate, TItem> container = null;

        //    public AnonymousYComparerImpl(StrTree<TCoordinate, TItem> container)
        //    {
        //        this.container = container;
        //    }

        //    public Int32 Compare(object o1, object o2)
        //    {
        //        return container.CompareDoubles(container.getCenterY((IExtents) ((IBoundable) o1).Bounds),
        //                                        container.getCenterY((IExtents) ((IBoundable) o2).Bounds));
        //    }
        //}

        //// TODO: Make this a delegate
        //private class AnonymousIntersectsOpImpl : IIntersectsOp
        //{
        //    private StrTree<TCoordinate, TItem> container = null;

        //    public AnonymousIntersectsOpImpl(StrTree<TCoordinate, TItem> container)
        //    {
        //        this.container = container;
        //    }

        //    public Boolean Intersects(object aBounds, object bBounds)
        //    {
        //        return ((IExtents) aBounds).Intersects((IExtents) bBounds);
        //    }
        //}

        private class StrItemBoundable : ItemBoundable<IExtents<TCoordinate>, TItem>
        {
            public StrItemBoundable(IExtents<TCoordinate> bounds, TItem item)
                : base(bounds, item) { }

            public override Boolean Intersects(IExtents<TCoordinate> bounds)
            {
                if (bounds == null || Bounds == null)
                {
                    return false;
                }

                return bounds.Intersects(Bounds);
            }
        }

        private class StrNode : AbstractNode<IExtents<TCoordinate>, IBoundable<IExtents<TCoordinate>>>
        {
            private IGeometryFactory<TCoordinate> _geoFactory;

            public StrNode(IGeometryFactory<TCoordinate> geoFactory, Int32 nodeCapacity) :
                base(nodeCapacity)
            {
                _geoFactory = geoFactory;
            }

            protected override IExtents<TCoordinate> ComputeBounds()
            {
                IExtents<TCoordinate> bounds = null;

                foreach (IBoundable<IExtents<TCoordinate>> childBoundable in Children)
                {
                    if (bounds == null)
                    {
                        bounds = _geoFactory.CreateExtents(childBoundable.Bounds);
                    }
                    else
                    {
                        bounds.ExpandToInclude(childBoundable.Bounds);
                    }
                }

                return bounds;
            }

            public override Boolean Intersects(IExtents<TCoordinate> bounds)
            {
                if (bounds == null || Bounds == null)
                {
                    return false;
                }

                return bounds.Intersects(Bounds);
            }

            protected override Boolean IsSearchMatch(IExtents<TCoordinate> query)
            {
                return query.Intersects(Bounds);
            }
        }

        #endregion

        private IGeometryFactory<TCoordinate> _geoFactory;

        /// <summary> 
        /// Constructs an STRtree with the default (10) node capacity.
        /// </summary>
        public StrTree(IGeometryFactory<TCoordinate> geoFactory) : this(geoFactory, 10) { }

        /// <summary> 
        /// Constructs an STRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public StrTree(IGeometryFactory<TCoordinate> geoFactory, Int32 nodeCapacity) :
            base(nodeCapacity)
        {
            _geoFactory = geoFactory;
        }

        public override void Insert(TItem item)
        {
            if (item.Bounds.IsEmpty)
            {
                return;
            }

            base.Insert(item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        public IEnumerable<TItem> Query(IExtents<TCoordinate> bounds)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an object, not an Envelope. [Jon Aquino 10/24/2003]
            return base.Query(bounds);
        }

        ///// <summary>
        ///// Returns items whose bounds intersect the given envelope.
        ///// </summary>
        //public void Query(IExtents<TCoordinate> searchEnv, Action<TItem> visitor)
        //{
        //    //Yes this method does something. It specifies that the bounds is an
        //    //Envelope. super.query takes an Object, not an Envelope. [Jon Aquino 10/24/2003]
        //    base.Query(searchEnv, visitor);
        //}

        ///// <summary> 
        ///// Removes a single item from the tree.
        ///// </summary>
        ///// <param name="item">The item to remove.</param>
        ///// <returns><see langword="true"/> if the item was found.</returns>
        //public Boolean Remove(TItem item)
        //{
        //    return base.Remove(item);
        //}

        protected override AbstractNode<IExtents<TCoordinate>, IBoundable<IExtents<TCoordinate>>> CreateNode(Int32 level)
        {
            return new StrNode(_geoFactory, level);
        }

        /// <summary>
        /// Creates the parent level for the given child level. First, orders the items
        /// by the x-values of the midpoints, and groups them into vertical slices.
        /// For each slice, orders the items by the y-values of the midpoints, and
        /// group them into runs of size M (the node capacity). For each run, creates
        /// a new (parent) node.
        /// </summary>
        protected override IList<IBoundable<IExtents<TCoordinate>>> CreateParentBoundables(
            IList<IBoundable<IExtents<TCoordinate>>> childBoundables, Int32 newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);

            Int32 minLeafCount = (Int32)Math.Ceiling((childBoundables.Count / (Double)NodeCapacity));

            List<IBoundable<IExtents<TCoordinate>>> sortedChildBoundables
                = new List<IBoundable<IExtents<TCoordinate>>>(childBoundables);

            sortedChildBoundables.Sort(XOrdinateComparer);

            IList<IList<IBoundable<IExtents<TCoordinate>>>> verticalSlices 
                = VerticalSlices(sortedChildBoundables, (Int32)Math.Ceiling(Math.Sqrt(minLeafCount)));

            IList<IBoundable<IExtents<TCoordinate>>> tempList 
                = CreateParentBoundablesFromVerticalSlices(verticalSlices, newLevel);

            return tempList;
        }

        protected static Comparison<IBoundable<IExtents<TCoordinate>>> XOrdinateComparer
        {
            get
            {
                return delegate(IBoundable<IExtents<TCoordinate>> left, IBoundable<IExtents<TCoordinate>> right)
                       {
                           return left.Bounds.Center[Ordinates.X].CompareTo(right.Bounds.Center[Ordinates.X]);
                       };
            }
        }

        protected static Comparison<IBoundable<IExtents<TCoordinate>>> YOrdinateComparer
        {
            get
            {
                return delegate(IBoundable<IExtents<TCoordinate>> left, IBoundable<IExtents<TCoordinate>> right)
                {
                    return left.Bounds.Center[Ordinates.Y].CompareTo(right.Bounds.Center[Ordinates.Y]);
                };
            }
        }

        protected override Comparison<IBoundable<IExtents<TCoordinate>>> CompareOp
        {
            get { return YOrdinateComparer; }
        }

        protected IList<IBoundable<IExtents<TCoordinate>>> CreateParentBoundablesFromVerticalSlices(
            IEnumerable<IList<IBoundable<IExtents<TCoordinate>>>> verticalSlices,
            Int32 newLevel)
        {
            Assert.IsTrue(Slice.CountGreaterThan(verticalSlices, 0));
            List<IBoundable<IExtents<TCoordinate>>> parentBoundables 
                = new List<IBoundable<IExtents<TCoordinate>>>();

            foreach (IList<IBoundable<IExtents<TCoordinate>>> verticalSlice in verticalSlices)
            {
                parentBoundables.AddRange(
                    CreateParentBoundablesFromVerticalSlice(verticalSlice, newLevel));
            }

            return parentBoundables;
        }

        protected IList<IBoundable<IExtents<TCoordinate>>> CreateParentBoundablesFromVerticalSlice(
            IList<IBoundable<IExtents<TCoordinate>>> childBoundables, Int32 newLevel)
        {
            return base.CreateParentBoundables(childBoundables, newLevel);
        }

        //protected override IBoundable<IExtents<TCoordinate>> CreateItemBoundable(IExtents<TCoordinate> bounds, TItem item)
        //{
        //    return new StrItemBoundable(bounds, item);
        //}

        //protected override Func<IExtents<TCoordinate>, IExtents<TCoordinate>, Boolean> IntersectsOp
        //{
        //    get
        //    {
        //        return delegate(IExtents<TCoordinate> left, IExtents<TCoordinate> right)
        //               {
        //                   return left.Intersects(right);
        //               };
        //    }
        //}

        /// <param name="childBoundables">
        /// Must be sorted by the x-value of the envelope midpoints.
        /// </param>
        protected IList<IList<IBoundable<IExtents<TCoordinate>>>> VerticalSlices(
            ICollection<IBoundable<IExtents<TCoordinate>>> childBoundables, Int32 sliceCount)
        {
            Int32 sliceCapacity = (Int32)Math.Ceiling(childBoundables.Count / (Double)sliceCount);
            IList<IList<IBoundable<IExtents<TCoordinate>>>> slices = new IList<IBoundable<IExtents<TCoordinate>>>[sliceCount];

            for (Int32 j = 0; j < sliceCount; j++)
            {
                List<IBoundable<IExtents<TCoordinate>>> sliceChildren
                    = new List<IBoundable<IExtents<TCoordinate>>>();
                slices[j] = sliceChildren;
                Int32 boundablesAddedToSlice = 0;

                foreach (IBoundable<IExtents<TCoordinate>> childBoundable in childBoundables)
                {
                    if (boundablesAddedToSlice >= sliceCapacity)
                    {
                        break;
                    }

                    slices[j].Add(childBoundable);
                    boundablesAddedToSlice++;
                }
            }

            return slices;
        }

        //private Double avg(Double a, Double b)
        //{
        //    return (a + b) / 2D;
        //}

        //private Double getCenterX(IExtents<TCoordinate> e)
        //{
        //    return avg(e.GetMin(Ordinates.X, e.MaxX);
        //}

        //private Double getCenterY(IExtents<TCoordinate> e)
        //{
        //    return avg(e.MinY, e.MaxY);
        //}
    }
}