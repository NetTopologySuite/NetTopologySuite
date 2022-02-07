using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGZTest : GeometryTestCase
    {
        [Test]
        public void TestPointXYPointDifference()
        {
            CheckDifference("MULTIPOINT Z((1 1 NaN), (5 5 NaN))", "POINT Z (5 5 99)",
                "POINT Z(1 1 99)");
        }

        // checks that Point Z is preserved
        [Test]
        public void TestPointPolygonIntersection()
        {
            CheckIntersection("POINT Z (5 5 99)", "POLYGON Z ((1 9 5, 9 9 9, 9 1 5, 1 1 1, 1 9 5))",
                "POINT Z(5 5 99)");
        }

        [Test]
        public void TestLineIntersectionPointZInterpolated()
        {
            CheckIntersection("LINESTRING (0 0 0, 10 10 10)", "LINESTRING (10 0 0, 0 10 10)",
                "POINT(5 5 5)");
        }

        [Test]
        public void TestLineIntersectionPointZValue()
        {
            CheckIntersection("LINESTRING (0 0 0, 10 10 10)", "LINESTRING (10 0 0, 5 5 999, 0 10 10)",
                "POINT(5 5 999)");
        }

        [Test]
        public void TestLineOverlapUnion()
        {
            CheckUnion("LINESTRING (0 0 0, 10 10 10)", "LINESTRING (5 5 990, 15 15 999)",
                "MULTILINESTRING Z((0 0 0, 5 5 990), (5 5 990, 10 10 10), (10 10 10, 15 15 999))");
        }

        [Test]
        public void TestLineLineXYDifferenceLineInterpolated()
        {
            CheckDifference("LINESTRING (0 0 0, 10 10 10)", "LINESTRING (5 5, 6 6)",
                "MULTILINESTRING ((0 0 0, 5 5 5), (6 6 6, 10 10 10))");
        }

        // from https://trac.osgeo.org/geos/ticket/435
        [Test]
        public void TestLineXYLineIntersection()
        {
            CheckIntersection("LINESTRING(0 0,0 10,10 10,10 0)", "LINESTRING(10 10 4,10 0 5,0 0 5)",
                "GEOMETRYCOLLECTION Z(POINT Z(0 0 5), LINESTRING Z(10 0 5, 10 10 4))");
        }

        [Test]
        public void TestLinePolygonIntersection()
        {
            CheckIntersection("LINESTRING Z (0 0 0, 5 5 5)", "POLYGON Z ((1 9 5, 9 9 9, 9 1 5, 1 1 1, 1 9 5))",
                "LINESTRING Z (1 1 1, 5 5 5)");
        }

        [Test]
        public void TestLinePolygonDifference()
        {
            CheckDifference("LINESTRING Z (0 5 0, 10 5 10)", "POLYGON Z ((1 9 5, 9 9 9, 9 1 5, 1 1 1, 1 9 5))",
                "MULTILINESTRING Z((0 5 0, 1 5 2), (9 5 8, 10 5 10))");
        }

        [Test]
        public void TestPointXYPolygonIntersection()
        {
            CheckIntersection("POINT Z(5 5 NaN)", "POLYGON Z ((1 9 50, 9 9 90, 9 1 50, 1 1 10, 1 9 50))",
                "POINT Z(5 5 50)");
        }

        // XY Polygon gets Z value from Point
        [Test]
        public void TestPointPolygonXYUnion()
        {
            CheckUnion("POINT Z (5 5 77)", "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "POLYGON Z((1 1 77, 1 9 77, 9 9 77, 9 1 77, 1 1 77))");
        }

        [Test]
        public void TestLinePolygonXYDifference()
        {
            CheckDifference("LINESTRING Z (0 5 0, 10 5 10)", "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "MULTILINESTRING Z((0 5 0, 1 5 1), (9 5 9, 10 5 10))");
        }

        [Test]
        public void TestLineXYPolygonDifference()
        {
            CheckDifference("LINESTRING (0 5, 10 5)", "POLYGON Z ((1 9 50, 9 9 90, 9 1 50, 1 1 10, 1 9 50))",
                "MULTILINESTRING Z((0 5 50, 1 5 30), (9 5 70, 10 5 50))");
        }

        [Test]
        public void TestPolygonXYPolygonIntersection()
        {
            CheckIntersection("POLYGON ((4 12, 2 6, 7 6, 11 4, 15 15, 4 12))",
                "POLYGON Z ((1 9 50, 9 9 90, 9 1 50, 1 1 10, 1 9 50))",
                "POLYGON Z((2 6 50, 3 9 60, 9 9 90, 9 5 70, 7 6 90, 2 6 50))");
        }

        [Test]
        public void TestPolygonXYPolygonUnion()
        {
            CheckUnion("POLYGON ((0 3, 3 3, 3 0, 0 0, 0 3))", "POLYGON Z ((1 9 50, 9 9 90, 9 1 50, 1 1 10, 1 9 50))",
                "POLYGON Z((0 0 10, 0 3 50, 1 3 20, 1 9 50, 9 9 90, 9 1 50, 3 1 20, 3 0 50, 0 0 10))");
        }

        // Test that operation on XY geoms produces XY (Z = NaN)
        [Test]
        public void TestPolygonXYPolygonXYIntersection()
        {
            CheckIntersection("POLYGON ((4 12, 2 6, 7 6, 11 4, 15 15, 4 12))", "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "POLYGON ((2 6, 3 9, 9 9, 9 5, 7 6, 2 6))");
        }

//=================================================

        private void CheckIntersection(string wktA, string wktB, string wktExpected)
        {
            CheckOverlay(SpatialFunction.Intersection, wktA, wktB, wktExpected);
        }

        private void CheckDifference(string wktA, string wktB, string wktExpected)
        {
            CheckOverlay(SpatialFunction.Difference, wktA, wktB, wktExpected);
        }

        private void CheckUnion(string wktA, string wktB, string wktExpected)
        {
            CheckOverlay(SpatialFunction.Union, wktA, wktB, wktExpected);
        }

        private void CheckOverlay(SpatialFunction opCode, string wktA, string wktB, string wktExpected)
        {
            var a = Read(wktA);
            var b = Read(wktB);
            var result = NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, opCode);
            var expected = Read(wktExpected);
            CheckEqualXYZ(expected, result);
        }
    }
}
