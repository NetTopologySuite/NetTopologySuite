using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-Boundable spatial object. Used internally by
    /// AbstractSTRtree.
    /// </summary>
    [Serializable]
    public class ItemBoundable<T, TItem> : IBoundable<T, TItem> where T : IIntersectable<T>, IExpandable<T>
    {
        private readonly T _bounds;
        private readonly TItem _item;

        /// <summary>
        ///
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="item"></param>
        public ItemBoundable(T bounds, TItem item)
        {
            _bounds = bounds;
            _item = item;
        }

        /// <summary>
        /// The bounds
        /// </summary>
        public T Bounds => _bounds;

        /// <summary>
        /// The item
        /// </summary>
        public TItem Item => _item;
    }
}
