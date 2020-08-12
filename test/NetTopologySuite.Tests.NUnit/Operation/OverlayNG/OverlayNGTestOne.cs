using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGTestOne : GeometryTestCase
    {


        [Test]
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
