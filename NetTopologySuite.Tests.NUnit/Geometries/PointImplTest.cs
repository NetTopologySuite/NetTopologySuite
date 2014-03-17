using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class PointImplTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public PointImplTest()
        {
            precisionModel = new PrecisionModel(1000);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [TestAttribute]
        public void TestEquals1()
        {
            Point p1 = (Point)reader.Read("POINT(1.234 5.678)");
            Point p2 = (Point)reader.Read("POINT(1.234 5.678)");
            Assert.IsTrue(p1.Equals(p2));
        }

        [TestAttribute]
        public void TestEquals2()
        {
            Point p1 = (Point)reader.Read("POINT(1.23 5.67)");
            Point p2 = (Point)reader.Read("POINT(1.23 5.67)");
            Assert.IsTrue(p1.Equals(p2));
        }

        [TestAttribute]
        public void TestEquals3()
        {
            Point p1 = (Point)reader.Read("POINT(1.235 5.678)");
            Point p2 = (Point)reader.Read("POINT(1.234 5.678)");
            Assert.IsTrue(!p1.Equals(p2));
        }

        [TestAttribute]
        public void TestEquals4()
        {
            Point p1 = (Point)reader.Read("POINT(1.2334 5.678)");
            Point p2 = (Point)reader.Read("POINT(1.2333 5.678)");
            Assert.IsTrue(p1.Equals(p2));
        }

        [TestAttribute]
        public void TestEquals5()
        {
            Point p1 = (Point)reader.Read("POINT(1.2334 5.678)");
            Point p2 = (Point)reader.Read("POINT(1.2335 5.678)");
            Assert.IsTrue(!p1.Equals(p2));
        }

        [TestAttribute]
        public void TestEquals6()
        {
            Point p1 = (Point)reader.Read("POINT(1.2324 5.678)");
            Point p2 = (Point)reader.Read("POINT(1.2325 5.678)");
            Assert.IsTrue(!p1.Equals(p2));
        }

        [TestAttribute]
        public void TestNegRounding1()
        {
            Point pLo = (Point)reader.Read("POINT(-1.233 5.678)");
            Point pHi = (Point)reader.Read("POINT(-1.232 5.678)");

            Point p1 = (Point)reader.Read("POINT(-1.2326 5.678)");
            Point p2 = (Point)reader.Read("POINT(-1.2325 5.678)");
            Point p3 = (Point)reader.Read("POINT(-1.2324 5.678)");

            Assert.IsTrue(!p1.Equals(p2));
            Assert.IsTrue(p3.Equals(p2));

            Assert.IsTrue(p1.Equals(pLo));
            Assert.IsTrue(p2.Equals(pHi));
            Assert.IsTrue(p3.Equals(pHi));
        }

        [TestAttribute]
        public void TestIsSimple()
        {
            Point p1 = (Point)reader.Read("POINT(1.2324 5.678)");
            Assert.IsTrue(p1.IsSimple);
            Point p2 = (Point)reader.Read("POINT EMPTY");
            Assert.IsTrue(p2.IsSimple);
        }
    }
}
