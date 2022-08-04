using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoverageGapFinderTest : GeometryTestCase
    {

        [Test]
        public void TestThreePolygonGap()
        {
            CheckGaps(
                "MULTIPOLYGON (((1 5, 1 9, 5 9, 5 6, 3 5, 1 5)), ((5 9, 9 9, 9 5, 7 5, 5 6, 5 9)), ((1 1, 1 5, 3 5, 7 5, 9 5, 9 1, 1 1)))",
                1,
                "LINESTRING (3 5, 7 5, 5 6, 3 5)"
                    );
        }

        private void CheckGaps(string wktCoverage, double gapWidth, string wktExpected)
        {
            var covGeom = Read(wktCoverage);
            var coverage = ToArray(covGeom);
            var actual = CoverageGapFinder.FindGaps(coverage, gapWidth);
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
