
using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Geometries;
using NUnit.Framework;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Coordinate;
#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class TriangleTest
    {

        private static IWktGeometryReader<coord> reader = TestFactories.GeometryFactory.WktReader;

        private const double Tolerance = 1E-5;

        [Test]
        public void TestArea3D()
        {
            checkArea3D("POLYGON((0 0 10, 100 0 110, 100 100 110, 0 0 10))", 7071.067811865475);
            checkArea3D("POLYGON((0 0 10, 100 0 10, 50 100 110, 0 0 10))", 7071.067811865475);
        }

        public void checkArea3D(String wkt, double expectedValue)
        {
            IGeometry<coord> g = reader.Read(wkt);
            ICoordinateSequence<coord> pt = g.Coordinates;
            double area3D = Triangle<coord>.Area3D(pt[0], pt[1], pt[2]);
            //		System.out.println("area3D = " + area3D);
            Assert.AreEqual(expectedValue, area3D, Tolerance);
        }

        [Test]
        public void TestArea()
        {
            // CW
            checkArea("POLYGON((10 10, 20 20, 20 10, 10 10))", 50);
            // CCW
            checkArea("POLYGON((10 10, 20 10, 20 20, 10 10))", -50);
            // degenerate point triangle
            checkArea("POLYGON((10 10, 10 10, 10 10, 10 10))", 0);
            // degenerate line triangle
            checkArea("POLYGON((10 10, 20 10, 15 10, 10 10))", 0);
        }

        public void checkArea(String wkt, double expectedValue)
        {
            IGeometry<coord> g = reader.Read(wkt);
            ICoordinateSequence<coord> pt = g.Coordinates;

            double signedArea = Triangle<coord>.SignedArea(pt[0], pt[1], pt[2]);
            Console.WriteLine("signed area = " + signedArea);
            Assert.AreEqual(expectedValue, signedArea, Tolerance);

            double area = Triangle<coord>.Area(pt[0], pt[1], pt[2]);
            Assert.AreEqual(Math.Abs(expectedValue), area, Tolerance);

        }
        [Test]
        public void TestAcute()
        {
            // right triangle
            checkAcute("POLYGON((10 10, 20 20, 20 10, 10 10))", false);
            // CCW right tri
            checkAcute("POLYGON((10 10, 20 10, 20 20, 10 10))", false);
            // acute
            checkAcute("POLYGON((10 10, 20 10, 15 20, 10 10))", true);
        }

        public void checkAcute(String wkt, Boolean expectedValue)
        {
            IGeometry<coord> g = reader.Read(wkt);
            ICoordinateSequence<coord> pt = g.Coordinates;

            Boolean isAcute = Triangle<coord>.IsAcute(pt[0], pt[1], pt[2]);
            Console.WriteLine("isAcute = " + isAcute);
            Assert.AreEqual(expectedValue, isAcute);
        }
    }
}
