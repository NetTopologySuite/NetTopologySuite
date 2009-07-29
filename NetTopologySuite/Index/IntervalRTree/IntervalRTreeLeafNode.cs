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
    public class IntervalRTreeLeafNode<TItem> : IntervalRTreeNode<TItem>
        where TItem : IBoundable<Interval>
    {

        ///<summary>
        ///</summary>
        ///<param name="item"></param>
        public IntervalRTreeLeafNode(TItem item)
            :base(item.Bounds, 0)
        {
            base.Add(item);
        }

        protected override Interval ComputeBounds()
        {
            return _bounds;
        }
    }
}
