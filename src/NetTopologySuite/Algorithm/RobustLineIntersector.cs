using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// A robust version of <see cref="LineIntersector"/>.
    /// </summary>
    public class RobustLineIntersector : LineIntersector
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public override void ComputeIntersection(Coordinate p, Coordinate p1, Coordinate p2)
        {
            IsProper = false;
            // do between check first, since it is faster than the orientation test
            if (Envelope.Intersects(p1, p2, p))
            {
                if ((Orientation.Index(p1, p2, p) == OrientationIndex.Collinear) &&
                    (Orientation.Index(p2, p1, p) == OrientationIndex.Collinear))
                {
                    IsProper = true;
                    if (p.Equals(p1) || p.Equals(p2))
                        IsProper = false;
                    Result = PointIntersection;
                    return;
                }
            }
            Result = NoIntersection;
        }

        /// <inheritdoc cref="LineIntersector.ComputeIntersect"/>
        public override int ComputeIntersect(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            IsProper = false;

            // first try a fast test to see if the envelopes of the lines intersect
            if (!Envelope.Intersects(p1, p2, q1, q2))
                return NoIntersection;

            // for each endpoint, compute which side of the other segment it lies
            // if both endpoints lie on the same side of the other segment,
            // the segments do not intersect
            var Pq1 = Orientation.Index(p1, p2, q1);
            var Pq2 = Orientation.Index(p1, p2, q2);

            if ((Pq1 > 0 && Pq2 > 0) ||
                (Pq1 < 0 && Pq2 < 0))
                return NoIntersection;

            var Qp1 = Orientation.Index(q1, q2, p1);
            var Qp2 = Orientation.Index(q1, q2, p2);

            if ((Qp1 > 0 && Qp2 > 0) ||
                (Qp1 < 0 && Qp2 < 0))
                return NoIntersection;

            /*
             * Intersection is collinear if each endpoint lies on the other line.
             */
            bool collinear = Pq1 == 0 && Pq2 == 0 && Qp1 == 0 && Qp2 == 0;
            if (collinear)
                return ComputeCollinearIntersection(p1, p2, q1, q2);

            /*
             * At this point we know that there is a single intersection point
             * (since the lines are not collinear).
             */

            /*
             *  Check if the intersection is an endpoint. If it is, copy the endpoint as
             *  the intersection point. Copying the point rather than computing it
             *  ensures the point has the exact value, which is important for
             *  robustness. It is sufficient to simply check for an endpoint which is on
             *  the other line, since at this point we know that the inputLines must
             *  intersect.
             */
            Coordinate p = null;
            double z = double.NaN;
            if (Pq1 == 0 || Pq2 == 0 || Qp1 == 0 || Qp2 == 0)
            {
                IsProper = false;

                /*
                 * Check for two equal endpoints.
                 * This is done explicitly rather than by the orientation tests
                 * below in order to improve robustness.
                 *
                 * [An example where the orientation tests fail to be consistent is
                 * the following (where the true intersection is at the shared endpoint
                 * POINT (19.850257749638203 46.29709338043669)
                 *
                 * LINESTRING ( 19.850257749638203 46.29709338043669, 20.31970698357233 46.76654261437082 )
                 * and
                 * LINESTRING ( -48.51001596420236 -22.063180333403878, 19.850257749638203 46.29709338043669 )
                 *
                 * which used to produce the INCORRECT result: (20.31970698357233, 46.76654261437082, NaN)
                 *
                 */
                if (p1.Equals2D(q1))
                {
                    p = p1;
                    z = zGet(p1, q1);
                }
                else if (p1.Equals2D(q2))
                {
                    p = p1;
                    z = zGet(p1, q2);
                }
                else if (p2.Equals2D(q1))
                {
                    p = p2;
                    z = zGet(p2, q1);
                }
                else if (p2.Equals2D(q2))
                {
                    p = p2;
                    z = zGet(p2, q2);
                }
                /*
                 * Now check to see if any endpoint lies on the interior of the other segment.
                 */
                else if (Pq1 == 0)
                {
                    p = q1;
                    z = zGetOrInterpolate(q1, p1, p2);
                }
                else if (Pq2 == 0)
                {
                    p = q2;
                    z = zGetOrInterpolate(q2, p1, p2);
                }
                else if (Qp1 == 0)
                {
                    p = p1;
                    z = zGetOrInterpolate(p1, q1, q2);
                }
                else if (Qp2 == 0)
                {
                    p = p2;
                    z = zGetOrInterpolate(p2, q1, q2);
                }
            }
            else
            {
                IsProper = true;
                p = Intersection(p1, p2, q1, q2);
                z = zInterpolate(p, p1, p2, q1, q2);
            }
            IntersectionPoint[0] = copyWithZ(p, z);
            return PointIntersection;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        private int ComputeCollinearIntersection(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            bool q1inP = Envelope.Intersects(p1, p2, q1);
            bool q2inP = Envelope.Intersects(p1, p2, q2);
            bool p1inQ = Envelope.Intersects(q1, q2, p1);
            bool p2inQ = Envelope.Intersects(q1, q2, p2);

            if (q1inP && q2inP)
            {
                IntersectionPoint[0] = copyWithZInterpolate(q1, p1, p2);
                IntersectionPoint[1] = copyWithZInterpolate(q2, p1, p2);
                return CollinearIntersection;
            }
            if (p1inQ && p2inQ)
            {
                IntersectionPoint[0] = copyWithZInterpolate(p1, q1, q2);
                IntersectionPoint[1] = copyWithZInterpolate(p2, q1, q2);
                return CollinearIntersection;
            }
            if (q1inP && p1inQ)
            {
                // if pts are equal Z is chosen arbitrarily
                IntersectionPoint[0] = copyWithZInterpolate(q1, p1, p2);
                IntersectionPoint[1] = copyWithZInterpolate(p1, q1, q2);
                return q1.Equals(p1) && !q2inP && !p2inQ ? PointIntersection : CollinearIntersection;
            }
            if (q1inP && p2inQ)
            {
                // if pts are equal Z is chosen arbitrarily
                IntersectionPoint[0] = copyWithZInterpolate(q1, p1, p2);
                IntersectionPoint[1] = copyWithZInterpolate(p2, q1, q2);
                return q1.Equals(p2) && !q2inP && !p1inQ ? PointIntersection : CollinearIntersection;
            }
            if (q2inP && p1inQ)
            {
                // if pts are equal Z is chosen arbitrarily
                IntersectionPoint[0] = copyWithZInterpolate(q2, p1, p2);
                IntersectionPoint[1] = copyWithZInterpolate(p1, q1, q2);
                return q2.Equals(p1) && !q1inP && !p2inQ ? PointIntersection : CollinearIntersection;
            }
            if (q2inP && p2inQ)
            {
                // if pts are equal Z is chosen arbitrarily
                IntersectionPoint[0] = copyWithZInterpolate(q2, p1, p2);
                IntersectionPoint[1] = copyWithZInterpolate(p2, q1, q2);
                return q2.Equals(p2) && !q1inP && !p1inQ ? PointIntersection : CollinearIntersection;
            }
            return NoIntersection;
        }

        private static Coordinate copyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return copyWithZ(p, zGetOrInterpolate(p, p1, p2));
        }

        private static Coordinate copyWithZ(Coordinate p, double z)
        {
            var pCopy = Copy(p);
            if (!double.IsNaN(z))
            {
                pCopy.Z = z;
            }
            return pCopy;
        }

        private static Coordinate Copy(Coordinate p)
        {
            return new CoordinateZ(p);
        }

        /// <summary>
        /// This method computes the actual value of the intersection point.
        /// To obtain the maximum precision from the intersection calculation,
        /// the coordinates are normalized by subtracting the minimum
        /// ordinate values (in absolute value).  This has the effect of
        /// removing common significant digits from the calculation to
        /// maintain more bits of precision.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        private Coordinate Intersection(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            var intPt = IntersectionSafe(p1, p2, q1, q2);

            /*
            // TESTING ONLY
            var intPtDD = CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
            var dist = intPt.Distance(intPtDD);
            System.Console.WriteLine(intPt + " - " + intPtDD + " dist = " + dist);
            //intPt = intPtDD;
             */

            /*
             * Due to rounding it can happen that the computed intersection is
             * outside the envelopes of the input segments.  Clearly this
             * is inconsistent.
             * This code checks this condition and forces a more reasonable answer
             *
             * MD - May 4 2005 - This is still a problem.  Here is a failure case:
             *
             * LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)
             * LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)
             * int point = (2097408.2633752143,1144595.8008114607)
             *
             * MD - Dec 14 2006 - This does not seem to be a failure case any longer
             */
            if (!IsInSegmentEnvelopes(intPt))
            {
                // compute a safer result
                // copy the coordinate, since it may be rounded later
                intPt = NearestEndpoint(p1, p2, q1, q2).Copy();
                // intPt = CentralEndpointIntersector.GetIntersection(p1, p2, q1, q2);
                // CheckDD(p1, p2, q1, q2, intPt);
            }

            if (PrecisionModel != null)
                PrecisionModel.MakePrecise(intPt);
            return intPt;
        }

        private void CheckDD(Coordinate p1, Coordinate p2, Coordinate q1,
            Coordinate q2, Coordinate intPt)
        {
            var intPtDD = CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
            bool isIn = IsInSegmentEnvelopes(intPtDD);
            Debug.WriteLine("DD in env = " + isIn + "  --------------------- " + intPtDD);
            double distance = intPt.Distance(intPtDD);
            if (distance > 0.0001)
                Debug.WriteLine("Distance = " + distance);
        }

        /// <summary>
        /// Computes a segment intersection using homogeneous coordinates.
        /// Round-off error can cause the raw computation to fail,
        /// (usually due to the segments being approximately parallel).
        /// If this happens, a reasonable approximation is computed instead.
        /// </summary>
        private static Coordinate IntersectionSafe(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            var intPt = IntersectionComputer.Intersection(p1, p2, q1, q2);
            if (intPt == null) {
                intPt = NearestEndpoint(p1, p2, q1, q2);
                //Console.WriteLine($"Snapped to {intPt}");
            }
            
            return intPt;
        }

        /// <summary>
        /// Tests whether a point lies in the envelopes of both input segments.
        /// A correctly computed intersection point should return <c>true</c>
        /// for this test.
        /// Since this test is for debugging purposes only, no attempt is
        /// made to optimize the envelope test.
        /// </summary>
        /// <param name="intPoint"></param>
        /// <returns><c>true</c> if the input point lies within both input segment envelopes.</returns>
        private bool IsInSegmentEnvelopes(Coordinate intPoint)
        {
            var env0 = new Envelope(InputLines[0][0], InputLines[0][1]);
            var env1 = new Envelope(InputLines[1][0], InputLines[1][1]);
            return env0.Contains(intPoint) && env1.Contains(intPoint);
        }

        /// <summary>
        /// Finds the endpoint of the segments P and Q which
        /// is closest to the other segment.
        /// This is a reasonable surrogate for the true
        /// intersection points in ill-conditioned cases
        /// (e.g. where two segments are nearly coincident,
        /// or where the endpoint of one segment lies almost on the other segment).
        /// </summary>
        /// <remarks>
        /// This replaces the older CentralEndpoint heuristic,
        /// which chose the wrong endpoint in some cases
        /// where the segments had very distinct slopes
        /// and one endpoint lay almost on the other segment.
        /// </remarks>
        /// <param name="p1">an endpoint of segment P</param>
        /// <param name="p2">an endpoint of segment P</param>
        /// <param name="q1">an endpoint of segment Q</param>
        /// <param name="q2">an endpoint of segment Q</param>
        /// <returns>the nearest endpoint to the other segment</returns>
        private static Coordinate NearestEndpoint(Coordinate p1, Coordinate p2,
            Coordinate q1, Coordinate q2)
        {
            var nearestPt = p1;
            double minDist = DistanceComputer.PointToSegment(p1, q1, q2);

            double dist = DistanceComputer.PointToSegment(p2, q1, q2);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPt = p2;
            }
            dist = DistanceComputer.PointToSegment(q1, p1, p2);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPt = q1;
            }
            dist = DistanceComputer.PointToSegment(q2, p1, p2);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPt = q2;
            }
            return nearestPt;
        }

        /*
         * Gets the Z value of the first argument if present, 
         * otherwise the value of the second argument.
         * 
         * @param p a coordinate, possibly with Z
         * @param q a coordinate, possibly with Z
         * @return the Z value if present
         */
        private static double zGet(Coordinate p, Coordinate q)
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
        private static double zGetOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
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
        private static double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
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
        private static double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
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
