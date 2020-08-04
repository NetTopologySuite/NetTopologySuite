using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;
using NUnit.Framework;

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
                "MULTILINESTRING ((100 100, 200 150, 100 200, 250 250, 100 300, 300 350, 100 400), (50 400, 0 350, 50 300, 0 250, 50 200, 0 150, 50 100))",
                0.01, 77.52, 349.99, 54.81);
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


        private void CheckCircle(string wkt, double tolerance,
            double x, double y, double expectedRadius)
        {
            CheckCircle(Read(wkt), tolerance, x, y, expectedRadius);
        }

        private void CheckCircle(Geometry geom, double tolerance,
            double x, double y, double expectedRadius)
        {
            var lec = new LargestEmptyCircle(geom, tolerance);
            Geometry centerPoint = lec.GetCenter();
            var centerPt = centerPoint.Coordinate;
            var expectedCenter = new Coordinate(x, y);
            CheckEqualXY(expectedCenter, centerPt, tolerance);

            var radiusLine = lec.GetRadiusLine();
            double actualRadius = radiusLine.Length;
            Assert.AreEqual(expectedRadius, actualRadius, tolerance, "Radius: ");

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
            var lec = new LargestEmptyCircle(geom, tolerance);

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
