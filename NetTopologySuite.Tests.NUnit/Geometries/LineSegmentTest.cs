using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test named predicate short-circuits
     */
    [TestFixtureAttribute]
    public class LineSegmentTest
    {
        WKTReader rdr = new WKTReader();

        private static double ROOT2 = Math.Sqrt(2);

        [TestAttribute]
        public void TestProjectionFactor()
        {
            // zero-length line
            var seg = new LineSegment(10, 0, 10, 0);
            Assert.IsTrue(Double.IsNaN(seg.ProjectionFactor(new Coordinate(11, 0))));

            var seg2 = new LineSegment(10, 0, 20, 0);
            Assert.IsTrue(seg2.ProjectionFactor(new Coordinate(11, 0)) == 0.1);

        }

        [TestAttribute]
        public void TestOffset()
        {
            CheckOffset(0, 0, 10, 10, 0.0, ROOT2, -1, 1);
            CheckOffset(0, 0, 10, 10, 0.0, -ROOT2, 1, -1);

            CheckOffset(0, 0, 10, 10, 1.0, ROOT2, 9, 11);
            CheckOffset(0, 0, 10, 10, 0.5, ROOT2, 4, 6);

            CheckOffset(0, 0, 10, 10, 0.5, -ROOT2, 6, 4);
            CheckOffset(0, 0, 10, 10, 0.5, -ROOT2, 6, 4);

            CheckOffset(0, 0, 10, 10, 2.0, ROOT2, 19, 21);
            CheckOffset(0, 0, 10, 10, 2.0, -ROOT2, 21, 19);

            CheckOffset(0, 0, 10, 10, 2.0, 5 * ROOT2, 15, 25);
            CheckOffset(0, 0, 10, 10, -2.0, 5 * ROOT2, -25, -15);

        }

        void CheckOffset(double x0, double y0, double x1, double y1, double segFrac, double offset,
            double expectedX, double expectedY)
        {
            LineSegment seg = new LineSegment(x0, y0, x1, y1);
            Coordinate p = seg.PointAlongOffset(segFrac, offset);

            Assert.IsTrue(EqualsTolerance(new Coordinate(expectedX, expectedY), p, 0.000001));
        }

        public static bool EqualsTolerance(Coordinate p0, Coordinate p1, double tolerance)
        {
            if (Math.Abs(p0.X - p1.X) > tolerance) return false;
            if (Math.Abs(p0.Y - p1.Y) > tolerance) return false;
            return true;
        }

        [TestAttribute]
        public void TestOrientationIndexCoordinate()
        {
            LineSegment seg = new LineSegment(0, 0, 10, 10);
            CheckOrientationIndex(seg, 10, 11, 1);
            CheckOrientationIndex(seg, 10, 9, -1);

            CheckOrientationIndex(seg, 11, 11, 0);

            CheckOrientationIndex(seg, 11, 11.0000001, 1);
            CheckOrientationIndex(seg, 11, 10.9999999, -1);

            CheckOrientationIndex(seg, -2, -1.9999999, 1);
            CheckOrientationIndex(seg, -2, -2.0000001, -1);
        }

        [TestAttribute]
        public void TestOrientationIndexSegment()
        {
            LineSegment seg = new LineSegment(100, 100, 110, 110);

            CheckOrientationIndex(seg, 100, 101, 105, 106, 1);
            CheckOrientationIndex(seg, 100, 99, 105, 96, -1);

            CheckOrientationIndex(seg, 200, 200, 210, 210, 0);

        }

        void CheckOrientationIndex(double x0, double y0, double x1, double y1, double px, double py,
            int expectedOrient)
        {
            LineSegment seg = new LineSegment(x0, y0, x1, y1);
            CheckOrientationIndex(seg, px, py, expectedOrient);
        }

        void CheckOrientationIndex(LineSegment seg,
            double px, double py,
            int expectedOrient)
        {
            Coordinate p = new Coordinate(px, py);
            int orient = seg.OrientationIndex(p);
            Assert.IsTrue(orient == expectedOrient);
        }

        void CheckOrientationIndex(LineSegment seg,
            double s0x, double s0y,
            double s1x, double s1y,
            int expectedOrient)
        {
            LineSegment seg2 = new LineSegment(s0x, s0y, s1x, s1y);
            int orient = seg.OrientationIndex(seg2);
            Assert.IsTrue(orient == expectedOrient);
        }
    }
}