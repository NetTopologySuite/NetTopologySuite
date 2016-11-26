using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    ///     Simplifies a linestring (sequence of points) using the
    ///     Visvalingam-Whyatt algorithm.
    ///     The Visvalingam-Whyatt algorithm simplifies geometry
    ///     by removing vertices while trying to minimize the area changed.
    /// </summary>
    /// <version>1.7</version>
    public class OldVWLineSimplifier
    {
        private readonly Coordinate[] _pts;
        private readonly double _tolerance;

        public OldVWLineSimplifier(Coordinate[] pts, double distanceTolerance)
        {
            _pts = pts;
            _tolerance = distanceTolerance*distanceTolerance;
        }

        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance)
        {
            var simp = new OldVWLineSimplifier(pts, distanceTolerance);
            return simp.Simplify();
        }

        public Coordinate[] Simplify()
        {
            var vwLine = VWVertex.BuildLine(_pts);
            var minArea = _tolerance;
            do
            {
                minArea = SimplifyVertex(vwLine);
            } while (minArea < _tolerance);
            var simp = vwLine.GetCoordinates();
            // ensure computed value is a valid line
            if (simp.Length >= 2)
                return simp;
            return new[] {simp[0], new Coordinate(simp[0])};
        }

        private double SimplifyVertex(VWVertex vwLine)
        {
            // Scan vertices in line and remove the one with smallest effective area.
            // TODO: use an appropriate data structure to optimize finding the smallest area vertex
            var curr = vwLine;
            var minArea = curr.GetArea();
            VWVertex minVertex = null;
            while (curr != null)
            {
                var area = curr.GetArea();
                if (area < minArea)
                {
                    minArea = area;
                    minVertex = curr;
                }
                curr = curr.Next;
            }
            if ((minVertex != null) && (minArea < _tolerance))
                minVertex.Remove();
            if (!vwLine.IsLive)
                return -1;
            return minArea;
        }

        internal class VWVertex
        {
            private const double MaxArea = double.MaxValue;

            private readonly Coordinate _pt;
            private double _area = MaxArea;

            public VWVertex(Coordinate pt)
            {
                _pt = pt;
            }

            public VWVertex Prev { get; set; }

            public VWVertex Next { get; set; }

            public bool IsLive { get; private set; } = true;

            public static VWVertex BuildLine(Coordinate[] pts)
            {
                VWVertex first = null;
                VWVertex prev = null;
                foreach (var c in pts)
                {
                    var v = new VWVertex(c);
                    if (first == null)
                        first = v;
                    v.Prev = prev;
                    if (prev != null)
                    {
                        prev.Next = v;
                        prev.UpdateArea();
                    }
                    prev = v;
                }
                return first;
            }

            public void UpdateArea()
            {
                if ((Prev == null) || (Next == null))
                {
                    _area = MaxArea;
                    return;
                }
                var d = Triangle.Area(Prev._pt, _pt, Next._pt);
                _area = Math.Abs(d);
            }

            public double GetArea()
            {
                return _area;
            }

            public VWVertex Remove()
            {
                var tmpPrev = Prev;
                var tmpNext = Next;
                VWVertex result = null;
                if (Prev != null)
                {
                    Prev.Next = tmpNext;
                    Prev.UpdateArea();
                    result = Prev;
                }
                if (Next != null)
                {
                    Next.Prev = tmpPrev;
                    Next.UpdateArea();
                    if (result == null)
                        result = Next;
                }
                IsLive = false;
                return result;
            }

            public Coordinate[] GetCoordinates()
            {
                var coords = new CoordinateList();
                var curr = this;
                do
                {
                    coords.Add(curr._pt, false);
                    curr = curr.Next;
                } while (curr != null);
                return coords.ToCoordinateArray();
            }
        }
    }
}