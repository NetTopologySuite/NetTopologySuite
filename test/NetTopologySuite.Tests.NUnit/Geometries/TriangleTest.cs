using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class TriangleTest
    {
        private static readonly WKTReader Reader = new WKTReader(new NtsGeometryServices(PrecisionModel.Floating.Value, 0));

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
            //TestContext.WriteLine("Z = " + z);
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
            //TestContext.WriteLine("area3D = " + area3D);
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
            //TestContext.WriteLine("signed area = " + signedArea);
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
            //TestContext.WriteLine("isAcute = " + isAcute);
            Assert.AreEqual(expectedValue, isAcute);
        }

        [Test]
        public void TestCircumCentre()
        {
            // right triangle
            CheckCircumCentre("POLYGON((10 10, 20 20, 20 10, 10 10))", new Coordinate(
        15.0, 15.0));
            // CCW right tri
            CheckCircumCentre("POLYGON((10 10, 20 10, 20 20, 10 10))", new Coordinate(
                15.0, 15.0));
            // acute
            CheckCircumCentre("POLYGON((10 10, 20 10, 15 20, 10 10))", new Coordinate(
                15.0, 13.75));
        }

        [Test]
        public void TestCircumradius()
        {
            // right triangle
            CheckCircumradius("POLYGON((10 10, 20 20, 20 10, 10 10))");
            // CCW right tri
            CheckCircumradius("POLYGON((10 10, 20 10, 20 20, 10 10))");
            // acute
            CheckCircumradius("POLYGON((10 10, 20 10, 15 20, 10 10))");
        }

        [Test]
        public void TestCentroid()
        {
            // right triangle
            CheckCentroid("POLYGON((10 10, 20 20, 20 10, 10 10))", new Coordinate(
                (10.0 + 20.0 + 20.0) / 3.0, (10.0 + 20.0 + 10.0) / 3.0));
            // CCW right tri
            CheckCentroid("POLYGON((10 10, 20 10, 20 20, 10 10))", new Coordinate(
                (10.0 + 20.0 + 20.0) / 3.0, (10.0 + 10.0 + 20.0) / 3.0));
            // acute
            CheckCentroid("POLYGON((10 10, 20 10, 15 20, 10 10))", new Coordinate(
                (10.0 + 20.0 + 15.0) / 3.0, (10.0 + 10.0 + 20.0) / 3.0));
        }

        public void CheckCentroid(string wkt, Coordinate expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            var centroid = Triangle.Centroid(pt[0], pt[1], pt[2]);
            //System.out.println("(Static) centroid = " + centroid);
            Assert.That(expectedValue.ToString(), Is.EqualTo(centroid.ToString()));

            // Test Instance version
            //
            var t = new Triangle(pt[0], pt[1], pt[2]);
            centroid = t.Centroid();
            //System.out.println("(Instance) centroid = " + centroid.toString());
            Assert.That(expectedValue.ToString(), Is.EqualTo(centroid.ToString()));
        }

        public static void CheckCircumCentre(string wkt, Coordinate expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            var circumcentre = Triangle.Circumcentre(pt[0], pt[1], pt[2]);
            //System.out.println("(Static) circumcentre = " + circumcentre);
            Assert.That(expectedValue.ToString(), Is.EqualTo(circumcentre.ToString()));

            // Test Instance version
            //
            var t = new Triangle(pt[0], pt[1], pt[2]);
            circumcentre = t.Circumcentre();
            //System.out.println("(Instance) circumcentre = " + circumcentre.toString());
            Assert.That(expectedValue.ToString(), Is.EqualTo(circumcentre.ToString()));
        }

        public static void CheckCircumradius(string wkt)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            var circumcentre = Triangle.Circumcentre(pt[0], pt[1], pt[2]);
            double circumradius = Triangle.Circumradius(pt[0], pt[1], pt[2]);
            //System.out.println("(Static) circumcentre = " + circumcentre);
            double rad0 = pt[0].Distance(circumcentre);
            double rad1 = pt[1].Distance(circumcentre);
            double rad2 = pt[2].Distance(circumcentre);
            Assert.That(rad0, Is.EqualTo(circumradius).Within(0.00001));
            Assert.That(rad1, Is.EqualTo(circumradius).Within(0.00001));
            Assert.That(rad2, Is.EqualTo(circumradius).Within(0.00001));
        }

        [Test]
        public void TestLongestSideLength()
        {
            // right triangle
            CheckLongestSideLength("POLYGON((10 10 1, 20 20 2, 20 10 3, 10 10 1))",
                14.142135623730951);
            // CCW right tri
            CheckLongestSideLength("POLYGON((10 10 1, 20 10 2, 20 20 3, 10 10 1))",
                14.142135623730951);
            // acute
            CheckLongestSideLength("POLYGON((10 10 1, 20 10 2, 15 20 3, 10 10 1))",
                11.180339887498949);
        }

        public static void CheckLongestSideLength(string wkt, double expectedValue)
        {
            var g = Reader.Read(wkt);
            var pt = g.Coordinates;

            double length = Triangle.LongestSideLength(pt[0], pt[1], pt[2]);
            //System.out.println("(Static) longestSideLength = " + length);
            Assert.That(expectedValue, Is.EqualTo(length).Within(0.00000001));

            // Test Instance version
            //
            var t = new Triangle(pt[0], pt[1], pt[2]);
            length = t.LongestSideLength();
            //System.out.println("(Instance) longestSideLength = " + length);
            Assert.That(expectedValue, Is.EqualTo(length).Within(0.00000001));
        }

        //===============================================================

        [Test]
        public void TestIsCCW()
        {
            CheckIsCCW("POLYGON ((30 90, 80 50, 20 20, 30 90))", false);
            CheckIsCCW("POLYGON ((90 90, 20 40, 10 10, 90 90))", true);
        }

        public static void CheckIsCCW(string wkt, bool expectedValue)
        {
            var pt = Reader.Read(wkt).Coordinates;
            bool actual = Triangle.IsCCW(pt[0], pt[1], pt[2]);
            Assert.That(expectedValue, Is.EqualTo(actual));
        }

        //===============================================================

        [Test]
        public void TestIntersects()
        {
            CheckIntersects("POLYGON ((30 90, 80 50, 20 20, 30 90))", "POINT (70 20)", false);
            // triangle vertex
            CheckIntersects("POLYGON ((30 90, 80 50, 20 20, 30 90))", "POINT (30 90)", true);
            CheckIntersects("POLYGON ((30 90, 80 50, 20 20, 30 90))", "POINT (40 40)", true);

            // on an edge
            CheckIntersects("POLYGON ((30 90, 70 50, 71.5 16.5, 30 90))", "POINT (50 70)", true);
        }

        public static void CheckIntersects(string wktTri, string wktPt, bool expectedValue)
        {
            var tri = Reader.Read(wktTri).Coordinates;
            var pt = Reader.Read(wktPt).Coordinate;

            bool actual = Triangle.Intersects(tri[0], tri[1], tri[2], pt);
            Assert.That(expectedValue, Is.EqualTo(actual));
        }

    }
}
