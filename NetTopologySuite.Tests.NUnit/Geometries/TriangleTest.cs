using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class TriangleTest
    {
        private static readonly PrecisionModel PrecisionModel = new PrecisionModel();
        private static readonly IGeometryFactory GeometryFactory = new GeometryFactory(PrecisionModel, 0);
        private static readonly WKTReader Reader = new WKTReader(GeometryFactory);

        private const double Tolerance = 1E-5;

        [Test]
        public void TestArea3D()
        {
            CheckArea3D("POLYGON((0 0 10, 100 0 110, 100 100 110, 0 0 10))", 7071.067811865475);
            CheckArea3D("POLYGON((0 0 10, 100 0 10, 50 100 110, 0 0 10))", 7071.067811865475);
        }

        public void CheckArea3D(String wkt, double expectedValue)
        {
            IGeometry g = Reader.Read(wkt);
            Coordinate[] pt = g.Coordinates;
            double area3D = Triangle.Area3D((Coordinate)pt[0], (Coordinate)pt[1], (Coordinate)pt[2]);
            //		System.out.println("area3D = " + area3D);
            Assert.AreEqual(expectedValue, area3D, Tolerance);
        }

        [Test]
        public void TestArea()
        {
            // CW
            CheckArea("POLYGON((10 10, 20 20, 20 10, 10 10))", 50);
            // CCW
            CheckArea("POLYGON((10 10, 20 10, 20 20, 10 10))", -50);
            // degenerate point triangle
            CheckArea("POLYGON((10 10, 10 10, 10 10, 10 10))", 0);
            // degenerate line triangle
            CheckArea("POLYGON((10 10, 20 10, 15 10, 10 10))", 0);
        }

        public static void CheckArea(String wkt, double expectedValue)
        {
            IGeometry g = Reader.Read(wkt);
            Coordinate[] pt = g.Coordinates;

            /*
            double signedArea = Triangle.SignedArea(pt[0], pt[1], pt[2]);
            Console.WriteLine("signed area = " + signedArea);
            Assert.AreEqual(expectedValue, signedArea, Tolerance);
            */
            double area = Triangle.Area(pt[0], pt[1], pt[2]);
            Assert.AreEqual(Math.Abs(expectedValue), area, Tolerance);
        }

        [Test]
        public void TestAcute()
        {
            // right triangle
            CheckAcute("POLYGON((10 10, 20 20, 20 10, 10 10))", false);
            // CCW right tri
            CheckAcute("POLYGON((10 10, 20 10, 20 20, 10 10))", false);
            // acute
            CheckAcute("POLYGON((10 10, 20 10, 15 20, 10 10))", true);
        }

        private static void CheckAcute(String wkt, Boolean expectedValue)
        {
            IGeometry g = Reader.Read(wkt);
            Coordinate[] pt = g.Coordinates;

            Boolean isAcute = Triangle.IsAcute(pt[0], pt[1], pt[2]);
            Console.WriteLine("isAcute = " + isAcute);
            Assert.AreEqual(expectedValue, isAcute);
        }
    }
}