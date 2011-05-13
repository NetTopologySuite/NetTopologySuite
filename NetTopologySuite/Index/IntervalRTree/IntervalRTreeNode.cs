using System;
using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Index.IntervalRTree
{
    public abstract class IntervalRTreeNode
    {
        public double Min { get; protected set; }
        public double Max { get; protected set; }

        protected IntervalRTreeNode()
        {
            Min = Double.PositiveInfinity;
            Max = Double.NegativeInfinity;
        }

        protected IntervalRTreeNode(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public abstract void Query(double queryMin, double queryMax, IItemVisitor visitor);

        protected bool Intersects(double queryMin, double queryMax)
        {
            if (Min > queryMax
                    || Max < queryMin)
                return false;
            return true;
        }

        public override String ToString()
        {
            return WKTWriter.ToLineString(new Coordinate(Min, 0), new Coordinate(Max, 0));
        }

        public class NodeComparator : IComparer<IntervalRTreeNode>
        {
            public static NodeComparator Instance = new NodeComparator();

            public int Compare(IntervalRTreeNode n1, IntervalRTreeNode n2)
            {
                double mid1 = (n1.Min + n1.Max) / 2;
                double mid2 = (n2.Min + n2.Max) / 2;
                if (mid1 < mid2) return -1;
                if (mid1 > mid2) return 1;
                return 0;
            }
        }

    }
}