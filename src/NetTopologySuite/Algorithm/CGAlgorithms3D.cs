using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Basic computational geometry algorithms
    /// for geometry and coordinates defined in 3-dimensional Cartesian space.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class CGAlgorithms3D
    {
        /// <summary>
        /// Computes the distance between the points <paramref name="p0"/> and
        /// <paramref name="p1"/> in 3D space
        /// </summary>
        /// <param name="p0">The first point</param>
        /// <param name="p1">The second point</param>
        /// <returns>The distance between the two points</returns>
        public static double Distance(Coordinate p0, Coordinate p1)
        {
            // default to 2D distance if either Z is not set
            if (double.IsNaN(p0.Z) || double.IsNaN(p1.Z))
                return p0.Distance(p1);

            double dx = p0.X - p1.X;
            double dy = p0.Y - p1.Y;
            double dz = p0.Z - p1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Computes the distance between the point <paramref name="p"/> and the
        /// segment from <paramref name="A"/> to <paramref name="B"/> in 3D space
        /// </summary>
        /// <param name="p">The point</param>
        /// <param name="A">The start point of the segment</param>
        /// <param name="B">The end point of the segment</param>
        /// <returns></returns>
        public static double DistancePointSegment(Coordinate p,
                Coordinate A, Coordinate B)
        {
            // if start = end, then just compute distance to one of the endpoints
            if (A.Equals2D(B) && A.Z.Equals(B.Z))
                return Distance(p, A);

            // otherwise use comp.graphics.algorithms Frequently Asked Questions method
            /*
             * (1) r = AC dot AB
             *         ---------
             *         ||AB||^2
             *
             * r has the following meaning:
             *   r=0 P = A
             *   r=1 P = B
             *   r<0 P is on the backward extension of AB
             *   r>1 P is on the forward extension of AB
             *   0<r<1 P is interior to AB
             */

            double len2 = (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y) + (B.Z - A.Z) * (B.Z - A.Z);
            if (double.IsNaN(len2))
                throw new ArgumentException("Ordinates must not be NaN");
            double r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y) + (p.Z - A.Z) * (B.Z - A.Z))
                / len2;

            if (r <= 0.0)
                return Distance(p, A);
            if (r >= 1.0)
                return Distance(p, B);

            // compute closest point q on line segment
            double qx = A.X + r * (B.X - A.X);
            double qy = A.Y + r * (B.Y - A.Y);
            double qz = A.Z + r * (B.Z - A.Z);
            // result is distance from p to q
            double dx = p.X - qx;
            double dy = p.Y - qy;
            double dz = p.Z - qz;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>Computes the distance between two 3D segments.</summary>
        /// <param name="A">The start point of the first segment</param>
        /// <param name="B">The end point of the first segment</param>
        /// <param name="C">The start point of the second segment</param>
        /// <param name="D">The end point of the second segment</param>
        /// <returns>The distance between the segments</returns>
        public static double DistanceSegmentSegment(
                Coordinate A, Coordinate B, Coordinate C, Coordinate D)
        {
            /*
             * This calculation is susceptible to round off errors when
             * passed large ordinate values.
             * It may be possible to improve this by using DD arithmetic.
             */
            if (A.Equals2D(B) && A.Z.Equals(B.Z))
                return DistancePointSegment(A, C, D);
            if (C.Equals2D(B) && C.Z.Equals(B.Z))
                return DistancePointSegment(C, A, B);

            /*
             * Algorithm derived from http://softsurfer.com/Archive/algorithm_0106/algorithm_0106.htm
             */
            double a = Vector3D.Dot(A, B, A, B);
            double b = Vector3D.Dot(A, B, C, D);
            double c = Vector3D.Dot(C, D, C, D);
            double d = Vector3D.Dot(A, B, C, A);
            double e = Vector3D.Dot(C, D, C, A);

            double denom = a * c - b * b;
            if (double.IsNaN(denom))
                throw new ArgumentException("Ordinates must not be NaN");

            double s;
            double t;
            if (denom <= 0.0)
            {
                /*
                 * The lines are parallel.
                 * In this case solve for the parameters s and t by assuming s is 0.
                 */
                s = 0;
                // choose largest denominator for optimal numeric conditioning
                if (b > c)
                    t = d / b;
                else
                    t = e / c;
            }
            else
            {
                s = (b * e - c * d) / denom;
                t = (a * e - b * d) / denom;
            }
            if (s < 0)
                return DistancePointSegment(A, C, D);
            if (s > 1)
                return DistancePointSegment(B, C, D);
            if (t < 0)
                return DistancePointSegment(C, A, B);
            if (t > 1)
            {
                return DistancePointSegment(D, A, B);
            }
            /*
             * The closest points are in interiors of segments,
             * so compute them directly
             */
            double x1 = A.X + s * (B.X - A.X);
            double y1 = A.Y + s * (B.Y - A.Y);
            double z1 = A.Z + s * (B.Z - A.Z);

            double x2 = C.X + t * (D.X - C.X);
            double y2 = C.Y + t * (D.Y - C.Y);
            double z2 = C.Z + t * (D.Z - C.Z);

            // length (p1-p2)
            return Distance(new CoordinateZ(x1, y1, z1), new CoordinateZ(x2, y2, z2));
        }
    }
}
