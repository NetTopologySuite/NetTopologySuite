using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    [TestFixture]
    public class DouglasPeuckerSimplifierTest 
    {
        [Test]
        public void TestPolygonNoReduction()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "POLYGON ((20 220, 40 220, 60 220, 80 220, 100 220, 120 220, 140 220, 140 180, 100 180, 60 180,     20 180, 20 220))",
                        10.0))
                .Test();
        }

        [Test]
        public void TestPolygonReductionWithSplit()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "POLYGON ((40 240, 160 241, 280 240, 280 160, 160 240, 40 140, 40 240))",
                        10.0))
                .Test();
        }

        [Test]
        public void TestPolygonReduction()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "POLYGON ((120 120, 121 121, 122 122, 220 120, 180 199, 160 200, 140 199, 120 120))",
                        10.0))
                .Test();
        }

        [Test]
        public void TestPolygonWithTouchingHole()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200), (120 120, 220 120, 180 199, 160 200, 140 199, 120 120))",
                        10.0))
                .SetExpectedResult("POLYGON ((80 200, 160 200, 240 200, 240 60, 80 60, 80 200), (160 200, 140 199, 120 120, 220 120, 180 199, 160 200)))")
                .Test();
        }

        [Test]
        public void TestFlattishPolygon()
        {
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    "POLYGON ((0 0, 50 0, 53 0, 55 0, 100 0, 70 1,  60 1, 50 1, 40 1, 0 0))",
                    10.0))
                .Test();
        }

        [Test]
        public void TestTinySquare()
        {
            new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    "POLYGON ((0 5, 5 5, 5 0, 0 0, 0 1, 0 5))",
                    10.0))
            .Test();
        }

        [Test]
        public void TestTinyHole()
        {
        new GeometryOperationValidator(
                DPSimplifierResult.GetResult(
                    "POLYGON ((10 10, 10 310, 370 310, 370 10, 10 10), (160 190, 180 190, 180 170, 160 190))",
                    30.0))
            .TestEmpty(false);
        }

        [Test]
        public void TestTinyLineString()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "LINESTRING (0 5, 1 5, 2 5, 5 5)",
                        10.0))
                .Test();
        }

        [Test]
        public void TestMultiPoint()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "MULTIPOINT(80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)",
                        10.0))
                .Test();
        }

        [Test]
        public void TestMultiLineString()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "MULTILINESTRING( (0 0, 50 0, 70 0, 80 0, 100 0), (0 0, 50 1, 60 1, 100 0) )",
                        10.0))
                .Test();
        }

        [Test]
        public void TestGeometryCollection()
        {
            new GeometryOperationValidator(
                    DPSimplifierResult.GetResult(
                        "GEOMETRYCOLLECTION ("
                        + "MULTIPOINT (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120),"
                        + "POLYGON ((80 200, 240 200, 240 60, 80 60, 80 200)),"
                        + "LINESTRING (80 200, 240 200, 240 60, 80 60, 80 200, 140 199, 120 120)"
                        + ")"
                        ,10.0))
                .Test();
        }
    }

    class DPSimplifierResult
    {
        private static WKTReader rdr = new WKTReader();

        public static IGeometry[] GetResult(String wkt, double tolerance)
        {
            IGeometry[] ioGeom = new Geometry[2];
            ioGeom[0] = rdr.Read(wkt);
            ioGeom[1] = DouglasPeuckerSimplifier.Simplify(ioGeom[0], tolerance);
            Console.WriteLine(ioGeom[1]);
            return ioGeom;
        }
    }
}
