using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /// <summary>
    /// Tests <see cref="Orientation.IsCCW(Coordinate[])"/>
    /// </summary>
    [TestFixture]
    public class OrientationIsCCWTest : GeometryTestCase
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
            catch (ArgumentException)
            {
                hasError = true;
            }
            Assert.IsTrue(hasError);
        }

        [Test]
        public void TestCCW()
        {
            CheckCCW(true, "POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
        }

        [Test]
        public void TestRingCW()
        {
            CheckCCW(false, "POLYGON ((60 180, 140 240, 100 180, 140 120, 60 180))");
        }

        [Test]
        public void TestCCWSmall()
        {
            CheckCCW(true, "POLYGON ((1 1, 9 1, 5 9, 1 1))");
        }

        [Test]
        public void TestDuplicateTopPoint()
        {
            CheckCCW(true, "POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
        }

        [Test]
        public void TestFlatTopSegment()
        {
            CheckCCW(false, "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
        }

        [Test]
        public void TestFlatMultipleTopSegment()
        {
            CheckCCW(false, "POLYGON ((100 200, 127 200, 151 200, 173 200, 200 200, 100 100, 100 200))");
        }

        [Test]
        public void TestDegenerateRingHorizontal()
        {
            CheckCCW(false, "POLYGON ((100 200, 100 200, 200 200, 100 200))");
        }

        [Test]
        public void TestDegenerateRingAngled()
        {
            CheckCCW(false, "POLYGON ((100 100, 100 100, 200 200, 100 100))");
        }

        [Test]
        public void TestDegenerateRingVertical()
        {
            CheckCCW(false, "POLYGON ((200 100, 200 100, 200 200, 200 100))");
        }

        /**
         * This case is an invalid ring, so answer is a default value
         */
        [Test]
        public void TestTopAngledSegmentCollapse()
        {
            CheckCCW(false, "POLYGON ((10 20, 61 20, 20 30, 50 60, 10 20))");
        }

        [Test]
        public void TestABATopFlatSegmentCollapse()
        {
            CheckCCW(true, "POLYGON ((71 0, 40 40, 70 40, 40 40, 20 0, 71 0))");
        }

        [Test]
        public void TestABATopFlatSegmentCollapseMiddleStart()
        {
            CheckCCW(true, "POLYGON ((90 90, 50 90, 10 10, 90 10, 50 90, 90 90))");
        }

        [Test]
        public void TestMultipleTopFlatSegmentCollapseSinglePoint()
        {
            CheckCCW(true, "POLYGON ((100 100, 200 100, 150 200, 170 200, 200 200, 100 200, 150 200, 100 100))");
        }

        [Test]
        public void TestMultipleTopFlatSegmentCollapseFlatTop()
        {
            CheckCCW(true, "POLYGON ((10 10, 90 10, 70 70, 90 70, 10 70, 30 70, 50 70, 10 10))");
        }

        /*
         * Signed-area orientation returns orientation of largest enclosed area
         */
        [Test]
        public void TestBowTieByArea()
        {
            CheckCCW(false, "POLYGON ((10 10, 50 10, 25 35, 35 35, 10 10))");
            CheckCCWArea(true, "POLYGON ((10 10, 50 10, 25 35, 35 35, 10 10))");
        }

        private void CheckCCW(bool expectedCCW, string wkt)
        {
            var pts2x = GetCoordinates(wkt);
            Assert.AreEqual(expectedCCW, Orientation.IsCCW(pts2x), $"Coordinate array isCCW: {expectedCCW}");
            var seq2x = GetCoordinateSequence(wkt);
            Assert.AreEqual(expectedCCW, Orientation.IsCCW(seq2x), $"CoordinateSequence isCCW: {expectedCCW}");
        }

        private void CheckCCWArea(bool expectedCCW, string wkt)
        {
            Assert.That(Orientation.IsCCWArea(GetCoordinates(wkt)),
                Is.EqualTo(expectedCCW), $"Coordinate[] isCCW: {expectedCCW}");
            Assert.That(Orientation.IsCCWArea(GetCoordinateSequence(wkt)),
                Is.EqualTo(expectedCCW), $"CoordinateSequence isCCW: {expectedCCW}");
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
