using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Quadrant values
    /// </summary>
    /// <remarks>
    /// The quadants are numbered as follows:
    /// <para>
    /// <code>
    /// 1 - NW | 0 - NE
    /// -------+-------
    /// 2 - SW | 3 - SE
    /// </code>
    /// </para>
    /// </remarks>
    public enum Quadrant
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// North-East
        /// </summary>
        NE = 0,

        /// <summary>
        /// North-West
        /// </summary>
        NW = 1,

        /// <summary>
        /// South-West
        /// </summary>
        SW = 2,

        /// <summary>
        /// South-East
        /// </summary>
        SE = 3
    }

    /// <summary>
    /// Utility functions for working with quadrants, which are numbered as follows:
    /// <para>
    /// <code>
    /// 1 - NW | 0 - NE
    /// -------+-------
    /// 2 - SW | 3 - SE
    /// </code>
    /// </para>
    /// </summary>
    public static class QuadrantExtensions
    {
        /// <summary>
        /// Returns the quadrant of a directed line segment (specified as x and y
        /// displacements, which cannot both be 0).
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <exception cref="ArgumentException">If the displacements are both 0</exception>
        public static Quadrant Quadrant(double dx, double dy)
        {
            if (dx == 0.0 && dy == 0.0)
                throw new ArgumentException("Cannot compute the quadrant for point ( "+ dx + ", " + dy + " )" );
            if (dx >= 0.0)
            {
                if (dy >= 0.0)
                     return Geometries.Quadrant.NE;
                return Geometries.Quadrant.SE;
            }
            if (dy >= 0.0)
                return Geometries.Quadrant.NW;
            return Geometries.Quadrant.SW;
        }

        /// <summary>
        /// Returns the quadrant of a directed line segment from p0 to p1.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <exception cref="ArgumentException"> if the points are equal</exception>
        public static Quadrant Quadrant(Coordinate p0, Coordinate p1)
        {
            if (p1.X == p0.X && p1.Y == p0.Y)
                throw new ArgumentException("Cannot compute the quadrant for two identical points " + p0);

            if (p1.X >= p0.X)
            {
                if (p1.Y >= p0.Y)
                    return Geometries.Quadrant.NE;
                return Geometries.Quadrant.SE;
            }
            if (p1.Y >= p0.Y)
                return Geometries.Quadrant.NW;
            return Geometries.Quadrant.SW;
        }

        /// <summary>
        /// Returns true if the quadrants are 1 and 3, or 2 and 4.
        /// </summary>
        /// <param name="quad1"></param>
        /// <param name="quad2"></param>
        public static bool IsOpposite(Quadrant quad1, Quadrant quad2)
        {
            if (quad1 == quad2)
                return false;
            int diff = (quad1 - quad2 + 4) % 4;
            // if quadrants are not adjacent, they are opposite
            if (diff == 2)
                return true;
            return false;
        }

        /// <summary>
        /// Returns the right-hand quadrant of the halfplane defined by the two quadrants,
        /// or -1 if the quadrants are opposite, or the quadrant if they are identical.
        /// </summary>
        /// <param name="quad1"></param>
        /// <param name="quad2"></param>
        public static Quadrant CommonHalfPlane(Quadrant quad1, Quadrant quad2)
        {
            // if quadrants are the same they do not determine a unique common halfplane.
            // Simply return one of the two possibilities
            if (quad1 == quad2)
                return quad1;
            int diff = (quad1 - quad2 + 4) % 4;
            // if quadrants are not adjacent, they do not share a common halfplane
            if (diff == 2)
                return Geometries.Quadrant.Undefined;

            var min = (quad1 < quad2) ? quad1 : quad2;
            var max = (quad1 > quad2) ? quad1 : quad2;
            // for this one case, the righthand plane is NOT the minimum index;
            if (min == 0 && max == Geometries.Quadrant.SW)
                return Geometries.Quadrant.SW;
            // in general, the halfplane index is the minimum of the two adjacent quadrants
            return min;
        }

        /// <summary>
        /// Returns whether the given quadrant lies within the given halfplane (specified
        /// by its right-hand quadrant).
        /// </summary>
        /// <param name="quad"></param>
        /// <param name="halfPlane"></param>
        public static bool IsInHalfPlane(Quadrant quad, Quadrant halfPlane)
        {
            if (halfPlane == Geometries.Quadrant.SE)
                return quad == Geometries.Quadrant.SE || quad == Geometries.Quadrant.SW;
            return quad == halfPlane || quad == halfPlane + 1;
        }

        /// <summary>
        /// Returns true if the given quadrant is 0 or 1.
        /// </summary>
        /// <param name="quad"></param>
        public static bool IsNorthern(Quadrant quad)
        {
            return quad == Geometries.Quadrant.NE || quad == Geometries.Quadrant.NW;
        }
    }
}
