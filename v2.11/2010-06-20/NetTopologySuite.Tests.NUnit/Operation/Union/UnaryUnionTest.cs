using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Union;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    [TestFixture]
    public class UnaryUnionTest
    {
        [Test]
        public void TestEmptyCollection()
        {
            DoTest(new String[] { }, "GEOMETRYCOLLECTION EMPTY");
        }
        [Test]
        public void TestPoints()
        {
            DoTest(new String[] { "POINT (1 1)", "POINT (2 2)" }, "MULTIPOINT ((1 1), (2 2))");
        }
        [Test]
        public void TestAll()
        {
            DoTest(new String[] { "GEOMETRYCOLLECTION (POLYGON ((0 0, 0 90, 90 90, 90 0, 0 0)),   POLYGON ((120 0, 120 90, 210 90, 210 0, 120 0)),  LINESTRING (40 50, 40 140),  LINESTRING (160 50, 160 140),  POINT (60 50),  POINT (60 140),  POINT (40 140))" },
                    "GEOMETRYCOLLECTION (POINT (60 140),   LINESTRING (40 90, 40 140), LINESTRING (160 90, 160 140), POLYGON ((0 0, 0 90, 40 90, 90 90, 90 0, 0 0)), POLYGON ((120 0, 120 90, 160 90, 210 90, 210 0, 120 0)))");
                  //"GEOMETRYCOLLECTION (POINT (60 50), POINT (60 140), LINESTRING (0 0, 0 90, 40 90), LINESTRING (40 90, 90 90, 90 0, 0 0), LINESTRING (120 0, 120 90, 160 90), LINESTRING (160 90, 210 90, 210 0, 120 0), LINESTRING (40 50, 40 90), LINESTRING (40 90, 40 140), LINESTRING (160 50, 160 90), LINESTRING (160 90, 160 140))"

        }

        private static void DoTest(String[] inputWKT, String expectedWKT)
        {
            IGeometry<Coordinate> result;
            List<IGeometry<Coordinate>> geoms = new List<IGeometry<Coordinate>>(GeometryUtils.ReadWKT(inputWKT));
            if (geoms.Count == 0)
                result = UnaryUnionOp<Coordinate>.Union(geoms, GeometryUtils.GeometryFactory);
            else
                result = UnaryUnionOp<Coordinate>.Union(geoms);

            Console.WriteLine(result);
            Assert.IsTrue(GeometryUtils.IsEqual(GeometryUtils.ReadWKT(expectedWKT), result));
        }

    }
}