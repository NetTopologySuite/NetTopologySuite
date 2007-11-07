using System;

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
    public static class QuadrantOp
    {
        /// <summary> 
        /// Returns the quadrant of a directed line segment (specified as x and y
        /// displacements, which cannot both be 0).
        /// </summary>
        public static Int32 Quadrant(Double dx, Double dy)
        {
            if (dx == 0.0 && dy == 0.0)
            {
                throw new ArgumentException("Cannot compute the quadrant for point ( " + dx + ", " + dy + " )");
            }

            if (dx >= 0)
            {
                if (dy >= 0)
                {
                    return 0;
                }
                else
                {
                    return 3;
                }
            }
            else
            {
                if (dy >= 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        /// <summary> 
        /// Returns the quadrant of a directed line segment from p0 to p1.
        /// </summary>
        public static Int32 Quadrant(ICoordinate p0, ICoordinate p1)
        {
            Double dx = p1.X - p0.X;
            Double dy = p1.Y - p0.Y;
            if (dx == 0.0 && dy == 0.0)
            {
                throw new ArgumentException("Cannot compute the quadrant for two identical points " + p0);
            }
            return Quadrant(dx, dy);
        }

        /// <summary>
        /// Returns true if the quadrants are 1 and 3, or 2 and 4.
        /// </summary>
        public static Boolean IsOpposite(Int32 quad1, Int32 quad2)
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
        public static Int32 CommonHalfPlane(Int32 quad1, Int32 quad2)
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
                return -1;
            }

            Int32 min = (quad1 < quad2) ? quad1 : quad2;
            Int32 max = (quad1 > quad2) ? quad1 : quad2;

            // for this one case, the righthand plane is NOT the minimum index;
            if (min == 0 && max == 3)
            {
                return 3;
            }

            // in general, the halfplane index is the minimum of the two adjacent quadrants
            return min;
        }

        /// <summary> 
        /// Returns whether the given quadrant lies within the given halfplane (specified
        /// by its right-hand quadrant).
        /// </summary>
        public static Boolean IsInHalfPlane(Int32 quad, Int32 halfPlane)
        {
            if (halfPlane == 3)
            {
                return quad == 3 || quad == 0;
            }
            return quad == halfPlane || quad == halfPlane + 1;
        }

        /// <summary> 
        /// Returns true if the given quadrant is 0 or 1.
        /// </summary>
        public static Boolean IsNorthern(Int32 quad)
        {
            return quad == 0 || quad == 1;
        }
    }
}