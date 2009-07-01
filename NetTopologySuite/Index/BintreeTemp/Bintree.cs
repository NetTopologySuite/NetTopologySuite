using System.Collections;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Index.BintreeTemp
{
    /// <summary>
    /// An <c>BinTree</c> (or "Binary Interval Tree")
    /// is a 1-dimensional version of a quadtree.
    /// It indexes 1-dimensional intervals (which of course may
    /// be the projection of 2-D objects on an axis).
    /// It supports range searching
    /// (where the range may be a single point).
    /// This implementation does not require specifying the extent of the inserted
    /// items beforehand.  It will automatically expand to accomodate any extent
    /// of dataset.
    /// This index is different to the Interval Tree of Edelsbrunner
    /// or the Segment Tree of Bentley.
    /// </summary>
    public class Bintree<TItem> : IEnumerable<TItem>
    {
        private readonly Root<TItem> root;

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
            root = new Root<TItem>();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Depth
        {
            get
            {
                if (root != null)
                    return root.Depth;
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
                if (root != null)
                    return root.Count;
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
                if (root != null)
                    return root.NodeCount;
                return 0;
            }
        }

        #region IEnumerable<TItem> Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TItem> GetEnumerator()
        {
            return root.AllItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

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
                min = min - minExtent/2.0;
                max = min + minExtent/2.0;
            }
            return new Interval(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInterval"></param>
        /// <param name="item"></param>
        public void Insert(Interval itemInterval, TItem item)
        {
            CollectStats(itemInterval);
            Interval insertInterval = EnsureExtent(itemInterval, minExtent);
            root.Insert(insertInterval, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public IEnumerable<TItem> Query(double x)
        {
            return Query(new Interval(x, x));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="foundItems"></param>
        public IEnumerable<TItem> Query(Interval interval)
        {
            return root.AddAllItemsFromOverlapping(interval);
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