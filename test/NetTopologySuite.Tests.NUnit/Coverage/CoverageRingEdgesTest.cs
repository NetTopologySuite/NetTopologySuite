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
        public void TestMultiPolygons()
        {
            CheckEdges("GEOMETRYCOLLECTION (MULTIPOLYGON (((5 9, 2.5 7.5, 1 5, 5 5, 5 9)), ((5 5, 9 5, 7.5 2.5, 5 1, 5 5))), MULTIPOLYGON (((5 9, 6.5 6.5, 9 5, 5 5, 5 9)), ((1 5, 5 5, 5 1, 3.5 3.5, 1 5))))",
                    "MULTILINESTRING ((1 5, 2.5 7.5, 5 9), (1 5, 3.5 3.5, 5 1), (1 5, 5 5), (5 1, 5 5), (5 1, 7.5 2.5, 9 5), (5 5, 5 9), (5 5, 9 5), (5 9, 6.5 6.5, 9 5))"
            );
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
