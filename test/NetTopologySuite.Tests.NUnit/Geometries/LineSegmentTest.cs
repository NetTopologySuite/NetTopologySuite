using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test named predicate short-circuits
     */
    [TestFixture]
    public class LineSegmentTest
    {
        WKTReader rdr = new WKTReader();

        private static double ROOT2 = Math.Sqrt(2);

        [Test]
        public void TestProjectionFactor()
        {
            // zero-length line
            var seg = new LineSegment(10, 0, 10, 0);
            Assert.IsTrue(double.IsNaN(seg.ProjectionFactor(new Coordinate(11, 0))));

            var seg2 = new LineSegment(10, 0, 20, 0);
            Assert.IsTrue(seg2.ProjectionFactor(new Coordinate(11, 0)) == 0.1);

        }

        [Test]
        public void TestLineIntersection()
        {
            // simple case
            CheckLineIntersection(
                0, 0, 10, 10,
                0, 10, 10, 0,
                5, 5);

            //Almost collinear - See JTS GitHub issue #464
            CheckLineIntersection(
                35613471.6165017, 4257145.306132293, 35613477.7705378, 4257160.528222711,
                35613477.77505724, 4257160.539653536, 35613479.85607389, 4257165.92369170,
                35613477.772841461, 4257160.5339209242);
        }

        private const double MAX_ABS_ERROR_INTERSECTION = 1e-5;

        private static void CheckLineIntersection(double p1x, double p1y, double p2x, double p2y,
            double q1x, double q1y, double q2x, double q2y,
            double expectedx, double expectedy)
        {
            var seg1 = new LineSegment(p1x, p1y, p2x, p2y);
            var seg2 = new LineSegment(q1x, q1y, q2x, q2y);

            var actual = seg1.LineIntersection(seg2);
            var expected = new Coordinate(expectedx, expectedy);
            double dist = actual.Distance(expected);
            //System.out.println("Expected: " + expected + "  Actual: " + actual + "  Dist = " + dist);
            Assert.That(dist, Is.LessThanOrEqualTo(MAX_ABS_ERROR_INTERSECTION));
        }

        [Test]
        public void TestOffsetPoint()
        {
            CheckOffsetPoint(0, 0, 10, 10, 0.0, ROOT2, -1, 1);
            CheckOffsetPoint(0, 0, 10, 10, 0.0, -ROOT2, 1, -1);

            CheckOffsetPoint(0, 0, 10, 10, 1.0, ROOT2, 9, 11);
            CheckOffsetPoint(0, 0, 10, 10, 0.5, ROOT2, 4, 6);

            CheckOffsetPoint(0, 0, 10, 10, 0.5, -ROOT2, 6, 4);
            CheckOffsetPoint(0, 0, 10, 10, 0.5, -ROOT2, 6, 4);

            CheckOffsetPoint(0, 0, 10, 10, 2.0, ROOT2, 19, 21);
            CheckOffsetPoint(0, 0, 10, 10, 2.0, -ROOT2, 21, 19);

            CheckOffsetPoint(0, 0, 10, 10, 2.0, 5 * ROOT2, 15, 25);
            CheckOffsetPoint(0, 0, 10, 10, -2.0, 5 * ROOT2, -25, -15);

        }

        [Test]
        public void TestOffsetLine()
        {
            CheckOffsetLine(0, 0, 10, 10, 0, 0, 0, 10, 10);

            CheckOffsetLine(0, 0, 10, 10, ROOT2, -1, 1, 9, 11);
            CheckOffsetLine(0, 0, 10, 10, -ROOT2, 1, -1, 11, 9);
        }

        static void CheckOffsetPoint(double x0, double y0, double x1, double y1, double segFrac, double offset,
                double expectedX, double expectedY)
        {
            var seg = new LineSegment(x0, y0, x1, y1);
            var p = seg.PointAlongOffset(segFrac, offset);

            Assert.IsTrue(EqualsTolerance(new Coordinate(expectedX, expectedY), p, 0.000001));
        }

        static void CheckOffsetLine(double x0, double y0, double x1, double y1, double offset,
            double expectedX0, double expectedY0, double expectedX1, double expectedY1)
        {
            var seg = new LineSegment(x0, y0, x1, y1);
            var actual = seg.Offset(offset);

            Assert.IsTrue(EqualsTolerance(new Coordinate(expectedX0, expectedY0), actual.P0, 0.000001));
            Assert.IsTrue(EqualsTolerance(new Coordinate(expectedX1, expectedY1), actual.P1, 0.000001));
        }

        public static bool EqualsTolerance(Coordinate p0, Coordinate p1, double tolerance)
        {
            if (Math.Abs(p0.X - p1.X) > tolerance) return false;
            if (Math.Abs(p0.Y - p1.Y) > tolerance) return false;
            return true;
        }

        [Test]
        public void TestReflect()
        {
            CheckReflect(0, 0, 10, 10, 1, 2, 2, 1);
            CheckReflect(0, 1, 10, 1, 1, 2, 1, 0);
        }

        void CheckReflect(double x0, double y0, double x1, double y1, double x, double y,
            double expectedX, double expectedY)
        {
            var seg = new LineSegment(x0, y0, x1, y1);
            var p = seg.Reflect(new Coordinate(x, y));
            Assert.That(EqualsTolerance(new Coordinate(expectedX, expectedY), p, 0.000001));
        }


        [Test]
        public void TestOrientationIndexCoordinate()
        {
            var seg = new LineSegment(0, 0, 10, 10);
            CheckOrientationIndex(seg, 10, 11, 1);
            CheckOrientationIndex(seg, 10, 9, -1);

            CheckOrientationIndex(seg, 11, 11, 0);

            CheckOrientationIndex(seg, 11, 11.0000001, 1);
            CheckOrientationIndex(seg, 11, 10.9999999, -1);

            CheckOrientationIndex(seg, -2, -1.9999999, 1);
            CheckOrientationIndex(seg, -2, -2.0000001, -1);
        }

        [Test]
        public void TestOrientationIndexSegment()
        {
            var seg = new LineSegment(100, 100, 110, 110);

            CheckOrientationIndex(seg, 100, 101, 105, 106, 1);
            CheckOrientationIndex(seg, 100, 99, 105, 96, -1);

            CheckOrientationIndex(seg, 200, 200, 210, 210, 0);

        }

        void CheckOrientationIndex(double x0, double y0, double x1, double y1, double px, double py,
            int expectedOrient)
        {
            var seg = new LineSegment(x0, y0, x1, y1);
            CheckOrientationIndex(seg, px, py, expectedOrient);
        }

        void CheckOrientationIndex(LineSegment seg,
            double px, double py,
            int expectedOrient)
        {
            var p = new Coordinate(px, py);
            int orient = seg.OrientationIndex(p);
            Assert.IsTrue(orient == expectedOrient);
        }

        void CheckOrientationIndex(LineSegment seg,
            double s0x, double s0y,
            double s1x, double s1y,
            int expectedOrient)
        {
            var seg2 = new LineSegment(s0x, s0y, s1x, s1y);
            int orient = seg.OrientationIndex(seg2);
            Assert.IsTrue(orient == expectedOrient);
        }
    }
}
