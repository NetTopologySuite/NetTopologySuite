using System.Collections;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// An <c>BinTree</c> (or "Binary Interval Tree")
    /// is a 1-dimensional version of a quadtree.
    /// It indexes 1-dimensional intervals (which may
    /// be the projection of 2-D objects on an axis).
    /// It supports range searching
    /// (where the range may be a single point).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This structure is dynamic - 
    /// new items can be added at any time,   
    /// and it will support deletion of items 
    /// (although this is not currently implemented).
    /// </para>
    /// <para>
    /// This implementation does not require specifying the extent of the inserted
    /// items beforehand.  It will automatically expand to accomodate any extent
    /// of dataset.</para>
    /// <para>This index is different to the Interval Tree of Edelsbrunner
    /// or the Segment Tree of Bentley.</para>
    /// </remarks>
    public class Bintree
    {
        /// <summary>
        /// Ensure that the Interval for the inserted item has non-zero extents.
        /// Use the current minExtent to pad it, if necessary.
        /// </summary>
        public static Interval EnsureExtent(Interval itemInterval, double minExtent)
        {
            double min = itemInterval.Min;
            double max = itemInterval.Max;
            // has a non-zero extent
            if (min != max) 
                return itemInterval;
            // pad extent
            if (min == max)
            {
                min = min - minExtent / 2.0;
                max = min + minExtent / 2.0;
            }
            return new Interval(min, max);
        }

        private readonly Root _root;
        
        /*
        * Statistics:
        * minExtent is the minimum extent of all items
        * inserted into the tree so far. It is used as a heuristic value
        * to construct non-zero extents for features with zero extent.
        * Start with a non-zero extent, in case the first feature inserted has
        * a zero extent in both directions.  This value may be non-optimal, but
        * only one feature will be inserted with this value.
        **/
        private double minExtent = 1.0;

        /// <summary>
        /// 
        /// </summary>
        public Bintree()
        {
            _root = new Root();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Depth
        {
            get
            {
                if (_root != null) 
                    return _root.Depth;
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                if (_root != null) 
                    return _root.Count;
                return 0;
            }
        }

        /// <summary>
        /// Compute the total number of nodes in the tree.
        /// </summary>
        /// <returns>The number of nodes in the tree.</returns>
        public int NodeSize
        {
            get
            {
                if (_root != null) 
                    return _root.NodeCount;
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInterval"></param>
        /// <param name="item"></param>
        public void Insert(Interval itemInterval, object item)
        {
            CollectStats(itemInterval);
            Interval insertInterval = EnsureExtent(itemInterval, minExtent);            
            _root.Insert(insertInterval, item);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            IList foundItems = new ArrayList();
            _root.AddAllItems(foundItems);
            return foundItems.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public IList Query(double x)
        {
            return Query(new Interval(x, x));
        }

        /// <summary>
        /// min and max may be the same value.
        /// </summary>
        /// <param name="interval"></param>
        public IList Query(Interval interval)
        {
            /*
             * the items that are matched are all items in intervals
             * which overlap the query interval
             */
            IList foundItems = new ArrayList();
            Query(interval, foundItems);
            return foundItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="foundItems"></param>
        public void Query(Interval interval, IList foundItems)
        {
            _root.AddAllItemsFromOverlapping(interval, foundItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        private void CollectStats(Interval interval)
        {
            double del = interval.Width;
            if (del < minExtent && del > 0.0)
                minExtent = del;
        }
    }
}
