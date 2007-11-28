namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-Boundable spatial object. Used internally by
    /// AbstractStrTree.
    /// </summary>
    public class ItemBoundable : IBoundable
    {
        private object bounds;
        private object item;

        public ItemBoundable(object bounds, object item)
        {
            this.bounds = bounds;
            this.item = item;
        }

        public object Bounds
        {
            get { return bounds; }
        }

        public object Item
        {
            get { return item; }
        }
    }
}