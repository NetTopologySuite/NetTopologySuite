using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A robust version of <see cref="LineIntersector{TCoordinate}"/>.
    /// </summary>
    public class RobustLineIntersector<TCoordinate> : LineIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public override void ComputeIntersection(TCoordinate p, TCoordinate p1, TCoordinate p2)
        {
            IsProper = false;

            // do between check first, since it is faster than the orientation test
            if (Extents<TCoordinate>.Intersects(p1, p2, p))
            {
                if ((CGAlgorithms<TCoordinate>.OrientationIndex(p1, p2, p) == 0) &&
                    (CGAlgorithms<TCoordinate>.OrientationIndex(p2, p1, p) == 0))
                {
                    IsProper = true;

                    if (p.Equals(p1) || p.Equals(p2))
                    {
                        IsProper = false;
                    }

                    IntersectionType = LineIntersectionType.Intersects;
                    return;
                }
            }

            IntersectionType = LineIntersectionType.DoesNotIntersect;
        }

        public override LineIntersectionType ComputeIntersect(TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2)
        {
            IsProper = false;

            // first try a fast test to see if the envelopes of the lines intersect
            if (!Extents<TCoordinate>.Intersects(p1, p2, q1, q2))
            {
                return LineIntersectionType.DoesNotIntersect;
            }

            // for each endpoint, compute which side of the other segment it lies
            // if both endpoints lie on the same side of the other segment,
            // the segments do not intersect
            Int32 Pq1 = CGAlgorithms<TCoordinate>.OrientationIndex(p1, p2, q1);
            Int32 Pq2 = CGAlgorithms<TCoordinate>.OrientationIndex(p1, p2, q2);

            if ((Pq1 > 0 && Pq2 > 0) || (Pq1 < 0 && Pq2 < 0))
            {
                return LineIntersectionType.DoesNotIntersect;
            }

            Int32 Qp1 = CGAlgorithms<TCoordinate>.OrientationIndex(q1, q2, p1);
            Int32 Qp2 = CGAlgorithms<TCoordinate>.OrientationIndex(q1, q2, p2);

            if ((Qp1 > 0 && Qp2 > 0) || (Qp1 < 0 && Qp2 < 0))
            {
                return LineIntersectionType.DoesNotIntersect;
            }

            Boolean collinear = (Pq1 == 0 && Pq2 == 0 && Qp1 == 0 && Qp2 == 0);

            if (collinear)
            {
                return computeCollinearIntersection(p1, p2, q1, q2);
            }

            /*
            *  Check if the intersection is an endpoint. If it is, copy the endpoint as
            *  the intersection point. Copying the point rather than computing it
            *  ensures the point has the exact value, which is important for
            *  robustness. It is sufficient to simply check for an endpoint which is on
            *  the other line, since at this point we know that the inputLines must
            *  intersect.
            */
            if (Pq1 == 0 || Pq2 == 0 || Qp1 == 0 || Qp2 == 0)
            {
                IsProper = false;

                if (Pq1 == 0)
                {
                    PointA = new TCoordinate(q1);
                }

                if (Pq2 == 0)
                {
                    PointA = new TCoordinate(q2);
                }

                if (Qp1 == 0)
                {
                    PointA = new TCoordinate(p1);
                }

                if (Qp2 == 0)
                {
                    PointA = new TCoordinate(p2);
                }
            }
            else
            {
                IsProper = true;
                PointA = Intersection(p1, p2, q1, q2);
            }

            return LineIntersectionType.Intersects;
        }

        /// <summary> 
        /// This method computes the actual value of the intersection point.
        /// To obtain the maximum precision from the intersection calculation,
        /// the coordinates are normalized by subtracting the minimum
        /// ordinate values (in absolute value).  This has the effect of
        /// removing common significant digits from the calculation to
        /// maintain more bits of precision.
        /// </summary>
        private TCoordinate Intersection(TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2)
        {
            TCoordinate n1 = new TCoordinate(p1);
            TCoordinate n2 = new TCoordinate(p2);
            TCoordinate n3 = new TCoordinate(q1);
            TCoordinate n4 = new TCoordinate(q2);
            TCoordinate normPt;
            normalizeToExtentCenter(ref n1, ref n2, ref n3, ref n4, out normPt);

            TCoordinate intersection = new TCoordinate();

            try
            {
                intersection = HCoordinate.Intersection(n1, n2, n3, n4);
            }
            catch (NotRepresentableException)
            {
                Assert.ShouldNeverReachHere("Coordinate for intersection is not calculable");
            }

            intersection = new TCoordinate(
                intersection[Ordinates.X] + normPt[Ordinates.X],
                intersection[Ordinates.Y] + normPt[Ordinates.Y]);

            /*
             *
             * MD - May 4 2005 - This is still a problem.  Here is a failure case:
             *
             * LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)
             * LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)
             * Int32 point = (2097408.2633752143,1144595.8008114607)
             */
            if (!isInSegmentEnvelopes(intersection))
            {
                Trace.WriteLine("Intersection outside segment envelopes: " + intersection);
            }

            /*
            // disabled until a better solution is found
            if(!IsInSegmentEnvelopes(intPt)) 
            {
                Trace.WriteLine("first value outside segment envelopes: " + intPt);

                IteratedBisectionIntersector ibi = new IteratedBisectionIntersector(p1, p2, q1, q2);
                intPt = ibi.Intersection;
            }
            if(!IsInSegmentEnvelopes(intPt)) 
            {
                Trace.WriteLine("ERROR - outside segment envelopes: " + intPt);

                IteratedBisectionIntersector ibi = new IteratedBisectionIntersector(p1, p2, q1, q2);
                Coordinate testPt = ibi.Intersection;
            }
            */

            if (PrecisionModel != null)
            {
                PrecisionModel.MakePrecise(intersection);
            }

            return intersection;
        }

        //private void NormalizeToMinimum(ref TCoordinate n1, ref TCoordinate n2, ref TCoordinate n3, ref TCoordinate n4,
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

        private LineIntersectionType computeCollinearIntersection(TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2)
        {
            Boolean p1q1p2 = Extents<TCoordinate>.Intersects(p1, p2, q1);
            Boolean p1q2p2 = Extents<TCoordinate>.Intersects(p1, p2, q2);
            Boolean q1p1q2 = Extents<TCoordinate>.Intersects(q1, q2, p1);
            Boolean q1p2q2 = Extents<TCoordinate>.Intersects(q1, q2, p2);

            if (p1q1p2 && p1q2p2)
            {
                PointA = q1;
                PointB = q2;
                return LineIntersectionType.Collinear;
            }

            if (q1p1q2 && q1p2q2)
            {
                PointA = p1;
                PointB = p2;
                return LineIntersectionType.Collinear;
            }

            if (p1q1p2 && q1p1q2)
            {
                PointA = q1;
                PointB = p1;
                return q1.Equals(p1) && !p1q2p2 && !q1p2q2
                           ? LineIntersectionType.Intersects
                           : LineIntersectionType.Collinear;
            }

            if (p1q1p2 && q1p2q2)
            {
                PointA = q1;
                PointB = p2;
                return q1.Equals(p2) && !p1q2p2 && !q1p1q2
                           ? LineIntersectionType.Intersects
                           : LineIntersectionType.Collinear;
            }

            if (p1q2p2 && q1p1q2)
            {
                PointA = q2;
                PointB = p1;
                return q2.Equals(p1) && !p1q1p2 && !q1p2q2
                           ? LineIntersectionType.Intersects
                           : LineIntersectionType.Collinear;
            }

            if (p1q2p2 && q1p2q2)
            {
                PointA = q2;
                PointB = p2;
                return q2.Equals(p2) && !p1q1p2 && !q1p1q2
                           ? LineIntersectionType.Intersects
                           : LineIntersectionType.Collinear;
            }

            return LineIntersectionType.DoesNotIntersect;
        }

        /// <summary>
        ///  Normalize the supplied coordinates to
        /// so that the midpoint of their intersection envelope
        /// lies at the origin.
        /// </summary>
        private void normalizeToExtentCenter(ref TCoordinate n00, ref TCoordinate n01, 
            ref TCoordinate n10, ref TCoordinate n11, out TCoordinate normPt)
        {
            Double minX0 = n00[Ordinates.X] < n01[Ordinates.X] ? n00[Ordinates.X] : n01[Ordinates.X];
            Double minY0 = n00[Ordinates.Y] < n01[Ordinates.Y] ? n00[Ordinates.Y] : n01[Ordinates.Y];
            Double maxX0 = n00[Ordinates.X] > n01[Ordinates.X] ? n00[Ordinates.X] : n01[Ordinates.X];
            Double maxY0 = n00[Ordinates.Y] > n01[Ordinates.Y] ? n00[Ordinates.Y] : n01[Ordinates.Y];

            Double minX1 = n10[Ordinates.X] < n11[Ordinates.X] ? n10[Ordinates.X] : n11[Ordinates.X];
            Double minY1 = n10[Ordinates.Y] < n11[Ordinates.Y] ? n10[Ordinates.Y] : n11[Ordinates.Y];
            Double maxX1 = n10[Ordinates.X] > n11[Ordinates.X] ? n10[Ordinates.X] : n11[Ordinates.X];
            Double maxY1 = n10[Ordinates.Y] > n11[Ordinates.Y] ? n10[Ordinates.Y] : n11[Ordinates.Y];

            Double intMinX = minX0 > minX1 ? minX0 : minX1;
            Double intMaxX = maxX0 < maxX1 ? maxX0 : maxX1;
            Double intMinY = minY0 > minY1 ? minY0 : minY1;
            Double intMaxY = maxY0 < maxY1 ? maxY0 : maxY1;

            Double intMidX = (intMinX + intMaxX) / 2.0;
            Double intMidY = (intMinY + intMaxY) / 2.0;
            normPt = new TCoordinate(intMidX, intMidY);

            Double n00X = intMidX - n00[Ordinates.X];
            Double n00Y = intMidY - n00[Ordinates.Y];
            Double n01X = intMidX - n01[Ordinates.X];
            Double n01Y = intMidY - n01[Ordinates.Y];
            Double n10X = intMidX - n10[Ordinates.X];
            Double n10Y = intMidY - n10[Ordinates.Y];
            Double n11X = intMidX - n10[Ordinates.X];
            Double n11Y = intMidY - n10[Ordinates.Y];

            n00 = new TCoordinate(n00X, n00Y);
            n01 = new TCoordinate(n01X, n01Y);
            n10 = new TCoordinate(n10X, n10Y);
            n11 = new TCoordinate(n11X, n11Y);
        }

        /// <summary> 
        /// Test whether a point lies in the envelopes of both input segments.
        /// </summary>
        /// <returns><see langword="true"/> if the input point lies within both input segment envelopes.</returns>
        /// <remarks>
        /// A correctly computed intersection point should return <see langword="true"/>
        /// for this test.
        /// Since this test is for debugging purposes only, no attempt is
        /// made to optimize the envelope test.
        /// </remarks>
        private Boolean isInSegmentEnvelopes(TCoordinate coordinate)
        {
            TCoordinate[] line1 = GetLineForIndex(0);
            TCoordinate[] line2 = GetLineForIndex(1);
            IExtents<TCoordinate> env0 = new Extents<TCoordinate>(line1[0], line1[1]);
            IExtents<TCoordinate> env1 = new Extents<TCoordinate>(line2[0], line2[1]);
            return env0.Contains(coordinate) && env1.Contains(coordinate);
        }

        //private Double SmallestInAbsValue(Double x1, Double x2, Double x3, Double x4)
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