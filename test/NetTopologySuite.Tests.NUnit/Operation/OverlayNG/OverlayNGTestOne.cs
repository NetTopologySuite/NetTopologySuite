using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGTestOne : GeometryTestCase
    {
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
