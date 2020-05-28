using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class UnaryUnionNGTest : GeometryTestCase
    {
        [Test]
        public void TestMultiPolygonNarrowGap()
        {
            CheckUnaryUnion("MULTIPOLYGON (((1 9, 5.7 9, 5.7 1, 1 1, 1 9)), ((9 9, 9 1, 6 1, 6 9, 9 9)))",
                1,
                "POLYGON ((1 9, 6 9, 9 9, 9 1, 6 1, 1 1, 1 9))");
        }

        [Test]
        public void TestPolygonsRounded()
        {
            CheckUnaryUnion("GEOMETRYCOLLECTION (POLYGON ((1 9, 6 9, 6 1, 1 1, 1 9)), POLYGON ((9 1, 2 8, 9 9, 9 1)))",
                1,
                "POLYGON ((1 9, 6 9, 9 9, 9 1, 6 4, 6 1, 1 1, 1 9))");
        }

        [Test]
        public void TestPolygonsOverlapping()
        {
            CheckUnaryUnion("GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), POLYGON ((250 250, 250 150, 150 150, 150 250, 250 250)))",
                1,
                "POLYGON ((100 200, 150 200, 150 250, 250 250, 250 150, 200 150, 200 100, 100 100, 100 200))");
        }

        private void CheckUnaryUnion(string wkt, double scaleFactor, string wktExpected)
        {
            var geom = Read(wkt);
            var expected = Read(wktExpected);
            var pm = new PrecisionModel(scaleFactor);
            var result = UnaryUnionNG.Union(geom, pm);
            CheckEqual(expected, result);
        }
    }
}
