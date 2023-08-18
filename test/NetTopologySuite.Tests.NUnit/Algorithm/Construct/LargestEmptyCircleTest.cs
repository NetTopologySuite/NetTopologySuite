using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Construct
{
    public class LargestEmptyCircleTest : GeometryTestCase
    {
        [Test]
        public void TestPointsSquare()
        {
            CheckCircle("MULTIPOINT ((100 100), (100 200), (200 200), (200 100))",
                0.01, 150, 150, 70.71);
        }

        [Test]
        public void TestPointsTriangleOnHull()
        {
            CheckCircle("MULTIPOINT ((100 100), (300 100), (150 50))",
                0.01, 216.66, 99.99, 83.33);
        }

        [Test]
        public void TestPointsTriangleInterior()
        {
            CheckCircle("MULTIPOINT ((100 100), (300 100), (200 250))",
                0.01, 200.00, 141.66, 108.33);
        }

        [Test]
        public void TestLinesOpenDiamond()
        {
            CheckCircle("MULTILINESTRING ((50 100, 150 50), (250 50, 350 100), (350 150, 250 200), (50 150, 150 200))",
                0.01, 200, 125, 90.13);
        }

        [Test]
        public void TestLinesCrossed()
        {
            CheckCircle("MULTILINESTRING ((100 100, 300 300), (100 200, 300 0))",
                0.01, 299.99, 150.00, 106.05);
        }

        [Test, Ignore("Investigate")]
        public void TestLinesZigzag()
        {
            CheckCircle(
                "MULTILINESTRING ((100 100, 200 150, 100 200, 250 250, 100 300, 300 350, 100 400), (70 380, 0 350, 50 300, 0 250, 50 200, 0 150, 50 120))",
                0.01, 77.52, 249.99, 54.81);
        }

        [Test]
        public void TestPointsLinesTriangle()
        {
            CheckCircle("GEOMETRYCOLLECTION (LINESTRING (100 100, 300 100), POINT (250 200))",
                0.01, 196.49, 164.31, 64.31);
        }

        [Test]
        public void TestPoint()
        {
            CheckCircleZeroRadius("POINT (100 100)",
                0.01);
        }

        [Test]
        public void TestLineFlat()
        {
            CheckCircleZeroRadius("LINESTRING (0 0, 50 50)",
                0.01);
        }

        [Test]
        public void TestThinExtent()
        {
            CheckCircle("MULTIPOINT ((100 100), (300 100), (200 100.1))",
               0.01);
        }

        //---------------------------------------------------------
        // Obstacles and Boundary

        [Test]
        public void TestBoundaryEmpty()
        {
            CheckCircle("MULTIPOINT ((2 2), (8 8), (7 5))",
                "POLYGON EMPTY",
                0.01, 4.127, 4.127, 3);
        }

        [Test]
        public void TestBoundarySquare()
        {
            CheckCircle("MULTIPOINT ((2 2), (6 4), (8 8))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                0.01, 1.00390625, 8.99609375, 7.065);
        }

        [Test]
        public void TestBoundarySquareObstaclesOutside()
        {
            CheckCircle("MULTIPOINT ((10 10), (10 0))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                0.01, 1.0044, 4.997, 10.29);
        }

        [Test]
        public void TestBoundaryMultiSquares()
        {
            CheckCircle("MULTIPOINT ((10 10), (10 0), (5 5))",
                "MULTIPOLYGON (((1 9, 9 9, 9 1, 1 1, 1 9)), ((15 20, 20 20, 20 15, 15 15, 15 20)))",
                0.01, 19.995, 19.997, 14.137);
        }

        [Test]
        public void TestBoundaryAsObstacle()
        {
            CheckCircle("GEOMETRYCOLLECTION (LINESTRING (1 9, 9 9, 9 1, 1 1, 1 9), POINT (4 3), POINT (7 6))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                0.01, 4, 6, 3);
        }


        [Test]
        public void TestObstacleEmptyElement()
        {
            CheckCircle("GEOMETRYCOLLECTION (LINESTRING EMPTY, POINT (4 3), POINT (7 6), POINT (4 6))",
                0.01, 5.5, 4.5, 2.12);
        }

        //========================================================

        /**
         * A coarse distance check, mainly testing 
         * that there is not a huge number of iterations.
         * (This will be revealed by CI taking a very long time!)
         * 
         * @param wkt
         * @param tolerance
         */
        private void CheckCircle(string wkt, double tolerance)
        {
            var geom = Read(wkt);
            var lec = new LargestEmptyCircle(geom, null, tolerance);
            var centerPoint = lec.GetCenter();
            double dist = geom.Distance(centerPoint);
            var radiusLine = lec.GetRadiusLine();
            double actualRadius = radiusLine.Length;
            Assert.That(Math.Abs(actualRadius - dist) < 2 * tolerance);
        }

        private void CheckCircle(string wktObstacles, double tolerance,
            double x, double y, double expectedRadius)
        {
            CheckCircle(Read(wktObstacles), null, tolerance, x, y, expectedRadius);
        }


        private void CheckCircle(string wktObstacles, string wktBoundary, double tolerance,
            double x, double y, double expectedRadius)
        {
            CheckCircle(Read(wktObstacles), Read(wktBoundary), tolerance, x, y, expectedRadius);
        }

        private void CheckCircle(Geometry geomObstacles, Geometry geomBoundary, double tolerance,
            double x, double y, double expectedRadius)
        {
            var lec = new LargestEmptyCircle(geomObstacles, geomBoundary, tolerance);
            Geometry centerPoint = lec.GetCenter();
            var centerPt = centerPoint.Coordinate;
            var expectedCenter = new Coordinate(x, y);
            CheckEqualXY(expectedCenter, centerPt, 2 * tolerance);

            var radiusLine = lec.GetRadiusLine();
            double actualRadius = radiusLine.Length;
            Assert.AreEqual(expectedRadius, actualRadius, 2 * tolerance, "Radius: ");

            CheckEqualXY("Radius line center point: ", centerPt, radiusLine.GetCoordinateN(0));
            var radiusPt = lec.GetRadiusPoint().Coordinate;
            CheckEqualXY("Radius line endpoint point: ", radiusPt, radiusLine.GetCoordinateN(1));
        }

        private void CheckCircleZeroRadius(string wkt, double tolerance)
        {
            CheckCircleZeroRadius(Read(wkt), tolerance);
        }

        private void CheckCircleZeroRadius(Geometry geom, double tolerance)
        {
            var lec = new LargestEmptyCircle(geom, null, tolerance);

            var radiusLine = lec.GetRadiusLine();
            double actualRadius = radiusLine.Length;
            Assert.AreEqual(0.0, actualRadius, tolerance, "Radius: ");

            var centerPt = lec.GetCenter().Coordinate;
            CheckEqualXY("Radius line center point: ", centerPt, radiusLine.GetCoordinateN(0));
            var radiusPt = lec.GetRadiusPoint().Coordinate;
            CheckEqualXY("Radius line endpoint point: ", radiusPt, radiusLine.GetCoordinateN(1));
        }
    }
}
