using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Buffer
{
    ///<summary>
    /// Stress tests <see cref="DepthSegment"/> to determine if the compare contract is maintained.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DepthSegmentStressTest : PerformanceTestCase
    {
        public DepthSegmentStressTest()
            :base("DepthSegmentStressTest")
        {
            RunSize = new int[] {20};
            RunIterations = 100;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(DepthSegmentStressTest));
        }

        public override void StartRun(int size)
        {
            Console.WriteLine("Running with size " + size);
            iter = 0;
        }

        private int iter = 0;
        private readonly Random _rnd = new Random(5432);

        public void XXrunSort()
        {
            Console.WriteLine("Iter # " + iter++);
            // do test work here
            var segs = CreateRandomDepthSegments(100);
            segs.Sort();
        }

        public void RunMin()
        {
            Console.WriteLine("Iter # " + iter++);
            // do test work here
            var segs = CreateRandomDepthSegments(100);
            var min = segs.Min();
        }

        public void RunCompare()
        {
            Console.WriteLine("Iter # " + iter++);
            var seg1 = CreateRandomDepthSegment();
            var seg2 = CreateRandomDepthSegment();
            var seg3 = CreateRandomDepthSegment();

            // do test work here
            bool fails = false;
            if (!IsSymmetric(seg1, seg2))
                fails = true;
            if (!IsTransitive(seg1, seg2, seg3))
                fails = true;

            if (fails)
            {
                Console.WriteLine("FAILS!");
                throw new Exception("FAILS!");
            }

        }

        private static bool IsSymmetric(DepthSegment seg1, DepthSegment seg2)
        {
            int cmp12 = seg1.CompareTo(seg2);
            int cmp21 = seg2.CompareTo(seg1);
            return cmp12 == -cmp21;
        }

        private bool IsTransitive(DepthSegment seg1, DepthSegment seg2, DepthSegment seg3)
        {
            int cmp12 = seg1.CompareTo(seg2);
            int cmp23 = seg2.CompareTo(seg3);
            int cmp13 = seg1.CompareTo(seg3);
            if (cmp12 > 0 && cmp23 > 0)
            {
                if (cmp13 <= 0)
                {
                    Console.WriteLine(seg1 + " " + seg2 + " " + seg3);
                    return false;
                }
            }
            return true;
        }

        private List<DepthSegment> CreateRandomDepthSegments(int n)
        {
            var segs = new List<DepthSegment>();
            for (int i = 0; i < n; i++)
            {
                segs.Add(CreateRandomDepthSegment());
            }
            return segs;
        }

        private int Randint(int max)
        {
            return _rnd.Next(max);
        }

        private DepthSegment CreateRandomDepthSegment()
        {
            double scale = 10;
            int max = 10;
            double x0 = Randint(max);
            double y0 = Randint(max);
            double ang = 2*Math.PI*_rnd.NextDouble();
            double x1 = Math.Round(x0 + max*Math.Cos(ang), MidpointRounding.AwayFromZero);
            double y1 = Math.Round(y0 + max*Math.Sin(ang), MidpointRounding.AwayFromZero);
            var seg = new LineSegment(x0, y0, x1, y1);
            seg.Normalize();
            return new DepthSegment(seg, 0);
        }

        private double Round(double x, double scale)
        {
            return Math.Round(x*scale)/scale;
        }

        /**
         * A segment from a directed edge which has been assigned a depth value
         * for its sides.
         */
        private class DepthSegment : IComparable<DepthSegment>
        {
            private LineSegment upwardSeg;
            private int leftDepth;

            public DepthSegment(LineSegment seg, int depth)
            {
                // input seg is assumed to be normalized
                upwardSeg = new LineSegment(seg);
                //upwardSeg.normalize();
                this.leftDepth = depth;
            }

            /**
             * Defines a comparison operation on DepthSegments
             * which orders them left to right
             *
             * <pre>
             * DS1 < DS2   if   DS1.seg is left of DS2.seg
             * DS1 > DS2   if   DS1.seg is right of DS2.seg
             * </pre>
             *
             * @param obj
             * @return the comparison value
             */

            public int CompareTo(DepthSegment other)
            {
                if (!envelopesOverlap(upwardSeg, other.upwardSeg))
                    return upwardSeg.CompareTo(other.upwardSeg);
                // check orientations
                int orientIndex = upwardSeg.OrientationIndex(other.upwardSeg);
                if (orientIndex != 0) return orientIndex;
                orientIndex = -other.upwardSeg.OrientationIndex(upwardSeg);
                if (orientIndex != 0) return orientIndex;
                // segments cross or are collinear.  Use segment ordering
                return upwardSeg.CompareTo(other.upwardSeg);

            }

            public int XcompareTo(object obj)
            {
                var other = (DepthSegment) obj;

                // if segments are collinear and vertical compare endpoints
                if (isVertical() && other.isVertical()
                    && upwardSeg.P0.X == other.upwardSeg.P0.X)
                    return compareX(this.upwardSeg, other.upwardSeg);
                // check if segments are trivially ordered along X
                if (upwardSeg.MaxX <= other.upwardSeg.MinX) return -1;
                if (upwardSeg.MinX >= other.upwardSeg.MaxX) return 1;
                /**
                 * try and compute a determinate orientation for the segments.
                 * Test returns 1 if other is left of this (i.e. this > other)
                 */
                int orientIndex = upwardSeg.OrientationIndex(other.upwardSeg);
                // if orientation is determinate, return it
                if (orientIndex != 0)
                    return orientIndex;

                /**
                 * If comparison between this and other is indeterminate,
                 * try the opposite call order.
                 * orientationIndex value is 1 if this is left of other,
                 * so have to flip sign to get proper comparison value of
                 * -1 if this is leftmost
                 */
                if (orientIndex == 0)
                    orientIndex = -1*other.upwardSeg.OrientationIndex(upwardSeg);

                // if orientation is determinate, return it
                if (orientIndex != 0)
                    return orientIndex;

                // otherwise, segs must be collinear - sort based on minimum X value
                return compareX(this.upwardSeg, other.upwardSeg);
            }

            private bool isVertical()
            {
                return upwardSeg.P0.X == upwardSeg.P1.X;
            }

            private bool envelopesOverlap(LineSegment seg1, LineSegment seg2)
            {
                if (seg1.MaxX <= seg2.MinX) return false;
                if (seg2.MaxX <= seg1.MinX) return false;
                if (seg1.MaxY <= seg2.MinY) return false;
                if (seg2.MaxX <= seg1.MinY) return false;
                return true;
            }

            /**
     * Compare two collinear segments for left-most ordering.
     * If segs are vertical, use vertical ordering for comparison.
     * If segs are equal, return 0.
     * Segments are assumed to be directed so that the second coordinate is >= to the first
     * (e.g. up and to the right).
     *
     * @param seg0 a segment to compare
     * @param seg1 a segment to compare
     * @return
     */

            private int compareX(LineSegment seg0, LineSegment seg1)
            {
                int compare0 = seg0.P0.CompareTo(seg1.P0);
                if (compare0 != 0)
                    return compare0;
                return seg0.P1.CompareTo(seg1.P1);

            }

            public override string ToString()
            {
                return upwardSeg.ToString();
            }

        }

    }
}