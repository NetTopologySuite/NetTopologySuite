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
            /*
             * CoverageGapFinder returns gap rings as (closed) LineStrings.
             * LineString.Normalized() does not rotate the ring to a canonical
             * start point/orientation the way Polygon/LinearRing normalization
             * does, so comparing rings directly as LineStrings is fragile with
             * respect to which vertex happens to be first.
             * Wrap them as polygons (test-only) for a robust, rotation- and
             * orientation-invariant comparison. JTS made the equivalent fix to
             * its own port of this test (see JTS commit e0bddb56e).
             */
            CheckEqual(RingsAsPolygons(expected), RingsAsPolygons(actual));
        }

        private static Geometry RingsAsPolygons(Geometry geom)
        {
            if (geom.IsEmpty)
                return geom;

            var polys = new Geometry[geom.NumGeometries];
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var line = (LineString)geom.GetGeometryN(i);
                var ring = line.Factory.CreateLinearRing(line.CoordinateSequence);
                polys[i] = line.Factory.CreatePolygon(ring);
            }
            return geom.Factory.BuildGeometry(polys);
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
