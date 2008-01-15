using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index
{
    public abstract class AbstractNode<TBounds, TBoundable> : ISpatialIndexNode<TBounds, TBoundable>
        where TBoundable : IBoundable<TBounds>
    {
        private List<TBoundable> _items;
        private List<ISpatialIndexNode<TBounds, TBoundable>> _subNodes;
        private TBounds _bounds = default(TBounds);
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

        public Int32 TotalItems
        {
            get
            {
                Int32 subSize = 0;

                foreach (ISpatialIndexNode<TBounds, TBoundable> node in ChildrenInternal)
                {
                    subSize += node.TotalItems;
                }

                return subSize + ItemCount;
            }
        }

        public Int32 TotalNodes
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
                if (Equals(_bounds, default(TBounds)))
                {
                    _bounds = ComputeBounds();
                }

                return _bounds;
            }
        }

        public Boolean IsLeaf
        {
            get { return Level == 0; }
        }

        public Boolean Remove(ISpatialIndexNode<TBounds, TBoundable> item)
        {
            if (IsLeaf)
            {
                return _subNodes.Remove(item);
            }
            else
            {
                foreach (AbstractNode<TBounds, TBoundable> node in _subNodes)
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

        public virtual void AddChild(ISpatialIndexNode<TBounds, TBoundable> child)
        {
            EnsureSubNodes();
            Debug.Assert(_subNodes != null);
            _subNodes.Add(child);
        }

        public virtual void AddChildren(IEnumerable<ISpatialIndexNode<TBounds, TBoundable>> children)
        {
            EnsureSubNodes();
            Debug.Assert(_subNodes != null);
            _subNodes.AddRange(children);
        }

        public virtual void AddItem(TBoundable item)
        {
            EnsureItems();
            Debug.Assert(_items != null);
            _items.Add(item);
        }

        public virtual void AddItems(IEnumerable<TBoundable> items)
        {
            EnsureItems();
            Debug.Assert(_items != null);
            _items.AddRange(items);
        }

        public virtual Int32 ChildCount
        {
            get { return _subNodes == null ? 0 : _subNodes.Count; }
        }

        public virtual Int32 ItemCount
        {
            get { return _items == null ? 0 : _items.Count; }
        }

        public virtual IEnumerable<TBoundable> Items
        {
            get
            {
                foreach (TBoundable item in _items)
                {
                    yield return item;
                }
            }
        }

        public virtual IEnumerable<ISpatialIndexNode<TBounds, TBoundable>> Children
        {
            get
            {
                foreach (ISpatialIndexNode<TBounds, TBoundable> node in _subNodes)
                {
                    yield return node;
                }
            }
        }

        public virtual Boolean RemoveItem(TBoundable item)
        {
            return _items.Remove(item);
        }

        public virtual Boolean RemoveChild(ISpatialIndexNode<TBounds, TBoundable> child)
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

        public bool HasItems
        {
            get { return _items != null && _items.Count > 0; }
        }

        public bool HasChildren
        {
            get { return _subNodes != null && _subNodes.Count > 0; }
        }

        public Boolean IsPrunable
        {
            get { return !(HasChildren || HasItems); }
        }

        public virtual IEnumerable<TBoundable> Query(TBounds query)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }

            foreach (TBoundable item in ItemsInternal)
            {
                if (item.Intersects(query))
                {
                    yield return item;
                }
            }

            foreach (ISpatialIndexNode<TBounds, TBoundable> node in ChildrenInternal)
            {
                if (node != null)
                {
                    foreach (TBoundable item in node.Query(query))
                    {
                        yield return item;
                    }
                }
            }
        }

        public virtual IEnumerable<TBoundable> Query(TBounds query, Predicate<TBoundable> predicate)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            foreach (TBoundable item in filterItems(query, predicate))
            {
                yield return item;
            }

            foreach (ISpatialIndexNode<TBounds, TBoundable> node in ChildrenInternal)
            {
                if (node != null)
                {
                    foreach (TBoundable item in node.Query(query, predicate))
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

        protected List<TBoundable> ItemsInternal
        {
            get { return _items; }
        }

        protected List<ISpatialIndexNode<TBounds, TBoundable>> ChildrenInternal
        {
            get { return _subNodes; }
        }

        protected void EnsureItems()
        {
            if (_items == null)
            {
                _items = new List<TBoundable>();
            }
        }

        protected virtual void CreateSubNodes() { }

        protected void EnsureSubNodes()
        {
            if (_subNodes == null)
            {
                _subNodes = new List<ISpatialIndexNode<TBounds, TBoundable>>();
                CreateSubNodes();
            }
        }

        private IEnumerable<TBoundable> filterItems(TBounds query, Predicate<TBoundable> predicate)
        {
            foreach (TBoundable item in ItemsInternal)
            {
                if (item.Intersects(query) && predicate(item))
                {
                    yield return item;
                }
            }
        }
    }
}