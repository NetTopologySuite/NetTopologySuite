using System;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.RTree
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TItem"></typeparam>
    public class RTreeLeafNode<TBounds, TItem> : RTreeNode<TBounds, TItem>
        where TBounds : IContainable<TBounds>, IIntersectable<TBounds>, IComparable<TBounds>
        where TItem : IBoundable<TBounds>
    {
        ///<summary>
        ///</summary>
        ///<param name="item"></param>
        public RTreeLeafNode(IBoundsFactory<TBounds> boundsFactory, TItem item)
            : base(boundsFactory, item.Bounds, 0)
        {
            base.Add(item);
        }

        protected override TBounds ComputeBounds()
        {
            return _bounds;
        }
    }
}