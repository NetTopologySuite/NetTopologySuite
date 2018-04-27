using NUnit.Framework;

namespace NetTopologySuite.Samples.SimpleTests.Tests
{
    [TestFixture]
    public class GeometryServicesTest
    {
        [Test]
        public void TestCreateWithSameParametersReturnsSameInstance()
        {
            var s = GeoAPI.GeometryServiceProvider.Instance;

            var gf1 = s.CreateGeometryFactory();
            var gf2 = s.CreateGeometryFactory();
            Assert.IsTrue(ReferenceEquals(gf1, gf2));

            gf1 = s.CreateGeometryFactory(31466);
            gf2 = s.CreateGeometryFactory(31466);
            Assert.IsTrue(ReferenceEquals(gf1, gf2));

            var pm1 = s.CreatePrecisionModel(1000);
            gf1 = s.CreateGeometryFactory(pm1);
            gf2 = s.CreateGeometryFactory(pm1);
            Assert.IsTrue(ReferenceEquals(gf1, gf2));

            var pm2 = s.CreatePrecisionModel(1000);
            gf2 = s.CreateGeometryFactory(pm2);
            Assert.IsTrue(ReferenceEquals(gf1, gf2));

            gf1 = s.CreateGeometryFactory(pm1, 31466);
            gf2 = s.CreateGeometryFactory(pm2, 31466);
            Assert.IsTrue(ReferenceEquals(gf1, gf2));

            var csf = NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequenceFactory.Instance;
            gf1 = s.CreateGeometryFactory(csf);
            gf2 = s.CreateGeometryFactory(csf);
            Assert.IsTrue(ReferenceEquals(gf1, gf2));

            gf1 = s.CreateGeometryFactory(pm1, 31466, csf);
            gf2 = s.CreateGeometryFactory(pm2, 31466, csf);
            Assert.IsTrue(ReferenceEquals(gf1, gf2));
        }

        [Test]
        public void TestCreateWithDifferentParametersReturnsDifferentInstance()
        {
            var s = GeoAPI.GeometryServiceProvider.Instance;

            var gf1 = s.CreateGeometryFactory(31466);
            var gf2 = s.CreateGeometryFactory(31467);
            Assert.IsFalse(ReferenceEquals(gf1, gf2));

            var pm1 = s.CreatePrecisionModel(1000);
            var pm2 = s.CreatePrecisionModel(100);
            gf1 = s.CreateGeometryFactory(pm1);
            gf2 = s.CreateGeometryFactory(pm2);
            Assert.IsFalse(ReferenceEquals(gf1, gf2));

            gf1 = s.CreateGeometryFactory(NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequenceFactory.Instance);
            gf2 = s.CreateGeometryFactory(NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance);
            Assert.IsFalse(ReferenceEquals(gf1, gf2));
        }
    }
}