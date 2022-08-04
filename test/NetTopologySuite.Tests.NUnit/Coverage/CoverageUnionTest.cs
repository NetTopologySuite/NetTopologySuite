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
                "MULTIPOLYGON (((1 9, 5 9, 5 5, 1 5, 1 9)), ((5 9, 9 9, 9 5, 5 5, 5 9)), ((1 5, 5 5, 5 1, 1 1, 1 5)), ((5 5, 9 5, 9 1, 5 1, 5 5)))",
                "POLYGON ((5 9, 9 9, 9 5, 9 1, 5 1, 1 1, 1 5, 1 9, 5 9))"
                    );
        }

        private void CheckUnion(string wktCoverage, string wktExpected)
        {
            var covGeom = Read(wktCoverage);
            var coverage = ToArray(covGeom);
            var actual = CoverageUnion.Union(coverage);
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
