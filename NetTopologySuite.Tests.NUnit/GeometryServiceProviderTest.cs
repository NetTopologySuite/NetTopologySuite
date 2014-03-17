using System;
using System.Collections.Generic;
using System.Threading;
using GeoAPI;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    [TestFixtureAttribute]
    public class GeometryServiceProviderTest
    {
        [TestAttribute]
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

        [TestAttribute]
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

        [TestAttribute]
        public void TestThreading()
        {
            try
            {
                var srids = new[] {4326, 31467, 3857, 27700};
                var precisionModels = new[]
                    {
                        new PrecisionModel(PrecisionModels.Floating), 
                        new PrecisionModel(PrecisionModels.FloatingSingle),
                        new PrecisionModel(1),
                        new PrecisionModel(10),
                        new PrecisionModel(100),
                    };

                const int numWorkItems = 30;
                var waitHandles = new WaitHandle[numWorkItems];
                for (var i = 0; i < numWorkItems; i++)
                {
                    waitHandles[i] = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(TestFacories, new object[] {srids, precisionModels, waitHandles[i], i+1, false});
                }

                WaitHandle.WaitAll(waitHandles);
                Console.WriteLine("\nDone!");
                Assert.LessOrEqual(srids.Length * precisionModels.Length, ((NtsGeometryServices)GeometryServiceProvider.Instance).NumFactories,
                    "Too many factories created!");
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.IsTrue(false);
            }

        }

        private static void TestFacories(object info)
        {
            var parameters = (object[]) info;
            var srids = (int[]) parameters[0];
            var precisionModels = (IPrecisionModel[]) parameters[1];
            var wh = (AutoResetEvent) parameters[2];
            var workItemId = (int) parameters[3];
            var verbose = (bool) parameters[4];
            var rnd = new Random();

            for (var i = 0; i < 1000; i++)
            {
                var srid = srids[rnd.Next(0, srids.Length)];
                var precisionModel = precisionModels[rnd.Next(0, precisionModels.Length)];

                var factory = GeometryServiceProvider.Instance.CreateGeometryFactory(precisionModel, srid);
                if (verbose)
                {
                    Console.WriteLine("Thread_{0}: SRID: {1}; PM({2}, {3})", Thread.CurrentThread.ManagedThreadId,
                                      factory.SRID, factory.PrecisionModel.PrecisionModelType,
                                      factory.PrecisionModel.Scale);
                }
            }
            Console.WriteLine("Thread_{0} finished workitem {1}!", Thread.CurrentThread.ManagedThreadId, workItemId);
            wh.Set();
        }

        #region ProjNet

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