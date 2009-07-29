using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.IntervalRTree
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TItem"></typeparam>
    public abstract class IntervalRTreeNode<TItem> : AbstractNode<Interval,TItem>, IComparable<Interval>
        where TItem : IBoundable<Interval>
    {
        ///<summary>
        ///</summary>
        ///<param name="level"></param>
        public IntervalRTreeNode(int level)
            :base(level)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="interval"></param>
        ///<param name="level"></param>
        public IntervalRTreeNode(Interval interval, int level)
            :base(interval, level)
        {
        }

        public override bool Intersects(Interval bounds)
        {
            return Bounds.Intersects(bounds);
        }

        protected override bool IsSearchMatch(Interval query)
        {
            return Bounds.Overlaps(query);
        }

        #region IComparable<Interval> Member

        public int CompareTo(Interval other)
        {

            Double centerThis = Bounds.Center;
            Double centerOther = other.Center;
            if (centerThis < centerOther) return -1;
            if (centerThis > centerOther) return 1;
            return 0;

        }

        #endregion
    }
}
