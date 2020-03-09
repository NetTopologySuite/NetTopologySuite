#nullable disable
using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class TriangleTest
    {
        private static readonly PrecisionModel PrecisionModel = new PrecisionModel();
        private static readonly GeometryFactory GeometryFactory = new GeometryFactory(PrecisionModel, 0);
        private static readonly WKTReader Reader = new WKTReader(GeometryFactory);

        private const double Tolerance = 1E-5;

        [Test]
        public void TestInterpolateZ()
        {
            CheckInterpolateZ("LINESTRING(1 1 0, 2 1 0, 1 2 10)", new Coordinate(1.5, 1.5), 5);
            CheckInterpolateZ("LINESTRING(1 1 0, 2 1 0, 1 2 10)", new Coordinate(1.2, 1.2), 2);
            CheckInterpolateZ("LINESTRING(1 1 0, 2 1 0, 1 2 10)", new Coordinate(0, 0), -10);
        }

        private static void CheckInterpolateZ(string wkt, Coordinate p, double expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            var t = new Triangle(pt[0], pt[1], pt[2]);
            double z = t.InterpolateZ(p);
            //System.Console.WriteLine("Z = " + z);
            Assert.AreEqual(expectedValue, z, Tolerance);
        }

        [Test]
        public void TestArea3D()
        {
            CheckArea3D("POLYGON((0 0 10, 100 0 110, 100 100 110, 0 0 10))",
                        7071.067811865475);
            CheckArea3D("POLYGON((0 0 10, 100 0 10, 50 100 110, 0 0 10))",
                        7071.067811865475);
        }

        private static void CheckArea3D(string wkt, double expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;
            var t = new Triangle(pt[0], pt[1], pt[2]);
            double area3D = t.Area3D();
            //System.Console.WriteLine("area3D = " + area3D);
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

        private static void CheckArea(string wkt, double expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            var t = new Triangle(pt[0], pt[1], pt[2]);
            double signedArea = t.SignedArea();
            //System.Console.WriteLine("signed area = " + signedArea);
            Assert.AreEqual(expectedValue, signedArea, Tolerance);

            double area = t.Area();
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

        private static void CheckAcute(string wkt, bool expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            var t = new Triangle(pt[0], pt[1], pt[2]);
            bool isAcute = t.IsAcute();
            //System.Console.WriteLine("isAcute = " + isAcute);
            Assert.AreEqual(expectedValue, isAcute);
        }
    }
}