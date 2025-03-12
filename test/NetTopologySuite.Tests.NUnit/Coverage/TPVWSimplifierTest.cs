using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System.Collections.Generic;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class TPVWSimplifierTest : GeometryTestCase
    {
        [Test]
        public void TestSimpleNoop()
        {
            CheckNoop("MULTILINESTRING ((9 9, 3 9, 1 4, 4 1, 9 1), (9 1, 2 4, 9 9))", 2);
        }

        [Test]
        public void TestSimple()
        {
            CheckSimplify(
                "MULTILINESTRING ((9 9, 3 9, 1 4, 4 1, 9 1), (9 1, 6 3, 2 4, 5 7, 9 9))", 2,
                "MULTILINESTRING ((9 9, 3 9, 1 4, 4 1, 9 1), (9 1, 2 4, 9 9))");
        }

        [Test]
        public void TestFreeRing()
        {
            CheckSimplify("MULTILINESTRING ((1 9, 9 9, 9 1), (1 9, 1 1, 9 1), (7 5, 8 8, 2 8, 2 2, 8 2, 7 5))",
                new int[] { 2 }, 2,
                "MULTILINESTRING ((1 9, 1 1, 9 1), (1 9, 9 9, 9 1), (8 8, 2 8, 2 2, 8 2, 8 8))");
        }

        [Test]
        public void TestNoFreeRing()
        {
            CheckSimplify("MULTILINESTRING ((1 19, 19 19, 19 1), (1 19, 1 1, 19 1), (10 10, 9 18, 2 18, 2 2, 7 6, 10 10), (10 10, 11 18, 18 18, 18 2, 13 6, 10 10))",
                new int[] { },
                2,
                "MULTILINESTRING ((1 19, 1 1, 19 1), (1 19, 19 19, 19 1), (10 10, 2 2, 2 18, 9 18, 10 10), (10 10, 11 18, 18 18, 18 2, 10 10))");
        }

        [Test]
        public void TestConstraint()
        {
            CheckSimplify("MULTILINESTRING ((6 8, 2 8, 2.1 5, 2 2, 6 2, 5.9 5, 6 8))",
                new int[] { },
                "MULTILINESTRING ((1 9, 9 9, 6 5, 9 1), (1 9, 1 1, 9 1))",
                1,
                "MULTILINESTRING ((1 9, 1 1, 9 1), (1 9, 9 9, 6 5, 9 1), (6 8, 2 8, 2 2, 6 2, 5.9 5, 6 8))");
        }



        private void CheckNoop(string wkt, double tolerance)
        {
            CheckSimplify(wkt, null, null, tolerance, wkt);
        }

        private void CheckSimplify(string wkt, double tolerance, string wktExpected)
        {
            CheckSimplify(wkt, null, null, tolerance, wktExpected);
        }

        private void CheckSimplify(string wkt, int[] freeRingIndex,
            double tolerance, string wktExpected)
        {
            CheckSimplify(wkt, freeRingIndex, null, tolerance, wktExpected);
        }

        private void CheckSimplify(string wkt, int[] freeRingIndex,
            string wktConstraints,
            double tolerance, string wktExpected)
        {
            var edges = CreateEdges(wkt, freeRingIndex, wktConstraints, tolerance);
            var cornerArea = new CornerArea();
            TPVWSimplifier.Simplify(edges, cornerArea, 1.0);
            var expected = Read(wktExpected);
            var actual = CreateResult(edges, expected.Factory);
            CheckEqual(expected, actual);
        }

        private TPVWSimplifier.Edge[] CreateEdges(string wkt, int[] freeRingIndex, string wktConstraints, double tolerance)
        {
            var edgeList = new List<TPVWSimplifier.Edge>();
            AddEdges(wkt, freeRingIndex, tolerance, edgeList);
            if (wktConstraints != null)
            {
                AddEdges(wktConstraints, null, 0.0, edgeList);
            }
            var edges = edgeList.ToArray();
            return edges;
        }

        private void AddEdges(string wkt, int[] freeRings, double tolerance, IList<TPVWSimplifier.Edge> edges)
        {
            var lines = (MultiLineString)Read(wkt);
            for (int i = 0; i < lines.NumGeometries; i++)
            {
                var line = (LineString)lines.GetGeometryN(i);
                bool isRemovable = false;
                bool isFreeRing = freeRings == null ? false : HasIndex(freeRings, i);
                var edge = new TPVWSimplifier.Edge(line.Coordinates, tolerance, isFreeRing, isRemovable);
                edges.Add(edge);
            }
        }

        private static bool HasIndex(int[] freeRings, int i)
        {
            foreach (int fr in freeRings)
            {
                if (fr == i)
                    return true;
            }
            return false;
        }

        private static MultiLineString CreateResult(TPVWSimplifier.Edge[] edges, GeometryFactory geomFactory)
        {
            var result = new LineString[edges.Length];
            for (int i = 0; i < edges.Length; i++)
            {
                var pts = edges[i].Coordinates;
                result[i] = geomFactory.CreateLineString(pts);
            }
            return geomFactory.CreateMultiLineString(result);
        }

    }
}
