using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Elevation
{
    internal static class ZInterpolate
    {
        public static Coordinate CopyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return CopyWithZ(p, zGetOrInterpolate(p, p1, p2));
        }

        public static Coordinate CopyWithZ(Coordinate p, double z)
        {
            Coordinate res;
            if (double.IsNaN(z))
                res = p.Copy();
            else
                res = new CoordinateZ(p) { Z = z };

            return res;
        }

        /*
        * Gets the Z value of the first argument if present, 
        * otherwise the value of the second argument.
        * 
        * @param p a coordinate, possibly with Z
        * @param q a coordinate, possibly with Z
        * @return the Z value if present
        */
        public static double zGet(Coordinate p, Coordinate q)
        {
            double z = p.Z;
            if (double.IsNaN(z))
            {
                z = q.Z; // may be NaN
            }
            return z;
        }

        /// <summary>
        /// Gets the Z value of a coordinate if present, or
        /// interpolates it from the segment it lies on.
        /// If the segment Z values are not fully populate
        /// NaN is returned.
        /// </summary>
        /// <param name="p">A coordinate, possibly with Z</param>
        /// <param name="p1">A segment endpoint, possibly with Z</param>
        /// <param name="p2">A segment endpoint, possibly with Z</param>
        /// <returns>The extracted or interpolated Z value (may be NaN)</returns>
        public static double zGetOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            double z = p.Z;
            if (!double.IsNaN(z))
                return z;
            return zInterpolate(p, p1, p2); // may be NaN
        }

        /// <summary>
        /// Interpolates a Z value for a point along
        /// a line segment between two points.
        /// The Z value of the interpolation point (if any) is ignored.
        /// If either segment point is missing Z,
        /// returns NaN.
        /// </summary>
        /// <param name="p">A coordinate, possibly with Z</param>
        /// <param name="p1">A segment endpoint, possibly with Z</param>
        /// <param name="p2">A segment endpoint, possibly with Z</param>
        /// <returns>The extracted or interpolated Z value (may be NaN)</returns>
        public static double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            double p1z = p1.Z;
            double p2z = p2.Z;
            if (double.IsNaN(p1z))
            {
                return p2z; // may be NaN
            }
            if (double.IsNaN(p2z))
            {
                return p1z; // may be NaN
            }
            if (p.Equals2D(p1))
            {
                return p1z; // not NaN
            }
            if (p.Equals2D(p2))
            {
                return p2z; // not NaN
            }
            double dz = p2z - p1z;
            if (dz == 0.0)
            {
                return p1z;
            }
            // interpolate Z from distance of p along p1-p2
            double dx = (p2.X - p1.X);
            double dy = (p2.Y - p1.Y);
            // seg has non-zero length since p1 < p < p2 
            double seglen = (dx * dx + dy * dy);
            double xoff = (p.X - p1.X);
            double yoff = (p.Y - p1.Y);
            double plen = (xoff * xoff + yoff * yoff);
            double frac = Math.Sqrt(plen / seglen);
            double zoff = dz * frac;
            double zInterpolated = p1z + zoff;
            return zInterpolated;
        }

        /// <summary>
        /// Interpolates a Z value for a point along
        /// two line segments and computes their average.
        /// The Z value of the interpolation point (if any) is ignored.
        /// If one segment point is missing Z that segment is ignored
        /// if both segments are missing Z, returns NaN.
        /// </summary>
        /// <param name="p">A coordinate</param>
        /// <param name="p1">A segment endpoint, possibly with Z</param>
        /// <param name="p2">A segment endpoint, possibly with Z</param>
        /// <param name="q1">A segment endpoint, possibly with Z</param>
        /// <param name="q2">A segment endpoint, possibly with Z</param>
        /// <returns>The averaged interpolated Z value (may be NaN)</returns>    
        public static double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            double zp = zInterpolate(p, p1, p2);
            double zq = zInterpolate(p, q1, q2);
            if (double.IsNaN(zp))
            {
                return zq; // may be NaN
            }
            if (double.IsNaN(zq))
            {
                return zp; // may be NaN
            }
            // both Zs have values, so average them
            return (zp + zq) / 2.0;
        }
    }
}