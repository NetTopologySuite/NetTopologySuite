using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class IsCCWTest : GeometryTestCase
    {
        [Test]
        public void TestTooFewPoints()
        {
            var pts = new Coordinate[] {new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 2)};
            bool hasError = false;
            try
            {
                bool isCCW = Orientation.IsCCW(pts);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                hasError = true;
            }
            Assert.IsTrue(hasError);
        }

        [Test]
        public void TestCCW()
        {
            CheckOrientationCcw(true, "POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
        }

        [Test]
        public void TestRingCW()
        {
            CheckOrientationCcw(false, "POLYGON ((60 180, 140 240, 100 180, 140 120, 60 180))");
        }

        [Test]
        public void TestCCWSmall()
        {
            CheckOrientationCcw(true, "POLYGON ((1 1, 9 1, 5 9, 1 1))");
        }

        [Test]
        public void TestDuplicateTopPoint()
        {
            CheckOrientationCcw(true, "POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
        }

        [Test]
        public void TestFlatTopSegment()
        {
            CheckOrientationCcw(false, "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
        }

        [Test]
        public void TestFlatMultipleTopSegment()
        {
            CheckOrientationCcw(false, "POLYGON ((100 200, 127 200, 151 200, 173 200, 200 200, 100 100, 100 200))");
        }

        [Test]
        public void TestDegenerateRingHorizontal()
        {
            CheckOrientationCcw(false, "POLYGON ((100 200, 100 200, 200 200, 100 200))");
        }

        [Test]
        public void TestDegenerateRingAngled()
        {
            CheckOrientationCcw(false, "POLYGON ((100 100, 100 100, 200 200, 100 100))");
        }

        [Test]
        public void TestDegenerateRingVertical()
        {
            CheckOrientationCcw(false, "POLYGON ((200 100, 200 100, 200 200, 200 100))");
        }

        /**
         * This case is an invalid ring, so answer is a default value
         */
        [Test]
        public void TestTopAngledSegmentCollapse()
        {
            CheckOrientationCcw(false, "POLYGON ((10 20, 61 20, 20 30, 50 60, 10 20))");
        }

        [Test]
        public void TestABATopFlatSegmentCollapse()
        {
            CheckOrientationCcw(true, "POLYGON ((71 0, 40 40, 70 40, 40 40, 20 0, 71 0))");
        }

        [Test]
        public void TestABATopFlatSegmentCollapseMiddleStart()
        {
            CheckOrientationCcw(true, "POLYGON ((90 90, 50 90, 10 10, 90 10, 50 90, 90 90))");
        }

        [Test]
        public void TestMultipleTopFlatSegmentCollapseSinglePoint()
        {
            CheckOrientationCcw(true, "POLYGON ((100 100, 200 100, 150 200, 170 200, 200 200, 100 200, 150 200, 100 100))");
        }

        [Test]
        public void TestMultipleTopFlatSegmentCollapseFlatTop()
        {
            CheckOrientationCcw(true, "POLYGON ((10 10, 90 10, 70 70, 90 70, 10 70, 30 70, 50 70, 10 10))");
        }

        private void CheckOrientationCcw(bool expectedCCW, string wkt)
        {
            var pts2x = GetCoordinates(wkt);
            Assert.AreEqual(expectedCCW, Orientation.IsCCW(pts2x), $"Coordinate array isCCW: {expectedCCW}");
            var seq2x = GetCoordinateSequence(wkt);
            Assert.AreEqual(expectedCCW, Orientation.IsCCW(seq2x), $"CoordinateSequence isCCW: {expectedCCW}");
        }

        private Coordinate[] GetCoordinates(string wkt)
        {
            var geom = Read(wkt);
            return geom.Coordinates;
        }
        private CoordinateSequence GetCoordinateSequence(string wkt)
        {
            var geom = Read(wkt);
            if (geom.GeometryType != "Polygon")
                throw new ArgumentException("wkt");
            var poly = (Polygon)geom;
            return poly.ExteriorRing.CoordinateSequence;
        }
    }
}
