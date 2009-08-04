using System;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.RTree
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TItem"></typeparam>
    public abstract class RTreeNode<TBounds, TItem> : AbstractNode<TBounds, TItem>
        where TBounds : IContainable<TBounds>, IIntersectable<TBounds>, IComparable<TBounds>
        where TItem : IBoundable<TBounds>
    {
        ///<summary>
        ///</summary>
        ///<param name="level"></param>
        public RTreeNode(IBoundsFactory<TBounds> boundsFactory, int level)
            : base(level)
        {
            BoundsFactory = boundsFactory;
        }

        ///<summary>
        ///</summary>
        ///<param name="bounds"></param>
        ///<param name="level"></param>
        public RTreeNode(IBoundsFactory<TBounds> boundsFactory, TBounds bounds, int level)
            : base(bounds, level)
        {
            BoundsFactory = boundsFactory;
        }

        protected IBoundsFactory<TBounds> BoundsFactory { get; set; }

        public override bool Intersects(TBounds bounds)
        {
            return Bounds.Intersects(bounds);
        }

        protected override bool IsSearchMatch(TBounds query)
        {
            return Bounds.Intersects(query);
        }

        #region IComparable<Interval> Member

        public int CompareTo(TBounds other)
        {
            return Bounds.CompareTo(other);
        }

        #endregion
    }
}