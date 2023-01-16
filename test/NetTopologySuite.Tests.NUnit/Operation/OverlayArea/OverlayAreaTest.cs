using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Operation.OverlayArea;
using NetTopologySuite.Tests.NUnit.Operation.OverlayArea;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayArea
{
    public class OverlayAreaTest : GeometryTestCase
    {


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

        [Test]
        public void TestAOverlapBWithHole()
        {
            CheckIntersectionArea(
                "POLYGON ((100 300, 305 299, 150 200, 300 150, 150 100, 300 50, 100 50, 100 300))",
                "POLYGON ((185 206, 350 206, 350 100, 185 100, 185 206), (230 190, 310 190, 310 120, 230 120, 230 190))");
        }

        [Test]
        public void TestAOverlapBMulti()
        {
            CheckIntersectionArea(
                "POLYGON ((50 250, 250 250, 250 50, 50 50, 50 250))",
                "MULTIPOLYGON (((100 200, 100 100, 0 100, 0 200, 100 200)), ((200 200, 300 200, 300 100, 200 100, 200 200)))");
        }

        [Test]
        public void TestAOverlapBMultiHole()
        {
            CheckIntersectionArea(
                "POLYGON ((60 200, 250 280, 111 135, 320 120, 50 40, 30 120, 60 200))",
                "MULTIPOLYGON (((55 266, 150 150, 170 290, 55 266)), ((100 0, 70 130, 260 160, 291 45, 100 0), (150 40, 125 98, 220 110, 150 40)))");
        }

        private void CheckIntersectionArea(string wktA, string wktB)
        {
            var a = Read(wktA);
            var b = Read(wktB);

            var ova = new NetTopologySuite.Operation.OverlayArea.OverlayArea(a);
            double ovIntArea = ova.IntersectionArea(b);

            double intAreaFull = a.Intersection(b).Area;

            //System.out.printf("OverlayArea: %f   Full overlay: %f\n", ovIntArea, intAreaFull);
            Assert.That(ovIntArea, Is.EqualTo(intAreaFull).Within(0.0001));
        }
    }
}
