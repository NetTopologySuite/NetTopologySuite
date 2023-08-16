using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Construct
{
    public class MaximumInscibedCircleTest : GeometryTestCase
    {
        [Test]
        public void TestSquare()
        {
            CheckCircle("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                0.001, 150, 150, 50);
        }

        [Test]
        public void TestDiamond()
        {
            CheckCircle("POLYGON ((150 250, 50 150, 150 50, 250 150, 150 250))",
                0.001, 150, 150, 70.71);
        }

        [Test]
        public void TestCircle()
        {
            var centre = Read("POINT (100 100)");
            var circle = centre.Buffer(100, 20);
            // MIC radius is less than 100 because buffer boundary segments lie inside circle
            CheckCircle(circle, 0.01, 100, 100, 99.92);
        }

        [Test]
        public void TestKite()
        {
            CheckCircle("POLYGON ((100 0, 200 200, 300 200, 300 100, 100 0))",
                0.01, 238.19, 138.19, 61.80);
        }

        [Test]
        public void TestKiteWithHole()
        {
            CheckCircle("POLYGON ((100 0, 200 200, 300 200, 300 100, 100 0), (200 150, 200 100, 260 100, 200 150))",
                0.01, 257.47, 157.47, 42.52);
        }

        [Test]
        public void TestDoubleKite()
        {
            CheckCircle(
                "MULTIPOLYGON (((150 200, 100 150, 150 100, 250 150, 150 200)), ((400 250, 300 150, 400 50, 560 150, 400 250)))",
                0.01, 411.38, 149.99, 78.75);
        }

        [Test, Description("Invalid polygon collapsed to a line")]
        public void TestCollapsedLine()
        {
            CheckCircle("POLYGON ((100 100, 200 200, 100 100, 100 100))",
                0.01);
        }

        [Test, Description("Invalid polygon collapsed to a flat line (originally caused infinite loop)")]
        public void TestCollapsedLineFlat()
        {
            CheckCircle("POLYGON((1 2, 1 2, 1 2, 1 2, 3 2, 1 2))",
                0.01);
        }

        [Test, Description("Invalid polygon collapsed to a point")]
        public void TestCollapsedPoint()
        {
            CheckCircle("POLYGON ((100 100, 100 100, 100 100, 100 100))",
                0.01, 100, 100, 0);
        }

        /**
         * Tests that a nearly flat geometry doesn't make the initial cell grid huge.
         * 
         * See https://github.com/libgeos/geos/issues/875
         */
        [Test]
        public void TestNearlyFlat()
        {
            CheckCircle("POLYGON ((59.3 100.00000000000001, 99.7 100.00000000000001, 99.7 100, 59.3 100, 59.3 100.00000000000001))",
               0.01);
        }

        [Test]
        public void TestVeryThin()
        {
            CheckCircle("POLYGON ((100 100, 200 300, 300 100, 450 250, 300 99.999999, 200 299.99999, 100 100))",
               0.01);
        }

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
            var mic = new MaximumInscribedCircle(geom, tolerance);
            Geometry centerPoint = mic.GetCenter();
            double dist = geom.Boundary.Distance(centerPoint);
            Assert.That(dist < 2 * tolerance);
        }



        private void CheckCircle(string wkt, double tolerance,
            double x, double y, double expectedRadius)
        {
            CheckCircle(Read(wkt), tolerance, x, y, expectedRadius);
        }

        private void CheckCircle(Geometry geom, double tolerance,
            double x, double y, double expectedRadius)
        {
            var mic = new MaximumInscribedCircle(geom, tolerance);
            var centerPoint = mic.GetCenter();
            var centerPt = centerPoint.Coordinate;
            var expectedCenter = new Coordinate(x, y);
            CheckEqualXY(expectedCenter, centerPt, tolerance);

            var radiusLine = mic.GetRadiusLine();
            double actualRadius = radiusLine.Length;
            Assert.AreEqual(expectedRadius, actualRadius, tolerance, "Radius: ");

            CheckEqualXY("Radius line center point: ", centerPt, radiusLine.GetCoordinateN(0));
            var radiusPt = mic.GetRadiusPoint().Coordinate;
            CheckEqualXY("Radius line endpoint point: ", radiusPt, radiusLine.GetCoordinateN(1));

        }
    }
}
