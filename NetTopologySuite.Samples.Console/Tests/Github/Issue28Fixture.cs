using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue28Fixture
    {
        [Test]
        public void test_wkt_wkb_result()
        {
            TestWktWkb(GeometryFactory.Default, Issue27Fixture.Poly1Wkt, Issue27Fixture.Poly1Wkb);
            TestWktWkb(GeometryFactory.Default, Issue27Fixture.Poly2Wkt, Issue27Fixture.Poly2Wkb);
        }

        private static void TestWktWkb(IGeometryFactory factory, string wkt, string wkb)
        {
            WKTReader r = new WKTReader(factory);
            IGeometry wktGeom = r.Read(wkt);
            WKBReader s = new WKBReader(factory);
            IGeometry wkbGeom = s.Read(WKBReader.HexToBytes(wkb));
            Assert.IsTrue(wktGeom.EqualsExact(wkbGeom));
        }
    }
}