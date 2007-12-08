namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-Boundable spatial object. Used internally by
    /// AbstractStrTree.
    /// </summary>
    public struct ItemBoundable<TBounds, TItem> : IBoundable<TBounds>
    {
        private readonly TBounds _bounds;
        private readonly TItem _item;

        public ItemBoundable(TBounds bounds, TItem item)
        {
            _bounds = bounds;
            _item = item;
        }

        public TBounds Bounds
        {
            get { return _bounds; }
        }

        public TItem Item
        {
            get { return _item; }
        }
    }
}