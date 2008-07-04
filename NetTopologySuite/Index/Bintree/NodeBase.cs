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
        /// <param name="interval"></param>
        /// <param name="centre"></param>
        public static int GetSubnodeIndex(Interval interval, double centre)
        {
            int subnodeIndex = -1;
            if (interval.Min >= centre) 
                subnodeIndex = 1;
            if (interval.Max <= centre) 
                subnodeIndex = 0;
            return subnodeIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        protected IList items = new ArrayList();

        /// <summary>
        /// Subnodes are numbered as follows:
        /// 0 | 1        
        /// .
        /// </summary>
        protected Node[] subnode = new Node[2];
        
        /// <summary>
        /// 
        /// </summary>
        public NodeBase() { }

        /// <summary>
        /// 
        /// </summary>
        public  IList Items
        {
            get
            {                
                return items;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public  void Add(object item)
        {
            items.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public  IList AddAllItems(IList items)
        {
            // items.addAll(this.items);
            foreach (object o in this.items)
                items.Add(o);
            for (int i = 0; i < 2; i++)
                if (subnode[i] != null)
                    subnode[i].AddAllItems(items);                            
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected abstract bool IsSearchMatch(Interval interval);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="resultItems"></param>
        /// <returns></returns>
        public  IList AddAllItemsFromOverlapping(Interval interval, IList resultItems)
        {
            if (!IsSearchMatch(interval))
                return items;
            // resultItems.addAll(items);
            foreach (object o in items)
                resultItems.Add(o);
            for (int i = 0; i < 2; i++)
                if (subnode[i] != null)
                    subnode[i].AddAllItemsFromOverlapping(interval, resultItems);                            
            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        public  int Depth
        {
            get
            {
                int maxSubDepth = 0;
                for (int i = 0; i < 2; i++)
                {
                    if (subnode[i] != null)
                    {
                        int sqd = subnode[i].Depth;
                        if (sqd > maxSubDepth)
                            maxSubDepth = sqd;
                    }
                }
                return maxSubDepth + 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  int Count
        {
            get
            {
                int subSize = 0;
                for (int i = 0; i < 2; i++)
                    if (subnode[i] != null)
                        subSize += subnode[i].Count;
                return subSize + items.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  int NodeCount
        {
            get
            {
                int subCount = 0;
                for (int i = 0; i < 2; i++)
                    if (subnode[i] != null)
                        subCount += subnode[i].NodeCount;
                return subCount + 1;
            }
        }
    }
}
