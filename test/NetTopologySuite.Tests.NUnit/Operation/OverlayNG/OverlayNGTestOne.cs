using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGTestOne : GeometryTestCase
    {
        //======  Tests for semantic of including collapsed edges as lines in result

        [Test, Ignore("")]
        public void TestCollapseTriBoxIntersection()
        {
            var a = Read("POLYGON ((1 2, 1 1, 9 1, 1 2))");
            var b = Read("POLYGON ((9 2, 9 1, 8 1, 8 2, 9 2))");
            var expected = Read("LINESTRING (8 1, 9 1)");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestCollapseTriBoxesIntersection()
        {
            var a = Read("MULTIPOLYGON (((1 4, 1 1, 2 1, 2 4, 1 4)), ((9 4, 9 1, 10 1, 10 4, 9 4)))");
            var b = Read("POLYGON ((0 2, 11 3, 11 2, 0 2))");
            var expected = Read("GEOMETRYCOLLECTION (LINESTRING (1 2, 2 2), POLYGON ((9 2, 9 3, 10 3, 10 2, 9 2)))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test, Ignore("")]
        public void TestCollapseBoxCutByTriangleUnion()
        {
            var a = Read("POLYGON ((100 10, 0 10, 100 11, 100 10))");
            var b = Read("POLYGON ((20 20, 0 20, 0 0, 20 0, 20 20))");
            var expected = Read("MULTIPOLYGON (((0 0, 0 10, 0 20, 20 20, 20 10, 20 0, 0 0)), ((20 10, 100 11, 100 10, 20 10)))");
            CheckEqual(expected, OverlayNGTest.Union(a, b, 1));
        }

        [Test, Ignore("")]
        public void TestCollapseBoxTriangleUnion()
        {
            var a = Read("POLYGON ((10 10, 100 10, 10 11, 10 10))");
            var b = Read("POLYGON ((90 0, 200 0, 200 200, 90 200, 90 0))");
            var expected = Read("MULTIPOLYGON (((90 10, 10 10, 10 11, 90 10)), ((90 10, 90 200, 200 200, 200 0, 90 0, 90 10)))");
            CheckEqual(expected, OverlayNGTest.Union(a, b, 1));
        }

        //==============================================


        [Test, Ignore("")]
        public void TestParallelSpikes()
        {
            var a = Read("POLYGON ((1 3.3, 1.3 1.4, 3.1 1.4, 3.1 0.9, 1.3 0.9, 1 -0.2, 0.8 1.3, 1 3.3))");
            var b = Read("POLYGON ((1 2.9, 2.9 2.9, 2.9 1.3, 1.7 1, 1.3 0.9, 1 0.4, 1 2.9))");
            var expected = Read("POLYGON EMPTY");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }


        [Test, Ignore("")]
        public void TestBoxHoleCollapseAlongBEdgeDifference()
        {
            var a = Read("POLYGON ((0 3, 3 3, 3 0, 0 0, 0 3), (1 1.2, 1 1.1, 2.3 1.1, 1 1.2))");
            var b = Read("POLYGON ((1 1, 2 1, 2 0, 1 0, 1 1))");
            var expected = Read("POLYGON EMPTY");
            CheckEqual(expected, OverlayNGTest.Difference(b, a, 1));
        }

        [Test, Ignore("")]
        public void TestPolyPolyTouchIntersection()
        {
            var a = Read("POLYGON ((300 0, 100 0, 100 100, 300 100, 300 0))");
            var b = Read("POLYGON ((100 200, 300 200, 300 100, 200 100, 200 0, 100 0, 100 200))");
            var expected = Read("GEOMETRYCOLLECTION (LINESTRING (200 100, 300 100), POLYGON ((200 0, 100 0, 100 100, 200 100, 200 0)))");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }


        [Test, Ignore("")]
        public void TestBoxHoleCollapseAlongBEdgeUnion()
        {
            var a = Read("POLYGON ((0 3, 3 3, 3 0, 0 0, 0 3), (1 1.2, 1 1.1, 2.3 1.1, 1 1.2))");
            var b = Read("POLYGON ((1 1, 2 1, 2 0, 1 0, 1 1))");
            var expected = Read("POLYGON ((0 0, 0 3, 3 3, 3 0, 2 0, 1 0, 0 0))");
            CheckEqual(expected, OverlayNGTest.Union(b, a, 1));
        }

        [Test, Ignore("")]
        public void TestRoundedBoxesIntersection()
        {
            var a = Read("POLYGON ((0.6 0.1, 0.6 1.9, 2.9 1.9, 2.9 0.1, 0.6 0.1))");
            var b = Read("POLYGON ((1.1 3.9, 2.9 3.9, 2.9 2.1, 1.1 2.1, 1.1 3.9))");
            var expected = Read("LINESTRING (1 2, 3 2)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        public static Geometry Union(Geometry a, Geometry b, double scaleFactor)
        {
            return OverlayNGTest.Union(a, b, scaleFactor);
        }

        public static Geometry Intersection(Geometry a, Geometry b, double scaleFactor)
        {
            return OverlayNGTest.Intersection(a, b, scaleFactor);
        }
    }

}
