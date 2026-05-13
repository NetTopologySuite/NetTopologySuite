using System;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Distance
{
    public class DirectedHausdorffDistanceTest : GeometryTestCase
    {
        private const double Tolerance = 0.001;

        //-- empty inputs

        [Test]
        public void TestEmptyPoint() => CheckDistanceEmpty("POINT EMPTY", "POINT (1 1)");

        [Test]
        public void TestEmptyLine() => CheckDistanceEmpty("LINESTRING EMPTY", "LINESTRING (0 0, 2 1)");

        [Test]
        public void TestEmptyPolygon() => CheckDistanceEmpty("POLYGON EMPTY", "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");

        //-- extreme and invalid inputs

        [Test]
        public void TestZeroTolerancePoint()
        {
            CheckDistance("POINT (5 5)", "LINESTRING (5 1, 9 5)", 0, "LINESTRING (5 5, 7 3)");
        }

        [Test]
        public void TestZeroToleranceLine()
        {
            CheckDistance("LINESTRING (1 5, 5 5)", "LINESTRING (5 1, 9 5)", 0, "LINESTRING (1 5, 5 1)");
        }

        [Test]
        public void TestZeroToleranceZeroLengthLineQuery()
        {
            CheckDistance("LINESTRING (5 5, 5 5)", "LINESTRING (5 1, 9 5)", 0, "LINESTRING (5 5, 7 3)");
        }

        [Test]
        public void TestZeroLengthLineQuery()
        {
            CheckDistance("LINESTRING (5 5, 5 5)", "LINESTRING (5 1, 9 5)", "LINESTRING (5 5, 7 3)");
        }

        [Test]
        public void TestZeroLengthPolygonQuery()
        {
            CheckDistance("POLYGON ((5 5, 5 5, 5 5, 5 5))", "LINESTRING (5 1, 9 5)", "LINESTRING (5 5, 7 3)");
        }

        [Test]
        public void TestZeroLengthLineTarget()
        {
            CheckDistance("POINT (5 5)", "LINESTRING (5 1, 5 1)", "LINESTRING (5 5, 5 1)");
        }

        [Test]
        public void TestNegativeTolerancePoint()
        {
            Assert.Throws<ArgumentException>(() =>
                CheckDistance("POINT (5 5)", "LINESTRING (5 1, 9 5)", -1, "LINESTRING (5 5, 7 3)"));
        }

        [Test]
        public void TestNegativeToleranceLine()
        {
            Assert.Throws<ArgumentException>(() =>
                CheckDistance("LINESTRING (1 5, 5 5)", "LINESTRING (5 1, 9 5)", -1, "LINESTRING (1 5, 5 1)"));
        }

        //--------------------------------------

        [Test]
        public void TestPointPoint()
        {
            CheckHausdorff("POINT (0 0)", "POINT (1 1)", "LINESTRING (0 0, 1 1)");
        }

        [Test]
        public void TestPointsPoints()
        {
            string a = "MULTIPOINT ((0 1), (2 3), (4 5), (6 6))";
            string b = "MULTIPOINT ((0.1 0), (1 0), (2 0), (3 0), (4 0), (5 0))";
            CheckDistance(a, b, "LINESTRING (6 6, 5 0)");
            CheckDistance(b, a, "LINESTRING (5 0, 2 3)");
            CheckHausdorff(a, b, "LINESTRING (6 6, 5 0)");
        }

        [Test]
        public void TestPointPolygonInterior()
        {
            CheckDistance("POINT (3 4)", "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))", 0);
        }

        [Test]
        public void TestPointsPolygon()
        {
            CheckDistance("MULTIPOINT ((4 3), (2 8), (8 5))", "POLYGON ((6 9, 6 4, 9 1, 1 1, 6 9))",
                "LINESTRING (2 8, 4.426966292134832 6.48314606741573)");
        }

        [Test]
        public void TestLineSegments()
        {
            CheckHausdorff("LINESTRING (0 0, 2 0)", "LINESTRING (0 0, 2 1)", "LINESTRING (2 0, 2 1)");
        }

        [Test]
        public void TestLineSegments2()
        {
            CheckHausdorff("LINESTRING (0 0, 2 0)", "LINESTRING (0 1, 1 2, 2 1)", "LINESTRING (1 0, 1 2)");
        }

        [Test]
        public void TestLinePoints()
        {
            CheckHausdorff("LINESTRING (0 0, 2 0)", "MULTIPOINT (0 2, 1 0, 2 1)", "LINESTRING (0 0, 0 2)");
        }

        [Test]
        public void TestLinesTopoEqual()
        {
            CheckDistance("MULTILINESTRING ((10 10, 10 90, 40 30), (40 30, 60 80, 90 30, 40 10))",
                "LINESTRING (10 10, 10 90, 40 30, 60 80, 90 30, 40 10)",
                0.0);
        }

        [Test]
        public void TestLinesPolygon()
        {
            CheckHausdorff("MULTILINESTRING ((1 1, 2 7), (7 1, 9 9))",
                "POLYGON ((3 7, 6 7, 6 4, 3 4, 3 7))",
                "LINESTRING (9 9, 6 7)");
        }

        [Test]
        public void TestLinesPolygon2()
        {
            string a = "MULTILINESTRING ((2 3, 2 7), (9 1, 9 8, 4 9))";
            string b = "POLYGON ((3 7, 6 8, 8 2, 3 4, 3 7))";
            CheckDistance(a, b, "LINESTRING (9 8, 6.3 7.1)");
            // Symmetric Hausdorff: a's nearest-to-LEC-of-b vertex is a tie between (2,3) and (9,3),
            // both at distance 3.5 from (5.5, 3). JTS's R-tree visits (2,3) first; NTS's reaches
            // (9,3) first. The Hausdorff distance value is identical; check that instead.
            CheckHausdorffDistanceValue(a, b, 3.5);
        }

        [Test]
        public void TestPolygonLineCrossingBoundaryResult()
        {
            CheckDistance("POLYGON ((2 8, 8 2, 2 1, 2 8))",
                "LINESTRING (6 5, 4 7, 0 0, 8 4)",
                "LINESTRING (2 8, 3.9384615384615387 6.892307692307693)");
        }

        [Test]
        public void TestPolygonLineCrossingInteriorPoint()
        {
            CheckDistanceStartPtLen("POLYGON ((2 8, 8 2, 2 1, 2 8))",
                "LINESTRING (6 5, 4 7, 0 0, 9 1)",
                "LINESTRING (4.555 2.989, 4.828 0.536)", 0.01);
        }

        [Test]
        public void TestPolygonPolygon()
        {
            string a = "POLYGON ((2 18, 18 18, 17 3, 2 2, 2 18))";
            string b = "POLYGON ((1 19, 5 12, 5 3, 14 10, 11 19, 19 19, 20 0, 1 1, 1 19))";
            CheckDistance(b, a, "LINESTRING (20 0, 17 3)");
            CheckDistance(a, b, "LINESTRING (6.6796875 18, 11 19)");
            CheckHausdorff(a, b, "LINESTRING (6.6796875 18, 11 19)");
        }

        [Test]
        public void TestPolygonPolygonHolesNested()
        {
            // B is contained in A
            string a = "POLYGON ((1 19, 19 19, 19 1, 1 1, 1 19), (6 8, 11 14, 15 7, 6 8))";
            string b = "POLYGON ((2 18, 18 18, 18 2, 2 2, 2 18), (10 17, 3 7, 17 5, 10 17))";
            CheckDistance(a, b, "LINESTRING (9.817138671875 12.58056640625, 7.863620425230705 13.948029178901006)");
            CheckDistance(b, a, 0.0);
        }

        [Test]
        public void TestMultiPolygons()
        {
            string a = "MULTIPOLYGON (((1 1, 1 10, 5 1, 1 1)), ((4 17, 9 15, 9 6, 4 17)))";
            string b = "MULTIPOLYGON (((1 12, 4 13, 8 10, 1 12)), ((3 8, 7 7, 6 2, 3 8)))";
            CheckDistance(a, b, "LINESTRING (1 1, 5.4 3.2)");
            CheckDistanceStartPtLen(b, a,
                "LINESTRING (2.669921875 12.556640625, 5.446115154109589 13.818546660958905)",
                0.01);
        }

        // Tests that target area interiors have distance = 0
        [Test]
        public void TestLinePolygonCrossing()
        {
            CheckDistance("LINESTRING (2 5, 5 10, 6 4)",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "LINESTRING (5 10, 5 9)");
        }

        [Test]
        public void TestNonVertexResult()
        {
            string wkt1 = "LINESTRING (1 1, 5 10, 9 1)";
            string wkt2 = "LINESTRING (0 10, 0 0, 10 0)";
            CheckHausdorff(wkt1, wkt2, "LINESTRING (6.53857421875 6.5382080078125, 6.53857421875 0)");
            CheckDistance(wkt1, wkt2, "LINESTRING (6.53857421875 6.5382080078125, 6.53857421875 0)");
        }

        [Test]
        public void TestDirectedLines()
        {
            string wkt1 = "LINESTRING (1 6, 3 5, 1 4)";
            string wkt2 = "LINESTRING (1 10, 9 5, 1 2)";
            CheckDistance(wkt1, wkt2, "LINESTRING (1 6, 2.797752808988764 8.876404494382022)");
            CheckDistance(wkt2, wkt1, "LINESTRING (9 5, 3 5)");
        }

        [Test]
        public void TestDirectedLines2()
        {
            string wkt1 = "LINESTRING (1 6, 3 5, 1 4)";
            string wkt2 = "LINESTRING (1 3, 1 9, 9 5, 1 1)";
            CheckDistance(wkt1, wkt2, "LINESTRING (3 5, 1 5)");
            CheckDistance(wkt2, wkt1, "LINESTRING (9 5, 3 5)");
        }

        // Tests that segments are detected as interior even for a large tolerance.
        [Test]
        public void TestInteriorSegmentsLargeTol()
        {
            string a = "POLYGON ((4 6, 5 6, 5 5, 4 5, 4 6))";
            string b = "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))";
            CheckDistance(a, b, 2.0, 0.0);
        }

        // Tests that segment endpoint nearest points which are interior to B have distance 0
        [Test]
        public void TestInteriorSegmentsSameExterior()
        {
            string a = "POLYGON ((1 9, 3 9, 4 5, 5.05 9, 9 9, 9 1, 1 1, 1 9))";
            string b = "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))";
            CheckDistance(a, b, 0.0);
        }

        //-----------------------------------------------------

        [Test]
        public void TestFullyWithinDistanceEmptyPoints()
        {
            CheckFullyWithinDistanceEmpty("POINT EMPTY", "MULTIPOINT ((1 1), (9 9))");
        }

        [Test]
        public void TestFullyWithinDistanceEmptyLine()
        {
            CheckFullyWithinDistanceEmpty("LINESTRING EMPTY", "LINESTRING (9 9, 1 1)");
        }

        [Test]
        public void TestFullyWithinDistancePoints()
        {
            string a = "MULTIPOINT ((1 9), (9 1))";
            string b = "MULTIPOINT ((1 1), (9 9))";
            CheckFullyWithinDistance(a, b, 1, false);
            CheckFullyWithinDistance(a, b, 8.1, true);
        }

        [Test]
        public void TestFullyWithinDistanceDisconnectedLines()
        {
            string a = "MULTILINESTRING ((1 9, 2 9), (8 1, 9 1))";
            string b = "LINESTRING (9 9, 1 1)";
            CheckFullyWithinDistance(a, b, 1, false);
            CheckFullyWithinDistance(a, b, 6, true);
            CheckFullyWithinDistance(b, a, 1, false);
            CheckFullyWithinDistance(b, a, 7.1, true);
        }

        [Test]
        public void TestFullyWithinDistanceDisconnectedPolygons()
        {
            string a = "MULTIPOLYGON (((1 9, 2 9, 2 8, 1 8, 1 9)), ((8 2, 9 2, 9 1, 8 1, 8 2)))";
            string b = "POLYGON ((1 2, 9 9, 2 1, 1 2))";
            CheckFullyWithinDistance(a, b, 1, false);
            CheckFullyWithinDistance(a, b, 5.3, true);
            CheckFullyWithinDistance(b, a, 1, false);
            CheckFullyWithinDistance(b, a, 7.1, true);
        }

        [Test]
        public void TestFullyWithinDistanceLines()
        {
            string a = "MULTILINESTRING ((1 1, 3 3), (7 7, 9 9))";
            string b = "MULTILINESTRING ((1 9, 1 5), (6 4, 8 2))";
            CheckFullyWithinDistance(a, b, 1, false);
            CheckFullyWithinDistance(a, b, 4, false);
            CheckFullyWithinDistance(a, b, 6, true);
        }

        [Test]
        public void TestFullyWithinDistancePolygons()
        {
            string a = "POLYGON ((1 4, 4 4, 4 1, 1 1, 1 4))";
            string b = "POLYGON ((10 10, 10 15, 15 15, 15 10, 10 10))";
            CheckFullyWithinDistance(a, b, 5, false);
            CheckFullyWithinDistance(a, b, 10, false);
            CheckFullyWithinDistance(a, b, 20, true);
        }

        [Test]
        public void TestFullyWithinDistancePolygonsNestedWithHole()
        {
            string a = "POLYGON ((2 8, 8 8, 8 2, 2 2, 2 8))";
            string b = "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (3 7, 7 7, 7 3, 3 3, 3 7))";
            CheckFullyWithinDistance(a, b, 1, false);
            CheckFullyWithinDistance(a, b, 2, true);
            CheckFullyWithinDistance(a, b, 3, true);
        }

        //======================================================================

        private void CheckHausdorff(string wkt1, string wkt2, string wktExpected)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            var pts = DirectedHausdorffDistance.HausdorffDistancePoints(g1, g2);
            var result = g1.Factory.CreateLineString(pts);
            var expected = Read(wktExpected);
            CheckEqualResult(expected, result);
        }

        private void CheckDistance(string wkt1, string wkt2, string wktExpected)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            var pts = DirectedHausdorffDistance.DistancePoints(g1, g2);
            var result = g1.Factory.CreateLineString(pts);
            var expected = Read(wktExpected);
            CheckEqualResult(expected, result);
        }

        private void CheckDistance(string wkt1, string wkt2, double tolerance, string wktExpected)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            var pts = DirectedHausdorffDistance.DistancePoints(g1, g2, tolerance);
            var result = g1.Factory.CreateLineString(pts);
            var expected = Read(wktExpected);
            CheckEqualResult(expected, result);
        }

        private void CheckDistanceStartPtLen(string wkt1, string wkt2,
            string wktExpected, double resultTolerance)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            var pts = DirectedHausdorffDistance.DistancePoints(g1, g2);
            var result = g1.Factory.CreateLineString(pts);
            var expected = Read(wktExpected);

            var resultPt = result.Coordinates[0];
            var expectedPt = expected.Coordinates[0];
            CheckEqualXY(expectedPt, resultPt, resultTolerance);

            double distResult = result.Length;
            double distExpected = expected.Length;
            Assert.That(distResult, Is.EqualTo(distExpected).Within(resultTolerance));
        }

        private void CheckDistance(string wkt1, string wkt2, double tolerance, double expectedDistance)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            double distResult = DirectedHausdorffDistance.Distance(g1, g2, tolerance);
            Assert.That(distResult, Is.EqualTo(expectedDistance).Within(Tolerance));
        }

        private void CheckDistance(string wkt1, string wkt2, double expectedDistance)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            double distResult = DirectedHausdorffDistance.Distance(g1, g2);
            Assert.That(distResult, Is.EqualTo(expectedDistance).Within(Tolerance));
        }

        private void CheckFullyWithinDistance(string a, string b, double distance, bool expected)
        {
            var g1 = Read(a);
            var g2 = Read(b);
            bool result = DirectedHausdorffDistance.IsFullyWithinDistance(g1, g2, distance);
            Assert.That(result, Is.EqualTo(expected));
        }

        private void CheckFullyWithinDistanceEmpty(string a, string b)
        {
            CheckFullyWithinDistance(a, b, 0, false);
            CheckFullyWithinDistance(b, a, 0, false);
            CheckFullyWithinDistance(a, b, 1, false);
            CheckFullyWithinDistance(b, a, 1, false);
            CheckFullyWithinDistance(a, b, 1000, false);
            CheckFullyWithinDistance(b, a, 1000, false);
        }

        private void CheckDistanceEmpty(string a, string b)
        {
            var g1 = Read(a);
            var g2 = Read(b);

            var resultPts = DirectedHausdorffDistance.DistancePoints(g1, g2);
            Assert.That(resultPts, Is.Null);

            double resultDist = DirectedHausdorffDistance.Distance(g1, g2);
            Assert.That(double.IsNaN(resultDist), Is.True);

            double hausdorffDist = DirectedHausdorffDistance.HausdorffDistance(g1, g2);
            Assert.That(double.IsNaN(hausdorffDist), Is.True);
        }

        private void CheckHausdorffDistanceValue(string wkt1, string wkt2, double expectedDistance)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            double distResult = DirectedHausdorffDistance.HausdorffDistance(g1, g2);
            Assert.That(distResult, Is.EqualTo(expectedDistance).Within(Tolerance));
        }

        private void CheckEqualResult(Geometry expected, Geometry actual)
        {
            bool equal = actual.EqualsExact(expected, Tolerance);
            if (!equal)
            {
                TestContext.WriteLine($"FAIL:\nExpected = {expected}\nActual   = {actual}");
            }
            Assert.That(equal, Is.True);
        }
    }
}
