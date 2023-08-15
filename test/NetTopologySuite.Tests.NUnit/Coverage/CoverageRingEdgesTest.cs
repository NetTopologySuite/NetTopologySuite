using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoverageRingEdgesTest : GeometryTestCase
    {
        [Test]
        public void TestTwoAdjacent()
        {
            CheckEdges("GEOMETRYCOLLECTION (POLYGON ((1 1, 1 6, 6 5, 9 6, 9 1, 1 1)), POLYGON ((1 9, 6 9, 6 5, 1 6, 1 9)))",
                "MULTILINESTRING ((1 6, 1 1, 9 1, 9 6, 6 5), (1 6, 1 9, 6 9, 6 5), (1 6, 6 5))");
        }

        [Test]
        public void TestTwoAdjacentWithFilledHole()
        {
            CheckEdges("GEOMETRYCOLLECTION (POLYGON ((1 1, 1 6, 6 5, 9 6, 9 1, 1 1), (2 4, 4 4, 4 2, 2 2, 2 4)), POLYGON ((1 9, 6 9, 6 5, 1 6, 1 9)), POLYGON ((4 2, 2 2, 2 4, 4 4, 4 2)))",
                "MULTILINESTRING ((1 6, 1 1, 9 1, 9 6, 6 5), (1 6, 1 9, 6 9, 6 5), (1 6, 6 5), (2 4, 2 2, 4 2, 4 4, 2 4))");
        }

        [Test]
        public void TestHolesAndFillWithDifferentEndpoints()
        {
            CheckEdges("GEOMETRYCOLLECTION (POLYGON ((0 10, 10 10, 10 0, 0 0, 0 10), (1 9, 4 8, 9 9, 9 1, 1 1, 1 9)), POLYGON ((9 9, 1 1, 1 9, 4 8, 9 9)), POLYGON ((1 1, 9 9, 9 1, 1 1)))",
                "MULTILINESTRING ((0 10, 0 0, 10 0, 10 10, 0 10), (1 1, 1 9, 4 8, 9 9), (1 1, 9 1, 9 9), (1 1, 9 9))");
        }

        [Test]
        public void TestTouchingSquares()
        {
            string wkt = "MULTIPOLYGON (((2 7, 2 8, 3 8, 3 7, 2 7)), ((1 6, 1 7, 2 7, 2 6, 1 6)), ((0 7, 0 8, 1 8, 1 7, 0 7)), ((0 5, 0 6, 1 6, 1 5, 0 5)), ((2 5, 2 6, 3 6, 3 5, 2 5)))";
            CheckEdgesSelected(wkt, 1,
                "MULTILINESTRING ((1 6, 0 6, 0 5, 1 5, 1 6), (1 6, 1 7), (1 6, 2 6), (1 7, 0 7, 0 8, 1 8, 1 7), (1 7, 2 7), (2 6, 2 5, 3 5, 3 6, 2 6), (2 6, 2 7), (2 7, 2 8, 3 8, 3 7, 2 7))");
            CheckEdgesSelected(wkt, 2,
                "MULTILINESTRING EMPTY");
        }

        [Test]
        public void TestAdjacentSquares()
        {
            string wkt = "GEOMETRYCOLLECTION (POLYGON ((1 3, 2 3, 2 2, 1 2, 1 3)), POLYGON ((3 3, 3 2, 2 2, 2 3, 3 3)), POLYGON ((3 1, 2 1, 2 2, 3 2, 3 1)), POLYGON ((1 1, 1 2, 2 2, 2 1, 1 1)))";
            CheckEdgesSelected(wkt, 1,
                "MULTILINESTRING ((1 2, 1 1, 2 1), (1 2, 1 3, 2 3), (2 1, 3 1, 3 2), (2 3, 3 3, 3 2))");
            CheckEdgesSelected(wkt, 2,
                "MULTILINESTRING ((1 2, 2 2), (2 1, 2 2), (2 2, 2 3), (2 2, 3 2))");
        }


        private void CheckEdges(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            var polygons = ToArray(geom);
            var edges = CoverageRingEdges.Create(polygons).Edges;
            var edgeLines = ToArray(edges, geom.Factory);
            var expected = Read(wktExpected);
            CheckEqual(expected, edgeLines);
        }

        private void CheckEdgesSelected(string wkt, int ringCount, string wktExpected)
        {
            var geom = Read(wkt);
            var polygons = ToArray(geom);
            var covEdges = CoverageRingEdges.Create(polygons);
            var edges = covEdges.SelectEdges(ringCount);
            var edgeLines = ToArray(edges, geom.Factory);
            var expected = Read(wktExpected);
            CheckEqual(expected, edgeLines);
        }

        private static MultiLineString ToArray(IList<CoverageEdge> edges, GeometryFactory geomFactory)
        {
            var lines = new LineString[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                lines[i] = edges[i].ToLineString(geomFactory);
            }
            return geomFactory.CreateMultiLineString(lines);

        }

        private static Geometry[] ToArray(Geometry geom)
        {
            var geoms = new Geometry[geom.NumGeometries];
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                geoms[i] = geom.GetGeometryN(i);
            }
            return geoms;
        }

    }

}
