using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class Precision
    {
        private const string wktpol  = "POLYGON ((130 310, 390 310, 390 190, 130 190, 130 310))";
        private const string wktline = "LINESTRING (390 350, 390.01 150)";
        [Test]
        public void OverlayUsingDefaultPrecision()
        {
            var factory = GeometryFactory.Default;
            var reader = new WKTReader(factory);
            var pol = reader.Read(wktpol);
            Assert.IsNotNull(pol);
            Assert.IsTrue(pol.IsValid);
            Assert.IsTrue(pol.Factory.PrecisionModel == factory.PrecisionModel);
            var line = reader.Read(wktline);
            Assert.IsNotNull(line);
            Assert.IsTrue(line.IsValid);
            Assert.IsTrue(line.Factory.PrecisionModel == factory.PrecisionModel);
            Assert.IsFalse(pol.Intersects(line));
        }
        [Test]
        public void OverlayUsingFixedPrecision()
        {
            var factory = GeometryFactory.Fixed;
            var reader = new WKTReader(factory);
            var pol = reader.Read(wktpol);
            Assert.IsNotNull(pol);
            Assert.IsTrue(pol.IsValid);
            Assert.IsTrue(pol.Factory.PrecisionModel == factory.PrecisionModel);
            var line = reader.Read(wktline);
            Assert.IsNotNull(line);
            Assert.IsTrue(line.IsValid);
            Assert.IsTrue(line.Factory.PrecisionModel == factory.PrecisionModel);
            Assert.IsTrue(pol.Intersects(line));
        }
    }
}
