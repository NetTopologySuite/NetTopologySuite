using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Index.IntervalRTree
{
    public abstract class IntervalRTreeNode<T>
    {
        public double Min { get; protected set; }
        public double Max { get; protected set; }

        protected IntervalRTreeNode()
        {
            Min = double.PositiveInfinity;
            Max = double.NegativeInfinity;
        }

        protected IntervalRTreeNode(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public abstract void Query(double queryMin, double queryMax, IItemVisitor<T> visitor);

        protected bool Intersects(double queryMin, double queryMax)
        {
            if (Min > queryMax
                    || Max < queryMin)
                return false;
            return true;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return WKTWriter.ToLineString(new Coordinate(Min, 0), new Coordinate(Max, 0));
        }

        public class NodeComparator : IComparer<IntervalRTreeNode<T>>
        {
            public static NodeComparator Instance = new NodeComparator();

            public int Compare(IntervalRTreeNode<T> n1, IntervalRTreeNode<T> n2)
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