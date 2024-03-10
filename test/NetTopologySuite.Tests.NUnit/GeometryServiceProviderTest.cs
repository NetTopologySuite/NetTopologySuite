using System;
using System.Threading;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    [TestFixture]
    public class GeometryServiceProviderTest
    {
        [Test]
        public void TestInitialized()
        {
            var nts =
                new NtsGeometryServices(
                    NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequenceFactory.Instance,
                    new PrecisionModel(10d), 4326);

            Assert.That(nts.ElevationModel, Is.Null);

            var factory = nts.CreateGeometryFactory();

            Assert.IsNotNull(factory);
            Assert.AreEqual(nts.DefaultSRID, factory.SRID);
            Assert.AreEqual(nts.DefaultPrecisionModel, factory.PrecisionModel);
            Assert.AreEqual(nts.DefaultCoordinateSequenceFactory, factory.CoordinateSequenceFactory);
        }

        [Test]
        public void TestThreading()
        {
            try
            {
                int[] srids = new[] {4326, 31467, 3857, 27700};
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
                for (int i = 0; i < numWorkItems; i++)
                {
                    waitHandles[i] = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(TestFacories, new object[] {srids, precisionModels, waitHandles[i], i+1, false});
                }

                WaitHandle.WaitAll(waitHandles);
                TestContext.WriteLine("\nDone!");
                Assert.LessOrEqual(srids.Length * precisionModels.Length, ((NtsGeometryServices)NtsGeometryServices.Instance).NumFactories,
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
            object[] parameters = (object[]) info;
            int[] srids = (int[]) parameters[0];
            var precisionModels = (PrecisionModel[]) parameters[1];
            var wh = (AutoResetEvent) parameters[2];
            int workItemId = (int) parameters[3];
            bool verbose = (bool) parameters[4];
            var rnd = new Random();

            for (int i = 0; i < 1000; i++)
            {
                int srid = srids[rnd.Next(0, srids.Length)];
                var precisionModel = precisionModels[rnd.Next(0, precisionModels.Length)];

                var factory = NtsGeometryServices.Instance.CreateGeometryFactory(precisionModel, srid);
                if (verbose)
                {
                    TestContext.WriteLine("Thread_{0}: SRID: {1}; PM({2}, {3})", Thread.CurrentThread.ManagedThreadId,
                                      factory.SRID, factory.PrecisionModel.PrecisionModelType,
                                      factory.PrecisionModel.Scale);
                }
            }
            TestContext.WriteLine("Thread_{0} finished workitem {1}!", Thread.CurrentThread.ManagedThreadId, workItemId);
            wh.Set();
        }
    }
}
