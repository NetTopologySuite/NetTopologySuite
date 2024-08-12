using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    internal class RelatePointLocatorTest : GeometryTestCase
    {

        const string gcPLA = "GEOMETRYCOLLECTION (POINT (1 1), POINT (2 1), LINESTRING (3 1, 3 9), LINESTRING (4 1, 5 4, 7 1, 4 1), LINESTRING (12 12, 14 14), POLYGON ((6 5, 6 9, 9 9, 9 5, 6 5)), POLYGON ((10 10, 10 16, 16 16, 16 10, 10 10)), POLYGON ((11 11, 11 17, 17 17, 17 11, 11 11)), POLYGON ((12 12, 12 16, 16 16, 16 12, 12 12)))";

        [Test]
        public void TestPoint()
        {
            //string wkt = "GEOMETRYCOLLECTION (POINT(0 0), POINT(1 1))";
            CheckDimLocation(gcPLA, 1, 1, DimensionLocation.POINT_INTERIOR);
            CheckDimLocation(gcPLA, 0, 1, DimensionLocation.EXTERIOR);
        }

        [Test]
        public void TestPointInLine()
        {
            CheckDimLocation(gcPLA, 3, 8, DimensionLocation.LINE_INTERIOR);
        }

        [Test]
        public void TestPointInArea()
        {
            CheckDimLocation(gcPLA, 8, 8, DimensionLocation.AREA_INTERIOR);
        }

        [Test]
        public void TestLine()
        {
            CheckDimLocation(gcPLA, 3, 3, DimensionLocation.LINE_INTERIOR);
            CheckDimLocation(gcPLA, 3, 1, DimensionLocation.LINE_BOUNDARY);
        }

        [Test]
        public void TestLineInArea()
        {
            CheckDimLocation(gcPLA, 11, 11, DimensionLocation.AREA_INTERIOR);
            CheckDimLocation(gcPLA, 14, 14, DimensionLocation.AREA_INTERIOR);
        }

        [Test]
        public void TestArea()
        {
            CheckDimLocation(gcPLA, 8, 8, DimensionLocation.AREA_INTERIOR);
            CheckDimLocation(gcPLA, 9, 9, DimensionLocation.AREA_BOUNDARY);
        }

        [Test]
        public void TestAreaInArea()
        {
            CheckDimLocation(gcPLA, 11, 11, DimensionLocation.AREA_INTERIOR);
            CheckDimLocation(gcPLA, 12, 12, DimensionLocation.AREA_INTERIOR);
            CheckDimLocation(gcPLA, 10, 10, DimensionLocation.AREA_BOUNDARY);
            CheckDimLocation(gcPLA, 16, 16, DimensionLocation.AREA_INTERIOR);
        }

        [Test]
        public void TestLineNode()
        {
            //CheckNodeLocation(gcPLA, 12.1, 12.2, Location.Interior);
            CheckNodeLocation(gcPLA, 3, 1, Location.Boundary);
        }

        private void CheckDimLocation(string wkt, double x, double y, int expectedDimLoc)
        {
            var geom = Read(wkt);
            var locator = new RelatePointLocator(geom);
            int actual = locator.LocateWithDim(new Coordinate(x, y));
            Assert.That(actual, Is.EqualTo(expectedDimLoc));
        }

        private void CheckNodeLocation(string wkt, double x, double y, Location expectedLoc)
        {
            var geom = Read(wkt);
            var locator = new RelatePointLocator(geom);
            var actual = locator.LocateNode(new Coordinate(x, y), null);
            Assert.That(actual, Is.EqualTo(expectedLoc));
        }
    }

}
