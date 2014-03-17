using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    //[Ignore("The Minimum Bounding Circle logic does not look to have been included in NTS as yet")]
    public class MinimumBoundingCircleTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public MinimumBoundingCircleTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [TestAttribute]
        public void TestEmptyPoint()
        {
            DoMinimumBoundingCircleTest("POINT EMPTY", "MULTIPOINT EMPTY");
        }

        [TestAttribute]
        public void TestPoint()
        {
            DoMinimumBoundingCircleTest("POINT (10 10)", "MULTIPOINT ((10 10))", new Coordinate(10, 10), 0);
        }

        [TestAttribute]
        public void TestPoints2()
        {
            DoMinimumBoundingCircleTest("MULTIPOINT ((10 10), (20 20))", "MULTIPOINT ((10 10), (20 20))", new Coordinate(15, 15), 7.0710678118654755);
        }

        [TestAttribute]
        public void TestPointsInLine()
        {
            DoMinimumBoundingCircleTest("MULTIPOINT ((10 10), (20 20), (30 30))", "MULTIPOINT ((10 10), (30 30))",
            new Coordinate(20, 20), 14.142135623730951);
        }

        [TestAttribute]
        public void TestPoints3()
        {
            DoMinimumBoundingCircleTest("MULTIPOINT ((10 10), (20 20), (10 20))", "MULTIPOINT ((10 10), (20 20), (10 20))",
            new Coordinate(15, 15), 7.0710678118654755);
        }

        [TestAttribute]
        public void TestObtuseTriangle() 
        {
            DoMinimumBoundingCircleTest("POLYGON ((100 100, 200 100, 150 90, 100 100))", "MULTIPOINT ((100 100), (200 100))",
                new Coordinate(150, 100), 50);
        }

        [TestAttribute]
        public void TestTriangleWithMiddlePoint()
        {
            DoMinimumBoundingCircleTest("MULTIPOINT ((10 10), (20 20), (10 20), (15 19))", "MULTIPOINT ((10 10), (20 20), (10 20))",
                new Coordinate(15, 15), 7.0710678118654755);
        }

        static double TOLERANCE = 1.0e-5;

        private void DoMinimumBoundingCircleTest(String wkt, String expectedWKT)
        {
            DoMinimumBoundingCircleTest(wkt, expectedWKT, null, -1);
        }

        private void DoMinimumBoundingCircleTest(String wkt, String expectedWKT, Coordinate expectedCentre, double expectedRadius)
        {
            MinimumBoundingCircle mbc = new MinimumBoundingCircle(reader.Read(wkt));
            Coordinate[] exPts = mbc.GetExtremalPoints();
            IGeometry actual = geometryFactory.CreateMultiPoint(exPts);
            double actualRadius = mbc.GetRadius();
            Coordinate actualCentre = mbc.GetCentre();
            Console.WriteLine("   Centre = " + actualCentre + "   Radius = " + actualRadius);

            IGeometry expected = reader.Read(expectedWKT);
            bool isEqual = actual.Equals(expected);
            // need this hack because apparently equals does not work for MULTIPOINT EMPTY
            if (actual.IsEmpty && expected.IsEmpty)
                isEqual = true;
  	        if (!isEqual)
  	        {
  	            Console.WriteLine("Actual = " + actual + ", Expected = " + expected);
  	        }
            Assert.IsTrue(isEqual);

            if (expectedCentre != null)
            {
                Assert.IsTrue(expectedCentre.Distance(actualCentre) < TOLERANCE);
            }
            if (expectedRadius >= 0)
            {
                Assert.IsTrue(Math.Abs(expectedRadius - actualRadius) < TOLERANCE);
            }
        }
    }
}
