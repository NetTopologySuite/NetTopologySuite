using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index
{
    public abstract class AbstractNode<TBounds, TItem> : ISpatialIndexNode<TBounds, TItem>
        where TItem : IBoundable<TBounds>
    {
        private readonly Int32 _level;
        protected TBounds _bounds;
        protected Boolean _boundsSet;
        private List<TItem> _items;
        private List<ISpatialIndexNode<TBounds, TItem>> _subNodes;

        /// <summary> 
        /// Constructs an <see cref="AbstractNode{TBounds, TItem}"/> at the 
        /// given level in the tree.
        /// </summary>
        /// <param name="level">
        /// 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </param>
        protected AbstractNode(Int32 level)
        {
            _level = level;
        }

        protected AbstractNode(TBounds bounds, Int32 level)
            : this(level)
        {
            _bounds = bounds;
        }

        protected List<TItem> ItemsInternal
        {
            get
            {
                if (_items == null)
                {
                    EnsureItems();
                }
                return _items;
            }
        }

        protected List<ISpatialIndexNode<TBounds, TItem>> SubNodesInternal
        {
            get
            {
                if (_subNodes == null)
                {
                    EnsureSubNodes();
                }

                return _subNodes;
            }
        }

        #region ISpatialIndexNode<TBounds,TItem> Members

        /// <summary>
        /// Returns 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </summary>
        public Int32 Level
        {
            get { return _level; }
        }

        public abstract Boolean Intersects(TBounds bounds);

        public virtual Int32 TotalItemCount
        {
            get
            {
                Int32 subSize = 0;

                foreach (ISpatialIndexNode<TBounds, TItem> node in SubNodesInternal)
                {
                    subSize += node.TotalItemCount;
                }

                return subSize + ItemCount;
            }
        }

        public Int32 TotalNodeCount
        {
            get
            {
                Int32 ndcount = 0;
                foreach (ISpatialIndexNode<TBounds, TItem> v in SubNodesInternal)
                    ndcount += v.TotalNodeCount;
                return ndcount + SubNodeCount;
            }
        }

        public virtual TBounds Bounds
        {
            get
            {
                if (!_boundsSet)
                {
                    _bounds = ComputeBounds();
                    _boundsSet = !Equals(_bounds, default(TBounds));
                }

                return _bounds;
            }
        }

        public virtual bool BoundsSet
        {
            get
            {
                return _boundsSet;
            }
        }

        public Boolean IsLeaf
        {
            get { return Level == 0; }
        }

        public virtual void Add(IBoundable<TBounds> child)
        {
            if (child is ISpatialIndexNode<TBounds, TItem>)
                addSubNode((ISpatialIndexNode<TBounds, TItem>)child);
            else if (child is TItem)
                addItem((TItem)child);
            else
                throw new ArgumentException();
        }

        public virtual void AddRange(IEnumerable<IBoundable<TBounds>> children)
        {
            foreach (IBoundable<TBounds> child in children)
            {
                Add(child);
            }
        }

        public IEnumerable<IBoundable<TBounds>> ChildBoundables
        {
            get
            {
                foreach (TItem item in Items)
                {
                    yield return item;
                }
                foreach (ISpatialIndexNode<TBounds, TItem> node in SubNodes)
                {
                    yield return node;
                }
            }
        }

        public virtual void Clear()
        {
            if (HasItems)
            {
                _items.Clear();
            }

            if (HasSubNodes)
            {
                _subNodes.Clear();
            }
        }

        public Boolean HasItems
        {
            get { return _items != null && _items.Count > 0; }
        }

        public Boolean HasSubNodes
        {
            get { return _subNodes != null && _subNodes.Count > 0; }
        }

        public Boolean IsEmpty
        {
            get
            {
                Boolean isEmpty = true;

                if (_items.Count != 0)
                {
                    isEmpty = false;
                }

                for (Int32 i = 0; i < 4; i++)
                {
                    if (_subNodes[i] != null)
                    {
                        if (!_subNodes[i].IsEmpty)
                        {
                            isEmpty = false;
                        }
                    }
                }

                return isEmpty;
            }
        }

        public Boolean IsPrunable
        {
            get { return !(HasSubNodes || HasItems); }
        }

        public virtual IEnumerable<TItem> Items
        {
            get
            {
                if (_items == null)
                {
                    yield break;
                }

                foreach (TItem item in _items)
                {
                    yield return item;
                }
            }
        }

        public virtual Int32 ItemCount
        {
            get { return _items == null ? 0 : _items.Count; }
        }

        public virtual IEnumerable<TItem> Query(TBounds query)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }
            if (ItemsInternal != null)
                foreach (TItem item in ItemsInternal)
                {
                    if (item.Intersects(query))
                    {
                        yield return item;
                    }
                }
            if (SubNodesInternal != null)
                foreach (ISpatialIndexNode<TBounds, TItem> node in SubNodesInternal)
                {
                    if (node != null)
                    {
                        foreach (TItem item in node.Query(query))
                        {
                            yield return item;
                        }
                    }
                }
        }

        public virtual IEnumerable<TItem> Query(TBounds query, Predicate<TItem> predicate)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            foreach (TItem item in filterItems(query, predicate))
            {
                yield return item;
            }

            foreach (ISpatialIndexNode<TBounds, TItem> node in SubNodesInternal)
            {
                if (node != null)
                {
                    foreach (TItem item in node.Query(query, predicate))
                    {
                        yield return item;
                    }
                }
            }
        }

        public IEnumerable<TResult> Query<TResult>(TBounds query, Func<TItem, TResult> predicate)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IBoundable<TBounds> child)
        {
            if (child is ISpatialIndexNode<TBounds, TItem>)
                return removeSubNode((ISpatialIndexNode<TBounds, TItem>)child);
            if (child is TItem)
                return removeItem((TItem)child);
            throw new ArgumentException();
        }

        public bool RemoveRange(IEnumerable<IBoundable<TBounds>> children)
        {
            foreach (IBoundable<TBounds> child in children)
            {
                Remove(child);
            }
            return true; //temp hack
        }

        public virtual IEnumerable<ISpatialIndexNode<TBounds, TItem>> SubNodes
        {
            get
            {
                if (_subNodes == null)
                {
                    yield break;
                }

                foreach (ISpatialIndexNode<TBounds, TItem> node in _subNodes)
                {
                    yield return node;
                }
            }
        }

        public virtual Int32 SubNodeCount
        {
            get { return _subNodes == null ? 0 : _subNodes.Count; }
        }

        #endregion

        /// <summary>
        /// Computes a representation of space that encloses this node,
        /// preferably not much bigger than the node's boundary yet fast to
        /// test for intersection with the bounds of other nodes and bounded items. 
        /// </summary>    
        protected abstract TBounds ComputeBounds();

        protected void EnsureItems()
        {
            if (_items == null)
            {
                _items = new List<TItem>();
            }
        }

        protected virtual void CreateSubNodes()
        {
        }

        protected void EnsureSubNodes()
        {
            if (_subNodes == null)
            {
                _subNodes = new List<ISpatialIndexNode<TBounds, TItem>>();
                CreateSubNodes();
            }
        }

        protected abstract Boolean IsSearchMatch(TBounds query);

        private IEnumerable<TItem> filterItems(TBounds query, Predicate<TItem> predicate)
        {
            foreach (TItem item in ItemsInternal)
            {
                if (item.Intersects(query) && predicate(item))
                {
                    yield return item;
                }
            }
        }


        private void addSubNode(ISpatialIndexNode<TBounds, TItem> child)
        {
            EnsureSubNodes();
            Debug.Assert(_subNodes != null);
            _subNodes.Add(child);
        }

        private void addSubNodes(IEnumerable<ISpatialIndexNode<TBounds, TItem>> children)
        {
            EnsureSubNodes();
            Debug.Assert(_subNodes != null);
            _subNodes.AddRange(children);
        }

        private void addItem(TItem item)
        {
            EnsureItems();
            Debug.Assert(_items != null);
            _items.Add(item);
        }

        private void addItems(IEnumerable<TItem> items)
        {
            EnsureItems();
            Debug.Assert(_items != null);
            _items.AddRange(items);
        }

        private Boolean removeSubNode(ISpatialIndexNode<TBounds, TItem> child)
        {
            return _subNodes.Remove(child);
        }

        protected virtual Boolean removeItem(TItem item)
        {
            return ItemsInternal.Remove(item);
        }

        private Boolean removeItem(ISpatialIndexNode<TBounds, TItem> item)
        {
            if (IsLeaf)
            {
                return _subNodes.Remove(item);
            }

            foreach (AbstractNode<TBounds, TItem> node in _subNodes)
            {
                if (node.Intersects(item.Bounds))
                {
                    if (node.removeItem(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}