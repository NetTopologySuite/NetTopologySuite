using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayArea;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayArea
{
    public class SimpleOverlayAreaTest : GeometryTestCase
    {
        [Test]
        public void TestDisjoint()
        {
            CheckIntersectionArea(
                "POLYGON ((10 90, 40 90, 40 60, 10 60, 10 90))",
                "POLYGON ((90 10, 50 10, 50 50, 90 50, 90 10))");
        }

        [Test, Ignore("fix this bug")]
        public void TestTouching()
        {
            CheckIntersectionArea(
                "POLYGON ((10 90, 50 90, 50 50, 10 50, 10 90))",
                "POLYGON ((90 10, 50 10, 50 50, 90 50, 90 10))");
        }

        [Test]
        public void TestRectangleAContainsB()
        {
            CheckIntersectionArea(
                "POLYGON ((100 300, 300 300, 300 100, 100 100, 100 300))",
                "POLYGON ((150 250, 250 250, 250 150, 150 150, 150 250))");
        }

        [Test]
        public void TestTriangleAContainsB()
        {
            CheckIntersectionArea(
                "POLYGON ((60 170, 270 370, 380 60, 60 170))",
                "POLYGON ((200 250, 245 155, 291 195, 200 250))");
        }

        [Test]
        public void TestRectangleOverlap()
        {
            CheckIntersectionArea(
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                "POLYGON ((250 250, 250 150, 150 150, 150 250, 250 250))");
        }

        [Test]
        public void TestRectangleTriangleOverlap()
        {
            CheckIntersectionArea(
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                "POLYGON ((300 200, 150 150, 300 100, 300 200))");
        }

        [Test]
        public void TestSawOverlap()
        {
            CheckIntersectionArea(
                "POLYGON ((100 300, 305 299, 150 200, 300 150, 150 100, 300 50, 100 50, 100 300))",
                "POLYGON ((400 350, 150 250, 350 200, 200 150, 350 100, 180 50, 400 50, 400 350))");
        }

        private void CheckIntersectionArea(string wktA, string wktB)
        {
            var a = (Polygon)Read(wktA);
            var b = (Polygon)Read(wktB);

            double ovIntArea = SimpleOverlayArea.IntersectionArea(a, b);

            double intAreaFull = a.Intersection(b).Area;

            //System.out.printf("OverlayArea: %f   Full overlay: %f\n", ovIntArea, intAreaFull);
            Assert.That(ovIntArea, Is.EqualTo(intAreaFull).Within(0.0001));
        }
    }
}
