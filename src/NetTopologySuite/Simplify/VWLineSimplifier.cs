using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a linestring (sequence of points) using the
    /// Visvalingam-Whyatt algorithm.
    /// The Visvalingam-Whyatt algorithm simplifies geometry
    /// by removing vertices while trying to minimize the area changed.
    /// </summary>
    /// <version>1.7</version>
    public class VWLineSimplifier
    {
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance)
        {
            var simp = new VWLineSimplifier(pts, distanceTolerance);
            return simp.Simplify();
        }

        private readonly Coordinate[] _pts;
        private readonly double _tolerance;

        public VWLineSimplifier(Coordinate[] pts, double distanceTolerance)
        {
            _pts = pts;
            _tolerance = distanceTolerance * distanceTolerance;
        }

        public Coordinate[] Simplify()
        {
            // Put every coordinate index into an AlternativePriorityQueueNode.
            // this array doubles as a way to check which coordinates are
            // excluded.
            var nodes = new PriorityQueueNode<IndexWithArea, int>[_pts.Length];

            // Store the coordinates, with their effective areas, in a min heap.
            // Visvalingam-Whyatt repeatedly deletes the coordinate with the
            // min effective area so a structure with efficient delete-min is
            // ideal.
            var queue = new AlternativePriorityQueue<IndexWithArea, int>(_pts.Length);

            // First and last coordinates are included automatically.
            for (int i = 1; i < _pts.Length - 1; i++)
            {
                double area = Triangle.Area(_pts[i - 1], _pts[i], _pts[i + 1]);

                nodes[i] = new PriorityQueueNode<IndexWithArea, int>(i);
                queue.Enqueue(nodes[i], new IndexWithArea(i, area));
            }

            // First = the coord that contributes the minimum effective area.
            while (queue.Head != null &&
                   queue.Head.Priority.Area <= _tolerance)
            {
                var node = queue.Dequeue();

                // Signal that this node has been deleted.
                nodes[node.Data] = null;

                // Find index of the not-yet-deleted coord before the deleted
                // one.  This could be the first coord in the sequence.
                int prev = node.Data - 1;
                while (prev > 0 && nodes[prev] == null) { --prev; }

                // Find index of the not-yet-deleted coord after the deleted
                // one.  This could be the last coord in the sequence.
                int next = node.Data + 1;
                while (next < _pts.Length - 1 && nodes[next] == null) { ++next; }

                // If the previous coord is not the first one, then its
                // effective area has changed.  For example:
                // 0  1  2  3  4
                //       ^--------- delete this
                // 0  1     3  4
                // 1's effective area was based on the "0 1 2" triangle.
                // Now, it should be based on the "0 1 3" triangle.
                // This means we take the triangle of curr-2, curr-1, curr+1.
                if (prev > 0 &&
                    queue.Contains(nodes[prev]))
                {
                    int prev2 = prev - 1;
                    while (prev2 > 0 && nodes[prev2] == null) { --prev2; }

                    double area = Triangle.Area(_pts[prev2], _pts[prev], _pts[next]);
                    queue.ChangePriority(nodes[prev], new IndexWithArea(prev, area));
                }

                // Same idea as what we did above for prev.
                // 3's effective area was based on the "2 3 4" triangle.
                // Now, it should be based on the "1 3 4" triangle.
                // This means we take the triangle of curr-1, curr+1, curr+2.
                if (next < _pts.Length - 1 &&
                    queue.Contains(nodes[next]))
                {
                    int next2 = next + 1;
                    while (next2 < _pts.Length - 1 && nodes[next2] == null) { ++next2; }

                    double area = Triangle.Area(_pts[prev], _pts[next], _pts[next2]);
                    queue.ChangePriority(nodes[next], new IndexWithArea(next, area));
                }
            }

            // Consolidate the results.  For consistency, and because it's a
            // good idea anyway, we want to make sure we don't output the same
            // coord multiple times in a row, so we pass "false" for the second
            // parameter to "Add".
            var list = new CoordinateList(nodes.Length);

            list.Add(_pts[0], false);
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                {
                    // This was one we deleted.
                    continue;
                }

                list.Add(_pts[i], false);
            }

            // Special-case: we want to make sure that we don't return a 1-
            // element array, as the outputs of this are typically used to
            // build an LineString.  So if there's just one coordinate, we
            // output it twice, mainly just to ensure equivalence to JTS / old
            // NTS.
            list.Add(_pts[_pts.Length - 1], list.Count == 1);

            return list.ToArray();
        }

        private struct IndexWithArea : IComparable<IndexWithArea>
        {
            private int index;
            private double area;

            public IndexWithArea(int index, double area)
            {
                this.index = index;
                this.area = area;
            }

            ////public int Index { get { return this.index; } }
            public double Area => this.area;

            public int CompareTo(IndexWithArea other)
            {
                int areaComparison = this.area.CompareTo(other.area);

                // Do the index comparison if areas are equal, to ensure
                // equivalence with JTS / old NTS.
                return areaComparison == 0
                           ? this.index.CompareTo(other.index)
                           : areaComparison;
            }
        }
    }
}
