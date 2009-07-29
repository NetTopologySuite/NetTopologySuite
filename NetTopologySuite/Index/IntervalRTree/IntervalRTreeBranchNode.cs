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
    public class IntervalRTreeBranchNode<TItem> : IntervalRTreeNode<TItem>
        where TItem : IBoundable<Interval>
    {
        private readonly IntervalRTreeNode<TItem> _node1;
        private readonly IntervalRTreeNode<TItem> _node2;

        ///<summary>
        ///</summary>
        ///<param name="level"></param>
        ///<param name="node1"></param>
        ///<param name="node2"></param>
        public IntervalRTreeBranchNode(int level, IntervalRTreeNode<TItem> node1, IntervalRTreeNode<TItem> node2)
            :base(BuildInterval(node1 ,node2), level)
        {
            base.Add(node1);
            base.Add(node2);

            _node1 = node1;
            _node2 = node2;
        }

        protected override Interval ComputeBounds()
        {
            return BuildInterval(_node1, _node2);
        }

        private static Interval BuildInterval(IntervalRTreeNode<TItem> node1, IntervalRTreeNode<TItem> node2)
        {
            Interval bounds = node1 == null ? Interval.Zero : node1.Bounds;
            bounds = node2 == null ? bounds : bounds.ExpandToInclude(node2.Bounds);
            return bounds;
        }

    }
}
