using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A robust version of <c>LineIntersector</c>.
    /// </summary>
    public class RobustLineIntersector : LineIntersector
    {
        /// <summary>
        /// 
        /// </summary>
        public RobustLineIntersector() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public override void ComputeIntersection(ICoordinate p, ICoordinate p1, ICoordinate p2) 
        {
            isProper = false;
            // do between check first, since it is faster than the orientation test
            if(Envelope.Intersects(p1, p2, p)) 
            {
                if( (CGAlgorithms.OrientationIndex(p1, p2, p) == 0) && 
                    (CGAlgorithms.OrientationIndex(p2, p1, p) == 0) ) 
                {
                    isProper = true;
                    if (p.Equals(p1) || p.Equals(p2)) 
                        isProper = false;                    
                    result = DoIntersect;
                    return;
                }
            }
            result = DontIntersect;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public override int ComputeIntersect(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2) 
        {            
            isProper = false;

            // first try a fast test to see if the envelopes of the lines intersect
            if (!Envelope.Intersects(p1, p2, q1, q2))
                return DontIntersect;

            // for each endpoint, compute which side of the other segment it lies
            // if both endpoints lie on the same side of the other segment,
            // the segments do not intersect
            int Pq1 = CGAlgorithms.OrientationIndex(p1, p2, q1);
            int Pq2 = CGAlgorithms.OrientationIndex(p1, p2, q2);

            if((Pq1 > 0 && Pq2 > 0) || (Pq1 < 0 && Pq2 < 0)) 
                return DontIntersect;            

            int Qp1 = CGAlgorithms.OrientationIndex(q1, q2, p1);
            int Qp2 = CGAlgorithms.OrientationIndex(q1, q2, p2);

            if ((Qp1 > 0 && Qp2 > 0) || (Qp1 < 0 && Qp2 < 0)) 
                return DontIntersect;            

            bool collinear = (Pq1 == 0 && Pq2 == 0 && Qp1 == 0 && Qp2 == 0);
            if (collinear) 
                return ComputeCollinearIntersection(p1, p2, q1, q2);

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
                isProper = false;
                if (Pq1 == 0) 
                    intPt[0] = new Coordinate(q1);            
                if (Pq2 == 0) 
                    intPt[0] = new Coordinate(q2);            
                if (Qp1 == 0) 
                    intPt[0] = new Coordinate(p1);
                if (Qp2 == 0) 
                    intPt[0] = new Coordinate(p2);
            }
            else
            {
                isProper = true;
                intPt[0] = Intersection(p1, p2, q1, q2);
            }
            return DoIntersect;
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        private int ComputeCollinearIntersection(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2) 
        {
            bool p1q1p2 = Envelope.Intersects(p1, p2, q1);
            bool p1q2p2 = Envelope.Intersects(p1, p2, q2);
            bool q1p1q2 = Envelope.Intersects(q1, q2, p1);
            bool q1p2q2 = Envelope.Intersects(q1, q2, p2);

            if (p1q1p2 && p1q2p2) 
            {
                intPt[0] = q1;
                intPt[1] = q2;
                return Collinear;
            }
            if (q1p1q2 && q1p2q2) 
            {
                intPt[0] = p1;
                intPt[1] = p2;
                return Collinear;
            }
            if (p1q1p2 && q1p1q2)
            {
                intPt[0] = q1;
                intPt[1] = p1;
                return q1.Equals(p1) && !p1q2p2 && !q1p2q2 ? DoIntersect : Collinear;
            }
            if (p1q1p2 && q1p2q2) 
            {
                intPt[0] = q1;
                intPt[1] = p2;
                return q1.Equals(p2) && !p1q2p2 && !q1p1q2 ? DoIntersect : Collinear;
            }
            if (p1q2p2 && q1p1q2) 
            {
                intPt[0] = q2;
                intPt[1] = p1;
                return q2.Equals(p1) && !p1q1p2 && !q1p2q2 ? DoIntersect : Collinear;
            }
            if (p1q2p2 && q1p2q2) 
            {
                intPt[0] = q2;
                intPt[1] = p2;
                return q2.Equals(p2) && !p1q1p2 && !q1p1q2 ? DoIntersect : Collinear;
            }
            return DontIntersect;
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
        private ICoordinate Intersection(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2)
        {
            ICoordinate n1 = new Coordinate(p1);
            ICoordinate n2 = new Coordinate(p2);
            ICoordinate n3 = new Coordinate(q1);
            ICoordinate n4 = new Coordinate(q2);
            ICoordinate normPt = new Coordinate();
            NormalizeToEnvCentre(n1, n2, n3, n4, normPt);

            ICoordinate intPt = null;
            try 
            {
                intPt = HCoordinate.Intersection(n1, n2, n3, n4);
            }
            catch (NotRepresentableException) 
            {
                Assert.ShouldNeverReachHere("Coordinate for intersection is not calculable");
            }

            intPt.X += normPt.X;
            intPt.Y += normPt.Y;

            /*
             *
             * MD - May 4 2005 - This is still a problem.  Here is a failure case:
             *
             * LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)
             * LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)
             * int point = (2097408.2633752143,1144595.8008114607)
             */
            if(!IsInSegmentEnvelopes(intPt)) 
              Trace.WriteLine("Intersection outside segment envelopes: " + intPt);
            
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

            if (precisionModel != null) 
                precisionModel.MakePrecise( intPt);
     
            return intPt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <param name="n3"></param>
        /// <param name="n4"></param>
        /// <param name="normPt"></param>
        private void NormalizeToMinimum(ICoordinate n1, ICoordinate n2, ICoordinate n3, ICoordinate n4, ICoordinate normPt)
        {
            normPt.X = SmallestInAbsValue(n1.X, n2.X, n3.X, n4.X);
            normPt.Y = SmallestInAbsValue(n1.Y, n2.Y, n3.Y, n4.Y);
            n1.X -= normPt.X; n1.Y -= normPt.Y;
            n2.X -= normPt.X; n2.Y -= normPt.Y;
            n3.X -= normPt.X; n3.Y -= normPt.Y;
            n4.X -= normPt.X; n4.Y -= normPt.Y;
        }

        /// <summary>
        ///  Normalize the supplied coordinates to
        /// so that the midpoint of their intersection envelope
        /// lies at the origin.
        /// </summary>
        /// <param name="n00"></param>
        /// <param name="n01"></param>
        /// <param name="n10"></param>
        /// <param name="n11"></param>
        /// <param name="normPt"></param>
        private void NormalizeToEnvCentre(ICoordinate n00, ICoordinate n01, ICoordinate n10, ICoordinate n11, ICoordinate normPt)
        {
            double minX0 = n00.X < n01.X ? n00.X : n01.X;
            double minY0 = n00.Y < n01.Y ? n00.Y : n01.Y;
            double maxX0 = n00.X > n01.X ? n00.X : n01.X;
            double maxY0 = n00.Y > n01.Y ? n00.Y : n01.Y;

            double minX1 = n10.X < n11.X ? n10.X : n11.X;
            double minY1 = n10.Y < n11.Y ? n10.Y : n11.Y;
            double maxX1 = n10.X > n11.X ? n10.X : n11.X;
            double maxY1 = n10.Y > n11.Y ? n10.Y : n11.Y;

            double intMinX = minX0 > minX1 ? minX0 : minX1;
            double intMaxX = maxX0 < maxX1 ? maxX0 : maxX1;
            double intMinY = minY0 > minY1 ? minY0 : minY1;
            double intMaxY = maxY0 < maxY1 ? maxY0 : maxY1;

            double intMidX = (intMinX + intMaxX) / 2.0;
            double intMidY = (intMinY + intMaxY) / 2.0;
            normPt.X = intMidX;
            normPt.Y = intMidY;           

            n00.X -= normPt.X; n00.Y -= normPt.Y;
            n01.X -= normPt.X; n01.Y -= normPt.Y;
            n10.X -= normPt.X; n10.Y -= normPt.Y;
            n11.X -= normPt.X; n11.Y -= normPt.Y;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="x3"></param>
        /// <param name="x4"></param>
        /// <returns></returns>
        private double SmallestInAbsValue(double x1, double x2, double x3, double x4)
        {
            double x = x1;
            double xabs = Math.Abs(x);
            if (Math.Abs(x2) < xabs) 
            {
                x = x2;
                xabs = Math.Abs(x2);
            }
            if (Math.Abs(x3) < xabs) 
            {
                x = x3;
                xabs = Math.Abs(x3);
            }
            if (Math.Abs(x4) < xabs) 
                x = x4;
            return x;
        }

        /// <summary> 
        /// Test whether a point lies in the envelopes of both input segments.
        /// A correctly computed intersection point should return <c>true</c>
        /// for this test.
        /// Since this test is for debugging purposes only, no attempt is
        /// made to optimize the envelope test.
        /// </summary>
        /// <param name="intPt"></param>
        /// <returns><c>true</c> if the input point lies within both input segment envelopes.</returns>
        private bool IsInSegmentEnvelopes(ICoordinate intPt)
        {
            IEnvelope env0 = new Envelope(inputLines[0, 0], inputLines[0, 1]);
            IEnvelope env1 = new Envelope(inputLines[1, 0], inputLines[1, 1]);
            return env0.Contains(intPt) && env1.Contains(intPt);
        }
    }
}
