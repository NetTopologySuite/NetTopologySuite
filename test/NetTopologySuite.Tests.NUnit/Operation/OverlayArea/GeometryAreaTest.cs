using NetTopologySuite.Operation.OverlayArea;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayArea
{
    public class GeometryAreaTest : GeometryTestCase
    {

        [Test]
        public void TestRectangle()
        {
            CheckArea(
                "POLYGON ((100 300, 300 300, 300 100, 100 100, 100 300))");
        }

        [Test]
        public void TestPolygon()
        {
            CheckArea(
                "POLYGON ((40 110, 97 295, 176 184, 240 300, 440 100, 244 164, 194 74, 110 30, 130 140, 40 110))");
        }

        [Test]
        public void TestPolygonWithHoles()
        {
            CheckArea(
                "POLYGON ((40 110, 97 295, 176 184, 240 300, 440 100, 244 164, 194 74, 110 30, 130 140, 40 110), (230 230, 280 230, 280 190, 230 190, 230 230), (100 220, 90 160, 130 190, 100 220))");
        }

        [Test]
        public void TestMultiPolygonWithHoles()
        {
            CheckArea(
                "MULTIPOLYGON (((40 110, 97 295, 176 184, 240 300, 440 100, 244 164, 194 74, 110 30, 130 140, 40 110), (230 230, 280 230, 280 190, 230 190, 230 230), (100 220, 90 160, 130 190, 100 220)), ((120 350, 170 280, 223 355, 370 280, 415 399, 150 430, 120 350)))");
        }

        [Test]
        public void TestLineString()
        {
            CheckArea(
                "LINESTRING (120 120, 290 140, 130 240, 280 320)");
        }

        private void CheckArea(string wkt)
        {
            var geom = Read(wkt);

            double ovArea = GeometryArea.Compute(geom);
            double area = geom.Area;

            Assert.That(ovArea, Is.EqualTo(area).Within(0.00001));
        }
    }
}
