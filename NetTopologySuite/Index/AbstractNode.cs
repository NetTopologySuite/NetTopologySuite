using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index
{
    public abstract class AbstractNode<TBounds, TItem> : ISpatialIndexNode<TBounds, TItem>
        where TItem : IBoundable<TBounds>
    {
        private List<TItem> _items;
        private List<ISpatialIndexNode<TBounds, TItem>> _subNodes;
        private TBounds _bounds;
        private Boolean _boundsSet;
        private readonly Int32 _level;

        /// <summary> 
        /// Constructs an AbstractNode at the given level in the tree
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

        public abstract Boolean Intersects(TBounds bounds);

        /// <summary>
        /// Returns 0 if this node is a leaf, 1 if a parent of a leaf, and so on; the
        /// root node will have the highest level.
        /// </summary>
        public Int32 Level
        {
            get { return _level; }
        }

        public Int32 TotalItemCount
        {
            get
            {
                Int32 subSize = 0;

                foreach (ISpatialIndexNode<TBounds, TItem> node in ChildrenInternal)
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
                throw new NotImplementedException();
            }
        }

        protected abstract Boolean IsSearchMatch(TBounds query);

        #region ISpatialIndexNode<TBounds> Members

        public virtual TBounds Bounds
        {
            get
            {
                if (!_boundsSet)
                {
                    _bounds = ComputeBounds();
                    _boundsSet = true;
                }

                return _bounds;
            }
        }

        public Boolean IsLeaf
        {
            get { return Level == 0; }
        }

        public Boolean Remove(ISpatialIndexNode<TBounds, TItem> item)
        {
            if (IsLeaf)
            {
                return _subNodes.Remove(item);
            }
            else
            {
                foreach (AbstractNode<TBounds, TItem> node in _subNodes)
                {
                    if (node.Intersects(item.Bounds))
                    {
                        if (node.Remove(item))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual void AddChild(ISpatialIndexNode<TBounds, TItem> child)
        {
            EnsureSubNodes();
            Debug.Assert(_subNodes != null);
            _subNodes.Add(child);
        }

        public virtual void AddChildren(IEnumerable<ISpatialIndexNode<TBounds, TItem>> children)
        {
            EnsureSubNodes();
            Debug.Assert(_subNodes != null);
            _subNodes.AddRange(children);
        }

        public virtual void AddItem(TItem item)
        {
            EnsureItems();
            Debug.Assert(_items != null);
            _items.Add(item);
        }

        public virtual void AddItems(IEnumerable<TItem> items)
        {
            EnsureItems();
            Debug.Assert(_items != null);
            _items.AddRange(items);
        }

        public virtual Int32 ChildCount
        {
            get { return _subNodes == null ? 0 : _subNodes.Count; }
        }

        public virtual void Clear()
        {
            if (HasItems)
            {
                _items.Clear();   
            }

            if (HasChildren)
            {
                _subNodes.Clear();
            }
        }

        public virtual Int32 ItemCount
        {
            get { return _items == null ? 0 : _items.Count; }
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

        public virtual IEnumerable<ISpatialIndexNode<TBounds, TItem>> Children
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

        public virtual Boolean RemoveItem(TItem item)
        {
            return _items.Remove(item);
        }

        public virtual Boolean RemoveChild(ISpatialIndexNode<TBounds, TItem> child)
        {
            return _subNodes.Remove(child);
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

        public Boolean HasItems
        {
            get { return _items != null && _items.Count > 0; }
        }

        public Boolean HasChildren
        {
            get { return _subNodes != null && _subNodes.Count > 0; }
        }

        public Boolean IsPrunable
        {
            get { return !(HasChildren || HasItems); }
        }

        public virtual IEnumerable<TItem> Query(TBounds query)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }

            foreach (TItem item in ItemsInternal)
            {
                if (item.Intersects(query))
                {
                    yield return item;
                }
            }

            foreach (ISpatialIndexNode<TBounds, TItem> node in ChildrenInternal)
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

            foreach (ISpatialIndexNode<TBounds, TItem> node in ChildrenInternal)
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

        #endregion

        /// <summary>
        /// Computes a representation of space that encloses this node,
        /// preferably not much bigger than the node's boundary yet fast to
        /// test for intersection with the bounds of other nodes and bounded items. 
        /// </summary>    
        protected abstract TBounds ComputeBounds();

        protected List<TItem> ItemsInternal
        {
            get { return _items; }
        }

        protected List<ISpatialIndexNode<TBounds, TItem>> ChildrenInternal
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

        protected void EnsureItems()
        {
            if (_items == null)
            {
                _items = new List<TItem>();
            }
        }

        protected virtual void CreateSubNodes() { }

        protected void EnsureSubNodes()
        {
            if (_subNodes == null)
            {
                _subNodes = new List<ISpatialIndexNode<TBounds, TItem>>();
                CreateSubNodes();
            }
        }

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

        #region ISpatialIndexNode<TBounds,TItem> Members


        public IEnumerable<TResult> Query<TResult>(TBounds query, Func<TItem, TResult> predicate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}