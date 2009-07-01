using System.Collections;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Index.BintreeTemp
{
    /// <summary> 
    /// The base class for nodes in a <c>Bintree</c>.
    /// </summary>
    public abstract class NodeBase<T>
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
        protected IList<T> items = new List<T>();

        /// <summary>
        /// Subnodes are numbered as follows:
        /// 0 | 1        
        /// .
        /// </summary>
        protected Node<T>[] subnode = new Node<T>[2];

        /// <summary>
        /// 
        /// </summary>
        public NodeBase() { }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<T> Items
        {
            get
            {
                foreach (T item in items)
                    yield return item;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            items.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public IEnumerable<T> AllItems
        {
            get
            {
                foreach (T o in this.items)
                    yield return o;
                for (int i = 0; i < 2; i++)
                    if (subnode[i] != null)
                        foreach (T o in subnode[i].AllItems)
                            yield return o;
            }
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
        public IEnumerable<T> AddAllItemsFromOverlapping(Interval interval)
        {
            if (!IsSearchMatch(interval))
                foreach (T item in Items)
                {
                    yield return item;
                }
            // resultItems.addAll(items);
            foreach (T o in items)
                yield return o;
            for (int i = 0; i < 2; i++)
                if (subnode[i] != null)
                    foreach (T item in  subnode[i].AddAllItemsFromOverlapping(interval))
                        yield return item;
      
        }

        /// <summary>
        /// 
        /// </summary>
        public int Depth
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
        public int Count
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
        public int NodeCount
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
