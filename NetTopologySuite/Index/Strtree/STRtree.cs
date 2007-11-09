using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>  
    /// A query-only R-tree created using the Sort-Tile-Recursive (STR) algorithm.
    /// For two-dimensional spatial data. 
    /// The STR packed R-tree is simple to implement and maximizes space
    /// utilization; that is, as many leaves as possible are filled to capacity.
    /// Overlap between nodes is far less than in a basic R-tree. However, once the
    /// tree has been built (explicitly or on the first call to #query), items may
    /// not be added or removed. 
    /// Described in: P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </summary>
    public class StrTree<TCoordinate, TItem> : AbstractStrTree, ISpatialIndex<TCoordinate, TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>,
            IConvertible
    {
        #region Nested types

        // TODO: Make this a delegate
        private class AnonymousXComparerImpl : IComparer
        {
            private StrTree<TCoordinate, TItem> container = null;

            public AnonymousXComparerImpl(StrTree<TCoordinate, TItem> container)
            {
                this.container = container;
            }

            public Int32 Compare(object o1, object o2)
            {
                return container.CompareDoubles(container.getCenterX((IExtents) ((IBoundable) o1).Bounds),
                                                container.getCenterX((IExtents) ((IBoundable) o2).Bounds));
            }
        }

        // TODO: Make this a delegate
        private class AnonymousYComparerImpl : IComparer
        {
            private StrTree<TCoordinate, TItem> container = null;

            public AnonymousYComparerImpl(StrTree<TCoordinate, TItem> container)
            {
                this.container = container;
            }

            public Int32 Compare(object o1, object o2)
            {
                return container.CompareDoubles(container.getCenterY((IExtents) ((IBoundable) o1).Bounds),
                                                container.getCenterY((IExtents) ((IBoundable) o2).Bounds));
            }
        }

        private class AnonymousAbstractNodeImpl : AbstractNode
        {
            public AnonymousAbstractNodeImpl(Int32 nodeCapacity) :
                base(nodeCapacity) {}

            protected override object ComputeBounds()
            {
                IExtents bounds = null;
               
                for (IEnumerator i = ChildBoundables.GetEnumerator(); i.MoveNext();)
                {
                    IBoundable childBoundable = (IBoundable) i.Current;
                    
                    if (bounds == null)
                    {
                        bounds = new Extents((IExtents) childBoundable.Bounds);
                    }
                    else
                    {
                        bounds.ExpandToInclude((IExtents) childBoundable.Bounds);
                    }
                }

                return bounds;
            }
        }

        // TODO: Make this a delegate
        private class AnonymousIntersectsOpImpl : IIntersectsOp
        {
            private StrTree<TCoordinate, TItem> container = null;

            public AnonymousIntersectsOpImpl(StrTree<TCoordinate, TItem> container)
            {
                this.container = container;
            }

            public Boolean Intersects(object aBounds, object bBounds)
            {
                return ((IExtents) aBounds).Intersects((IExtents) bBounds);
            }
        }

        #endregion

        /// <summary> 
        /// Constructs an STRtree with the default (10) node capacity.
        /// </summary>
        public StrTree() : this(10) {}

        /// <summary> 
        /// Constructs an STRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public StrTree(Int32 nodeCapacity) :
            base(nodeCapacity) {}

        /// <summary>
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        public void Insert(IExtents<TCoordinate> itemEnv, TItem item)
        {
            if (itemEnv.IsEmpty)
            {
                return;
            }

            base.Insert(itemEnv, item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        public IEnumerable<TItem> Query(IExtents<TCoordinate> searchEnv)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an object, not an Envelope. [Jon Aquino 10/24/2003]
            return base.Query(searchEnv);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        public void Query(IExtents<TCoordinate> searchEnv, Action<TItem> visitor)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an Object, not an Envelope. [Jon Aquino 10/24/2003]
            base.Query(searchEnv, visitor);
        }

        /// <summary> 
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemEnv">The Envelope of the item to remove.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> if the item was found.</returns>
        public Boolean Remove(IExtents<TCoordinate> itemEnv, TItem item)
        {
            return base.Remove(itemEnv, item);
        }

        protected override AbstractNode CreateNode(Int32 level)
        {
            return new AnonymousAbstractNodeImpl(level);
        }

        /// <summary>
        /// Creates the parent level for the given child level. First, orders the items
        /// by the x-values of the midpoints, and groups them into vertical slices.
        /// For each slice, orders the items by the y-values of the midpoints, and
        /// group them into runs of size M (the node capacity). For each run, creates
        /// a new (parent) node.
        /// </summary>
        protected override IList CreateParentBoundables(IList childBoundables, Int32 newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);
            Int32 minLeafCount = (Int32) Math.Ceiling((childBoundables.Count/(Double) NodeCapacity));
            ArrayList sortedChildBoundables = new ArrayList(childBoundables);
            sortedChildBoundables.Sort(new AnonymousXComparerImpl(this));
            IList[] verticalSlices = VerticalSlices(sortedChildBoundables,
                                                    (Int32) Math.Ceiling(Math.Sqrt(minLeafCount)));
            IList tempList = CreateParentBoundablesFromVerticalSlices(verticalSlices, newLevel);
            return tempList;
        }

        protected IList CreateParentBoundablesFromVerticalSlices(IList[] verticalSlices, Int32 newLevel)
        {
            Assert.IsTrue(verticalSlices.Length > 0);
            IList parentBoundables = new ArrayList();

            for (Int32 i = 0; i < verticalSlices.Length; i++)
            {
                IList tempList = CreateParentBoundablesFromVerticalSlice(verticalSlices[i], newLevel);
                
                foreach (object o in tempList)
                {
                    parentBoundables.Add(o);
                }
            }

            return parentBoundables;
        }

        protected IList CreateParentBoundablesFromVerticalSlice(IList childBoundables, Int32 newLevel)
        {
            return base.CreateParentBoundables(childBoundables, newLevel);
        }

        protected override IComparer GetComparer()
        {
            return new AnonymousYComparerImpl(this);
        }

        protected override IIntersectsOp IntersectsOp
        {
            get { return new AnonymousIntersectsOpImpl(this); }
        }

        /// <param name="childBoundables">
        /// Must be sorted by the x-value of the envelope midpoints.
        /// </param>
        protected IList[] VerticalSlices(IList childBoundables, Int32 sliceCount)
        {
            Int32 sliceCapacity = (Int32) Math.Ceiling(childBoundables.Count/(Double) sliceCount);
            IList[] slices = new IList[sliceCount];
            IEnumerator i = childBoundables.GetEnumerator();

            for (Int32 j = 0; j < sliceCount; j++)
            {
                slices[j] = new ArrayList();
                Int32 boundablesAddedToSlice = 0;

                /* 
                 *          Diego Guidi says:
                 *          the line below introduce an error: 
                 *          the first element at the iteration (not the first) is lost! 
                 *          This is simply a different implementation of Iteration in .NET against Java
                 */
                // while (i.MoveNext() && boundablesAddedToSlice < sliceCapacity)
                while (boundablesAddedToSlice < sliceCapacity && i.MoveNext())
                {
                    IBoundable childBoundable = (IBoundable) i.Current;
                    slices[j].Add(childBoundable);
                    boundablesAddedToSlice++;
                }
            }

            return slices;
        }

        private Double avg(Double a, Double b)
        {
            return (a + b)/2D;
        }

        private Double getCenterX(IExtents<TCoordinate> e)
        {
            return avg(e.MinX, e.MaxX);
        }

        private Double getCenterY(IExtents<TCoordinate> e)
        {
            return avg(e.MinY, e.MaxY);
        }
    }
}