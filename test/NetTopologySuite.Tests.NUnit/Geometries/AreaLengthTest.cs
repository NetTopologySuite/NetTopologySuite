using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class AreaLengthTest
    {
        private readonly WKTReader _reader;

        public AreaLengthTest()
        {
            var gs = new NtsGeometryServices(PrecisionModel.Fixed.Value, 0);
            _reader = new WKTReader(gs);
        }

        private static double TOLERANCE = 1E-5;

        [Test]
        public void TestLength()
        {
            CheckLength("MULTIPOINT (220 140, 180 280)", 0.0);
            CheckLength("LINESTRING (220 140, 180 280)", 145.6021977);
            CheckLength("LINESTRING (0 0, 100 100)", 141.4213562373095);
            CheckLength("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20))", 80.0);
            CheckLength("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20), (25 35, 35 35, 35 25, 25 25, 25 35))", 120.0);
        }

        [Test]
        public void TestArea()
        {
            CheckArea("MULTIPOINT (220 140, 180 280)", 0.0);
            CheckArea("LINESTRING (220 140, 180 280)", 0.0);
            CheckArea("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20))", 400.0);
            CheckArea("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20), (25 35, 35 35, 35 25, 25 25, 25 35))", 300.0);
        }

        public void CheckLength(string wkt, double expectedValue)
        {
            var g = _reader.Read(wkt);
            double len = g.Length;
            //TestContext.WriteLine(len);
            Assert.AreEqual(expectedValue, len, TOLERANCE);
        }

        public void CheckArea(string wkt, double expectedValue)
        {
            var g = _reader.Read(wkt);
            Assert.AreEqual(expectedValue, g.Area, TOLERANCE);
        }
    }
}
