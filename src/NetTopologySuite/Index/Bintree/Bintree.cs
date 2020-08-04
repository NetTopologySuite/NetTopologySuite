using System.Collections.Generic;
//using NetTopologySuite.DataStructures;

namespace NetTopologySuite.Index.Bintree
{
    public class Bintree : Bintree<object>
    {}

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
    /// items beforehand.  It will automatically expand to accommodate any extent
    /// of dataset.</para>
    /// <para>This index is different to the Interval Tree of Edelsbrunner
    /// or the Segment Tree of Bentley.</para>
    /// </remarks>
    public class Bintree<T>
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
            //return Interval.Create(min, max);
        }

        private readonly Root<T> _root;

        /*
        * Statistics:
        * minExtent is the minimum extent of all items
        * inserted into the tree so far. It is used as a heuristic value
        * to construct non-zero extents for features with zero extent.
        * Start with a non-zero extent, in case the first feature inserted has
        * a zero extent in both directions.  This value may be non-optimal, but
        * only one feature will be inserted with this value.
        **/
        private double _minExtent = 1.0;

        /// <summary>
        ///
        /// </summary>
        public Bintree()
        {
            _root = new Root<T>();
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
        public void Insert(Interval itemInterval, T item)
        {
            CollectStats(itemInterval);
            var insertInterval = EnsureExtent(itemInterval, _minExtent);
            _root.Insert(insertInterval, item);
        }

        /// <summary>
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemInterval">itemEnv the Envelope of the item to be removed</param>
        /// <param name="item">the item to remove</param>
        /// <returns><c>true</c> if the item was found (and thus removed)</returns>
        public bool Remove(Interval itemInterval, T item)
        {
            var insertInterval = EnsureExtent(itemInterval, _minExtent);
            return _root.Remove(insertInterval, item);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            var foundItems = new List<T>();
            _root.AddAllItems(foundItems);
            return foundItems.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public IList<T> Query(double x)
        {
            return Query(new Interval(x, x));
            //return Query(Interval.Create(x));
        }

        /// <summary>
        /// Queries the tree to find all candidate items which
        /// may overlap the query interval.
        /// If the query interval is <tt>null</tt>, all items in the tree are found.
        /// <c>min</c> and <c>max</c> may be the same value.
        /// </summary>
        /// <param name="interval">The interval to query for or <c>null</c></param>
        public IList<T> Query(Interval interval)
        {
            /*
             * the items that are matched are all items in intervals
             * which overlap the query interval
             */
            var foundItems = new List<T>();
            Query(interval, foundItems);
            return foundItems;
        }

        /// <summary>
        /// Adds items in the tree which potentially overlap the query interval
        /// to the given collection.
        /// If the query interval is <c>null</c>, add all items in the tree.
        /// </summary>
        /// <param name="interval">A query interval, or <c>null</c></param>
        /// <param name="foundItems">The candidate items found</param>
        public void Query(Interval interval, ICollection<T> foundItems)
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
            if (del < _minExtent && del > 0.0)
                _minExtent = del;
        }
    }
}
