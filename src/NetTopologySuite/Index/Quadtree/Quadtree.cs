using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// A Quadtree is a spatial index structure for efficient range querying
    /// of items bounded by 2D rectangles.<br/>
    /// <see cref="Geometry"/>s can be indexed by using their <see cref="Envelope"/>s.<br/>
    /// Any type of object can also be indexed, as long as it has an extent that can be
    /// represented by an <see cref="Envelope"/>.
    /// <para/>
    /// This Quadtree index provides a <b>primary filter</b>
    /// for range rectangle queries.  The various query methods return a list of
    /// all items which <i>may</i> intersect the query rectangle.  Note that
    /// it may thus return items which do <b>not</b> in fact intersect the query rectangle.
    /// A secondary filter is required to test for actual intersection
    /// between the query rectangle and the envelope of each candidate item.
    /// The secondary filter may be performed explicitly,
    /// or it may be provided implicitly by subsequent operations executed on the items
    /// (for instance, if the index query is followed by computing a spatial predicate
    /// between the query geometry and tree items,
    /// the envelope intersection check is performed automatically.
    /// <para/>
    /// This implementation does not require specifying the extent of the inserted
    /// items beforehand.  It will automatically expand to accommodate any extent
    /// of dataset.
    /// <para/>
    /// This data structure is also known as an <c>MX-CIF quadtree</c>
    /// following the terminology usage of Samet and others.
    /// </summary>
    [System.Serializable]
    public class Quadtree<T> : ISpatialIndex<T>
    {
        /// <summary>
        /// Ensure that the envelope for the inserted item has non-zero extents.
        /// Use the current minExtent to pad the envelope, if necessary.
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="minExtent"></param>
        public static Envelope EnsureExtent(Envelope itemEnv, double minExtent)
        {
            //The names "ensureExtent" and "minExtent" are misleading -- sounds like
            //this method ensures that the extents are greater than minExtent.
            //Perhaps we should rename them to "ensurePositiveExtent" and "defaultExtent".
            //[Jon Aquino]
            double minx = itemEnv.MinX;
            double maxx = itemEnv.MaxX;
            double miny = itemEnv.MinY;
            double maxy = itemEnv.MaxY;
            // has a non-zero extent
            if (minx != maxx && miny != maxy)
                return itemEnv;
            // pad one or both extents
            if (minx == maxx)
            {
                minx = minx - minExtent / 2.0;
                maxx = maxx + minExtent / 2.0;
            }
            if (miny == maxy)
            {
                miny = miny - minExtent / 2.0;
                maxy = maxy + minExtent / 2.0;
            }
            return new Envelope(minx, maxx, miny, maxy);
        }

        private readonly Root<T> _root;

        /// <summary>
        /// minExtent is the minimum envelope extent of all items
        /// inserted into the tree so far. It is used as a heuristic value
        /// to construct non-zero envelopes for features with zero X and/or Y extent.
        /// Start with a non-zero extent, in case the first feature inserted has
        /// a zero extent in both directions.  This value may be non-optimal, but
        /// only one feature will be inserted with this value.
        /// </summary>
        private double _minExtent = 1.0;

        /// <summary>
        /// Constructs a Quadtree with zero items.
        /// </summary>
        public Quadtree()
        {
            _root = new Root<T>();
        }

        /// <summary>
        /// Returns the number of levels in the tree.
        /// </summary>
        public int Depth
        {
            get
            {
                //I don't think it's possible for root to be null. Perhaps we should
                //remove the check. [Jon Aquino]
                //Or make an assertion [Jon Aquino 10/29/2003]
                if (_root != null)
                    return _root.Depth;
                return 0;
            }
        }

        /// <summary>
        /// Tests whether the index contains any items.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (_root == null) return true;
                return _root.IsEmpty;
            }
        }

        /// <summary>
        /// Returns the number of items in the tree.
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
        ///
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="item"></param>
        public void Insert(Envelope itemEnv, T item)
        {
            CollectStats(itemEnv);
            var insertEnv = EnsureExtent(itemEnv, _minExtent);
            _root.Insert(insertEnv, item);
        }

        /// <summary>
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemEnv">The Envelope of the item to be removed.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found (and thus removed).</returns>
        public bool Remove(Envelope itemEnv, T item)
        {
            var posEnv = EnsureExtent(itemEnv, _minExtent);
            return _root.Remove(posEnv, item);
        }

        /// <summary>
        /// Queries the tree and returns items which may lie in the given search envelope.
        /// </summary>
        /// <remarks>
        /// Precisely, the items that are returned are all items in the tree
        /// whose envelope <b>may</b> intersect the search Envelope.
        /// Note that some items with non-intersecting envelopes may be returned as well;
        /// the client is responsible for filtering these out.
        /// In most situations there will be many items in the tree which do not
        /// intersect the search envelope and which are not returned - thus
        /// providing improved performance over a simple linear scan.
        /// </remarks>
        /// <param name="searchEnv">The envelope of the desired query area.</param>
        /// <returns>A List of items which may intersect the search envelope</returns>
        public IList<T> Query(Envelope searchEnv)
        {
            /*
            * the items that are matched are the items in quads which
            * overlap the search envelope
            */
            var visitor = new ArrayListVisitor<T>();
            Query(searchEnv, visitor);
            return visitor.Items;
        }

        /// <summary>
        /// Queries the tree and visits items which may lie in the given search envelope.
        /// </summary>
        /// <remarks>
        /// Precisely, the items that are visited are all items in the tree
        /// whose envelope <b>may</b> intersect the search Envelope.
        /// Note that some items with non-intersecting envelopes may be visited as well;
        /// the client is responsible for filtering these out.
        /// In most situations there will be many items in the tree which do not
        /// intersect the search envelope and which are not visited - thus
        /// providing improved performance over a simple linear scan.
        /// </remarks>
        /// <param name="searchEnv">The envelope of the desired query area.</param>
        /// <param name="visitor">A visitor object which is passed the visited items</param>
        public void Query(Envelope searchEnv, IItemVisitor<T> visitor)
        {
            /*
             * the items that are matched are the items in quads which
             * overlap the search envelope
             */
            _root.Visit(searchEnv, visitor);
        }

        /// <summary>
        /// Return a list of all items in the Quadtree.
        /// </summary>
        public IList<T> QueryAll()
        {
            IList<T> foundItems = new List<T>();
            _root.AddAllItems(ref foundItems);
            return foundItems;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemEnv"></param>
        private void CollectStats(Envelope itemEnv)
        {
            double delX = itemEnv.Width;
            if (delX < _minExtent && delX > 0.0)
                _minExtent = delX;

            double delY = itemEnv.Height;
            if (delY < _minExtent && delY > 0.0)
                _minExtent = delY;
        }

        /// <summary>
        /// Gets a value indicating the root node of this QuadTree
        /// </summary>
        /// <returns>The root node of this QuadTree</returns>
        public Root<T> Root => _root;
    }

    /// <summary>
    /// Item visitor that specifically excludes a predefined area.
    /// </summary>
    /// <typeparam name="T">The type of the items to visit</typeparam>
    public class ExcludingItemVisitor<T> : IItemVisitor<T> where T:Geometry
    {
        private readonly Envelope _exclude;
        private readonly List<T> _items = new List<T>();

        /// <summary>
        /// Initialize with <paramref name="exclude"/>
        /// </summary>
        /// <param name="exclude"></param>
        public ExcludingItemVisitor(Envelope exclude)
        {
            _exclude = exclude;
        }

        /// <inheritdoc cref="IItemVisitor{T}.VisitItem"/>>
        public void VisitItem(T item)
        {
            // If we have no intersection with _exclude, add it.
            if (!_exclude.Intersects(item.EnvelopeInternal))
                _items.Add(item);
        }

        /// <summary>
        /// Get a value indicating the gathered items
        /// </summary>
        public IList<T> Items => _items;
    }
}
