using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoverageUnionTest : GeometryTestCase
    {

        [Test]
        public void TestChessboard4()
        {
            CheckUnion(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 5 9, 5 5, 1 5, 1 9)), POLYGON ((5 9, 9 9, 9 5, 5 5, 5 9)), POLYGON ((1 5, 5 5, 5 1, 1 1, 1 5)), POLYGON ((5 5, 9 5, 9 1, 5 1, 5 5)))",
                "POLYGON ((5 9, 9 9, 9 5, 9 1, 5 1, 1 1, 1 5, 1 9, 5 9))"
            );
        }

        [Test]
        public void TestEmpty()
        {
            CheckUnion(
                "GEOMETRYCOLLECTION EMPTY",
                null
                );
        }

        [Test]
        public void TestHoleTouchingSide()
        {
            CheckUnion(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 6, 2 6, 1 9)), POLYGON ((1 1, 1 9, 2 6, 5 3, 9 6, 9 1, 1 1)))",
                "POLYGON ((9 6, 9 1, 1 1, 1 9, 9 9, 9 6), (9 6, 2 6, 5 3, 9 6))"
                );
        }

        [Test]
        public void TestHolesTouchingSide()
        {
            CheckUnion(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 6, 5 7, 2 6, 1 9)), POLYGON ((1 1, 1 9, 2 6, 4 3, 5 7, 7 3, 9 6, 9 1, 1 1)))",
                "POLYGON ((9 9, 9 6, 9 1, 1 1, 1 9, 9 9), (5 7, 7 3, 9 6, 5 7), (2 6, 4 3, 5 7, 2 6))"
                );
        }

        [Test]
        public void TestHolesTouching() {
            CheckUnion(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 6, 7 7, 5 7, 2 6, 1 9)), POLYGON ((1 1, 1 9, 2 6, 4 3, 5 7, 7 3, 7 7, 9 6, 9 1, 1 1)))",
                "POLYGON ((9 9, 9 6, 9 1, 1 1, 1 9, 9 9), (5 7, 7 3, 7 7, 5 7), (2 6, 4 3, 5 7, 2 6))"
                );
        }


        private void CheckUnion(string wktCoverage, string wktExpected)
        {
            var covGeom = Read(wktCoverage);
            var coverage = ToArray(covGeom);
            var actual = CoverageUnion.Union(coverage);
            if (wktExpected == null)
            {
                Assert.That(actual, Is.Null);
                return;
            }
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
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
