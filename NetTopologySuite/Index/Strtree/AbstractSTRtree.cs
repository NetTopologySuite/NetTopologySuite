using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Base class for STR trees and SIR trees. 
    /// <remarks>
    /// <para>
    /// STR-packed R-trees are described in:
    /// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </para>
    /// <para>
    /// This implementation is based on boundables rather than just AbstractNodes,
    /// because the STR algorithm operates on both nodes and
    /// data, both of which are treated here as boundables.
    /// </para>
    /// </remarks>
    /// </summary>
    public abstract class AbstractStrTree<TBounds, TItem> : ISpatialIndex<TBounds, TItem>
        where TBounds : IContainable<TBounds>, IIntersectable<TBounds>
        where TItem : IBoundable<TBounds>
    {
        ///// <returns>
        ///// A test for intersection between two bounds, necessary because subclasses
        ///// of AbstractStrTree have different implementations of bounds.
        ///// </returns>
        //protected interface IIntersectsOp
        //{
        //    /// <summary>
        //    /// For STRtrees, the bounds will be Envelopes; 
        //    /// for SIRtrees, Intervals;
        //    /// for other subclasses of AbstractStrTree, some other class.
        //    /// </summary>
        //    /// <param name="aBounds">The bounds of one spatial object.</param>
        //    /// <param name="bBounds">The bounds of another spatial object.</param>                        
        //    /// <returns>Whether the two bounds intersect.</returns>
        //    Boolean Intersects(object aBounds, object bBounds);
        //}

        private ISpatialIndexNode<TBounds, TItem> _root;
        private Boolean _built;
        private Boolean _isDisposed;
        private readonly List<IBoundable<TBounds>> _bulkLoadStorage = new List<IBoundable<TBounds>>();
        private readonly Int32 _nodeCapacity;

        /// <summary> 
        /// Constructs an AbstractStrTree with the specified maximum number of child
        /// nodes that a node may have.
        /// </summary>
        protected AbstractStrTree(Int32 nodeCapacity)
        {
            Assert.IsTrue(nodeCapacity > 1, "Node capacity must be greater than 1");
            _nodeCapacity = nodeCapacity;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        protected IList<IBoundable<TBounds>> BulkLoadStorage
        {
            get
            {
                return _bulkLoadStorage;
            }
        }

        #region ISpatialIndex<TBounds,TItem> Members

        public virtual void Insert(TItem item)
        {
            checkState();
            Insert(item.Bounds, item);
            //_bulkLoadStorage.Add(item);
            //_root.AddItem(item);
        }

        public void InsertRange(IEnumerable<TItem> items)
        {
            checkState();
            //Insert(item.Bounds, item);
            //_bulkLoadStorage.Add(item);
            _root.AddRange(Caster.Upcast<IBoundable<TBounds>, TItem>(items));
        }

        private void Insert(TBounds bounds, TItem item)
        {
            _bulkLoadStorage.Add(item);
        }


        public void BulkLoad(IEnumerable<TItem> items)
        {
            Assert.IsTrue(!IsBuilt, "Cannot insert items into an STR packed R-tree after it has been built.");
            foreach (TItem item in items)
                Insert(item);
        }

        /// <remarks>
        /// Also builds the tree, if necessary.
        /// </remarks>
        public virtual Boolean Remove(TItem item)
        {
            checkState();
            ensureBuilt();

            //if (_bulkLoadStorage.Count == 0)
            if (Count == 0)
            {
                return false;
            }

            TBounds searchBounds = item.Bounds;

            return _root.Intersects(searchBounds) && remove(searchBounds, _root, item);
        }

        public IEnumerable<TResult> Query<TResult>(TBounds bounds, Func<TItem, TResult> selector)
        {
            throw new NotImplementedException();
        }

        /// <remarks>
        /// Also builds the tree, if necessary.
        /// </remarks>
        public IEnumerable<TItem> Query(TBounds searchBounds)
        {
            return Query(searchBounds, null);
        }

        /// <remarks>
        /// Also builds the tree, if necessary.
        /// </remarks>
        public IEnumerable<TItem> Query(TBounds searchBounds, Predicate<TItem> filter)
        {
            ensureBuilt();

            if (Count == 0)
            {
                Assert.IsTrue(Equals(_root.Bounds, default(TBounds)));
                yield break;
            }

            if (_root.Intersects(searchBounds))
            {
                foreach (TItem item in query(searchBounds, _root, filter))
                {
                    yield return item;
                }
            }
        }
        #endregion

        public Boolean IsDisposed
        {
            get { return _isDisposed; }
        }

        protected void Dispose(Boolean disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                _root.Clear();
                _root = null;
            }
        }

        /// <summary> 
        /// Creates parent nodes, grandparent nodes, and so forth up to the root
        /// node, for the data that has been inserted into the tree. Can only be
        /// called once, and thus should be called only after all of the data has been
        /// inserted into the tree.
        /// </summary>
        public void Build()
        {
            Assert.IsTrue(!IsBuilt);
            _root = (_bulkLoadStorage.Count == 0)
                       ? CreateNode(0)
                       : createHigherLevels(_bulkLoadStorage, -1);
            IsBuilt = true;
        }

        public TBounds Bounds
        {
            get
            {
                if (!IsBuilt)
                {
                    throw new InvalidOperationException("Index not built.");
                }

                return _root.Bounds;
            }
        }

        /// <summary> 
        /// Returns the maximum number of child nodes that a node may have.
        /// </summary>
        public Int32 NodeCapacity
        {
            get { return _nodeCapacity; }
        }

        public Int32 Count
        {
            get
            {
                ensureBuilt();

                return _root.TotalItemCount;
            }
        }

        public Int32 Depth
        {
            get
            {
                ensureBuilt();

                return !_root.HasSubNodes ? 0 : GetDepth(_root);
            }
        }

        //public void Insert(TBounds bounds, TItem item)
        //{
        //}

        //protected AbstractNode LastNode(IList nodes)
        //{
        //    return (AbstractNode) nodes[nodes.Count - 1];
        //}

        //protected static Int32 CompareDoubles(Double a, Double b)
        //{
        //    return a > b ? 1 : a < b ? -1 : 0;
        //}

        //protected abstract IBoundable<TBounds> CreateItemBoundable(TBounds bounds, TItem item);

        ///// <returns>
        ///// A test for intersection between two bounds, necessary because subclasses
        ///// of AbstractStrTree have different implementations of bounds.
        ///// </returns>
        //protected abstract Func<TBounds, TBounds, Boolean> IntersectsOp { get; }

        protected abstract Comparison<IBoundable<TBounds>> CompareOp { get; }

        protected IEnumerable<IBoundable<TBounds>> BoundablesAtLevel(Int32 level)
        {
            return boundablesAtLevel(level, _root);
        }

        protected static Int32 GetCount(AbstractNode<TBounds, IBoundable<TBounds>> node)
        {
            Int32 size = 0;

            foreach (AbstractNode<TBounds, IBoundable<TBounds>> boundable in node.SubNodes)
            {
                size += GetCount(boundable);
            }

            size += node.ItemCount;


            return size;
        }

        protected static Int32 GetDepth(ISpatialIndexNode<TBounds, TItem> node)
        {
            Int32 maxChildDepth = 0;

            foreach (ISpatialIndexNode<TBounds, TItem> childBoundable in node.SubNodes)
            {
                if (childBoundable == null)
                {
                    continue;
                }

                Int32 childDepth = GetDepth(childBoundable);

                if (childDepth > maxChildDepth)
                {
                    maxChildDepth = childDepth;
                }
            }

            return maxChildDepth + 1;
        }

        protected abstract ISpatialIndexNode<TBounds, TItem> CreateNode(Int32 level);

        /// <summary>
        /// Sorts the childBoundables then divides them into groups of size M, where
        /// M is the node capacity.
        /// </summary>
        protected virtual IList<IBoundable<TBounds>> CreateParentBoundables(IList<IBoundable<TBounds>> childBoundables, Int32 newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);

            List<IBoundable<TBounds>> parentBoundables = new List<IBoundable<TBounds>>();
            parentBoundables.Add(CreateNode(newLevel));

            List<IBoundable<TBounds>> sortedChildBoundables
                = new List<IBoundable<TBounds>>(childBoundables);
            sortedChildBoundables.Sort(CompareOp);

            foreach (IBoundable<TBounds> childBoundable in sortedChildBoundables)
            {
                ISpatialIndexNode<TBounds, TItem> lastNode
                    = Slice.GetLast(parentBoundables) as ISpatialIndexNode<TBounds, TItem>;

                if (lastNode.SubNodeCount == NodeCapacity)
                {
                    lastNode = CreateNode(newLevel);
                    parentBoundables.Add(lastNode);
                }

                lastNode.Add(childBoundable);
            }

            return parentBoundables;
        }

        protected ISpatialIndexNode<TBounds, TItem> Root
        {
            get { return _root; }
            set { _root = value; }
        }

        protected void NullifyRoot()
        {
            _root = null;
        }

        public bool IsBuilt
        {
            get { return _built; }
            protected set { _built = value; }
        }

        //private void query(object searchBounds, AbstractNode node, IItemVisitor visitor)
        //{
        //    foreach (object obj in node.ChildBoundables)
        //    {
        //        IBoundable childBoundable = (IBoundable)obj;

        //        if (!IntersectsOp.Intersects(childBoundable.Bounds, searchBounds))
        //        {
        //            continue;
        //        }

        //        if (childBoundable is AbstractNode)
        //        {
        //            Query(searchBounds, (AbstractNode)childBoundable, visitor);
        //        }
        //        else if (childBoundable is ItemBoundable)
        //        {
        //            visitor.VisitItem(((ItemBoundable)childBoundable).Item);
        //        }
        //        else
        //        {
        //            Assert.ShouldNeverReachHere();
        //        }
        //    }
        //}

        //private static IEnumerable<TItem> query(TBounds searchBounds, AbstractNode<TBounds, IBoundable<TBounds>> node, Predicate<TItem> filter)
        //{
        //   foreach (IBoundable<TBounds> childBoundable in node.SubNodes)
        //    {
        //        if (!childBoundable.Intersects(searchBounds))
        //        {
        //            continue;
        //        }

        //        if (childBoundable is AbstractNode<TBounds, IBoundable<TBounds>>)
        //        {
        //            query(searchBounds, childBoundable as AbstractNode<TBounds, IBoundable<TBounds>>, filter);
        //        }
        //        else if (childBoundable is ItemBoundable<TBounds, TItem>)
        //        {
        //            ItemBoundable<TBounds, TItem> itemBoundable = (ItemBoundable<TBounds, TItem>)childBoundable;

        //            if (filter(itemBoundable.Item))
        //            {
        //               yield return itemBoundable.Item;
        //            }
        //        }
        //        else
        //        {
        //            Assert.ShouldNeverReachHere();
        //        }
        //    }
        //}

        private static IEnumerable<TItem> query(TBounds searchBounds, ISpatialIndexNode<TBounds, TItem> node, Predicate<TItem> filter)
        {
            foreach (TItem item in node.Items)
            {
                if (searchBounds.Intersects(item.Bounds))
                {
                    if (filter == null)
                        yield return item;
                    else
                    {
                        if (filter(item))
                            yield return item;
                    }
                }
            }

            foreach (ISpatialIndexNode<TBounds, TItem> childBoundable in node.SubNodes)
            {
                if (!childBoundable.Intersects(searchBounds))
                {
                    continue;
                }
                foreach (TItem t in query(searchBounds, childBoundable, filter))
                    yield return t;

            }




        }

        private static Boolean removeItem(ISpatialIndexNode<TBounds, TItem> node, TItem item)
        {
            IBoundable<TBounds> itemToRemove = null;

            foreach (IBoundable<TBounds> childBoundable in node.SubNodes)
            {
                if (childBoundable is ItemBoundable<TBounds, TItem>)
                {
                    if (Equals(((ItemBoundable<TBounds, TItem>)childBoundable).Item, item))
                    {
                        itemToRemove = childBoundable;
                    }
                }
            }

            if (itemToRemove != null)
            {
                node.Remove(itemToRemove);
                return true;
            }

            return false;
        }

        private static Boolean remove(TBounds searchBounds, ISpatialIndexNode<TBounds, TItem> node, TItem item)
        {
            // first try removing item from this node
            Boolean found = removeItem(node, item);

            if (found)
            {
                return true;
            }

            ISpatialIndexNode<TBounds, TItem> childToPrune = null;

            // next try removing item from lower nodes
            foreach (ISpatialIndexNode<TBounds, TItem> childBoundable in node.SubNodes)
            {
                if (childBoundable == null || !childBoundable.Intersects(searchBounds))
                {
                    continue;
                }

                found = remove(searchBounds, childBoundable, item);

                // if found, record child for pruning and exit
                if (found)
                {
                    childToPrune = childBoundable;
                    break;
                }
            }

            // prune child if possible
            if (childToPrune != null)
            {
                if (childToPrune.SubNodeCount == 0)
                {
                    node.Remove(childToPrune);
                }
            }

            return found;
        }

        /// <param name="level">-1 to get items.</param>
        private static IEnumerable<IBoundable<TBounds>> boundablesAtLevel(Int32 level, ISpatialIndexNode<TBounds, TItem> top)
        {
            Assert.IsTrue(level > -2);

            if (top.Level == level)
            {
                yield return top;
                yield break;
            }

            foreach (IBoundable<TBounds> boundable in top.SubNodes)
            {
                if (boundable is AbstractNode<TBounds, IBoundable<TBounds>>)
                {
                    IEnumerable<IBoundable<TBounds>> nextLevelBoundables =
                        boundablesAtLevel(level, boundable as ISpatialIndexNode<TBounds, TItem>);

                    foreach (IBoundable<TBounds> nextLevelBoundable in nextLevelBoundables)
                    {
                        yield return nextLevelBoundable;
                    }
                }
                else
                {
                    Assert.IsTrue(boundable is ItemBoundable<TBounds, TItem>);

                    if (level == -1)
                    {
                        yield return boundable;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the levels higher than the given level.
        /// </summary>
        /// <param name="boundablesOfALevel">The level to build on.</param>
        /// <param name="level">The level of the boundables, or -1 if the boundables are item
        /// boundables (that is, below level 0).</param>
        /// <returns>The root, which may be a ParentNode or a LeafNode.</returns>
        private ISpatialIndexNode<TBounds, TItem> createHigherLevels(IList<IBoundable<TBounds>> boundablesOfALevel, Int32 level)
        {
            Assert.IsTrue(boundablesOfALevel.Count != 0);
            IList<IBoundable<TBounds>> parentBoundables = CreateParentBoundables(boundablesOfALevel, level + 1);

            if (parentBoundables.Count == 1)
            {
                return Slice.GetFirst(parentBoundables) as ISpatialIndexNode<TBounds, TItem>;
            }

            return createHigherLevels(parentBoundables, level + 1);
        }

        private void ensureBuilt()
        {
            if (!IsBuilt)
            {
                Build();
            }
        }

        private void checkState()
        {
            if (IsBuilt)
            {
                throw new InvalidOperationException("Cannot insert items into an STR packed " +
                                                    "R-tree after it has been built.");
            }
        }

        ISpatialIndexNode<TBounds, TItem> ISpatialIndexNodeFactory<TBounds,TItem>.CreateNode(Int32 level)
        {
            return (ISpatialIndexNode<TBounds, TItem>)CreateNode(level);
        }
    }
}