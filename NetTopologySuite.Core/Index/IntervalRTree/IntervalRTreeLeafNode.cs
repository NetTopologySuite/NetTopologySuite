namespace NetTopologySuite.Index.IntervalRTree
{
    public class IntervalRTreeLeafNode<T> : IntervalRTreeNode<T>
    {
        private readonly T _item;

        public IntervalRTreeLeafNode(double min, double max, T item)
            : base(min, max)
        {
            _item = item;
        }

        public override void Query(double queryMin, double queryMax, IItemVisitor<T> visitor)
        {
            if (!Intersects(queryMin, queryMax))
                return;

            visitor.VisitItem(_item);
        }
    }
}