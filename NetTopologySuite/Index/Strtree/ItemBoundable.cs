using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Boundable wrapper for a non-Boundable spatial object. Used internally by
    /// AbstractSTRtree.
    /// </summary>
#if PCL
    [System.Runtime.Serialization.DataContract]
#else
    [Serializable]
#endif
    public class ItemBoundable<T, TItem> : IBoundable<T, TItem> where T : IIntersectable<T>, IExpandable<T>
    {
#if PCL
    [System.Runtime.Serialization.DataMember(Name="Bounds")]
#endif
        private readonly T _bounds;
#if PCL
    [System.Runtime.Serialization.DataMember(Name = "Item")]
#endif
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
        public T Bounds 
        {
            get
            {
                return _bounds;
            }
        }

        /// <summary>
        /// The item
        /// </summary>
        public TItem Item
        {
            get
            {
                return _item;
            }
        }
    }
}
