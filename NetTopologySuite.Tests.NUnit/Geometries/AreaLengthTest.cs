using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class AreaLengthTest
    {
        private PrecisionModel precisionModel;
        private GeometryFactory geometryFactory;
        WKTReader reader;

        public AreaLengthTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        private static double TOLERANCE = 1E-5;

        [Test]
        public void TestLength()
        {
            checkLength("MULTIPOINT (220 140, 180 280)", 0.0);
            checkLength("LINESTRING (220 140, 180 280)", 145.6021977);
            checkLength("LINESTRING (0 0, 100 100)", 141.4213562373095);
            checkLength("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20))", 80.0);
            checkLength("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20), (25 35, 35 35, 35 25, 25 25, 25 35))", 120.0);
        }

        [Test]
        public void TestArea()
        {
            checkArea("MULTIPOINT (220 140, 180 280)", 0.0);
            checkArea("LINESTRING (220 140, 180 280)", 0.0);
            checkArea("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20))", 400.0);
            checkArea("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20), (25 35, 35 35, 35 25, 25 25, 25 35))", 300.0);
        }

        public void checkLength(string wkt, double expectedValue)
        {
            var g = reader.Read(wkt);
            double len = g.Length;
            //System.Console.WriteLine(len);
            Assert.AreEqual(expectedValue, len, TOLERANCE);
        }

        public void checkArea(string wkt, double expectedValue)
        {
            var g = reader.Read(wkt);
            Assert.AreEqual(expectedValue, g.Area, TOLERANCE);
        }
    }
}
