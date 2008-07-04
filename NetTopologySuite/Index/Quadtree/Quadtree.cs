using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// A Quadtree is a spatial index structure for efficient querying
    /// of 2D rectangles.  If other kinds of spatial objects
    /// need to be indexed they can be represented by their
    /// envelopes    
    /// The quadtree structure is used to provide a primary filter
    /// for range rectangle queries.  The Query() method returns a list of
    /// all objects which may intersect the query rectangle.  Note that
    /// it may return objects which do not in fact intersect.
    /// A secondary filter is required to test for exact intersection.
    /// Of course, this secondary filter may consist of other tests besides
    /// intersection, such as testing other kinds of spatial relationships.
    /// This implementation does not require specifying the extent of the inserted
    /// items beforehand.  It will automatically expand to accomodate any extent
    /// of dataset.
    /// This data structure is also known as an <c>MX-CIF quadtree</c>
    /// following the usage of Samet and others.
    /// </summary>
    public class Quadtree : ISpatialIndex
    {
        /// <summary>
        /// Ensure that the envelope for the inserted item has non-zero extents.
        /// Use the current minExtent to pad the envelope, if necessary.
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="minExtent"></param>
        public static IEnvelope EnsureExtent(IEnvelope itemEnv, double minExtent)
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
                maxx = minx + minExtent / 2.0;
            }
            if (miny == maxy) 
            {
                miny = miny - minExtent / 2.0;
                maxy = miny + minExtent / 2.0;
            }
            return new Envelope(minx, maxx, miny, maxy);
        }

        private Root root;

        /// <summary>
        /// minExtent is the minimum envelope extent of all items
        /// inserted into the tree so far. It is used as a heuristic value
        /// to construct non-zero envelopes for features with zero X and/or Y extent.
        /// Start with a non-zero extent, in case the first feature inserted has
        /// a zero extent in both directions.  This value may be non-optimal, but
        /// only one feature will be inserted with this value.
        /// </summary>
        private double minExtent = 1.0;

        /// <summary>
        /// Constructs a Quadtree with zero items.
        /// </summary>
        public Quadtree()
        {
            root = new Root();
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
                if (root != null) 
                    return root.Depth;
                return 0;
            }
        }

        /// <summary> 
        /// Returns the number of items in the tree.
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
        /// 
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="item"></param>
        public void Insert(IEnvelope itemEnv, object item)
        {
            CollectStats(itemEnv);
            IEnvelope insertEnv = EnsureExtent(itemEnv, minExtent);
            root.Insert(insertEnv, item);
        }

        /// <summary> 
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemEnv">The Envelope of the item to remove.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found.</returns>
        public bool Remove(IEnvelope itemEnv, object item)
        {
            IEnvelope posEnv = EnsureExtent(itemEnv, minExtent);
            return root.Remove(posEnv, item);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <returns></returns>
        public IList Query(IEnvelope searchEnv)
        {
            /*
            * the items that are matched are the items in quads which
            * overlap the search envelope
            */
            ArrayListVisitor visitor = new ArrayListVisitor();
            Query(searchEnv, visitor);
            return visitor.Items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="visitor"></param>
        public void Query(IEnvelope searchEnv, IItemVisitor visitor)
        {
            /*
            * the items that are matched are the items in quads which
            * overlap the search envelope
            */
            root.Visit(searchEnv, visitor);
        }

        /// <summary>
        /// Return a list of all items in the Quadtree.
        /// </summary>
        public IList QueryAll()
        {
            IList foundItems = new ArrayList();
            root.AddAllItems(ref foundItems);
            return foundItems;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemEnv"></param>
        private void CollectStats(IEnvelope itemEnv)
        {
            double delX = itemEnv.Width;
            if (delX < minExtent && delX > 0.0)
            minExtent = delX;

            double delY = itemEnv.Height;
            if (delY < minExtent && delY > 0.0)
            minExtent = delY;
        }
    }
}
