using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// The base class for nodes in a <see cref="Quadtree{TCoordinate, TItem}"/>.
    /// </summary>
    /// <summary>
    /// Subquads are numbered as follows:
    /// 2 | 3
    /// --+--
    /// 0 | 1
    /// </summary>
    public abstract class BaseQuadNode<TCoordinate, TItem> : AbstractNode<IExtents<TCoordinate>, TItem>, IEnumerable<TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
        where TItem : IBoundable<IExtents<TCoordinate>>
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

        protected BaseQuadNode(IExtents<TCoordinate> bounds)
            : base(bounds, -1)
        {
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
                if (ChildrenInternal[i] != null)
                {
                    found = ChildrenInternal[i].RemoveItem(item);

                    if (found)
                    {
                        // trim subtree if empty
                        if (ChildrenInternal[i].IsPrunable)
                        {
                            ChildrenInternal[i] = null;
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
            if (ItemsInternal.Contains(item))
            {
                RemoveItem(item);
                found = true;
            }

            return found;
        }

        public Boolean HasChildren
        {
            get
            {
                for (Int32 i = 0; i < 4; i++)
                {
                    if (ChildrenInternal[i] != null)
                    {
                        return true;
                    }
                }

                return false;
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

        public Int32 Depth
        {
            get
            {
                Int32 maxSubDepth = 0;

                foreach (BaseQuadNode<TCoordinate, TItem> node in ChildrenInternal)
                {
                    if (node != null)
                    {
                        Int32 sqd = node.Depth;

                        if (sqd > maxSubDepth)
                        {
                            maxSubDepth = sqd;
                        }
                    }
                }

                return maxSubDepth + 1;
            }
        }

        // [codekaizen] this isn't used in JTS.
        //public Int32 NodeCount
        //{
        //    get
        //    {
        //        Int32 subSize = 0;

        //        for (Int32 i = 0; i < 4; i++)
        //        {
        //            if (ItemsInternal[i] != null)
        //            {
        //                subSize += _subNodes[i].Count;
        //            }
        //        }

        //        return subSize + 1;
        //    }
        //}

        protected override void CreateSubNodes()
        {
            ChildrenInternal.AddRange(new ISpatialIndexNode<IExtents<TCoordinate>, TItem>[4]);
        }

        #region IEnumerable<TItem> Members

        public IEnumerator<TItem> GetEnumerator()
        {
            foreach (TItem item in ItemsInternal)
            {
                yield return item;
            }

            foreach (Node<TCoordinate, TItem> node in ChildrenInternal)
            {
                foreach (TItem item in node)
                {
                    yield return item;
                }
            }
        }

        #endregion

        //protected ICoordinateFactory<TCoordinate> CoordinateFactory
        //{
        //    get { return _coordFactory; }
        //}

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}