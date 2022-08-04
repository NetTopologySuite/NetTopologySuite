using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoveragePolygonValidatorTest : GeometryTestCase
    {


        //========  Invalid cases   =============================
        [Test]
        public void TestCollinearUnmatchedEdge()
        {
            CheckInvalid("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                "POLYGON ((100 300, 180 300, 180 200, 100 200, 100 300))",
                "LINESTRING (100 200, 200 200)");
        }

        [Test]
        public void TestDuplicateGeometry()
        {
            CheckInvalid("POLYGON ((1 3, 3 3, 3 1, 1 1, 1 3))",
                "MULTIPOLYGON (((1 3, 3 3, 3 1, 1 1, 1 3)), ((5 3, 5 1, 3 1, 3 3, 5 3)))",
                "LINEARRING (1 3, 1 1, 3 1, 3 3, 1 3)");
        }

        [Test]
        public void TestCrossingSegment()
        {
            CheckInvalid("POLYGON ((1 9, 9 9, 9 3, 1 3, 1 9))",
                "POLYGON ((1 1, 5 6, 9 1, 1 1))",
                "LINESTRING (1 3, 9 3)");
        }

        [Test]
        public void TestCrossingAndInteriorSegments()
        {
            CheckInvalid("POLYGON ((1 1, 3 4, 7 4, 9 1, 1 1))",
                "POLYGON ((1 9, 9 9, 9 3, 1 3, 1 9))",
                "LINESTRING (1 1, 3 4, 7 4, 9 1)");
        }

        [Test]
        public void TestTargetVertexTouchesSegment()
        {
            CheckInvalid("POLYGON ((1 9, 9 9, 9 5, 1 5, 1 9))",
                "POLYGON ((1 1, 5 5, 9 1, 1 1))",
                "LINESTRING (9 5, 1 5)");
        }

        [Test]
        public void TestAdjVertexTouchesSegment()
        {
            CheckInvalid("POLYGON ((1 1, 5 5, 9 1, 1 1))",
                "POLYGON ((1 9, 9 9, 9 5, 1 5, 1 9))",
                "LINESTRING (1 1, 5 5, 9 1)");
        }

        [Test]
        public void TestInteriorSegmentTouchingEdge()
        {
            CheckInvalid("POLYGON ((4 3, 4 7, 8 9, 8 1, 4 3))",
                "POLYGON ((1 7, 6 7, 6 3, 1 3, 1 7))",
                "LINESTRING (8 1, 4 3, 4 7, 8 9)");
        }

        [Test]
        public void TestInteriorSegmentTouchingNodes()
        {
            CheckInvalid("POLYGON ((4 2, 4 8, 8 9, 8 1, 4 2))",
                "POLYGON ((1 5, 4 8, 7 5, 4 2, 1 5))",
                "LINESTRING (4 2, 4 8)");
        }

        [Test]
        public void TestInteriorSegmentsTouching()
        {
            CheckInvalid("POLYGON ((1 9, 5 9, 8 7, 5 7, 3 5, 8 2, 1 2, 1 9))",
                "POLYGON ((5 9, 9 9, 9 1, 5 1, 5 9))",
                "LINESTRING (5 9, 8 7, 5 7, 3 5, 8 2, 1 2)");
        }

        [Test]
        public void TestTargetMultiPolygon()
        {
            CheckInvalid("MULTIPOLYGON (((4 8, 9 9, 9 7, 4 8)), ((3 5, 9 6, 9 4, 3 5)), ((2 2, 9 3, 9 1, 2 2)))",
                "POLYGON ((1 1, 1 9, 5 9, 6 7, 5 5, 6 3, 5 1, 1 1))",
                "MULTILINESTRING ((9 7, 4 8, 9 9), (9 4, 3 5, 9 6), (9 1, 2 2, 9 3))");
        }

        [Test]
        public void TestBothMultiPolygon()
        {
            CheckInvalid("MULTIPOLYGON (((4 8, 9 9, 9 7, 4 8)), ((3 5, 9 6, 9 4, 3 5)), ((2 2, 9 3, 9 1, 2 2)))",
                "MULTIPOLYGON (((1 6, 1 9, 5 9, 6 7, 5 5, 1 6)), ((1 4, 5 5, 6 3, 5 1, 1 1, 1 4)))",
                "MULTILINESTRING ((9 7, 4 8, 9 9), (9 4, 3 5, 9 6), (9 1, 2 2, 9 3))");
        }

        /**
         * Shows need to evaluate both start and end point of intersecting segments
         * in InvalidSegmentDetector,
         * since matched segments are not tested
         */
        [Test]
        public void TestInteriorSegmentsWithMatch()
        {
            CheckInvalid("POLYGON ((7 6, 1 1, 3 6, 7 6))",
                "MULTIPOLYGON (((1 9, 9 9, 9 1, 1 1, 3 6, 1 9)), ((0 1, 0 9, 1 9, 3 6, 1 1, 0 1)))",
                "LINESTRING (3 6, 7 6, 1 1)");
        }

        [Test]
        public void TestAdjacentHoleOverlap()
        {
            CheckInvalid("POLYGON ((3 3, 3 7, 6 8, 7 3, 3 3))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (3 7, 7 7, 7 3, 3 3, 3 7))",
                "LINESTRING (3 7, 6 8, 7 3)");
        }

        [Test]
        public void TestTargetHoleOverlap()
        {
            CheckInvalid("POLYGON ((1 1, 1 9, 9 9, 9 1, 1 1), (2 2, 8 2, 8 8, 5 4, 3 5, 2 5, 2 2))",
                "POLYGON ((2 2, 2 5, 3 5, 8 6.7, 8 2, 2 2))",
                "LINESTRING (8 2, 8 8, 5 4, 3 5)");
        }

        [Test]
        public void TestFullyContained()
        {
            CheckInvalid("POLYGON ((3 7, 7 7, 7 3, 3 3, 3 7))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "LINESTRING (3 7, 7 7, 7 3, 3 3, 3 7)");
        }

        //========  Gap cases   =============================

        [Test]
        public void TestGap()
        {
            CheckInvalidGap("POLYGON ((1 5, 9 5, 9 1, 1 1, 1 5))",
                "POLYGON ((1 9, 5 9, 5 5.1, 1 5, 1 9))",
                0.5,
                "LINESTRING (1 5, 9 5)");
        }

        //========  Valid cases   =============================

        [Test]
        public void TestMatchedEdges()
        {
            CheckValid("POLYGON ((3 7, 7 7, 7 3, 3 3, 3 7))",
                "MULTIPOLYGON (((1 7, 3 7, 3 3, 1 3, 1 7)), ((3 9, 7 9, 7 7, 3 7, 3 9)), ((9 7, 9 3, 7 3, 7 7, 9 7)), ((3 1, 3 3, 7 3, 7 1, 3 1)))");
        }

        [Test]
        public void TestRingsCCW()
        {
            CheckValid("POLYGON ((1 1, 6 5, 4 9, 1 9, 1 1))",
                "POLYGON ((1 1, 9 1, 9 4, 6 5, 1 1))");
        }

        [Test]
        public void TestTargetCoveredAndMatching()
        {
            CheckValid("POLYGON ((1 7, 5 7, 9 7, 9 3, 5 3, 1 3, 1 7))",
                "MULTIPOLYGON (((5 9, 9 7, 5 7, 1 7, 5 9)), ((1 7, 5 7, 5 3, 1 3, 1 7)), ((9 3, 5 3, 5 7, 9 7, 9 3)), ((1 3, 5 3, 9 3, 5 1, 1 3)))");
        }

        //-- confirms zero-length segments are skipped in processing
        [Test]
        public void TestRepeatedCommonVertexInTarget()
        {
            CheckValid("POLYGON ((1 1, 1 3, 5 3, 5 3, 9 1, 1 1))",
                "POLYGON ((1 9, 9 9, 9 5, 5 3, 1 3, 1 9))");
        }

        //-- confirms zero-length segments are skipped in processing
        [Test]
        public void TestRepeatedCommonVertexInAdjacent()
        {
            CheckValid("POLYGON ((1 1, 1 3, 5 3, 9 1, 1 1))",
                "POLYGON ((1 9, 9 9, 9 5, 5 3, 5 3, 1 3, 1 9))");
        }

        //----------------------------------------------------------------------

        private void CheckInvalid(string wktTarget, string wktAdj, string wktExpected)
        {
            var target = Read(wktTarget);
            var adj = Read(wktAdj);
            var adjPolygons = ExtractPolygons(adj);
            var actual = CoveragePolygonValidator.Validate(target, adjPolygons);
            //System.out.println(actual);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        private void CheckInvalidGap(string wktTarget, string wktAdj,
            double gapWidth, string wktExpected)
        {
            var target = Read(wktTarget);
            var adj = Read(wktAdj);
            var adjPolygons = ExtractPolygons(adj);
            var actual = CoveragePolygonValidator.Validate(target, adjPolygons, gapWidth);
            //System.out.println(actual);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        private void CheckValid(string wktTarget, string wktAdj)
        {
            var target = Read(wktTarget);
            var adj = Read(wktAdj);
            var adjPolygons = ExtractPolygons(adj);
            var actual = CoveragePolygonValidator.Validate(target, adjPolygons);
            var expected = Read("LINESTRING EMPTY");    //TODO: check equals LINESTRING EMPTY
            CheckEqual(expected, actual);
        }

        private Geometry[] ExtractPolygons(Geometry geom)
        {
            var polygons = Extracter.GetPolygons(geom);
            return GeometryFactory.ToPolygonArray(polygons);
        }
    }
}
