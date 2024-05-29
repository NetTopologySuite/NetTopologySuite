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
            CheckLocation(gcPLA, 1, 1, DimensionLocation.POINT_INTERIOR);
            CheckLocation(gcPLA, 0, 1, (int)Location.Exterior);
        }

        [Test]
        public void TestPointInLine()
        {
            CheckLocation(gcPLA, 3, 8, DimensionLocation.LINE_INTERIOR);
        }

        [Test]
        public void TestPointInArea()
        {
            CheckLocation(gcPLA, 8, 8, DimensionLocation.AREA_INTERIOR);
        }

        [Test]
        public void TestLine()
        {
            CheckLocation(gcPLA, 3, 3, DimensionLocation.LINE_INTERIOR);
            CheckLocation(gcPLA, 3, 1, DimensionLocation.LINE_BOUNDARY);
        }

        [Test]
        public void TestLineInArea()
        {
            CheckLocation(gcPLA, 11, 11, DimensionLocation.AREA_INTERIOR);
            CheckLocation(gcPLA, 14, 14, DimensionLocation.AREA_INTERIOR);
        }

        [Test]
        public void TestArea()
        {
            CheckLocation(gcPLA, 8, 8, DimensionLocation.AREA_INTERIOR);
            CheckLocation(gcPLA, 9, 9, DimensionLocation.AREA_BOUNDARY);
        }

        [Test]
        public void TestAreaInArea()
        {
            CheckLocation(gcPLA, 11, 11, DimensionLocation.AREA_INTERIOR);
            CheckLocation(gcPLA, 12, 12, DimensionLocation.AREA_INTERIOR);
            CheckLocation(gcPLA, 10, 10, DimensionLocation.AREA_BOUNDARY);
            CheckLocation(gcPLA, 16, 16, DimensionLocation.AREA_INTERIOR);
        }

        [Test]
        public void TestLineNode()
        {
            //checkNodeLocation(gcPLA, 12.1, 12.2, Location.INTERIOR);
            CheckNodeLocation(gcPLA, 3, 1, Location.Boundary);
        }

        private void CheckLocation(string wkt, double i, double j, int expected)
        {
            var geom = Read(wkt);
            var locator = new RelatePointLocator(geom);
            int actual = locator.LocateWithDim(new Coordinate(i, j));
            Assert.That(actual, Is.EqualTo(expected));
        }

        private void CheckNodeLocation(string wkt, double i, double j, Location expected)
        {
            var geom = Read(wkt);
            var locator = new RelatePointLocator(geom);
            var actual = locator.LocateNode(new Coordinate(i, j), null);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }

}
