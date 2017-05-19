using GeoAPI.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    ///     Boundable wrapper for a non-Boundable spatial object. Used internally by
    ///     AbstractSTRtree.
    /// </summary>
    public class ItemBoundable<T, TItem> : IBoundable<T, TItem> where T : IIntersectable<T>, IExpandable<T>
    {
        /// <summary>
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="item"></param>
        public ItemBoundable(T bounds, TItem item)
        {
            Bounds = bounds;
            Item = item;
        }

        /// <summary>
        ///     The bounds
        /// </summary>
        public T Bounds { get; }

        /// <summary>
        ///     The item
        /// </summary>
        public TItem Item { get; }
    }
}