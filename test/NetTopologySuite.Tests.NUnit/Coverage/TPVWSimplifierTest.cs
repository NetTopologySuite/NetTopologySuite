using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NUnit.Framework;

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
            CheckSimplify("MULTILINESTRING ((1 9, 9 9, 9 1), (1 9, 1 1, 9 1), (5 5, 4 8, 2 8, 2 2, 4 2, 5 5), (5 5, 6 8, 8 8, 8 2, 6 2, 5 5))",
                new int[] { },
                2,
                "MULTILINESTRING ((1 9, 1 1, 9 1), (1 9, 9 9, 9 1), (5 5, 2 2, 2 8, 5 5), (5 5, 8 2, 8 8, 5 5))");
        }

        [Test]
        public void TestConstraint()
        {
            CheckSimplify("MULTILINESTRING ((6 8, 2 8, 2.1 5, 2 2, 6 2, 5.9 5, 6 8))",
                new int[] { },
                "MULTILINESTRING ((1 9, 9 9, 6 5, 9 1), (1 9, 1 1, 9 1))",
                1,
                "MULTILINESTRING ((6 8, 2 8, 2 2, 6 2, 5.9 5, 6 8))");
        }



        private void CheckNoop(string wkt, double tolerance)
        {
            var geom = (MultiLineString)Read(wkt);
            var actual = TPVWSimplifier.Simplify(geom, tolerance);
            CheckEqual(geom, actual);
        }

        private void CheckSimplify(string wkt, double tolerance, string wktExpected)
        {
            var geom = (MultiLineString)Read(wkt);
            var actual = TPVWSimplifier.Simplify(geom, tolerance);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
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
            var lines = (MultiLineString)Read(wkt);
            var freeRings = new System.Collections.BitArray(lines.NumGeometries);
            foreach (int index in freeRingIndex)
                freeRings[index] = true;
            var constraints = (MultiLineString)(string.IsNullOrWhiteSpace(wktConstraints) ? null : Read(wktConstraints));
            var actual = TPVWSimplifier.Simplify(lines, freeRings, constraints, tolerance);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

    }
}
