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
    public class OverlayNGInvalidTest : OverlayNGTestCase
    {

        [Test]
        public void TestPolygonFlatIntersection()
        {
            CheckIntersection(
                "POLYGON ((10 40, 40 40, 40 10, 10 10, 10 40))",
                "POLYGON ((50 30, 19 30, 50 30))",
                "LINESTRING (40 30, 19 30)");
        }

        [Test]
        public void TestPolygonAdjacentElementIntersection()
        {
            CheckIntersection(
            "MULTIPOLYGON (((10 10, 10 40, 40 40, 40 10, 10 10)), ((70 10, 40 10, 40 40, 70 40, 70 10)))",
            "POLYGON ((20 50, 60 50, 60 20, 20 20, 20 50))",
            "POLYGON ((40 40, 60 40, 60 20, 40 20, 20 20, 20 40, 40 40))");
        }

        [Test]
        public void TestPolygonInvertedIntersection()
        {
            CheckIntersection(
            "POLYGON ((10 40, 70 40, 70 0, 40 0, 50 20, 30 20, 40 0, 10 0, 10 40))",
            "POLYGON ((20 50, 60 50, 60 10, 20 10, 20 50))",
            "POLYGON ((60 40, 60 10, 45 10, 50 20, 30 20, 35 10, 20 10, 20 40, 60 40))");
        }

        // AKA self-touching polygon
        [Test]
        public void TestPolygonExvertedIntersection()
        {
            CheckIntersection(
            "POLYGON ((10 30, 70 30, 70 0, 40 30, 10 0, 10 30))",
            "POLYGON ((20 50, 60 50, 60 10, 20 10, 20 50))",
            "MULTIPOLYGON (((40 30, 20 10, 20 30, 40 30)), ((60 30, 60 10, 40 30, 60 30)))");
        }
    }
}
