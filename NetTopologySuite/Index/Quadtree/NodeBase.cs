using System;
using System.Collections;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// The base class for nodes in a <c>Quadtree</c>.
    /// </summary>
    public abstract class NodeBase
    {
        /// <summary> 
        /// Returns the index of the subquad that wholly contains the given envelope.
        /// If none does, returns -1.
        /// </summary>
        public static Int32 GetSubnodeIndex(IExtents env, ICoordinate centre)
        {
            Int32 subnodeIndex = -1;

            if (env.MinX >= centre.X)
            {
                if (env.MinY >= centre.Y)
                {
                    subnodeIndex = 3;
                }
                if (env.MaxY <= centre.Y)
                {
                    subnodeIndex = 1;
                }
            }

            if (env.MaxX <= centre.X)
            {
                if (env.MinY >= centre.Y)
                {
                    subnodeIndex = 2;
                }
                if (env.MaxY <= centre.Y)
                {
                    subnodeIndex = 0;
                }
            }

            return subnodeIndex;
        }

        protected IList items = new ArrayList();

        /// <summary>
        /// subquads are numbered as follows:
        /// 2 | 3
        /// --+--
        /// 0 | 1
        /// </summary>
        protected Node[] subnode = new Node[4];

        public NodeBase() {}

        public IList Items
        {
            get { return items; }
        }

        public Boolean HasItems
        {
            get
            {
                // return !items.IsEmpty; 
                if (items.Count == 0)
                {
                    return false;
                }
                return true;
            }
        }

        public void Add(object item)
        {
            items.Add(item);
        }

        /// <summary> 
        /// Removes a single item from this subtree.
        /// </summary>
        /// <param name="itemEnv">The envelope containing the item.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> if the item was found and removed.</returns>
        public Boolean Remove(IExtents itemEnv, object item)
        {
            // use envelope to restrict nodes scanned
            if (!IsSearchMatch(itemEnv))
            {
                return false;
            }

            Boolean found = false;
            for (Int32 i = 0; i < 4; i++)
            {
                if (subnode[i] != null)
                {
                    found = subnode[i].Remove(itemEnv, item);
                    if (found)
                    {
                        // trim subtree if empty
                        if (subnode[i].IsPrunable)
                        {
                            subnode[i] = null;
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
            if (items.Contains(item))
            {
                items.Remove(item);
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
                    if (subnode[i] != null)
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
                if (items.Count != 0)
                {
                    isEmpty = false;
                }
                for (Int32 i = 0; i < 4; i++)
                {
                    if (subnode[i] != null)
                    {
                        if (!subnode[i].IsEmpty)
                        {
                            isEmpty = false;
                        }
                    }
                }
                return isEmpty;
            }
        }

        /// <summary>
        /// Insert items in <c>this</c> into the parameter!
        /// </summary>
        /// <param name="resultItems">IList for adding items.</param>
        /// <returns>Parameter IList with <c>this</c> items.</returns>
        public IList AddAllItems(ref IList resultItems)
        {
            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            // resultItems.addAll(this.items);
            foreach (object o in items)
            {
                resultItems.Add(o);
            }

            for (Int32 i = 0; i < 4; i++)
            {
                if (subnode[i] != null)
                {
                    subnode[i].AddAllItems(ref resultItems);
                }
            }

            return resultItems;
        }

        protected abstract Boolean IsSearchMatch(IExtents searchEnv);

        public void AddAllItemsFromOverlapping(IExtents searchEnv, ref IList resultItems)
        {
            if (!IsSearchMatch(searchEnv))
            {
                return;
            }

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            foreach (object o in items)
            {
                resultItems.Add(o);
            }

            for (Int32 i = 0; i < 4; i++)
            {
                if (subnode[i] != null)
                {
                    subnode[i].AddAllItemsFromOverlapping(searchEnv, ref resultItems);
                }
            }
        }

        public void Visit(IExtents searchEnv, IItemVisitor visitor)
        {
            if (!IsSearchMatch(searchEnv))
            {
                return;
            }

            // this node may have items as well as subnodes (since items may not
            // be wholely contained in any single subnode
            VisitItems(searchEnv, visitor);

            for (Int32 i = 0; i < 4; i++)
            {
                if (subnode[i] != null)
                {
                    subnode[i].Visit(searchEnv, visitor);
                }
            }
        }

        private void VisitItems(IExtents searchEnv, IItemVisitor visitor)
        {
            // would be nice to filter items based on search envelope, but can't until they contain an envelope
            for (IEnumerator i = items.GetEnumerator(); i.MoveNext();)
            {
                visitor.VisitItem(i.Current);
            }
        }

        public Int32 Depth
        {
            get
            {
                Int32 maxSubDepth = 0;
                for (Int32 i = 0; i < 4; i++)
                {
                    if (subnode[i] != null)
                    {
                        Int32 sqd = subnode[i].Depth;
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
                    if (subnode[i] != null)
                    {
                        subSize += subnode[i].Count;
                    }
                }
                return subSize + items.Count;
            }
        }

        public Int32 NodeCount
        {
            get
            {
                Int32 subSize = 0;

                for (Int32 i = 0; i < 4; i++)
                {
                    if (subnode[i] != null)
                    {
                        subSize += subnode[i].Count;
                    }
                }

                return subSize + 1;
            }
        }
    }
}