using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue180Fixture
    {
        /// <summary>
        /// Same results with JTS, see: https://github.com/NetTopologySuite/NetTopologySuite/issues/180
        /// </summary>
        [Test]
        public void expected_no_interception_point()
        {
            var factory = GeometryFactory.Default;
            var reader = new WKTReader(factory);
            var g1 = reader.Read(
                @"LINESTRING (500252.36136968032 3268279.9946693764, 500197.63371806522 3268255.4002767489)");
            Assert.IsNotNull(g1);
            Assert.IsTrue(g1.IsValid);
            var g2 = reader.Read(
                @"LINESTRING (499815.091 3269179.8250000011, 500224.99754436983 3268267.6974732862)");
            Assert.IsNotNull(g2);
            Assert.IsTrue(g2.IsValid);
            var ret = g1.Intersection(g2);
            Assert.IsNotNull(ret);
            Assert.IsInstanceOf<ILineString>(ret);
            Assert.AreEqual("LINESTRING EMPTY", ret.ToString());
        }
    }
}