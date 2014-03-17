using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class MinimumDiameterTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public MinimumDiameterTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [TestAttribute]
        public void TestMinimumDiameter1() 
        {
            DoMinimumDiameterTest(true, "POINT (0 240)", new Coordinate(0, 240), new Coordinate(0, 240));
        }

        [TestAttribute]
        public void TestMinimumDiameter2() 
        {
            DoMinimumDiameterTest(true, "LINESTRING (0 240, 220 240)", new Coordinate(0, 240), new Coordinate(0, 240));
        }

        [TestAttribute]
        public void TestMinimumDiameter3()
        {
            DoMinimumDiameterTest(true, "POLYGON ((0 240, 220 240, 220 0, 0 0, 0 240))", new Coordinate(220, 240), new Coordinate(0, 240));
        }

        [TestAttribute]
        public void TestMinimumDiameter4() 
        {
            DoMinimumDiameterTest(true, "POLYGON ((0 240, 220 240, 220 0, 0 0, 0 240))", new Coordinate(220, 240), new Coordinate(0, 240));
        }

        [TestAttribute]
        public void TestMinimumDiameter5() 
        {
            DoMinimumDiameterTest(true, "POLYGON ((0 240, 160 140, 220 0, 0 0, 0 240))", new Coordinate(185.86206896551724, 79.65517241379311), new Coordinate(0, 0));
        }

        [TestAttribute]
        public void TestMinimumDiameter6() 
        {
            DoMinimumDiameterTest(false, "LINESTRING ( 39 119, 162 197, 135 70, 95 35, 33 66, 111 82, 97 131, 48 160, -4 182, 57 195, 94 202, 90 174, 75 134, 47 114, 0 100, 59 81, 123 60, 136 43, 163 75, 145 114, 93 136, 92 159, 105 175 )", new Coordinate(64.46262341325811, 196.41184767277855), new Coordinate(95, 35));
        }

        private void DoMinimumDiameterTest(bool convex, String wkt, Coordinate c0, Coordinate c1) 
        {
            Coordinate[] minimumDiameter = new MinimumDiameter(new WKTReader().Read(wkt), convex).Diameter.Coordinates;
            double tolerance = 1E-10;
            Assert.AreEqual(c0.X, minimumDiameter[0].X, tolerance);
            Assert.AreEqual(c0.Y, minimumDiameter[0].Y, tolerance);
            Assert.AreEqual(c1.X, minimumDiameter[1].X, tolerance);
            Assert.AreEqual(c1.Y, minimumDiameter[1].Y, tolerance);
        }
    }
}
