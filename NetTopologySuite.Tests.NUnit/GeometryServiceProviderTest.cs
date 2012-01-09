using System;
using GeoAPI;
using GeoAPI.CoordinateSystems;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    [TestFixture]
    public class GeometryServiceProviderTest
    {
        [Test]
        public void TestUninitialized()
        {
            var nts = new NtsGeometryServices();
            var ntsFromGeoApi = GeometryServiceProvider.Instance;

            Assert.IsNotNull(ntsFromGeoApi);
            Assert.IsNotNull(ntsFromGeoApi.DefaultCoordinateSequenceFactory);
            Assert.IsNotNull(ntsFromGeoApi.DefaultPrecisionModel);

            Assert.IsTrue(nts.DefaultCoordinateSequenceFactory == ntsFromGeoApi.DefaultCoordinateSequenceFactory);
            Assert.IsTrue(nts.DefaultPrecisionModel.Equals(ntsFromGeoApi.DefaultPrecisionModel));
        }

        [Test]
        public void TestInitialized()
        {
            var nts =
                new NtsGeometryServices(
                    NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequenceFactory.Instance,
                    new PrecisionModel(10d), 4326);

            Assert.Throws<ArgumentNullException>(() => GeometryServiceProvider.Instance = null);

            GeometryServiceProvider.Instance = nts;
            var factory = nts.CreateGeometryFactory();

            Assert.IsNotNull(factory);
            Assert.AreEqual(nts.DefaultSRID, factory.SRID);
            Assert.AreEqual(nts.DefaultPrecisionModel, factory.PrecisionModel);
            Assert.AreEqual(nts.DefaultCoordinateSequenceFactory, factory.CoordinateSequenceFactory);

            // restore default!
            GeometryServiceProvider.Instance = new NtsGeometryServices();
        }

        #region ProjNet

        //ToDo: Move this to some other test
        [Test, Ignore]
        public void TestProjNetCoordinateSystemProvider()
        {
            //var csp = new ProjNetCoordinateSystemServices();
            //var ids = new List<int> { 4326, 31466, };

            //foreach (var id in ids)
            //{
            //    RunTestWkt(csp, id);
            //}
        }

        private static void RunTestWkt(ICoordinateSystemServices<ICoordinateSystem> csp, int srid)
        {
            var wkt = csp.GetCoordinateSystemInitializationString("EPSG", srid);
            wkt = wkt.Replace(",", ", ");
            Console.WriteLine(wkt);

            var cs = csp.GetCoordinateSytem(wkt);
            Assert.IsNotNull(cs);
            Console.WriteLine(cs.WKT);
        }

        #endregion ProjNet
    }
}