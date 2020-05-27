using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// A key for sorting and comparing edges in a noded arrangement.
    /// Relies on the fact that in a correctly noded arrangement
    /// edges are identical (up to direction)
    /// iff they have their first segment in common.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class EdgeKey : IComparable<EdgeKey>
    {


        public static EdgeKey Create(Edge edge)
        {
            return new EdgeKey(edge);
        }

        private double _p0x;
        private double _p0y;
        private double _p1x;
        private double _p1y;

        private EdgeKey(Edge edge)
        {
            InitPoints(edge);
        }

        private void InitPoints(Edge edge)
        {
            bool direction = edge.Direction;
            if (direction)
            {
                Init(edge.GetCoordinate(0),
                    edge.GetCoordinate(1));
            }
            else
            {
                int len = edge.Count;
                Init(edge.GetCoordinate(len - 1),
                    edge.GetCoordinate(len - 2));
            }
        }

        private void Init(Coordinate p0, Coordinate p1)
        {
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
            if (!(o is EdgeKey))
            {
                return false;
            }
            var ek = (EdgeKey)o;
            return _p0x == ek._p0x
                && _p0y == ek._p0y
                && _p1x == ek._p1x
                && _p1y == ek._p1y;
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            //Algorithm from Effective Java by Joshua Bloch
            int result = 17;
            result = 37 * result + GetHashCode(_p0x);
            result = 37 * result + GetHashCode(_p0y);
            result = 37 * result + GetHashCode(_p1x);
            result = 37 * result + GetHashCode(_p1y);
            return result;
        }

        /// <summary>
        /// Computes a hash code for a double value, using the algorithm from
        /// Joshua Bloch's book <i>Effective Java"</i>
        /// </summary>
        /// <param name="x">The value to compute for</param>
        /// <returns>A hashcode for x</returns>
        public static int GetHashCode(double x)
        {
            long f = BitConverter.DoubleToInt64Bits(x);
            return (int)(f ^ (f >> 32));
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
