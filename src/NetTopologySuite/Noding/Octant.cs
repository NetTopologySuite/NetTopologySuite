using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Octants in the Cartesian plane.
    /// Octants are numbered as follows:
    ///  <para>
    ///   \2|1/
    ///  3 \|/ 0
    ///  ---+--
    ///  4 /|\ 7
    ///   /5|6\
    /// </para>
    ///  If line segments lie along a coordinate axis, the octant is the lower of the two possible values.
    /// </summary>
    public enum Octants
    {
        /// <summary>
        ///
        /// </summary>
        Null = -1,

        /// <summary>
        ///
        /// </summary>
        Zero    = 0,

        /// <summary>
        ///
        /// </summary>
        One     = 1,

        /// <summary>
        ///
        /// </summary>
        Two     = 2,

        /// <summary>
        ///
        /// </summary>
        Three   = 3,

        /// <summary>
        ///
        /// </summary>
        Four    = 4,

        /// <summary>
        ///
        /// </summary>
        Five    = 5,

        /// <summary>
        ///
        /// </summary>
        Six     = 6,

        /// <summary>
        ///
        /// </summary>
        Seven   = 7,
    }

    /// <summary>
    ///  Methods for computing and working with <see cref="Octants"/> of the Cartesian plane.
    /// </summary>
    public static class Octant
    {
        /// <summary>
        /// Returns the octant of a directed line segment (specified as x and y
        /// displacements, which cannot both be 0).
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public static Octants GetOctant(double dx, double dy)
        {
            if (dx == 0.0 && dy == 0.0)
                throw new ArgumentException("Cannot compute the octant for point ( " + dx + ", " + dy + " )");

            double adx = Math.Abs(dx);
            double ady = Math.Abs(dy);

            if (dx >= 0)
            {
                if (dy >= 0)
                {
                    if (adx >= ady)
                        return Octants.Zero;
                    else
                        return Octants.One;
                }
                else // dy < 0
                {
                    if (adx >= ady)
                        return Octants.Seven;
                    else
                        return Octants.Six;
                }
            }
            else // dx < 0
            {
                if (dy >= 0)
                {
                    if (adx >= ady)
                        return Octants.Three;
                    else
                        return Octants.Two;
                }
                else // dy < 0
                {
                    if (adx >= ady)
                        return Octants.Four;
                    else
                        return Octants.Five;
                }
            }
        }

        /// <summary>
        /// Returns the octant of a directed line segment from p0 to p1.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static Octants GetOctant(Coordinate p0, Coordinate p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            if (dx == 0.0 && dy == 0.0)
                throw new ArgumentException("Cannot compute the octant for two identical points " + p0);
            return GetOctant(dx, dy);
        }
    }
}
