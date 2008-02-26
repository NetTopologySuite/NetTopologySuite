using System;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-boundable object. Used internally by
    /// AbstractStrTree.
    /// </summary>
    public abstract class ItemBoundable<TBounds, TItem> : IBoundable<TBounds>
    {
        private readonly TBounds _bounds;
        private readonly TItem _item;

        protected ItemBoundable(TBounds bounds, TItem item)
        {
            _bounds = bounds;
            _item = item;
        }

        public TItem Item
        {
            get { return _item; }
        }

        #region IBoundable<TBounds> Members

        public TBounds Bounds
        {
            get { return _bounds; }
        }

        public abstract Boolean Intersects(TBounds bounds);

        #endregion
    }
}