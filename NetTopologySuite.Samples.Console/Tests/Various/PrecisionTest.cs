using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class Precision
    {
        private const string wktpol  = "POLYGON ((130 310, 390 310, 390 190, 130 190, 130 310))";
        private const string wktline = "LINESTRING (390 350, 390.01 150)";

        [Test]
        public void OverlayUsingDefaultPrecision()
        {
            IGeometryFactory factory = GeometryFactory.Default;
            WKTReader reader = new WKTReader(factory);

            IGeometry pol = reader.Read(wktpol);
            Assert.IsNotNull(pol);
            Assert.IsTrue(pol.IsValid);
            Assert.IsTrue(pol.Factory.PrecisionModel == factory.PrecisionModel);

            IGeometry line = reader.Read(wktline);
            Assert.IsNotNull(line);
            Assert.IsTrue(line.IsValid);
            Assert.IsTrue(line.Factory.PrecisionModel == factory.PrecisionModel);

            Assert.IsFalse(pol.Intersects(line));
        }

        [Test]
        public void OverlayUsingFixedPrecision()
        {
            IGeometryFactory factory = GeometryFactory.Fixed;
            WKTReader reader = new WKTReader(factory);

            IGeometry pol = reader.Read(wktpol);
            Assert.IsNotNull(pol);
            Assert.IsTrue(pol.IsValid);
            Assert.IsTrue(pol.Factory.PrecisionModel == factory.PrecisionModel);

            IGeometry line = reader.Read(wktline);
            Assert.IsNotNull(line);
            Assert.IsTrue(line.IsValid);
            Assert.IsTrue(line.Factory.PrecisionModel == factory.PrecisionModel);

            Assert.IsTrue(pol.Intersects(line));
        }
    }
}
