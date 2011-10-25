using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Specifies and implements various fundamental Computational Geometric algorithms.
    /// The algorithms supplied in this class are robust for double-precision floating point.
    /// </summary>
    public static class CGAlgorithms
    {        
        /// <summary> 
        /// A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        public const int Clockwise      = -1;
        /// <summary> 
        /// A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        public const int Right         = Clockwise;

        /// <summary>
        /// A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        public const int CounterClockwise  = 1;
        /// <summary>
        /// A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        public const int Left              = CounterClockwise;

        /// <summary>
        /// A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        public const int Collinear         = 0;
        /// <summary>
        /// A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        public const int Straight = Collinear;
        
        /// <summary> 
        /// Returns the index of the direction of the point <c>q</c>
        /// relative to a vector specified by <c>p1-p2</c>.
        /// </summary>
        /// <param name="p1">The origin point of the vector.</param>
        /// <param name="p2">The final point of the vector.</param>
        /// <param name="q">The point to compute the direction to.</param>
        /// <returns> 
        /// 1 if q is counter-clockwise (left) from p1-p2,
        /// -1 if q is clockwise (right) from p1-p2,
        /// 0 if q is collinear with p1-p2.
        /// </returns>
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q) 
        {
            /**
             * MD - 9 Aug 2010
             * It seems that the basic algorithm is slightly orientation dependent,
             * when computing the orientation of a point very close to a line.
             * This is possibly due to the arithmetic in the translation to the origin.
             * 
             * For instance, the following situation produces identical results 
             * in spite of the inverse orientation of the line segment:
             * 
             * Coordinate p0 = new Coordinate(219.3649559090992, 140.84159161824724);
             * Coordinate p1 = new Coordinate(168.9018919682399, -5.713787599646864);
             * 
             * Coordinate p = new Coordinate(186.80814046338352, 46.28973405831556);
             * int orient = orientationIndex(p0, p1, p);
             * int orientInv = orientationIndex(p1, p0, p);

             * A way to force consistent results is to normalize the orientation of the vector
             * using the following code.
             * However, this may make the results of orientationIndex inconsistent
             * through the triangle of points, so it's not clear this is 
             * an appropriate patch.
             * 
             */
            /*
            // Normalize orientation of vector to provide consistent results
            // This produces repeatable results for single cases, but does not fully solve robustness issues
               if (p2.x < p1.x || (p2.x == p1.x && p2.y < p1.y))
                    return -orientationIndex(p2, p1, q);
             */
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = q.X - p2.X;
            double dy2 = q.Y - p2.Y;
            return RobustDeterminant.SignOfDet2x2(dx1, dy1, dx2, dy2);
        }        

        /// <summary> 
        /// Tests whether a point lies inside or on a ring.
        /// </summary>
        /// <remarks>
        /// <para>The ring may be oriented in either direction.</para>
        /// <para>A point lying exactly on the ring boundary is considered to be inside the ring.</para>
        /// <para>This method does <i>not</i> first check the point against the envelope
        /// of the ring.</para>
        /// </remarks>
        /// <param name="p">Point to check for ring inclusion.</param>
        /// <param name="ring">An array of <see cref="Coordinate"/>s representing the ring (which must have first point identical to last point)</param>
        /// <returns>true if p is inside ring.</returns>
        /// <see cref="IPointInRing"/>
        public static bool IsPointInRing(Coordinate p, Coordinate[] ring) 
        {
            return LocatePointInRing(p, ring) != Location.Exterior;
        }

        ///<summary>
        /// Determines whether a point lies in the interior, on the boundary, or in the exterior of a ring.
        ///</summary>
        /// <remarks>
        /// <para>The ring may be oriented in either direction.</para>
        /// <para>This method does <i>not</i> first check the point against the envelope of the ring.</para>
        /// </remarks>
        /// <param name="p">Point to check for ring inclusion</param>
        /// <param name="ring">An array of coordinates representing the ring (which must have first point identical to last point)</param>
        /// <returns>The <see cref="Location"/> of p relative to the ring</returns>
        public static Location LocatePointInRing(Coordinate p, Coordinate[] ring)
        {
            return RayCrossingCounter.LocatePointInRing(p, ring);
        }

        /// <summary> 
        /// Tests whether a point lies on the line segments defined by a
        /// list of coordinates.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="pt"></param>
        /// <returns>true if the point is a vertex of the line
        /// or lies in the interior of a line segment in the linestring
        /// </returns>
        public static bool IsOnLine(Coordinate p, Coordinate[] pt) 
        {
            LineIntersector lineIntersector = new RobustLineIntersector();
            for (int i = 1; i < pt.Length; i++) 
            {
                Coordinate p0 = pt[i - 1];
                Coordinate p1 = pt[i];
                lineIntersector.ComputeIntersection(p, p0, p1);
                if (lineIntersector.HasIntersection) 
                    return true;                
            }
            return false;
        }
 
		/// <summary>
        /// Computes whether a ring defined by an array of <see cref="Coordinate" />s is oriented counter-clockwise.
        /// </summary>>
        /// <remarks>
        /// <list type="Bullet">
        /// <item>The list of points is assumed to have the first and last points equal.</item>
        /// <item>This will handle coordinate lists which contain repeated points.</item>
        /// </list>
        /// <para>This algorithm is only guaranteed to work with valid rings. If the ring is invalid (e.g. self-crosses or touches), the computed result may not be correct.</para>
        /// </remarks>
        /// <param name="ring">An array of <see cref="Coordinate"/>s froming a ring</param>
        /// <returns>true if the ring is oriented <see cref="Orientation.CounterClockwise"/></returns>
        /// <exception cref="ArgumentException">If there are too few points to determine orientation (&lt;3)</exception>
        public static bool IsCCW(Coordinate[] ring) 
        {
            // # of points without closing endpoint
            int nPts = ring.Length - 1;
            
            // sanity check
            if (nPts < 3)
                throw new ArgumentException("Ring has fewer than 3 points, so orientation cannot be determined");

            // find highest point
            Coordinate hiPt = ring[0];
            int hiIndex = 0;
            for (int i = 1; i <= nPts; i++)
            {
                Coordinate p = ring[i];
                if (p.Y > hiPt.Y)
                {
                    hiPt = p;
                    hiIndex = i;
                }
            }

            // find distinct point before highest point
            int iPrev = hiIndex;
            do
            {
                iPrev = iPrev - 1;
                if (iPrev < 0) iPrev = nPts;
            } 
            while (ring[iPrev].Equals2D(hiPt) && iPrev != hiIndex);

            // find distinct point after highest point
            int iNext = hiIndex;
            do
                iNext = (iNext + 1) % nPts;
            while (ring[iNext].Equals2D(hiPt) && iNext != hiIndex);

            Coordinate prev = ring[iPrev];
            Coordinate next = ring[iNext];

            /*
             * This check catches cases where the ring contains an A-B-A configuration of points.
             * This can happen if the ring does not contain 3 distinct points
             * (including the case where the input array has fewer than 4 elements),
             * or it contains coincident line segments.
             */
            if (prev.Equals2D(hiPt) || next.Equals2D(hiPt) || prev.Equals2D(next))
                return false;

            int disc = ComputeOrientation(prev, hiPt, next);

            /*
             *  If disc is exactly 0, lines are collinear.  There are two possible cases:
             *  (1) the lines lie along the x axis in opposite directions
             *  (2) the lines lie on top of one another
             *
             *  (1) is handled by checking if next is left of prev ==> CCW
             *  (2) will never happen if the ring is valid, so don't check for it
             *  (Might want to assert this)
             */
            bool isCCW = false;
            if (disc == 0)            
                // poly is CCW if prev x is right of next x
                isCCW = (prev.X > next.X);            
            else
                // if area is positive, points are ordered CCW
                isCCW = (disc > 0);
            return isCCW;
        }

        /// <summary>
        /// Computes the orientation of a point q to the directed line segment p1-p2.
        /// The orientation of a point relative to a directed line segment indicates
        /// which way you turn to get to q after travelling from p1 to p2.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q"></param>
        /// <returns> 
        /// 1 if q is counter-clockwise from p1-p2,
        /// -1 if q is clockwise from p1-p2,
        /// 0 if q is collinear with p1-p2-
        /// </returns>
        public static int ComputeOrientation(Coordinate p1, Coordinate p2, Coordinate q) 
        {
            return OrientationIndex(p1, p2, q);
        }

        /// <summary> 
        /// Computes the distance from a point p to a line segment AB.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns> The distance from p to line segment AB.</returns>
        public static double DistancePointLine(Coordinate p, Coordinate A, Coordinate B)
        {
            // if start = end, then just compute distance to one of the endpoints
            if (A.X == B.X && A.Y == B.Y) 
                return p.Distance(A);

            // otherwise use comp.graphics.algorithms Frequently Asked Questions method
            /*(1)     	      AC dot AB
                        r =   ---------
                              ||AB||^2
             
		                r has the following meaning:
		                r=0 Point = A
		                r=1 Point = B
		                r<0 Point is on the backward extension of AB
		                r>1 Point is on the forward extension of AB
		                0<r<1 Point is interior to AB
	        */

            double r =  ( (p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y) )
                        /
                        ( (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y) );

            if (r <= 0.0) return p.Distance(A);
            if (r >= 1.0) return p.Distance(B);


            /*(2)
		                    (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
		                s = -----------------------------
		             	                Curve^2

		                Then the distance from C to Point = |s|*Curve.
	        */

            double s =  ( (A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y) )
                        /
                        ( (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y) );

            return System.Math.Abs(s) * System.Math.Sqrt(((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y)));
        }

        /// <summary> 
        /// Computes the perpendicular distance from a point p
        /// to the (infinite) line containing the points AB
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns>The perpendicular distance from p to line AB.</returns>
        public static double DistancePointLinePerpendicular(Coordinate p, Coordinate A, Coordinate B)
        {
            // use comp.graphics.algorithms Frequently Asked Questions method
            /*(2)
                            (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                        s = -----------------------------
                                         Curve^2

                        Then the distance from C to Point = |s|*Curve.
            */

            double s =  ( (A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y) )
                        /
                        ( (B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y) );

            return System.Math.Abs(s) * System.Math.Sqrt(((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y)));
        }

        /// <summary>
        /// Computes the distance from a point to a sequence of line segments.
        /// </summary>
        /// <param name="p">A point</param>
        /// <param name="line">A sequence of contiguous line segments defined by their vertices</param>
        /// <returns>The minimum distance between the point and the line segments</returns>
        /// <exception cref="ArgumentException">If there are too few points to make up a line (at least one?)</exception>
        public static double DistancePointLine(Coordinate p, Coordinate[] line)
        {
            if (line.Length == 0)
                throw new ArgumentException("Line array must contain at least one vertex");

            // this handles the case of length = 1
            double minDistance = p.Distance(line[0]);
            for (int i = 0; i < line.Length - 1; i++)
            {
                double dist = CGAlgorithms.DistancePointLine(p, line[i], line[i + 1]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                }
            }
            return minDistance;
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
        public static double DistanceLineLine(Coordinate A, Coordinate B, Coordinate C, Coordinate D)
        {
            // check for zero-length segments
            if (A.Equals(B))    
                return DistancePointLine(A,C,D);
            if (C.Equals(D))    
                return DistancePointLine(D,A,B);

            // AB and CD are line segments
            /* from comp.graphics.algo

	            Solving the above for r and s yields
				            (Ay-Cy)(Dx-Cx)-(Ax-Cx)(Dy-Cy)
	                    r = ----------------------------- (eqn 1)
				            (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)

		 	                (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
		                s = ----------------------------- (eqn 2)
			                (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
	            Let Point be the position vector of the intersection point, then
		            Point=A+r(B-A) or
		            Px=Ax+r(Bx-Ax)
		            Py=Ay+r(By-Ay)
	            By examining the values of r & s, you can also determine some other
                limiting conditions:
		            If 0<=r<=1 & 0<=s<=1, intersection exists
		            r<0 or r>1 or s<0 or s>1 line segments do not intersect
		            If the denominator in eqn 1 is zero, AB & CD are parallel
		            If the numerator in eqn 1 is also zero, AB & CD are collinear.

	        */
            double r_top = (A.Y - C.Y) * (D.X - C.X) - (A.X - C.X) * (D.Y - C.Y);
            double r_bot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

            double s_top = (A.Y - C.Y) * (B.X - A.X) - (A.X - C.X) * (B.Y - A.Y);
            double s_bot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

            if ((r_bot==0) || (s_bot == 0))             
                return  Math.Min(DistancePointLine(A,C,D),
	                    Math.Min(DistancePointLine(B,C,D),
	                    Math.Min(DistancePointLine(C,A,B),
	                    DistancePointLine(D,A,B) ) ) );

            
            double s = s_top/s_bot;
            double r = r_top/r_bot;

            if ((r < 0) || ( r > 1) || (s < 0) || (s > 1))	
                //no intersection
                return  Math.Min(DistancePointLine(A,C,D),
	                    Math.Min(DistancePointLine(B,C,D),
	                    Math.Min(DistancePointLine(C,A,B),
	                    DistancePointLine(D,A,B) ) ) );
            
            return 0.0; //intersection exists
        }

        /// <summary>
        /// Computes the signed area for a ring.
        /// <remarks>
        /// <para>
        /// The signed area is
        /// </para>  
        /// <list type="Table">
        /// <item>positive</item><description>if the ring is oriented CW</description>
        /// <item>negative</item><description>if the ring is oriented CCW</description>
        /// <item>zero</item><description>if the ring is degenerate or flat</description>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <param name="ring">The coordinates of the ring</param>
        /// <returns>The signed area of the ring</returns>
        public static double SignedArea(Coordinate[] ring)
        {
            if (ring.Length < 3) 
                return 0.0;

            double sum = 0.0;
            for (int i = 0; i < ring.Length - 1; i++) 
            {
                double bx = ring[i].X;
                double by = ring[i].Y;
                double cx = ring[i + 1].X;
                double cy = ring[i + 1].Y;
                sum += (bx + cx) * (cy - by);
            }
            return -sum  / 2.0;
        }

        /// <summary>
        /// Computes the signed area for a ring.
        /// <remarks>
        /// <para>
        /// The signed area is
        /// </para>  
        /// <list type="Table">
        /// <item>positive</item><description>if the ring is oriented CW</description>
        /// <item>negative</item><description>if the ring is oriented CCW</description>
        /// <item>zero</item><description>if the ring is degenerate or flat</description>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <param name="ring">The coordinates forming the ring</param>
        /// <returns>The signed area of the ring</returns>
        public static double SignedArea(ICoordinateSequence ring)
        {
            int n = ring.Count;
            if (n < 3) return 0.0;
            double sum = 0.0;
            var p = new Coordinate();
            ring.GetCoordinate(0, p);
            double bx = p.X;
            double by = p.Y;
            for (int i = 1; i < n; i++)
            {
                ring.GetCoordinate(i, p);
                double cx = p.X;
                double cy = p.Y;
                sum += (bx + cx) * (cy - by);
                bx = cx;
                by = cy;
            }
            return -sum / 2.0;
        }

        /// <summary>
        /// Computes the length of a linestring specified by a sequence of points.
        /// </summary>
        /// <param name="pts">The points specifying the linestring</param>
        /// <returns>The length of the linestring</returns>
        public static double Length(ICoordinateSequence pts)
        {
            // optimized for processing CoordinateSequences
            int n = pts.Count;
            if (n <= 1) return 0.0;

            double len = 0.0;

            var p = new Coordinate();
            pts.GetCoordinate(0, p);
            double x0 = p.X;
            double y0 = p.Y;

            for (int i = 1; i < n; i++)
            {
                pts.GetCoordinate(i, p);
                double x1 = p.X;
                double y1 = p.Y;
                double dx = x1 - x0;
                double dy = y1 - y0;

                len += Math.Sqrt(dx * dx + dy * dy);

                x0 = x1;
                y0 = y1;
            }
            return len;
        }
    }
}
