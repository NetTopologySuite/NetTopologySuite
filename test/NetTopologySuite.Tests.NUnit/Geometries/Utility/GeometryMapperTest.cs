using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    public class GeometryMapperTest : GeometryTestCase
    {
        /**
         * Mapping: 
         *   LineString -> LineString, 
         *   Point -> empty LineString, 
         *   Polygon -> null
         */
        private static readonly GeometryMapper.IMapOp KEEP_LINE = new GeometryMapper.MapOp(
            geom =>
            {
                if (geom is Point)
                {
                    return geom.Factory.CreateEmpty(Dimension.Curve);
                }
                if (geom is LineString)
                    return geom;
                return null;
            });

        private static readonly GeometryMapper.IMapOp BOUNDARY = new GeometryMapper.MapOp(geom => geom.Boundary);

        [Test]
        public void TestFlatMapInputEmpty()
        {
            CheckFlatMap("GEOMETRYCOLLECTION( POINT EMPTY, LINESTRING EMPTY)",
                Dimension.Curve, KEEP_LINE, "LINESTRING EMPTY");
        }

        [Test]
        public void TestFlatMapInputMulti()
        {
            CheckFlatMap("GEOMETRYCOLLECTION( MULTILINESTRING((0 0, 1 1), (1 1, 2 2)), LINESTRING(2 2, 3 3))",
                Dimension.Curve, KEEP_LINE, "MULTILINESTRING ((0 0, 1 1), (1 1, 2 2), (2 2, 3 3))");
        }

        [Test]
        public void TestFlatMapResultEmpty()
        {
            CheckFlatMap("GEOMETRYCOLLECTION( LINESTRING(0 0, 1 1), LINESTRING(1 1, 2 2))",
                Dimension.Curve, KEEP_LINE, "MULTILINESTRING((0 0, 1 1), (1 1, 2 2))");

            CheckFlatMap("GEOMETRYCOLLECTION( POINT(0 0), POINT(0 0), LINESTRING(0 0, 1 1))",
                Dimension.Curve, KEEP_LINE, "LINESTRING(0 0, 1 1)");

            CheckFlatMap("MULTIPOINT((0 0), (1 1))",
                Dimension.Curve, KEEP_LINE, "LINESTRING EMPTY");
        }

        [Test]
        public void TestFlatMapResultNull()
        {
            CheckFlatMap("GEOMETRYCOLLECTION( POINT(0 0), LINESTRING(0 0, 1 1), POLYGON ((1 1, 1 2, 2 1, 1 1)))",
                Dimension.Curve, KEEP_LINE, "LINESTRING(0 0, 1 1)");
        }

        [Test]
        public void TestFlatMapBoundary()
        {
            CheckFlatMap("GEOMETRYCOLLECTION( POINT(0 0), LINESTRING(0 0, 1 1), POLYGON ((1 1, 1 2, 2 1, 1 1)))",
                Dimension.Point, BOUNDARY, "GEOMETRYCOLLECTION (POINT (0 0), POINT (1 1), LINEARRING (1 1, 1 2, 2 1, 1 1))");

            CheckFlatMap("LINESTRING EMPTY",
                Dimension.Point, BOUNDARY, "POINT EMPTY");
        }


        private void CheckFlatMap(string wkt, Dimension dim, GeometryMapper.IMapOp op, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = GeometryMapper.FlatMap(geom, dim, op);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }
    }
}
