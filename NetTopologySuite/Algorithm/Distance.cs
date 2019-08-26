using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions to compute distance between basic geometric structures.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class DistanceComputer
    {

        /// <summary>
        /// Computes the distance from a line segment AB to a line segment CD
        /// <para/>
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="A">The first point of the first line</param>
        /// <param name="B">The second point of the first line (must be different to A)</param>
        /// <param name="C">The first point of the second line</param>
        /// <param name="D">The second point of the second line (must be different to C)</param>
        /// <returns>The distance from a line segment AB to a line segment CD</returns>
        public static double SegmentToSegment(Coordinate A, Coordinate B,
            Coordinate C, Coordinate D)
        {
            // check for zero-length segments
            if (A.Equals(B))
                return PointToSegment(A, C, D);
            if (C.Equals(D))
                return PointToSegment(D, A, B);

            // AB and CD are line segments
            /*
             * from comp.graphics.algo
             *
             * Solving the above for r and s yields
             *
             *     (Ay-Cy)(Dx-Cx)-(Ax-Cx)(Dy-Cy)
             * r = ----------------------------- (eqn 1)
             *     (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
             *
             *     (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
             * s = ----------------------------- (eqn 2)
             *     (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
             *
             * Let P be the position vector of the
             * intersection point, then
             *   P=A+r(B-A) or
             *   Px=Ax+r(Bx-Ax)
             *   Py=Ay+r(By-Ay)
             * By examining the values of r & s, you can also determine some other limiting
             * conditions:
             *   If 0<=r<=1 & 0<=s<=1, intersection exists
             *      r<0 or r>1 or s<0 or s>1 line segments do not intersect
             *   If the denominator in eqn 1 is zero, AB & CD are parallel
             *   If the numerator in eqn 1 is also zero, AB & CD are collinear.
             */

            bool noIntersection = false;
            if (!Envelope.Intersects(A, B, C, D))
            {
                noIntersection = true;
            }
            else
            {
                double denom = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

                if (denom == 0)
                {
                    noIntersection = true;
                }
                else
                {
                    double r_num = (A.Y - C.Y) * (D.X - C.X) - (A.X - C.X) * (D.Y - C.Y);
                    double s_num = (A.Y - C.Y) * (B.X - A.X) - (A.X - C.X) * (B.Y - A.Y);

                    double s = s_num / denom;
                    double r = r_num / denom;

                    if ((r < 0) || (r > 1) || (s < 0) || (s > 1))
                    {
                        noIntersection = true;
                    }
                }
            }
            if (noIntersection)
            {
                return MathUtil.Min(
                      PointToSegment(A, C, D),
                      PointToSegment(B, C, D),
                      PointToSegment(C, A, B),
                      PointToSegment(D, A, B));
            }
            // segments intersect
            return 0.0;
        }

        /// <summary>
        /// Computes the distance from a point to a sequence of line segments.
        /// </summary>
        /// <param name="p">A point</param>
        /// <param name="line">A sequence of contiguous line segments defined by their vertices</param>
        /// <returns>The minimum distance between the point and the line segments</returns>
        public static double PointToSegmentString(Coordinate p, Coordinate[] line)
        {
            if (line.Length == 0)
                throw new ArgumentException(
                    "Line array must contain at least one vertex");
            // this handles the case of length = 1
            double minDistance = p.Distance(line[0]);
            for (int i = 0; i < line.Length - 1; i++)
            {
                double dist = PointToSegment(p, line[i], line[i + 1]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                }
            }
            return minDistance;
        }

        /// <summary>
        /// Computes the distance from a point to a sequence of line segments.
        /// </summary>
        /// <param name="p">A point</param>
        /// <param name="line">A sequence of contiguous line segments defined by their vertices</param>
        /// <returns>The minimum distance between the point and the line segments</returns>
        public static double PointToSegmentString(Coordinate p, CoordinateSequence line)
        {
            if (line.Count == 0)
                throw new ArgumentException(
                    "Line array must contain at least one vertex");

            var lastStart = line.GetCoordinate(0);
            var currentEnd = lastStart.Copy();

            // this handles the case of length = 1
            double minDistance = p.Distance(lastStart);
            for (int i = 1; i < line.Count; i++)
            {
                line.GetCoordinate(i, currentEnd);
                double dist = PointToSegment(p, lastStart, currentEnd);
                if (dist < minDistance) minDistance = dist;
                lastStart.CoordinateValue = currentEnd;
            }
            return minDistance;
        }

        /// <summary>
        /// Computes the distance from a point p to a line segment AB
        /// <para/>
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="p">The point to compute the distance for</param>
        /// <param name="A">The first point of the first line</param>
        /// <param name="B">The second point of the first line (must be different to A)</param>
        /// <returns>The distance from p to line segment AB</returns>
        public static double PointToSegment(Coordinate p, Coordinate A,
            Coordinate B)
        {
            // if start = end, then just compute distance to one of the endpoints
            if (A.X == B.X && A.Y == B.Y)
                return p.Distance(A);

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

            double len2 = (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y);
            double r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y))
                / len2;

            if (r <= 0.0)
                return p.Distance(A);
            if (r >= 1.0)
                return p.Distance(B);

            /*
             * (2) s = (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
             *         -----------------------------
             *                    L^2
             *
             * Then the distance from C to P = |s|*L.
             *
             * This is the same calculation as {@link #distancePointLinePerpendicular}.
             * Unrolled here for performance.
             */
            double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y)) / len2;
            return Math.Abs(s) * Math.Sqrt(len2);
        }

        /// <summary>
        /// Computes the perpendicular distance from a point p to the (infinite) line
        /// containing the points AB
        /// </summary>
        /// <param name="p">The point to compute the distance for</param>
        /// <param name="A">The first point of the first line</param>
        /// <param name="B">The second point of the first line (must be different to A)</param>
        /// <returns>The perpendicular distance from p to line segment AB</returns>
        public static double PointToLinePerpendicular(Coordinate p,
            Coordinate A, Coordinate B)
        {
            // use comp.graphics.algorithms Frequently Asked Questions method
            /*
             * (2) s = (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
             *         -----------------------------
             *                    L^2
             *
             * Then the distance from C to P = |s|*L.
             */
            double len2 = (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y);
            double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y))
                / len2;

            return Math.Abs(s) * Math.Sqrt(len2);
        }

    }
}
