namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-Boundable spatial object. Used internally by
    /// AbstractSTRtree.
    /// </summary>
    public class ItemBoundable : IBoundable 
    {
        private readonly object _bounds;
        private readonly object _item;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="item"></param>
        public ItemBoundable(object bounds, object item) 
        {
            _bounds = bounds;
            _item = item;
        }

        /// <summary>
        /// The bounds
        /// </summary>
        public object Bounds 
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// The item
        /// </summary>
        public object Item
        {
            get
            {
                return _item;
            }
        }
    }
}
