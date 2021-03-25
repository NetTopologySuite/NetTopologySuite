using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    [TestFixture]
    public class DouglasPeuckerSimplifierTest : GeometryTestCase
    {
        [Test]
        public void TestEmptyPolygon()
        {
            const string geomStr = "POLYGON(EMPTY)";
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    geomStr,
                    1))
                .SetExpectedResult(geomStr)
                .Test();
        }

        [Test]
        public void TestPoint()
        {
            const string geomStr = "POINT (10 10)";
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    geomStr,
                    1))
                .SetExpectedResult(geomStr)
                .Test();
        }

        [Test]
        public void TestPolygonNoReduction()
        {
            const string geomStr =
                "POLYGON ((20 220, 40 220, 60 220, 80 220, 100 220, 120 220, 140 220, 140 180, 100 180, 60 180, 20 180, 20 220))";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [Test]
        public void TestPolygonReductionWithSplit()
        {
            const string geomStr = "POLYGON ((40 240, 160 241, 280 240, 280 160, 160 240, 40 140, 40 240))";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [Test]
        public void TestPolygonReduction()
        {
            const string geomStr = "POLYGON ((120 120, 121 121, 122 122, 220 120, 180 199, 160 200, 140 199, 120 120))";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [Test]
        public void TestPolygonWithTouchingHole()
        {
            const string geomStr =
                "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))";
            const string resStr =
                //"POLYGON ((80 200, 160 200, 240 200, 240 60, 80 60, 80 200), (160 200, 140 199, 120 120, 220 120, 180 199, 160 200)))";
                  "POLYGON((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .SetExpectedResult(resStr)
                .Test();
        }

        [Test]
        public void TestFlattishPolygon()
        {
            const string geomStr = "POLYGON ((0 0, 50 0, 53 0, 55 0, 100 0, 70 1,  60 1, 50 1, 40 1, 0 0))";
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    geomStr,
                    10.0))
                .Test();
        }

        [Test]
        public void TestTinySquare()
        {
            const string geomStr = "POLYGON ((0 5, 5 5, 5 0, 0 0, 0 1, 0 5))";
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    geomStr,
                    10.0))
            .Test();
        }

        [Test]
        public void TestTinyHole()
        {
            const string geomStr =
                "POLYGON ((10 10, 10 310, 370 310, 370 10, 10 10), (160 190, 180 190, 180 170, 160 190))";
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    geomStr,
                    30.0))
            .TestEmpty(false);
        }

        [Test]
        public void TestTinyLineString()
        {
            const string geomStr = "LINESTRING (0 5, 1 5, 2 5, 5 5)";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [Test]
        public void TestMultiPoint()
        {
            const string geomStr = "MULTIPOINT(80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .SetExpectedResult(geomStr)
                .Test();
        }

        [Test]
        public void TestMultiLineString()
        {
            const string geomStr = "MULTILINESTRING( (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [Test]
        public void TestMultiLineStringWithEmpty()
        {
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    "MULTILINESTRING( EMPTY, (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )",
                    10.0))
                .Test();
        }

        [Test]
        public void TestMultiPolygonWithEmpty()
        {
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    "MULTIPOLYGON (EMPTY, ((-36 91.5, 4.5 91.5, 4.5 57.5, -36 57.5, -36 91.5)), ((25.5 57.5, 61.5 57.5, 61.5 23.5, 25.5 23.5, 25.5 57.5)))",
                    10.0))
                .Test();
        }

        [Test]
        public void TestGeometryCollection()
        {
            const string geomStr = "GEOMETRYCOLLECTION ("
                                   + "MULTIPOINT (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120),"
                                   + "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200)),"
                                   + "LINESTRING (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)"
                                   + ")";
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        /*
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

        private void CheckDP(string wkt, double tolerance, string wktExpected)
        {
            var geom = Read(wkt);
            var result = DouglasPeuckerSimplifier.Simplify(geom, tolerance);
            var expected = Read(wktExpected);
            CheckEqual(expected, result);
        }
    }

    static class DPSimplifierResult
    {
        private static readonly WKTReader Rdr = new WKTReader();

        public static Geometry[] GetResult(string wkt, double tolerance)
        {
            var ioGeom = new Geometry[2];
            ioGeom[0] = Rdr.Read(wkt);
            ioGeom[1] = DouglasPeuckerSimplifier.Simplify(ioGeom[0], tolerance);
            //TestContext.WriteLine(ioGeom[1]);
            return ioGeom;
        }
    }
}
