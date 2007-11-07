using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary> 
    /// The base class for nodes in a <c>Bintree</c>.
    /// </summary>
    public abstract class NodeBase
    {
        /// <summary> 
        /// Returns the index of the subnode that wholely contains the given interval.
        /// If none does, returns -1.
        /// </summary>
        public static Int32 GetSubnodeIndex(Interval interval, Double centre)
        {
            Int32 subnodeIndex = -1;
            if (interval.Min >= centre)
            {
                subnodeIndex = 1;
            }
            if (interval.Max <= centre)
            {
                subnodeIndex = 0;
            }
            return subnodeIndex;
        }

        protected IList items = new ArrayList();

        /// <summary>
        /// Subnodes are numbered as follows:
        /// 0 | 1        
        /// .
        /// </summary>
        protected Node[] subnode = new Node[2];

        public NodeBase() {}

        public IList Items
        {
            get { return items; }
        }

        public void Add(object item)
        {
            items.Add(item);
        }

        public IList AddAllItems(IList items)
        {
            // items.addAll(this.items);
            foreach (object o in this.items)
            {
                items.Add(o);
            }

            for (Int32 i = 0; i < 2; i++)
            {
                if (subnode[i] != null)
                {
                    subnode[i].AddAllItems(items);
                }
            }
            return items;
        }

        protected abstract Boolean IsSearchMatch(Interval interval);

        public IList AddAllItemsFromOverlapping(Interval interval, IList resultItems)
        {
            if (!IsSearchMatch(interval))
            {
                return items;
            }

            // resultItems.addAll(items);
            foreach (object o in items)
            {
                resultItems.Add(o);
            }

            for (Int32 i = 0; i < 2; i++)
            {
                if (subnode[i] != null)
                {
                    subnode[i].AddAllItemsFromOverlapping(interval, resultItems);
                }
            }

            return items;
        }

        public Int32 Depth
        {
            get
            {
                Int32 maxSubDepth = 0;

                for (Int32 i = 0; i < 2; i++)
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

                for (Int32 i = 0; i < 2; i++)
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
                Int32 subCount = 0;

                for (Int32 i = 0; i < 2; i++)
                {
                    if (subnode[i] != null)
                    {
                        subCount += subnode[i].NodeCount;
                    }
                }

                return subCount + 1;
            }
        }
    }
}