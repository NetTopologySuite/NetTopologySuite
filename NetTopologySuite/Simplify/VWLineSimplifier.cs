using System;

using GeoAPI.Geometries;
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
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance) => Simplify(pts, distanceTolerance, isRing: false);

        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance, bool isRing)
        {
            VWLineSimplifier simp = new VWLineSimplifier(pts, distanceTolerance, isRing);
            return simp.Simplify();
        }

        private readonly Coordinate[] _pts;
        private readonly double _tolerance;
        private readonly bool _isRing;

        public VWLineSimplifier(Coordinate[] pts, double distanceTolerance)
            : this(pts, distanceTolerance, isRing: false)
        {
        }

        public VWLineSimplifier(Coordinate[] pts, double distanceTolerance, bool isRing)
        {
            _pts = pts;
            _tolerance = distanceTolerance * distanceTolerance;
            _isRing = isRing;
            if (isRing)
            {
                if (pts.Length < 4 || !pts[0].Equals(pts[pts.Length - 1]))
                {
                    throw new ArgumentException("Ring must be closed and must contain at least 4 points", nameof(pts));
                }
            }
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

            for (int i = 1; i < _pts.Length - 1; i++)
            {
                double area = Triangle.Area(_pts[i - 1], _pts[i], _pts[i + 1]);

                nodes[i] = new PriorityQueueNode<IndexWithArea, int>(i);
                queue.Enqueue(nodes[i], new IndexWithArea(i, area));
            }

            if (_isRing)
            {
                // we could maybe-sorta save 1 round of the algorithm by taking
                // advantage of the fact that the beginning and end points are
                // identical, but this is cleaner for now and it's probably not
                // going to be meaningfully faster to do so.
                nodes[0] = new PriorityQueueNode<IndexWithArea, int>(0);
                queue.Enqueue(nodes[0], new IndexWithArea(0, Triangle.Area(_pts[_pts.Length - 1], _pts[0], _pts[1])));

                nodes[nodes.Length - 1] = new PriorityQueueNode<IndexWithArea, int>(nodes.Length - 1);
                queue.Enqueue(nodes[nodes.Length - 1], new IndexWithArea(0, Triangle.Area(_pts[_pts.Length - 2], _pts[_pts.Length - 1], _pts[0])));
            }

            // First = the coord that contributes the minimum effective area.
            while (queue.Head != null &&
                   queue.Head.Priority.Area <= _tolerance)
            {
                if (_isRing && queue.Count < 4)
                {
                    // we're left with just a single triangle, and it's below
                    // tolerance... just clear the rest now.  don't do this in
                    // the non-ring case, for compatibility.
                    while (queue.Head != null)
                    {
                        nodes[queue.Dequeue().Data] = null;
                    }

                    break;
                }

                var node = queue.Dequeue();

                // Signal that this node has been deleted.
                nodes[node.Data] = null;

                // Find index of the not-yet-deleted coord before the deleted
                // one.  This could be the first coord in the sequence.
                // Find index of the not-yet-deleted coord after the deleted
                // one.  This could be the last coord in the sequence.
                int prev, prev2, next, next2;
                if (_isRing)
                {
                    prev = PrevInRing(node.Data);
                    prev2 = PrevInRing(prev);
                    next = NextInRing(node.Data);
                    next2 = NextInRing(next);
                }
                else
                {
                    prev = node.Data - 1;
                    while (prev > 0 && nodes[prev] == null) { --prev; }

                    prev2 = prev - 1;
                    while (prev2 > 0 && nodes[prev2] == null) { --prev2; }

                    next = node.Data + 1;
                    while (next < nodes.Length - 1 && nodes[next] == null) { ++next; }

                    next2 = next + 1;
                    while (next2 < nodes.Length - 1 && nodes[next2] == null) { ++next2; }
                }

                // If the previous coord is not the first one, then its
                // effective area has changed.  For example:
                // 0  1  2  3  4
                //       ^--------- delete this
                // 0  1     3  4
                // 1's effective area was based on the "0 1 2" triangle.
                // Now, it should be based on the "0 1 3" triangle.
                // This means we take the triangle of curr-2, curr-1, curr+1.
                if (prev2 >= 0)
                {
                    double area = Triangle.Area(_pts[prev2], _pts[prev], _pts[next]);
                    queue.ChangePriority(nodes[prev], new IndexWithArea(prev, area));
                }

                // Same idea as what we did above for prev.
                // 3's effective area was based on the "2 3 4" triangle.
                // Now, it should be based on the "1 3 4" triangle.
                // This means we take the triangle of curr-1, curr+1, curr+2.
                if (next2 < nodes.Length)
                {
                    double area = Triangle.Area(_pts[prev], _pts[next], _pts[next2]);
                    queue.ChangePriority(nodes[next], new IndexWithArea(next, area));
                }
            }

            // Consolidate the results.  For consistency, and because it's a
            // good idea anyway, we want to make sure we don't output the same
            // coord multiple times in a row, so we pass "false" for the second
            // parameter to "Add".
            CoordinateList list = new CoordinateList();

            // non-ring versions always keep endpoints
            if (!_isRing)
            {
                list.Add(_pts[0], false);
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                // only add ones we didn't delete
                if (nodes[i] != null)
                {
                    list.Add(_pts[i], false);
                }
            }

            if (_isRing)
            {
                // Special-case: rings need to be closed.
                if (list.Count > 0)
                {
                    list.Add(list[0], true);
                }
            }
            else
            {
                // Special-case: we want to make sure that we don't return a 1-
                // element array, as the outputs of this are typically used to
                // build an ILineString.  So if there's just one coordinate, we
                // output it twice, mainly just to ensure equivalence to JTS /
                // old NTS.
                list.Add(_pts[_pts.Length - 1], list.Count == 1);
            }

            return list.ToArray();

            int PrevInRing(int idx)
            {
                do
                {
                    if (idx == 0)
                    {
                        idx = nodes.Length;
                    }
                }
                while (nodes[--idx] == null);
                return idx;
            }

            int NextInRing(int idx)
            {
                do
                {
                    if (idx == nodes.Length - 1)
                    {
                        idx = -1;
                    }
                }
                while (nodes[++idx] == null);
                return idx;
            }
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
            public double Area { get { return this.area; } }

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