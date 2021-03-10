using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGMixedPointsTest : GeometryTestCase
    {
        [Test]
        public void TestSimpleLineIntersection()
        {
            var a = Read("LINESTRING (1 1, 9 1)");
            var b = Read("POINT (5 1)");
            var expected = Read("POINT (5 1)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }
        [Test]
        public void TestLinePointInOutIntersection()
        {
            var a = Read("LINESTRING (1 1, 9 1)");
            var b = Read("MULTIPOINT ((5 1), (15 1))");
            var expected = Read("POINT (5 1)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }
        [Test]
        public void TestSimpleLineUnion()
        {
            var a = Read("LINESTRING (1 1, 9 1)");
            var b = Read("POINT (5 1)");
            var expected = Read("LINESTRING (1 1, 9 1)");
            CheckEqual(expected, OverlayNGTest.Union(a, b, 1));
        }
        [Test]
        public void TestSimpleLineDifference()
        {
            var a = Read("LINESTRING (1 1, 9 1)");
            var b = Read("POINT (5 1)");
            var expected = Read("LINESTRING (1 1, 9 1)");
            CheckEqual(expected, OverlayNGTest.Difference(a, b, 1));
        }
        [Test]
        public void TestSimpleLineSymDifference()
        {
            var a = Read("LINESTRING (1 1, 9 1)");
            var b = Read("POINT (5 1)");
            var expected = Read("LINESTRING (1 1, 9 1)");
            CheckEqual(expected, OverlayNGTest.SymDifference(a, b, 1));
        }
        [Test]
        public void TestLinePointSymDifference()
        {
            var a = Read("LINESTRING (1 1, 9 1)");
            var b = Read("POINT (15 1)");
            var expected = Read("GEOMETRYCOLLECTION (POINT (15 1), LINESTRING (1 1, 9 1))");
            CheckEqual(expected, OverlayNGTest.SymDifference(a, b, 1));
        }

        [Test]
        public void TestPolygonInsideIntersection()
        {
            var a = Read("POLYGON ((4 2, 6 2, 6 0, 4 0, 4 2))");
            var b = Read("POINT (5 1)");
            var expected = Read("POINT (5 1)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }
        [Test]
        public void TestPolygonDisjointIntersection()
        {
            var a = Read("POLYGON ((4 2, 6 2, 6 0, 4 0, 4 2))");
            var b = Read("POINT (15 1)");
            var expected = Read("POINT EMPTY");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestPointEmptyLinestringUnion()
        {
            var a = Read("LINESTRING EMPTY");
            var b = Read("POINT (10 10)");
            var expected = Read("POINT (10 10)");
            var actual = OverlayNGTest.Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLinestringEmptyPointUnion()
        {
            var a = Read("LINESTRING (10 10, 20 20)");
            var b = Read("POINT EMPTY");
            var expected = Read("LINESTRING (10 10, 20 20)");
            var actual = OverlayNGTest.Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        /**
         * Result is empty because Line is not rounded.
         */
        [Test]
        public void TestPointLineIntersectionPrec()
        {
            var a = Read("POINT (10.1 10.4)");
            var b = Read("LINESTRING (9.6 10, 20.1 19.6)");
            var expected = Read("POINT EMPTY");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

    }

}
