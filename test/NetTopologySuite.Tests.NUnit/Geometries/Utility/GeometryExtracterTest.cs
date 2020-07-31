using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    public class GeometryExtracterTest
    {


        private static readonly WKTReader Rdr = new WKTReader();

        [Test]
        public void TestExtractByTypeName()
        {
            var gc = Rdr.Read(
                "GEOMETRYCOLLECTION ( POINT (1 1), LINESTRING (0 0, 10 10), LINESTRING (10 10, 20 20), LINEARRING (10 10, 20 20, 15 15, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)), GEOMETRYCOLLECTION ( POINT (1 1) ) )");

            // verify that LinearRings are included when extracting LineStrings
            var lineStringsAndLinearRings = GeometryExtracter.Extract(gc, Geometry.TypeNameLineString);
            Assert.AreEqual(3, lineStringsAndLinearRings.Count);

            // verify that only LinearRings are extracted
            var linearRings = GeometryExtracter.Extract(gc, Geometry.TypeNameLinearRing);
            Assert.AreEqual(1, linearRings.Count);

            // verify that nested geometries are extracted
            var points = GeometryExtracter.Extract(gc, Geometry.TypeNamePoint);
            Assert.AreEqual(2, points.Count);
        }

        [Test]
        public void TestExtract()
        {
            var gc = Rdr.Read(
                "GEOMETRYCOLLECTION ( POINT (1 1), LINESTRING (0 0, 10 10), LINESTRING (10 10, 20 20), LINEARRING (10 10, 20 20, 15 15, 10 10), POLYGON ((0 0, 100 0, 100 100, 0 100, 0 0)), GEOMETRYCOLLECTION ( POINT (1 1) ) )");

            // verify that LinearRings are included when extracting LineStrings
            var lineStringsAndLinearRings = GeometryExtracter.Extract<LineString>(gc);
            Assert.AreEqual(3, lineStringsAndLinearRings.Count);

            // verify that only LinearRings are extracted
            var linearRings = GeometryExtracter.Extract<LinearRing>(gc);
            Assert.AreEqual(1, linearRings.Count);

            // verify that nested geometries are extracted
            var points = GeometryExtracter.Extract<Point>(gc);
            Assert.AreEqual(2, points.Count);
        }
    }
}
