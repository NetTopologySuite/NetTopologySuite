using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    [TestFixture]
    public class DouglasPeuckerSimplifierTest : GeometryTestCase
    {
        [Test]
        public void TestPoint()
        {
            CheckDPNoChange("POINT (10 10)", 1);
        }

        [Test]
        public void TestPolygonEmpty()
        {
            CheckDPNoChange("POLYGON(EMPTY)", 1);
        }

        [Test]
        public void TestPolygonWithFlatVertices()
        {
            CheckDP("POLYGON ((20 220, 40 220, 60 220, 80 220, 100 220, 120 220, 140 220, 140 180, 100 180, 60 180, 20 180, 20 220))",
                10.0,
                "POLYGON ((20 220, 140 220, 140 180, 20 180, 20 220))");
        }

        [Test]
        public void TestPolygonReductionWithSplit()
        {
            CheckDP("POLYGON ((40 240, 160 241, 280 240, 280 160, 160 240, 40 140, 40 240))",
                1,
                "MULTIPOLYGON (((40 240, 160 240, 40 140, 40 240)), ((160 240, 280 240, 280 160, 160 240)))");
        }

        [Test]
        public void TestPolygonReduction()
        {
            CheckDP("POLYGON ((120 120, 121 121, 122 122, 220 120, 180 199, 160 200, 140 199, 120 120))",
                10.0,
                "POLYGON ((120 120, 220 120, 180 199, 160 200, 140 199, 120 120)))");
        }

        [Test]
        public void TestPolygonWithTouchingHole()
        {
            CheckDP("POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))",
                10,
                "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))");
        }
        [Test]
        public void TestFlattishPolygon()
        {
            CheckDP("POLYGON ((0 0, 50 0, 53 0, 55 0, 100 0, 70 1,  60 1, 50 1, 40 1, 0 0))",
                10,
                "POLYGON EMPTY");
        }

        [Test]
        public void TestTinySquare()
        {
            CheckDP("POLYGON ((0 5, 5 5, 5 0, 0 0, 0 1, 0 5))",
                10,
                "POLYGON EMPTY");
        }

        [Test]
        public void TestTinyHole()
        {
            CheckDP("POLYGON ((10 10, 10 310, 370 310, 370 10, 10 10), (160 190, 180 190, 180 170, 160 190))",
                30,
                "POLYGON ((10 10, 10 310, 370 310, 370 10, 10 10))");
        }

        [Test]
        public void TestTinyLineString()
        {
            CheckDP("LINESTRING (0 5, 1 5, 2 5, 5 5)",
                10,
                "LINESTRING (0 5, 5 5)");
        }

        [Test]
        public void TestMultiPoint()
        {
            CheckDPNoChange("MULTIPOINT(80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)",
                10);
        }

        [Test]
        public void TestMultiLineString()
        {
            CheckDP("MULTILINESTRING((0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )",
                10,
                "MULTILINESTRING ((0 0, 100 0), (0 0, 100 0))");
        }

        [Test]
        public void TestMultiLineStringWithEmpty()
        {
            CheckDP("MULTILINESTRING( EMPTY, (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )",
                10,
                "MULTILINESTRING ((0 0, 100 0), (0 0, 100 0))");
        }

        [Test]
        public void TestMultiPolygonWithEmpty()
        {
            CheckDP("MULTIPOLYGON (EMPTY, ((10 90, 10 10, 90 10, 50 60, 10 90)), ((70 90, 90 90, 90 70, 70 70, 70 90)))",
                10,
                "MULTIPOLYGON (((10 90, 10 10, 90 10, 10 90)), ((70 90, 90 90, 90 70, 70 70, 70 90)))");
        }

        [Test]
        public void TestGeometryCollection()
        {
            CheckDPNoChange("GEOMETRYCOLLECTION (MULTIPOINT (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120), POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200)), LINESTRING (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120))",
              10.0);
        }

        /**
         * Test that a polygon made invalid by simplification
         * is fixed in a sensible way.
         * Fixed by buffer(0) area-base orientation
         * See https://github.com/locationtech/jts/issues/498
         */
        [Test]
        public void TestInvalidPolygonFixed()
        {
            CheckDP(
                "POLYGON ((21.32686 47.78723, 21.32386 47.79023, 21.32186 47.80223, 21.31486 47.81023, 21.32786 47.81123, 21.33986 47.80223, 21.33886 47.81123, 21.32686 47.82023, 21.32586 47.82723, 21.32786 47.82323, 21.33886 47.82623, 21.34186 47.82123, 21.36386 47.82223, 21.40686 47.81723, 21.32686 47.78723))",
                0.0036,
                "POLYGON ((21.32686 47.78723, 21.31486 47.81023, 21.32786 47.81123, 21.33986 47.80223, 21.328068201892744 47.823286782334385, 21.33886 47.82623, 21.34186 47.82123, 21.40686 47.81723, 21.32686 47.78723))"
                );
        }

        /**
         * Test that a collapsed polygon is removed.
         * Not an error in JTS, but included to avoid regression errors in future.
         * 
         * See https://trac.osgeo.org/geos/ticket/1115
         */
        [Test]
        public void TestPolygonCollapseRemoved()
        {
            CheckDP(
                "MULTIPOLYGON (((-76.02716827 36.55671692, -75.99866486 36.55665207, -75.91191864 36.54253006, -75.92480469 36.47397614, -75.97727966 36.4780159, -75.97628784 36.51792526, -76.02716827 36.55671692)), ((-75.90198517 36.55619812, -75.8781662 36.55587387, -75.77315521 36.22925568, -75.78317261 36.22519302, -75.90198517 36.55619812)))",
                0.05,
                "POLYGON ((-76.02716827 36.55671692, -75.91191864 36.54253006, -75.92480469 36.47397614, -76.02716827 36.55671692))"
                );
        }

        // see https://trac.osgeo.org/geos/ticket/1064
        [Test]
        public void TestPolygonRemoveFlatEndpoint()
        {
            CheckDP(
              "POLYGON ((42 42, 0 42, 0 100, 42 100, 100 42, 42 42))",
                1,
                "POLYGON ((100 42, 0 42, 0 100, 42 100, 100 42))"
                );
        }

        [Test]
        public void TestPolygonEndpointCollapse()
        {
            CheckDP(
              "POLYGON ((5 2, 9 1, 1 1, 5 2))",
                1,
                "POLYGON EMPTY"
                );
        }

        private void CheckDP(string wkt, double tolerance, string wktExpected)
        {
            var geom = Read(wkt);
            var result = DouglasPeuckerSimplifier.Simplify(geom, tolerance);
            var expected = Read(wktExpected);
            CheckEqual(expected, result);
        }

        private void CheckDPNoChange(string wkt, double tolerance)
        {
            CheckDP(wkt, tolerance, wkt);
        }
    }
}
