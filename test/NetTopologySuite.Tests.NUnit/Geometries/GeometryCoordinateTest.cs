using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryCoordinateTest : GeometryTestCase
    {

        [Test]
        public void TestPoint()
        {
            CheckCoordinate("POINT (1 1)", 1, 1);
        }

        [Test]
        public void TestLineString()
        {
            CheckCoordinate("LINESTRING (1 1, 2 2)", 1, 1);
        }

        [Test]
        public void TestPolygon()
        {
            CheckCoordinate("POLYGON ((1 1, 1 2, 2 1, 1 1))", 1, 1);
        }

        [Test]
        public void TestEmptyElementsAll()
        {
            CheckCoordinate("GEOMETRYCOLLECTION ( LINESTRING EMPTY, POINT EMPTY )");
        }

        [Test]
        public void TestEmptyFirstElementPolygonal()
        {
            CheckCoordinate("MULTIPOLYGON ( EMPTY, ((1 1, 1 2, 2 1, 1 1)) )", 1, 1);
        }

        [Test]
        public void TestEmptyFirstElement()
        {
            CheckCoordinate("GEOMETRYCOLLECTION ( LINESTRING EMPTY, POINT(1 1) )", 1, 1);
        }

        [Test]
        public void TestEmptySecondElement()
        {
            CheckCoordinate("GEOMETRYCOLLECTION ( POINT(1 1), LINESTRING EMPTY )", 1, 1);
        }

        private void CheckCoordinate(string wkt, int x, int y)
        {
            CheckCoordinate(Read(wkt), new Coordinate(x, y));
        }

        private void CheckCoordinate(Geometry g, Coordinate expected)
        {
            var actual = g.Coordinate;
            CheckEqualXY(expected, actual);
        }

        private void CheckCoordinate(string wkt)
        {
            var g = Read(wkt);
            var actual = g.Coordinate;
            Assert.That(actual, Is.Null);
        }
    }
}
