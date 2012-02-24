using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// A robust version of <see cref="LineIntersector{TCoordinate}"/>.
    /// </summary>
    public class RobustLineIntersector<TCoordinate> : LineIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Floating Precision Coordinate Factory is used for internal computation in
        /// the RobustLineIntersector Class to avoid problems with FixedPrecision
        ///</summary>
        public static ICoordinateFactory<TCoordinate> FloatingPrecisionCoordinateFactory = null;

        public RobustLineIntersector(IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
        }

        public override Intersection<TCoordinate> ComputeIntersection(TCoordinate p,
                                                                      TCoordinate p1, TCoordinate p2)
        {
            // do between check first, since it is faster than the orientation test
            if (Extents<TCoordinate>.Intersects(p1, p2, p))
            {
                Boolean isProper;

                if ((CGAlgorithms<TCoordinate>.OrientationIndex(p1, p2, p) == 0) &&
                    (CGAlgorithms<TCoordinate>.OrientationIndex(p2, p1, p) == 0))
                {
                    isProper = true;

                    if (p.Equals(p1) || p.Equals(p2))
                    {
                        isProper = false;
                    }

                    return new Intersection<TCoordinate>(p,
                                                         new Pair<TCoordinate>(p, p),
                                                         new Pair<TCoordinate>(p1, p2),
                                                         false,
                                                         false,
                                                         isProper);
                }
            }

            return new Intersection<TCoordinate>(new Pair<TCoordinate>(p, p), new Pair<TCoordinate>(p1, p2));
        }

        public override Intersection<TCoordinate> ComputeIntersection(TCoordinate p,
                                                                      Pair<TCoordinate> line)
        {
            return ComputeIntersection(p, line.First, line.Second);
            /*
            TCoordinate p1 = line.First;
            TCoordinate p2 = line.Second;

            // do between check first, since it is faster than the orientation test
            if (Extents<TCoordinate>.Intersects(p1, p2, p))
            {
                Boolean isProper;

                if ((CGAlgorithms<TCoordinate>.OrientationIndex(p1, p2, p) == 0) &&
                    (CGAlgorithms<TCoordinate>.OrientationIndex(p2, p1, p) == 0))
                {
                    isProper = true;

                    if (p.Equals(p1) || p.Equals(p2))
                    {
                        isProper = false;
                    }

                    return new Intersection<TCoordinate>(p,
                                                         new Pair<TCoordinate>(p, p),
                                                         line,
                                                         false,
                                                         false,
                                                         isProper);
                }
            }

            return new Intersection<TCoordinate>(new Pair<TCoordinate>(p, p), line);
             */
        }

        protected override Intersection<TCoordinate> ComputeIntersectInternal(Pair<TCoordinate> line0,
                                                                              Pair<TCoordinate> line1)
        {
            Boolean isProper;

            // first try a fast test to see if the envelopes of the lines don't intersect
            if (!Extents<TCoordinate>.Intersects(line0.First, line0.Second,
                                                 line1.First, line1.Second))
            {
                return new Intersection<TCoordinate>(line0, line1);
            }

            // for each endpoint, compute which side of the other segment it lies
            // if both endpoints lie on the same side of the other segment,
            // the segments do not intersect
            Int32 pq1 = CGAlgorithms<TCoordinate>.OrientationIndex(line0.First,
                                                                   line0.Second,
                                                                   line1.First);
            Int32 pq2 = CGAlgorithms<TCoordinate>.OrientationIndex(line0.First,
                                                                   line0.Second,
                                                                   line1.Second);

            if ((pq1 > 0 && pq2 > 0) || (pq1 < 0 && pq2 < 0))
            {
                return new Intersection<TCoordinate>(line0, line1);
            }

            Int32 qp1 = CGAlgorithms<TCoordinate>.OrientationIndex(line1.First,
                                                                   line1.Second,
                                                                   line0.First);
            Int32 qp2 = CGAlgorithms<TCoordinate>.OrientationIndex(line1.First,
                                                                   line1.Second,
                                                                   line0.Second);

            if ((qp1 > 0 && qp2 > 0) || (qp1 < 0 && qp2 < 0))
            {
                return new Intersection<TCoordinate>(line0, line1);
            }

            Boolean collinear = (pq1 == 0 && pq2 == 0 && qp1 == 0 && qp2 == 0);

            if (collinear)
            {
                return computeCollinearIntersection(line0, line1);
            }

            TCoordinate intersection = default(TCoordinate);

            /*
            *  Check if the intersection is an endpoint. If it is, copy the endpoint as
            *  the intersection point. Copying the point rather than computing it
            *  ensures the point has the exact value, which is important for
            *  robustness. It is sufficient to simply check for an endpoint which is on
            *  the other line, since at this point we know that the inputLines must
            *  intersect.
            */
            if (pq1 == 0 || pq2 == 0 || qp1 == 0 || qp2 == 0)
            {
                isProper = false;

                if (pq1 == 0)
                {
                    intersection = CoordinateFactory.Create(line1.First);
                }

                if (pq2 == 0)
                {
                    intersection = CoordinateFactory.Create(line1.Second);
                }

                if (qp1 == 0)
                {
                    intersection = CoordinateFactory.Create(line0.First);
                }

                if (qp2 == 0)
                {
                    intersection = CoordinateFactory.Create(line0.Second);
                }
            }
            else
            {
                isProper = true;
                intersection = computeIntersection(GeometryFactory, line0, line1);
            }

            return new Intersection<TCoordinate>(intersection, line0, line1, false, false, isProper);
        }

        private TCoordinate computeIntersection(IGeometryFactory<TCoordinate> geoFactory,
                                                Pair<TCoordinate> line0, Pair<TCoordinate> line1)
        {
            TCoordinate intersection = computeIntersectionWithNormalization(line0, line1);

            /*  FROM JTS:
             *
             * MD - May 4 2005 - This is still a problem.  Here is a failure case:
             *
             * LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)
             * LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)
             * intersection point = (2097408.2633752143,1144595.8008114607)
             *
             * MD - Dec 14 2006 - This does not seem to be a failure case any longer
             */
            if (!isInSegmentExtents(geoFactory, intersection, line0, line1))
            {
                Trace.Info("NTS", "Intersection outside segment envelopes: " + intersection);
            }

            if (PrecisionModel != null)
            {
                PrecisionModel.MakePrecise(intersection);
            }

            return intersection;
        }

        /// <summary>
        /// This method computes the actual value of the intersection point.
        /// To obtain the maximum precision from the intersection calculation,
        /// the coordinates are normalized by subtracting the minimum
        /// ordinate values (in absolute value).  This has the effect of
        /// removing common significant digits from the calculation to
        /// maintain more bits of precision.
        /// </summary>
        private TCoordinate computeIntersectionWithNormalization(Pair<TCoordinate> line0, Pair<TCoordinate> line1)
        {
            ICoordinateFactory<TCoordinate> cf = FloatingPrecisionCoordinateFactory ?? CoordinateFactory;

            TCoordinate n1 = cf.Create(line0.First);
            TCoordinate n2 = cf.Create(line0.Second);
            TCoordinate n3 = cf.Create(line1.First);
            TCoordinate n4 = cf.Create(line1.Second);

            TCoordinate normPt;
            normalizeToExtentCenter(cf, ref n1, ref n2, ref n3, ref n4, out normPt);

            TCoordinate intersection = cf.Create();

            try
            {
                intersection = safeHCoordinateIntersection(cf, n1, n2, n3, n4);
            }
            catch (NotRepresentableException)
            {
                Assert.ShouldNeverReachHere("Coordinate for intersection is not calculable.");
            }

            intersection = CoordinateFactory.Create(
                intersection[Ordinates.X] + normPt[Ordinates.X],
                intersection[Ordinates.Y] + normPt[Ordinates.Y]);

            return intersection;
        }

        // Computes a segment intersection using homogeneous coordinates.
        // Round-off error can cause the raw computation to fail,
        // (usually due to the segments being approximately parallel).
        // If this happens, a reasonable approximation is computed instead.
        private static TCoordinate safeHCoordinateIntersection(ICoordinateFactory<TCoordinate> cf, TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2)
        {
            TCoordinate intersectionPoint;
            try
            {
                TCoordinate hP1 = cf.Homogenize(p1);
                TCoordinate hP2 = cf.Homogenize(p2);
                TCoordinate hQ1 = cf.Homogenize(q1);
                TCoordinate hQ2 = cf.Homogenize(q2);

                intersectionPoint = intersectHomogeneous(hP1, hP2, hQ1, hQ2);
                //DoubleComponent x, y, w;
                //intersectionPoint.GetComponents(out x, out y, out w);
                Double x = (Double)intersectionPoint[Ordinates.X];
                Double y = (Double)intersectionPoint[Ordinates.Y];
                Double w = (Double)intersectionPoint[Ordinates.W];
                return cf.Create((Double)x / (Double)w, (Double)y / (Double)w);
            }
            catch (NotRepresentableException e)
            {
                Trace.Error("NTS", String.Format("[{0}] {1}", e.GetType(), e.Message));

                // compute an approximate result
                intersectionPoint = CentralEndpointIntersector<TCoordinate>
                    .GetIntersection(cf, p1, p2, q1, q2);
            }

            return intersectionPoint;
        }

        private static TCoordinate intersectHomogeneous(TCoordinate hP1,
                                                        TCoordinate hP2,
                                                        TCoordinate hQ1,
                                                        TCoordinate hQ2)
        {
            // create lines
            //xP = hP1.y * hP2.w - hP2.y * hP1.w;
            //yP = hP2.x * hP1.w - hP1.x * hP2.w;
            //wP = hP1.x * hP2.y - hP2.x * hP1.y;

            //xQ = hQ1.y * hQ2.w - hQ2.y * hQ1.w;
            //yQ = hQ2.x * hQ1.w - hQ1.x * hQ2.w;
            //wQ = hQ1.x * hQ2.y - hQ2.x * hQ1.y;

            // compute cross-products
            TCoordinate p = hP1.Cross(hP2);
            TCoordinate q = hQ1.Cross(hQ2);

            // intersect lines in projective space via homogeneous coordinate cross-product
            TCoordinate intersection = p.Cross(q);
            return intersection;
        }

        private static Intersection<TCoordinate> computeCollinearIntersection(Pair<TCoordinate> line0,
                                                                              Pair<TCoordinate> line1)
        {
            Boolean p1_q1_p2 = Extents<TCoordinate>.Intersects(line0.First,
                                                               line0.Second,
                                                               line1.First);
            Boolean p1_q2_p2 = Extents<TCoordinate>.Intersects(line0.First,
                                                               line0.Second,
                                                               line1.Second);
            Boolean q1_p1_q2 = Extents<TCoordinate>.Intersects(line1.First,
                                                               line1.Second,
                                                               line0.First);
            Boolean q1_p2_q2 = Extents<TCoordinate>.Intersects(line1.First,
                                                               line1.Second,
                                                               line0.Second);

            // colinear, where line0 contains line1
            if (p1_q1_p2 && p1_q2_p2)
            {
                return new Intersection<TCoordinate>(line1.First, line1.Second,
                                                     line0, line1,
                                                     false, false, false);
            }

            // colinear, where line1 contains line0
            if (q1_p1_q2 && q1_p2_q2)
            {
                return new Intersection<TCoordinate>(line0.First, line0.Second,
                                                     line0, line1,
                                                     false, false, false);
            }

            // line0 contains first point of line1 and line1 contains first point of line0
            if (p1_q1_p2 && q1_p1_q2)
            {
                return line1.First.Equals(line0.First)
                           ? new Intersection<TCoordinate>(line0.First,
                                                           line0, line1,
                                                           false, false, false)
                           : new Intersection<TCoordinate>(line0.First, line1.First,
                                                           line0, line1,
                                                           false, false, false);
            }

            // line0 contains first point of line1 and line1 contains second point of line0
            if (p1_q1_p2 && q1_p2_q2)
            {
                return line1.First.Equals(line0.Second)
                           ? new Intersection<TCoordinate>(line0.Second,
                                                           line0, line1,
                                                           false, false, false)
                           : new Intersection<TCoordinate>(line0.Second, line1.First,
                                                           line0, line1,
                                                           false, false, false);
            }

            // line0 contains second point of line1 and line1 contains first point of line0
            if (p1_q2_p2 && q1_p1_q2)
            {
                return line1.Second.Equals(line0.First)
                           ? new Intersection<TCoordinate>(line0.First,
                                                           line0, line1,
                                                           false, false, false)
                           : new Intersection<TCoordinate>(line0.First, line1.Second,
                                                           line0, line1,
                                                           false, false, false);
            }

            if (p1_q2_p2 && q1_p2_q2)
            {
                return line1.Second.Equals(line0.Second)
                           ? new Intersection<TCoordinate>(line0.Second,
                                                           line0, line1,
                                                           false, false, false)
                           : new Intersection<TCoordinate>(line0.Second, line1.Second,
                                                           line0, line1,
                                                           false, false, false);
            }

            return new Intersection<TCoordinate>();
        }

        /// <summary>
        /// Normalize the supplied coordinates to
        /// so that the midpoint of their intersection envelope
        /// lies at the origin.
        /// </summary>
        private void normalizeToExtentCenter(ICoordinateFactory<TCoordinate> cf,
                                             ref TCoordinate n00, ref TCoordinate n01,
                                             ref TCoordinate n10, ref TCoordinate n11,
                                             out TCoordinate normPt)
        {
            //DoubleComponent n00X, n00Y, n01X, n01Y, n10X, n10Y, n11X, n11Y;
            //n00.GetComponents(out n00X, out n00Y);
            //n01.GetComponents(out n01X, out n01Y);
            //n10.GetComponents(out n10X, out n10Y);
            //n11.GetComponents(out n11X, out n11Y);
            var n00xy = n00.ToArray2D();
            var n01xy = n01.ToArray2D();
            var n10xy = n10.ToArray2D();
            var n11xy = n11.ToArray2D();

            ////Double minX0 = n00[Ordinates.X] < n01[Ordinates.X] ? n00[Ordinates.X] : n01[Ordinates.X];
            ////Double minY0 = n00[Ordinates.Y] < n01[Ordinates.Y] ? n00[Ordinates.Y] : n01[Ordinates.Y];
            ////Double maxX0 = n00[Ordinates.X] > n01[Ordinates.X] ? n00[Ordinates.X] : n01[Ordinates.X];
            ////Double maxY0 = n00[Ordinates.Y] > n01[Ordinates.Y] ? n00[Ordinates.Y] : n01[Ordinates.Y];
            //DoubleComponent minX0 = n00X.LessThan(n01X) ? n00X : n01X;
            //DoubleComponent minY0 = n00Y.LessThan(n01Y) ? n00Y : n01Y;
            //DoubleComponent maxX0 = n00X.GreaterThan(n01X) ? n00X : n01X;
            //DoubleComponent maxY0 = n00Y.GreaterThan(n01Y) ? n00Y : n01Y;
            var minX0 = n00xy[0] < n01xy[0] ? n00xy[0] : n01xy[0];
            var minY0 = n00xy[1] < n01xy[1] ? n00xy[1] : n01xy[1];
            var maxX0 = n00xy[0] > n01xy[0] ? n00xy[0] : n01xy[0];
            var maxY0 = n00xy[1] > n01xy[1] ? n00xy[1] : n01xy[1];

            ////Double minX1 = n10[Ordinates.X] < n11[Ordinates.X] ? n10[Ordinates.X] : n11[Ordinates.X];
            ////Double minY1 = n10[Ordinates.Y] < n11[Ordinates.Y] ? n10[Ordinates.Y] : n11[Ordinates.Y];
            ////Double maxX1 = n10[Ordinates.X] > n11[Ordinates.X] ? n10[Ordinates.X] : n11[Ordinates.X];
            ////Double maxY1 = n10[Ordinates.Y] > n11[Ordinates.Y] ? n10[Ordinates.Y] : n11[Ordinates.Y];
            //DoubleComponent minX1 = n10X.LessThan(n11X) ? n10X : n11X;
            //DoubleComponent minY1 = n10Y.LessThan(n11Y) ? n10Y : n11Y;
            //DoubleComponent maxX1 = n10X.GreaterThan(n11X) ? n10X : n11X;
            //DoubleComponent maxY1 = n10Y.GreaterThan(n11Y) ? n10Y : n11Y;
            var minX1 = n10xy[0] < n11xy[0] ? n10xy[0] : n11xy[0];
            var minY1 = n10xy[1] < n11xy[1] ? n10xy[1] : n11xy[1];
            var maxX1 = n10xy[0] > n11xy[0] ? n10xy[0] : n11xy[0];
            var maxY1 = n10xy[1] > n11xy[1] ? n10xy[1] : n11xy[1];

            var intMinX = minX0 > minX1 ? minX0 : minX1;
            var intMaxX = maxX0 < maxX1 ? maxX0 : maxX1;
            var intMinY = minY0 > minY1 ? minY0 : minY1;
            var intMaxY = maxY0 < maxY1 ? maxY0 : maxY1;
            //DoubleComponent intMinX = minX0.GreaterThan(minX1) ? minX0 : minX1;
            //DoubleComponent intMaxX = maxX0.LessThan(maxX1) ? maxX0 : maxX1;
            //DoubleComponent intMinY = minY0.GreaterThan(minY1) ? minY0 : minY1;
            //DoubleComponent intMaxY = maxY0.LessThan(maxY1) ? maxY0 : maxY1;

            //Double intMidX = (intMinX + intMaxX)/2.0;
            //Double intMidY = (intMinY + intMaxY)/2.0;
            var intMidX = (intMinX + intMaxX) / 2.0;
            var intMidY = (intMinY + intMaxY) / 2.0;
            normPt = cf.Create(intMidX, intMidY);

            //Double n00X = n00[Ordinates.X] - intMidX;
            //Double n00Y = n00[Ordinates.Y] - intMidY;
            //Double n01X = n01[Ordinates.X] - intMidX;
            //Double n01Y = n01[Ordinates.Y] - intMidY;
            //Double n10X = n10[Ordinates.X] - intMidX;
            //Double n10Y = n10[Ordinates.Y] - intMidY;
            //Double n11X = n11[Ordinates.X] - intMidX;
            //Double n11Y = n11[Ordinates.Y] - intMidY;

            //n00X -= intMidX;
            //n00Y -= intMidY;
            //n01X -= intMidX;
            //n01Y -= intMidY;
            //n10X -= intMidX;
            //n10Y -= intMidY;
            //n11X -= intMidX;
            //n11Y -= intMidY;
            n00xy[0] -= intMidX;
            n00xy[1] -= intMidY;
            n01xy[0] -= intMidX;
            n01xy[1] -= intMidY;
            n10xy[0] -= intMidX;
            n10xy[1] -= intMidY;
            n11xy[0] -= intMidX;
            n11xy[1] -= intMidY;

            //n00 = cf.Create((Double)n00X, (Double)n00Y);
            //n01 = cf.Create((Double)n01X, (Double)n01Y);
            //n10 = cf.Create((Double)n10X, (Double)n10Y);
            //n11 = cf.Create((Double)n11X, (Double)n11Y);
            n00 = cf.Create(n00xy[0], n00xy[1]);
            n01 = cf.Create(n01xy[0], n01xy[1]);
            n10 = cf.Create(n10xy[0], n10xy[1]);
            n11 = cf.Create(n11xy[0], n11xy[1]);
        }

        /// <summary>
        /// Test whether a point lies in the envelopes of both input segments.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the input point lies within both input segment envelopes.
        /// </returns>
        /// <remarks>
        /// A correctly computed intersection point should return <see langword="true"/>
        /// for this test.
        /// Since this test is for debugging purposes only, no attempt is
        /// made to optimize the envelope test.
        /// </remarks>
        private static Boolean isInSegmentExtents(IGeometryFactory<TCoordinate> geoFactory,
                                                  TCoordinate coordinate,
                                                  Pair<TCoordinate> line0,
                                                  Pair<TCoordinate> line1)
        {
            IExtents<TCoordinate> extent0 = new Extents<TCoordinate>(geoFactory,
                                                                     line0.First,
                                                                     line0.Second);
            IExtents<TCoordinate> extent1 = new Extents<TCoordinate>(geoFactory,
                                                                     line1.First,
                                                                     line1.Second);
            return extent0.Contains(coordinate) && extent1.Contains(coordinate);
        }

        // [codekaizen 2008-01-15]  Method 'normalizeToMinimum' is not used in JTS
        //                          /JTS/src/com/vividsolutions/jts/algorithm/RobustLineIntersector.java:1.37

        //private void normalizeToMinimum(ref TCoordinate n1, ref TCoordinate n2, ref TCoordinate n3, ref TCoordinate n4,
        //                                out TCoordinate normPt)
        //{
        //    Double normX = SmallestInAbsValue(n1[Ordinates.X], n2[Ordinates.X], n3[Ordinates.X], n4[Ordinates.X]);
        //    Double normY = SmallestInAbsValue(n1[Ordinates.Y], n2[Ordinates.Y], n3[Ordinates.Y], n4[Ordinates.Y]);
        //    Double n1X = normX - n1[Ordinates.X];
        //    Double n1Y = normY - n1[Ordinates.Y];
        //    Double n2X = normX - n2[Ordinates.X];
        //    Double n2Y = normY - n2[Ordinates.Y];
        //    Double n3X = normX - n3[Ordinates.X];
        //    Double n3Y = normY - n3[Ordinates.Y];
        //    Double n4X = normX - n4[Ordinates.X];
        //    Double n4Y = normY - n4[Ordinates.Y];

        //    normPt = new Coordinate(normX, normY);
        //    n1 = new Coordinate(n1X, n1Y);
        //    n2 = new Coordinate(n2X, n2Y);
        //    n3 = new Coordinate(n3X, n3Y);
        //    n4 = new Coordinate(n4X, n4Y);
        //}

        // [codekaizen 2008-01-15]  Method 'smallestInAbsValue' only used in 'normalizeToMinimum',
        //                          which is unused in
        //                          /JTS/src/com/vividsolutions/jts/algorithm/RobustLineIntersector.java:1.37

        //private Double smallestInAbsValue(Double x1, Double x2, Double x3, Double x4)
        //{
        //    Double x = x1;
        //    Double xabs = Math.Abs(x);

        //    if (Math.Abs(x2) < xabs)
        //    {
        //        x = x2;
        //        xabs = Math.Abs(x2);
        //    }

        //    if (Math.Abs(x3) < xabs)
        //    {
        //        x = x3;
        //        xabs = Math.Abs(x3);
        //    }

        //    if (Math.Abs(x4) < xabs)
        //    {
        //        x = x4;
        //    }

        //    return x;
        //}
    }
}