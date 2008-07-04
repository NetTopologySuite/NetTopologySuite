namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-Boundable spatial object. Used internally by
    /// AbstractSTRtree.
    /// </summary>
    public class ItemBoundable : IBoundable 
    {
        private object bounds;
        private object item;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="item"></param>
        public ItemBoundable(object bounds, object item) 
        {
            this.bounds = bounds;
            this.item = item;
        }

        /// <summary>
        /// 
        /// </summary>
        public object Bounds 
        {
            get
            {
                return bounds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object Item
        {
            get
            {
                return item;
            }
        }
    }
}
