using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// Utility functions for working with quadrants, which are numbered as follows:
    /// <para>
    /// 1 | 0
    /// --+--
    /// 2 | 3
    /// </para>
    /// </summary>
    public static class QuadrantOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Returns the quadrant of a directed line segment (specified as x and y
        /// displacements, which cannot both be 0).
        /// </summary>
        public static Quadrants Quadrant(Double dx, Double dy)
        {
            if (dx == 0.0 && dy == 0.0)
            {
                throw new ArgumentException("Cannot compute the quadrant for point ( " + dx + ", " + dy + " )");
            }

            if (dx >= 0)
            {
                if (dy >= 0)
                {
                    return Quadrants.I;
                }
                else
                {
                    return Quadrants.IV;
                }
            }
            else
            {
                if (dy >= 0)
                {
                    return Quadrants.II;
                }
                else
                {
                    return Quadrants.III;
                }
            }
        }

        /// <summary> 
        /// Returns the <see cref="Quadrants"/> value of the <paramref name="coordinate"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate to compute the quadrant for.</param>
        public static Quadrants Quadrant(TCoordinate coordinate)
        {
            return Quadrant(coordinate[Ordinates.X], coordinate[Ordinates.Y]);
        }
        
        /// <summary> 
        /// Returns the quadrant of a directed line segment.
        /// </summary>
        public static Quadrants Quadrant(Pair<TCoordinate> segment)
        {
            return Quadrant(segment.First, segment.Second);
        }

        /// <summary> 
        /// Returns the quadrant of a directed line segment from p0 to p1.
        /// </summary>
        public static Quadrants Quadrant(TCoordinate p0, TCoordinate p1)
        {
            Double dx = p1[Ordinates.X] - p0[Ordinates.X];
            Double dy = p1[Ordinates.Y] - p0[Ordinates.Y];

            if (dx == 0.0 && dy == 0.0)
            {
                throw new ArgumentException("Cannot compute the quadrant for two identical points " + p0);
            }

            return Quadrant(dx, dy);
        }

        /// <summary>
        /// Returns true if the quadrants are 1 and 3, or 2 and 4.
        /// </summary>
        public static Boolean IsOpposite(Quadrants quad1, Quadrants quad2)
        {
            if (quad1 == quad2)
            {
                return false;
            }

            Int32 diff = (quad1 - quad2 + 4) % 4;

            // if quadrants are not adjacent, they are opposite
            if (diff == 2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the right-hand quadrant of the halfplane defined by the two quadrants,
        /// or -1 if the quadrants are opposite, or the quadrant if they are identical.
        /// </summary>
        public static Quadrants CommonHalfPlane(Quadrants quad1, Quadrants quad2)
        {
            // if quadrants are the same they do not determine a unique common halfplane.
            // Simply return one of the two possibilities
            if (quad1 == quad2)
            {
                return quad1;
            }

            Int32 diff = (quad1 - quad2 + 4) % 4;

            // if quadrants are not adjacent, they do not share a common halfplane
            if (diff == 2)
            {
                return Quadrants.None;
            }

            Quadrants min = (quad1 < quad2) ? quad1 : quad2;
            Quadrants max = (quad1 > quad2) ? quad1 : quad2;

            // for this one case, the righthand plane is NOT the minimum index;
            if (min == Quadrants.I && max == Quadrants.IV)
            {
                return Quadrants.IV;
            }

            // in general, the halfplane index is the minimum of the two adjacent quadrants
            return min;
        }

        /// <summary> 
        /// Returns whether the given quadrant lies within the given halfplane (specified
        /// by its right-hand quadrant).
        /// </summary>
        public static Boolean IsInHalfPlane(Quadrants quad, Quadrants halfPlane)
        {
            if (halfPlane == Quadrants.IV)
            {
                return quad == Quadrants.IV || quad == Quadrants.I;
            }

            return quad == halfPlane || quad == (halfPlane + 1);
        }

        /// <summary> 
        /// Returns true if the given quadrant is <see cref="Quadrants.I"/> 
        /// or <see cref="Quadrants.II"/>.
        /// </summary>
        public static Boolean IsNorthern(Quadrants quad)
        {
            return quad == Quadrants.I || quad == Quadrants.II;
        }
    }
}