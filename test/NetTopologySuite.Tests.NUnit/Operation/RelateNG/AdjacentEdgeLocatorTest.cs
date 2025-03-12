using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    public class AdjacentEdgeLocatorTest : GeometryTestCase
    {

        [Test]
        public void TestAdjacent2()
        {
            CheckLocation(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 5 9, 5 1, 1 1, 1 9)), POLYGON ((9 9, 9 1, 5 1, 5 9, 9 9)))",
                5, 5, Location.Interior
                );
        }

        [Test]
        public void TestNonAdjacent()
        {
            CheckLocation(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 4 9, 5 1, 1 1, 1 9)), POLYGON ((9 9, 9 1, 5 1, 5 9, 9 9)))",
                5, 5, Location.Boundary
                );
        }

        [Test]
        public void TestAdjacent6WithFilledHoles()
        {
            CheckLocation(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 5 9, 6 6, 1 5, 1 9), (2 6, 4 8, 6 6, 2 6)), POLYGON ((2 6, 4 8, 6 6, 2 6)), POLYGON ((9 9, 9 5, 6 6, 5 9, 9 9)), POLYGON ((9 1, 5 1, 6 6, 9 5, 9 1), (7 2, 6 6, 8 3, 7 2)), POLYGON ((7 2, 6 6, 8 3, 7 2)), POLYGON ((1 1, 1 5, 6 6, 5 1, 1 1)))",
                6, 6, Location.Interior
                );
        }

        [Test]
        public void TestAdjacent5WithEmptyHole()
        {
            CheckLocation(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 5 9, 6 6, 1 5, 1 9), (2 6, 4 8, 6 6, 2 6)), POLYGON ((2 6, 4 8, 6 6, 2 6)), POLYGON ((9 9, 9 5, 6 6, 5 9, 9 9)), POLYGON ((9 1, 5 1, 6 6, 9 5, 9 1), (7 2, 6 6, 8 3, 7 2)), POLYGON ((1 1, 1 5, 6 6, 5 1, 1 1)))",
                6, 6, Location.Boundary
                );
        }

        [Test]
        public void TestContainedAndAdjacent()
        {
            const string wkt = "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9)), POLYGON ((9 2, 2 2, 2 8, 9 8, 9 2)))";
            CheckLocation(wkt,
                9, 5, Location.Boundary
                );
            CheckLocation(wkt,
                9, 8, Location.Boundary
                );
        }

        /**
         * Tests a bug caused by incorrect point-on-segment logic.
         */
        [Test]
        public void TestDisjointCollinear()
        {
            CheckLocation(
                "GEOMETRYCOLLECTION (MULTIPOLYGON (((1 4, 4 4, 4 1, 1 1, 1 4)), ((5 4, 8 4, 8 1, 5 1, 5 4))))",
                2, 4, Location.Boundary
                );
        }

        private void CheckLocation(string wkt, int x, int y, Location expectedLoc)
        {
            var geom = Read(wkt);
            var ael = new AdjacentEdgeLocator(geom);
            var loc = ael.Locate(new Coordinate(x, y));
            Assert.That(expectedLoc, Is.EqualTo(loc), $"Locations are not equal: {loc} != {expectedLoc}");
        }
    }
}
