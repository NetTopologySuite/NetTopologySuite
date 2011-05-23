using System;

namespace GisSharpBlog.NetTopologySuite.Index.IntervalRTree
{
    public class IntervalRTreeBranchNode : IntervalRTreeNode
    {
        private readonly IntervalRTreeNode _node1;
        private readonly IntervalRTreeNode _node2;

        public IntervalRTreeBranchNode(IntervalRTreeNode n1, IntervalRTreeNode n2)
        {
            _node1 = n1;
            _node2 = n2;
            BuildExtent(_node1, _node2);
        }

        private void BuildExtent(IntervalRTreeNode n1, IntervalRTreeNode n2)
        {
            Min = Math.Min(n1.Min, n2.Min);
            Max = Math.Max(n1.Max, n2.Max);
        }

        public override void Query(double queryMin, double queryMax, IItemVisitor visitor)
        {
            if (!Intersects(queryMin, queryMax))
            {
                //			System.out.println("Does NOT Overlap branch: " + this);
                return;
            }
            //		System.out.println("Overlaps branch: " + this);
            if (_node1 != null) _node1.Query(queryMin, queryMax, visitor);
            if (_node2 != null) _node2.Query(queryMin, queryMax, visitor);
        }

    }
}