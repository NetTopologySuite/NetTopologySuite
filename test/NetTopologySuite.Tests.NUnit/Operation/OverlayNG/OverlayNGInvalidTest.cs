using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    /**
     * Tests OverlayNG handling invalid geometry.
     * OverlayNG can handle "mildlt" invalid geometry.
     * 
     * @author mdavis
     *
     */
    public class OverlayNGInvalidTest : GeometryTestCase
    {

        [Test]
        public void TestPolygonFlatIntersection()
        {
            var a = Read("POLYGON ((10 40, 40 40, 40 10, 10 10, 10 40))");
            var b = Read("POLYGON ((50 30, 19 30, 50 30))");
            var expected = Read("LINESTRING (40 30, 19 30)");
            CheckEqualExact(expected, Intersection(a, b));
        }

        [Test]
        public void TestPolygonAdjacentElementIntersection()
        {
            var a = Read("MULTIPOLYGON (((10 10, 10 40, 40 40, 40 10, 10 10)), ((70 10, 40 10, 40 40, 70 40, 70 10)))");
            var b = Read("POLYGON ((20 50, 60 50, 60 20, 20 20, 20 50))");
            var expected = Read("POLYGON ((40 40, 60 40, 60 20, 40 20, 20 20, 20 40, 40 40))");
            CheckEqualExact(expected, Intersection(a, b));
        }

        [Test]
        public void TestPolygonInvertedIntersection()
        {
            var a = Read("POLYGON ((10 40, 70 40, 70 0, 40 0, 50 20, 30 20, 40 0, 10 0, 10 40))");
            var b = Read("POLYGON ((20 50, 60 50, 60 10, 20 10, 20 50))");
            var expected = Read("POLYGON ((60 40, 60 10, 45 10, 50 20, 30 20, 35 10, 20 10, 20 40, 60 40))");
            CheckEqualExact(expected, Intersection(a, b));
        }

        // AKA self-touching polygon
        [Test]
        public void TestPolygonExvertedIntersection()
        {
            var a = Read("POLYGON ((10 30, 70 30, 70 0, 40 30, 10 0, 10 30))");
            var b = Read("POLYGON ((20 50, 60 50, 60 10, 20 10, 20 50))");
            var expected = Read("MULTIPOLYGON (((40 30, 20 10, 20 30, 40 30)), ((60 30, 60 10, 40 30, 60 30)))");
            CheckEqualExact(expected, Intersection(a, b));
        }

        //============================================================


        public static Geometry Difference(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Difference, pm);
        }

        public static Geometry SymDifference(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.SymDifference, pm);
        }

        public static Geometry Intersection(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Intersection, pm);
        }

        public static Geometry Union(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Union, pm);
        }

    }
}
