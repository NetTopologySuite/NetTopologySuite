using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// A key for sorting and comparing edges in a noded arrangement.
    /// Relies on the fact that in a correctly noded arrangement
    /// edges are identical (up to direction)
    /// if they have their first segment in common.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class EdgeKey : IComparable<EdgeKey>
    {
        public static EdgeKey Create(Edge edge)
        {
            return new EdgeKey(edge);
        }

        private readonly double _p0x;
        private readonly double _p0y;
        private readonly double _p1x;
        private readonly double _p1y;

        
        private EdgeKey(Edge edge)
        {
            bool direction = edge.Direction;
            Coordinate p0, p1;
            if (direction)
            {
                p0 = edge.GetCoordinate(0);
                p1 = edge.GetCoordinate(1);
            }
            else
            {
                int idx = edge.Count - 1;
                p0 = edge.GetCoordinate(idx--);
                p1 = edge.GetCoordinate(idx);
            }
            _p0x = p0.X;
            _p0y = p0.Y;
            _p1x = p1.X;
            _p1y = p1.Y;
        }

        public int CompareTo(EdgeKey ek)
        {
            if (_p0x < ek._p0x) return -1;
            if (_p0x > ek._p0x) return 1;
            if (_p0y < ek._p0y) return -1;
            if (_p0y > ek._p0y) return 1;
            // first points are equal, compare second
            if (_p1x < ek._p1x) return -1;
            if (_p1x > ek._p1x) return 1;
            if (_p1y < ek._p1y) return -1;
            if (_p1y > ek._p1y) return 1;
            return 0;
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object o)
        {
            return o is EdgeKey ek
                && _p0x == ek._p0x
                && _p0y == ek._p0y
                && _p1x == ek._p1x
                && _p1y == ek._p1y;
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            //Algorithm from Effective Java by Joshua Bloch
            int result = 17;
            result = 37 * result + _p0x.GetHashCode();
            result = 37 * result + _p0y.GetHashCode();
            result = 37 * result + _p1x.GetHashCode();
            result = 37 * result + _p1y.GetHashCode();
            return unchecked(result);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return "EdgeKey(" + Format(_p0x, _p0y)
              + ", " + Format(_p1x, _p1y) + ")";
        }

        private static string Format(double x, double y)
        {
            return OrdinateFormat.Default.Format(x) + " " + OrdinateFormat.Default.Format(y);
        }
    }
}
