using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Mathematics
{
    /// <summary>
    /// Functions for performing vector mathematics.
    /// </summary>
    public static class VectorMath
    {
        /// <summary>
        /// Computes the normal vector to the triangle p0-p1-p2. In order to compute the normal each
        /// triangle coordinate must have a Z value. If this is not the case, the returned Coordinate
        /// will have NaN values. The returned vector has unit length.
        /// </summary>
        /// <param name="p0">A point</param>
        /// <param name="p1">A point</param>
        /// <param name="p2">A point</param>
        /// <returns>The normal vector to the triangle <paramref name="p0"/>-<paramref name="p1"/>-<paramref name="p2"/></returns>
        public static Coordinate NormalToTriangle(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            var v1 = new CoordinateZ(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v2 = new CoordinateZ(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            var cp = CrossProduct(v1, v2);
            Normalize(cp);
            return cp;
        }

        /// <summary>
        /// Normalizes the vector <paramref name="v"/>
        /// </summary>
        /// <param name="v">The normalized <paramref name="v"/></param>
        public static void Normalize(Coordinate v)
        {
            double absVal = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            v.X /= absVal;
            v.Y /= absVal;
            if (v is CoordinateZ)
            {
                v.Z /= absVal;
            }
        }

        /// <summary>
        /// Computes the cross product of <paramref name="v1"/> and <paramref name="v2"/>
        /// </summary>
        /// <param name="v1">A vector</param>
        /// <param name="v2">A vector</param>
        /// <returns>The cross product of <paramref name="v1"/> and <paramref name="v2"/></returns>
        public static Coordinate CrossProduct(Coordinate v1, Coordinate v2)
        {
            double x = Det(v1.Y, v1.Z, v2.Y, v2.Z);
            double y = -Det(v1.X, v1.Z, v2.X, v2.Z);
            double z = Det(v1.X, v1.Y, v2.X, v2.Y);
            return new CoordinateZ(x, y, z);
        }

        /// <summary>
        /// Computes the dot product of <paramref name="v1"/> and <paramref name="v2"/>
        /// </summary>
        /// <param name="v1">A vector</param>
        /// <param name="v2">A vector</param>
        /// <returns>The dot product of <paramref name="v1"/> and <paramref name="v2"/></returns>
        public static double DotProduct(Coordinate v1, Coordinate v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Computes the determinant of a 2x2 matrix
        /// </summary>
        /// <param name="a1">The m[0,0] value</param>
        /// <param name="a2">The m[0,1] value</param>
        /// <param name="b1">The m[1,0] value</param>
        /// <param name="b2">The m[1,1] value</param>
        /// <returns>The determinant</returns>
        public static double Det(double a1, double a2, double b1, double b2)
        {
            return (a1 * b2) - (a2 * b1);
        }
    }
}
