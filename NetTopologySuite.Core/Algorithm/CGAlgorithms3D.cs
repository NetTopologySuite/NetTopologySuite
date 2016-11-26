using System;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Algorithm
{
    /**
     * Basic computational geometry algorithms 
     * for geometry and coordinates defined in 3-dimensional Cartesian space.
     * 
     * @author mdavis
     *
     */
    public class CGAlgorithms3D
    {
        public static double Distance(Coordinate p0, Coordinate p1)
        {
            // default to 2D distance if either Z is not set
            if (Double.IsNaN(p0.Z) || Double.IsNaN(p1.Z))
                return p0.Distance(p1);

            var dx = p0.X - p1.X;
            var dy = p0.Y - p1.Y;
            var dz = p0.Z - p1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static double DistancePointSegment(Coordinate p,
                Coordinate A, Coordinate B)
        {
            // if start = end, then just compute distance to one of the endpoints
            if (A.Equals3D(B))
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

            var len2 = (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y) + (B.Z - A.Z) * (B.Z - A.Z);
            if (Double.IsNaN(len2))
                throw new ArgumentException("Ordinates must not be NaN");
            var r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y) + (p.Z - A.Z) * (B.Z - A.Z))
                / len2;

            if (r <= 0.0)
                return Distance(p, A);
            if (r >= 1.0)
                return Distance(p, B);

            // compute closest point q on line segment
            var qx = A.X + r * (B.X - A.X);
            var qy = A.Y + r * (B.Y - A.Y);
            var qz = A.Z + r * (B.Z - A.Z);
            // result is distance from p to q
            var dx = p.X - qx;
            var dy = p.Y - qy;
            var dz = p.Z - qz;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        /**
         * Computes the distance between two 3D segments.
         * 
         * @param A the start point of the first segment
         * @param B the end point of the first segment
         * @param C the start point of the second segment
         * @param D the end point of the second segment
         * @return the distance between the segments
         */
        public static double DistanceSegmentSegment(
                Coordinate A, Coordinate B, Coordinate C, Coordinate D)
        {
            /**
             * This calculation is susceptible to roundoff errors when 
             * passed large ordinate values.
             * It may be possible to improve this by using {@link DD} arithmetic.
             */
            if (A.Equals3D(B))
                return DistancePointSegment(A, C, D);
            if (C.Equals3D(B))
                return DistancePointSegment(C, A, B);

            /**
             * Algorithm derived from http://softsurfer.com/Archive/algorithm_0106/algorithm_0106.htm
             */
            var a = Vector3D.Dot(A, B, A, B);
            var b = Vector3D.Dot(A, B, C, D);
            var c = Vector3D.Dot(C, D, C, D);
            var d = Vector3D.Dot(A, B, C, A);
            var e = Vector3D.Dot(C, D, C, A);

            var denom = a * c - b * b;
            if (Double.IsNaN(denom))
                throw new ArgumentException("Ordinates must not be NaN");

            double s;
            double t;
            if (denom <= 0.0)
            {
                /**
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
            /**
             * The closest points are in interiors of segments,
             * so compute them directly
             */
            var x1 = A.X + s * (B.X - A.X);
            var y1 = A.Y + s * (B.Y - A.Y);
            var z1 = A.Z + s * (B.Z - A.Z);

            var x2 = C.X + t * (D.X - C.X);
            var y2 = C.Y + t * (D.Y - C.Y);
            var z2 = C.Z + t * (D.Z - C.Z);

            // length (p1-p2)
            return Distance(new Coordinate(x1, y1, z1), new Coordinate(x2, y2, z2));
        }


    }
}