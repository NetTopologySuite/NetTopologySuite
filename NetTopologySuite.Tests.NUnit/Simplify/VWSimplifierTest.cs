using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    // Taken some tests from DPSimplifierTest class.
    // JTS doesn't have any specific test for VWSimplifier
    [TestFixtureAttribute]
    public class VWSimplifierTest
    {
        [TestAttribute]
        public void TestEmptyPolygon()
        {
            const string geomStr = "POLYGON(EMPTY)";
            new GeometryOperationValidator(
                VWSimplifierResult.GetResult(
                    geomStr,
                    1))
                .SetExpectedResult(geomStr)
                .Test();
        }

        [TestAttribute]
        public void TestPolygonNoReduction()
        {
            const string geomStr =
                "POLYGON ((20 220, 40 220, 60 220, 80 220, 100 220, 120 220, 140 220, 140 180, 100 180, 60 180, 20 180, 20 220))";
            new GeometryOperationValidator(
                    VWSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [TestAttribute]
        public void TestPolygonReductionWithSplit()
        {
            const string geomStr = "POLYGON ((40 240, 160 241, 280 240, 280 160, 160 240, 40 140, 40 240))";
            new GeometryOperationValidator(
                    VWSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [TestAttribute]
        public void TestPolygonReduction()
        {
            const string geomStr = "POLYGON ((120 120, 121 121, 122 122, 220 120, 180 199, 160 200, 140 199, 120 120))";
            new GeometryOperationValidator(
                    VWSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [TestAttribute]
        public void TestFlattishPolygon()
        {
            const string geomStr = "POLYGON ((0 0, 50 0, 53 0, 55 0, 100 0, 70 1,  60 1, 50 1, 40 1, 0 0))";
            new GeometryOperationValidator(
                VWSimplifierResult.GetResult(
                    geomStr,
                    10.0))
                .Test();
        }

        [TestAttribute]
        public void TestTinySquare()
        {
            const string geomStr = "POLYGON ((0 5, 5 5, 5 0, 0 0, 0 1, 0 5))";
            new GeometryOperationValidator(
                VWSimplifierResult.GetResult(
                    geomStr,
                    10.0))
            .Test();
        }

        [TestAttribute]
        public void TestTinyHole()
        {
            const string geomStr =
                "POLYGON ((10 10, 10 310, 370 310, 370 10, 10 10), (160 190, 180 190, 180 170, 160 190))";
            new GeometryOperationValidator(
                VWSimplifierResult.GetResult(
                    geomStr,
                    30.0))
            .TestEmpty(false);
        }

        [TestAttribute]
        public void TestTinyLineString()
        {
            const string geomStr = "LINESTRING (0 5, 1 5, 2 5, 5 5)";
            new GeometryOperationValidator(
                    VWSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [TestAttribute]
        public void TestMultiLineString()
        {
            const string geomStr = "MULTILINESTRING( (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )";
            new GeometryOperationValidator(
                    VWSimplifierResult.GetResult(
                        geomStr,
                        10.0))
                .Test();
        }

        [TestAttribute]
        public void TestMultiLineStringWithEmpty()
        {
            new GeometryOperationValidator(
                VWSimplifierResult.GetResult(
                    "MULTILINESTRING( EMPTY, (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )",
                    10.0))
                .Test();
        }

        [TestAttribute]
        public void TestMultiPolygonWithEmpty()
        {
            new GeometryOperationValidator(
                VWSimplifierResult.GetResult(
                    "MULTIPOLYGON (EMPTY, ((-36 91.5, 4.5 91.5, 4.5 57.5, -36 57.5, -36 91.5)), ((25.5 57.5, 61.5 57.5, 61.5 23.5, 25.5 23.5, 25.5 57.5)))",
                    10.0))
                .Test();
        }       
    }

    static class VWSimplifierResult
    {
        private static readonly WKTReader Rdr = new WKTReader();

        public static IGeometry[] GetResult(String wkt, double tolerance)
        {
            IGeometry[] ioGeom = new IGeometry[2];
            ioGeom[0] = Rdr.Read(wkt);
            ioGeom[1] = VWSimplifier.Simplify(ioGeom[0], tolerance);
            Console.WriteLine(ioGeom[1]);
            return ioGeom;
        }
    }
}
