using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using System;

namespace NetTopologySuite.Coverage
{
    internal sealed class Corner : IComparable<Corner>
    {

        private readonly LinkedLine _edge;
        private readonly int _index;
        private readonly int _prev;
        private readonly int _next;
        private readonly double _area;

        public Corner(LinkedLine edge, int i, double area)
        {
            _edge = edge;
            _index = i;
            _prev = edge.Prev(i);
            _next = edge.Next(i);
            _area = area;
        }

        public bool IsVertex(int index)
        {
            return index == _index
                || index == _prev
                || index == _next;
        }

        public int Index => _index;

        public Coordinate Coordinate => _edge.GetCoordinate(_index);

        public double Area => _area;

        public Coordinate Prev()
        {
            return _edge.GetCoordinate(_prev);
        }

        public Coordinate Next()
        {
            return _edge.GetCoordinate(_next);
        }

        /// <summary>
        /// Orders corners by increasing area
        /// To ensure equal-area corners have a deterministic ordering,
        /// if area is equal then compares corner index.
        /// </summary>
        public int CompareTo(Corner o)
        {
            int res = _area.CompareTo(o._area);
            if (res != 0) return res;
            return _index.CompareTo(o._index);
        }

        public Envelope Envelope
        {
            get
            {
                var pp = _edge.GetCoordinate(_prev);
                var p = _edge.GetCoordinate(_index);
                var pn = _edge.GetCoordinate(_next);
                var env = new Envelope(pp, pn);
                env.ExpandToInclude(p);
                return env;
            }
        }

        public bool IsVertex(Coordinate v)
        {
            if (v.Equals2D(_edge.GetCoordinate(_prev))) return true;
            if (v.Equals2D(_edge.GetCoordinate(_index))) return true;
            if (v.Equals2D(_edge.GetCoordinate(_next))) return true;
            return false;
        }

        public bool IsBaseline(Coordinate p0, Coordinate p1)
        {
            var prev = Prev();
            var next = Next();
            if (prev.Equals2D(p0) && next.Equals2D(p1)) return true;
            if (prev.Equals2D(p1) && next.Equals2D(p0)) return true;
            return false;
        }

        public bool Intersects(Coordinate v)
        {
            var pp = _edge.GetCoordinate(_prev);
            var p = _edge.GetCoordinate(_index);
            var pn = _edge.GetCoordinate(_next);
            return Triangle.Intersects(pp, p, pn, v);
        }

        public bool IsRemoved
            => _edge.Prev(_index) != _prev || _edge.Next(_index) != _next;


        public LineString ToLineString()
        {
            var pp = _edge.GetCoordinate(_prev);
            var p = _edge.GetCoordinate(_index);
            var pn = _edge.GetCoordinate(_next);
            return (new GeometryFactory()).CreateLineString(
                new Coordinate[] { SafeCoord(pp), SafeCoord(p), SafeCoord(pn) });
        }

        public override string ToString()
        {
            return $"CRNR(Area: {Area}, Geom: {ToLineString()})";
            //return ToLineString().ToString();
        }

        private static Coordinate SafeCoord(Coordinate p)
        {
            if (p == null) return new Coordinate(double.NaN, double.NaN);
            return p;
        }
    }
}
