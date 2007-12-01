using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// The base class for nodes in a <see cref="Quadtree{TCoordinate, TItem}"/>.
    /// </summary>
    public abstract class NodeBase<TCoordinate, TItem> : IEnumerable<TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Returns the index of the subquad that wholly contains the given envelope.
        /// If none does, returns -1.
        /// </summary>
        public static Int32 GetSubnodeIndex(IExtents<TCoordinate> extents, TCoordinate center)
        {
            Int32 subnodeIndex = -1;

            if (extents.GetMin(Ordinates.X) >= center[Ordinates.X])
            {
                if (extents.GetMin(Ordinates.Y) >= center[Ordinates.Y])
                {
                    subnodeIndex = 3;
                }
                if (extents.GetMax(Ordinates.Y) <= center[Ordinates.Y])
                {
                    subnodeIndex = 1;
                }
            }

            if (extents.GetMax(Ordinates.X) <= center[Ordinates.X])
            {
                if (extents.GetMin(Ordinates.Y) >= center[Ordinates.Y])
                {
                    subnodeIndex = 2;
                }
                if (extents.GetMax(Ordinates.Y) <= center[Ordinates.Y])
                {
                    subnodeIndex = 0;
                }
            }

            return subnodeIndex;
        }

        private readonly List<TItem> _items = new List<TItem>();

        /// <summary>
        /// subquads are numbered as follows:
        /// 2 | 3
        /// --+--
        /// 0 | 1
        /// </summary>
        private readonly Node<TCoordinate, TItem>[] _subNodes = new Node<TCoordinate, TItem>[4];

        public IEnumerable<TItem> Items
        {
            get { return _items; }
        }

        public Boolean HasItems
        {
            get
            {
                return _items.Count == 0;
            }
        }

        public void Add(TItem item)
        {
            _items.Add(item);
        }

        /// <summary> 
        /// Removes a single item from this subtree.
        /// </summary>
        /// <param name="itemExtents">The <see cref="IExtents{TCoordinate}"/> containing the item.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> if the item was found and removed.</returns>
        public Boolean Remove(IExtents<TCoordinate> itemExtents, TItem item)
        {
            // use envelope to restrict nodes scanned
            if (!IsSearchMatch(itemExtents))
            {
                return false;
            }

            Boolean found = false;

            for (Int32 i = 0; i < 4; i++)
            {
                if (_subNodes[i] != null)
                {
                    found = _subNodes[i].Remove(itemExtents, item);

                    if (found)
                    {
                        // trim subtree if empty
                        if (_subNodes[i].IsPrunable)
                        {
                            _subNodes[i] = null;
                        }

                        break;
                    }
                }
            }

            // if item was found lower down, don't need to search for it here
            if (found)
            {
                return found;
            }

            // otherwise, try and remove the item from the list of items in this node
            if (_items.Contains(item))
            {
                _items.Remove(item);
                found = true;
            }

            return found;
        }

        public Boolean IsPrunable
        {
            get { return !(HasChildren || HasItems); }
        }

        public Boolean HasChildren
        {
            get
            {
                for (Int32 i = 0; i < 4; i++)
                {
                    if (_subNodes[i] != null)
                    {
                        return true;
                    }
                }

                return false;
            }
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

        ///// <summary>
        ///// Insert items in <c>this</c> into the parameter!
        ///// </summary>
        ///// <param name="resultItems">IList for adding items.</param>
        ///// <returns>Parameter IList with <c>this</c> items.</returns>
        //public IList AddAllItems(ref IList resultItems)
        //{
        //    // this node may have items as well as subnodes (since items may not
        //    // be wholely contained in any single subnode
        //    // resultItems.addAll(this.items);
        //    foreach (object o in _items)
        //    {
        //        resultItems.Add(o);
        //    }

        //    for (Int32 i = 0; i < 4; i++)
        //    {
        //        if (_subNodes[i] != null)
        //        {
        //            _subNodes[i].AddAllItems(ref resultItems);
        //        }
        //    }

        //    return resultItems;
        //}

        //public void AddAllItemsFromOverlapping(IExtents query, ref IList<TItem> resultItems)
        //{
        //    if (!IsSearchMatch(query))
        //    {
        //        return;
        //    }

        //    // this node may have items as well as subnodes (since items may not
        //    // be wholely contained in any single subnode
        //    foreach (TItem item in _items)
        //    {
        //        resultItems.Add(item);
        //    }

        //    for (Int32 i = 0; i < 4; i++)
        //    {
        //        if (_subNodes[i] != null)
        //        {
        //            _subNodes[i].AddAllItemsFromOverlapping(query, ref resultItems);
        //        }
        //    }
        //}

        public IEnumerable<TItem> Query(IExtents<TCoordinate> query)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            foreach (TItem item in _items)
            {
                yield return item;
            }

            for (Int32 i = 0; i < 4; i++)
            {
                if (_subNodes[i] != null)
                {
                    foreach (TItem item in _subNodes[i].Query(query))
                    {
                        yield return item;
                    }
                }
            }
        }

        public IEnumerable<TItem> Query(IExtents<TCoordinate> query, Predicate<TItem> predicate)
        {
            if (!IsSearchMatch(query))
            {
                yield break;
            }

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            foreach (TItem item in visitItems(query, predicate))
            {
                yield return item;
            }

            for (Int32 i = 0; i < 4; i++)
            {
                if (_subNodes[i] != null)
                {
                    foreach (TItem item in _subNodes[i].Query(query, predicate))
                    {
                        yield return item;
                    }
                }
            }
        }

        public Int32 Depth
        {
            get
            {
                Int32 maxSubDepth = 0;
                
                for (Int32 i = 0; i < 4; i++)
                {
                    if (_subNodes[i] != null)
                    {
                        Int32 sqd = _subNodes[i].Depth;
                        if (sqd > maxSubDepth)
                        {
                            maxSubDepth = sqd;
                        }
                    }
                }

                return maxSubDepth + 1;
            }
        }

        public Int32 Count
        {
            get
            {
                Int32 subSize = 0;
                
                for (Int32 i = 0; i < 4; i++)
                {
                    if (_subNodes[i] != null)
                    {
                        subSize += _subNodes[i].Count;
                    }
                }

                return subSize + _items.Count;
            }
        }

        public Int32 NodeCount
        {
            get
            {
                Int32 subSize = 0;

                for (Int32 i = 0; i < 4; i++)
                {
                    if (_subNodes[i] != null)
                    {
                        subSize += _subNodes[i].Count;
                    }
                }

                return subSize + 1;
            }
        }

        #region IEnumerable<TItem> Members

        public IEnumerator<TItem> GetEnumerator()
        {
            foreach (TItem item in _items)
            {
                yield return item;
            }

            foreach (Node<TCoordinate, TItem> node in _subNodes)
            {
                foreach (TItem item in node)
                {
                    yield return item;
                }
            }
        }

        #endregion

        protected abstract Boolean IsSearchMatch(IExtents<TCoordinate> query);

        protected List<TItem> ItemsInternal
        {
            get { return _items; }
        }

        protected Node<TCoordinate, TItem>[] SubNodes
        {
            get { return _subNodes; }
        }

        private IEnumerable<TItem> visitItems(IExtents<TCoordinate> query, Predicate<TItem> predicate)
        {
            // would be nice to filter items based on search envelope, 
            // but can't until they contain an envelope
            foreach (TItem item in _items)
            {
                if(predicate(item))
                {
                    yield return item;
                }
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}