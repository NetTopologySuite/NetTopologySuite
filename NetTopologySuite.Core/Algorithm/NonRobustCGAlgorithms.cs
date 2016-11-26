using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Non-robust versions of various fundamental Computational Geometric algorithms,
    /// FOR TESTING PURPOSES ONLY!.
    /// The non-robustness is due to rounding error in floating point computation.
    /// </summary>
    public static class NonRobustCGAlgorithms
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ring"></param>
        /// <returns></returns>
        public static bool IsPointInRing(Coordinate p, Coordinate[] ring)
        {
            int i, i1; // point index; i1 = i-1 mod n
            double xInt; // x intersection of e with ray
            int crossings = 0; // number of edge/ray crossings
            double x1, y1, x2, y2;
            int nPts = ring.Length;

            /* For each line edge l = (i-1, i), see if it crosses ray from test point in positive x direction. */
            for (i = 1; i < nPts; i++)
            {
                i1 = i - 1;
                Coordinate p1 = ring[i];
                Coordinate p2 = ring[i1];
                x1 = p1.X - p.X;
                y1 = p1.Y - p.Y;
                x2 = p2.X - p.X;
                y2 = p2.Y - p.Y;

                if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
                {
                    /* e straddles x axis, so compute intersection. */
                    xInt = (x1*y2 - x2*y1)/(y2 - y1);
                    /* crosses ray if strictly positive intersection. */
                    if (0.0 < xInt) crossings++;
                }
            }

            /* p is inside if an odd number of crossings. */
            if ((crossings%2) == 1)
                return true;
            else return false;
        }

        /// <summary>
        /// Computes whether a ring defined by an array of <c>Coordinate</c> is
        /// oriented counter-clockwise.
        /// This will handle coordinate lists which contain repeated points.
        /// </summary>
        /// <param name="ring">an array of coordinates forming a ring.</param>
        /// <returns>
        /// <c>true</c> if the ring is oriented counter-clockwise.
        /// throws <c>ArgumentException</c> if the ring is degenerate (does not contain 3 different points)
        /// </returns>
        public static bool IsCCW(Coordinate[] ring)
        {
            // # of points without closing endpoint
            int nPts = ring.Length - 1;

            // check that this is a valid ring - if not, simply return a dummy value
            if (nPts < 4)
                return false;

            // algorithm to check if a Ring is stored in CCW order
            // find highest point
            Coordinate hip = ring[0];
            int hii = 0;
            for (int i = 1; i <= nPts; i++)
            {
                Coordinate p = ring[i];
                if (p.Y > hip.Y)
                {
                    hip = p;
                    hii = i;
                }
            }

            // find different point before highest point
            int iPrev = hii;
            do
                iPrev = (iPrev - 1)%nPts; while (ring[iPrev].Equals(hip) && iPrev != hii);

            // find different point after highest point
            int iNext = hii;
            do
                iNext = (iNext + 1)%nPts; while (ring[iNext].Equals(hip) && iNext != hii);

            Coordinate prev = ring[iPrev];
            Coordinate next = ring[iNext];
            if (prev.Equals(hip) || next.Equals(hip) || prev.Equals(next))
                throw new ArgumentException("degenerate ring (does not contain 3 different points)");

            // translate so that hip is at the origin.
            // This will not affect the area calculation, and will avoid
            // finite-accuracy errors (i.e very small vectors with very large coordinates)
            // This also simplifies the discriminant calculation.
            double prev2x = prev.X - hip.X;
            double prev2y = prev.Y - hip.Y;
            double next2x = next.X - hip.X;
            double next2y = next.Y - hip.Y;

            // compute cross-product of vectors hip->next and hip->prev
            // (e.g. area of parallelogram they enclose)
            double disc = next2x*prev2y - next2y*prev2x;

            /* If disc is exactly 0, lines are collinear.  There are two possible cases:
                    (1) the lines lie along the x axis in opposite directions
                    (2) the line lie on top of one another
                    
                    (2) should never happen, so we're going to ignore it!
                        (Might want to assert this)
            
                    (1) is handled by checking if next is left of prev ==> CCW
            */
            if (disc == 0.0)
                return (prev.X > next.X); // poly is CCW if prev x is right of next x
            else return (disc > 0.0); // if area is positive, points are ordered CCW                 
        }

        /// <summary>
        /// Computes the orientation of a point q to the directed line segment p1-p2.
        /// The orientation of a point relative to a directed line segment indicates
        /// which way you turn to get to q after travelling from p1 to p2.
        /// </summary>
        /// <returns>1 if q is counter-clockwise from p1-p2</returns>
        /// <returns>-1 if q is clockwise from p1-p2</returns>
        /// <returns>0 if q is collinear with p1-p2</returns>
        public static int ComputeOrientation(Coordinate p1, Coordinate p2, Coordinate q)
        {
            return OrientationIndex(p1, p2, q);
        }

        /// <summary>
        /// Returns the index of the direction of the point <c>q</c> relative to
        /// a vector specified by <c>p1-p2</c>.
        /// </summary>
        /// <param name="p1">the origin point of the vector</param>
        /// <param name="p2">the final point of the vector</param>
        /// <param name="q">the point to compute the direction to</param>
        /// <returns> 1 if q is counter-clockwise (left) from p1-p2</returns>
        /// <returns>-1 if q is clockwise (right) from p1-p2</returns>
        /// <returns>0 if q is collinear with p1-p2</returns>
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
        {
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = q.X - p2.X;
            double dy2 = q.Y - p2.Y;
            double det = dx1*dy2 - dx2*dy1;

            if (det > 0.0) return 1;
            if (det < 0.0) return -1;
            return 0;
        }

        /// <summary> 
        /// Computes the distance from a line segment AB to a line segment CD.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="A">A point of one line.</param>
        /// <param name="B">The second point of the line (must be different to A).</param>
        /// <param name="C">One point of the line.</param>
        /// <param name="D">Another point of the line (must be different to A).</param>
        /// <returns>The distance from line segment AB to line segment CD.</returns>
        public static double DistanceLineLine(Coordinate A, Coordinate B,
                                              Coordinate C, Coordinate D)
        {
            // check for zero-length segments
            if (A.Equals(B))
                return CGAlgorithms.DistancePointLine(A, C, D);
            if (C.Equals(D))
                return CGAlgorithms.DistancePointLine(D, A, B);

            // AB and CD are line segments
            /*
             * from comp.graphics.algo
             * 
             * Solving the above for r and s yields 
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
             * P=A+r(B-A) or 
             * Px=Ax+r(Bx-Ax) 
             * Py=Ay+r(By-Ay) 
             * By examining the values of r & s, you can also determine some other limiting
             * conditions: 
             * If 0<=r<=1 & 0<=s<=1, intersection exists 
             *    r<0 or r>1 or s<0 or s>1 line segments do not intersect 
             * If the denominator in eqn 1 is zero, AB & CD are parallel 
             * If the numerator in eqn 1 is also zero, AB & CD are collinear.
             */
            
            double r_top = (A.Y - C.Y)*(D.X - C.X) - (A.X - C.X)*(D.Y - C.Y);
            double r_bot = (B.X - A.X)*(D.Y - C.Y) - (B.Y - A.Y)*(D.X - C.X);

            double s_top = (A.Y - C.Y)*(B.X - A.X) - (A.X - C.X)*(B.Y - A.Y);
            double s_bot = (B.X - A.X)*(D.Y - C.Y) - (B.Y - A.Y)*(D.X - C.X);

            if ((r_bot == 0) || (s_bot == 0))
            {
                return Math
                    .Min(
                        CGAlgorithms.DistancePointLine(A, C, D),
                        Math.Min(
                            CGAlgorithms.DistancePointLine(B, C, D),
                            Math.Min(CGAlgorithms.DistancePointLine(C, A, B),
                                     CGAlgorithms.DistancePointLine(D, A, B))));

            }
            double s = s_top/s_bot;
            double r = r_top/r_bot;

            if ((r < 0) || (r > 1) || (s < 0) || (s > 1))
            {
                // no intersection
                return Math
                    .Min(
                        CGAlgorithms.DistancePointLine(A, C, D),
                        Math.Min(
                            CGAlgorithms.DistancePointLine(B, C, D),
                            Math.Min(CGAlgorithms.DistancePointLine(C, A, B),
                                     CGAlgorithms.DistancePointLine(D, A, B))));
            }
            return 0.0; // intersection exists
        }

    }
}
