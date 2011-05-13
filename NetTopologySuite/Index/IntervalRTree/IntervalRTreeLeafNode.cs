using System;

namespace GisSharpBlog.NetTopologySuite.Index.IntervalRTree
{
    public class IntervalRTreeLeafNode : IntervalRTreeNode
    {
        private readonly Object _item;

        public IntervalRTreeLeafNode(double min, double max, Object item)
            : base(min, max)
        {
            _item = item;
        }

        public override void Query(double queryMin, double queryMax, IItemVisitor visitor)
        {
            if (!Intersects(queryMin, queryMax))
                return;

            visitor.VisitItem(_item);
        }
    }
}