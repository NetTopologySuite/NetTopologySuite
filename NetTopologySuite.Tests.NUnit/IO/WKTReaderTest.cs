using System;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Test for <see cref="WKTReader" />
    /// </summary>
    [TestFixture]
    public class WKTReaderTest
    {
        WKTWriter writer = new WKTWriter();
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public WKTReaderTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [Test]
        public void TestReadNaN()
        {
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10 NaN)")));
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10 nan)")));
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10 NAN)")));
        }

        [Test]
        public void TestReadPoint()
        {
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10)")));
            Assert.AreEqual("POINT EMPTY", writer.Write(reader.Read("POINT EMPTY")));
        }

        [Test]
        public void TestReadLineString()
        {
            Assert.AreEqual("LINESTRING (10 10, 20 20, 30 40)", writer.Write(reader.Read("LINESTRING (10 10, 20 20, 30 40)")));
            Assert.AreEqual("LINESTRING EMPTY", writer.Write(reader.Read("LINESTRING EMPTY")));
        }

        [Test]
        public void TestReadLinearRing()
        {
            try
            {
                reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 99)");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.IndexOf("must form a closed linestring") > -1);
            }
            Assert.AreEqual("LINEARRING (10 10, 20 20, 30 40, 10 10)", writer.Write(reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 10)")));
            Assert.AreEqual("LINEARRING EMPTY", writer.Write(reader.Read("LINEARRING EMPTY")));
        }

        [Test]
        public void TestReadPolygon()
        {
            Assert.AreEqual("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))", writer.Write(reader.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))")));
            Assert.AreEqual("POLYGON EMPTY", writer.Write(reader.Read("POLYGON EMPTY")));
        }

        [Test]
        public void TestReadMultiPoint()
        {
            Assert.AreEqual("MULTIPOINT ((10 10), (20 20))", writer.Write(reader.Read("MULTIPOINT ((10 10), (20 20))")));
            Assert.AreEqual("MULTIPOINT EMPTY", writer.Write(reader.Read("MULTIPOINT EMPTY")));
        }

        [Test]
        public void TestReadMultiLineString()
        {
            Assert.AreEqual("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))", writer.Write(reader.Read("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))")));
            Assert.AreEqual("MULTILINESTRING EMPTY", writer.Write(reader.Read("MULTILINESTRING EMPTY")));
        }

        [Test]
        public void TestReadMultiPolygon()
        {
            Assert.AreEqual("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))", writer.Write(reader.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))")));
            Assert.AreEqual("MULTIPOLYGON EMPTY", writer.Write(reader.Read("MULTIPOLYGON EMPTY")));
        }

        [Test]
        public void TestReadGeometryCollection()
        {
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))", writer.Write(reader.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))", writer.Write(reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))", writer.Write(reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION EMPTY", writer.Write(reader.Read("GEOMETRYCOLLECTION EMPTY")));
        }

        [Test]
        public void TestReadZ()
        {
            Assert.AreEqual(new Coordinate(1, 2, 3), reader.Read("POINT (1 2 3)").Coordinate);
        }

        [Test]
        public void TestReadLargeNumbers()
        {
            PrecisionModel precisionModel = new PrecisionModel(1E9);
            GeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
            WKTReader reader = new WKTReader(geometryFactory);
            IGeometry point1 = reader.Read("POINT (123456789.01234567890 10)");
            IPoint point2 = geometryFactory.CreatePoint(new Coordinate(123456789.01234567890, 10));
            Assert.AreEqual(point1.Coordinate.X, point2.Coordinate.X, 1E-7);
            Assert.AreEqual(point1.Coordinate.Y, point2.Coordinate.Y, 1E-7);
        }

        [Test]
        public void TestThreading()
        {
            var wkts = new[]
                {
                    "POINT ( 10 20 )",
                    "LINESTRING EMPTY",
                    "LINESTRING(0 0, 10 10)",
                    "MULTILINESTRING ((50 100, 100 200), (100 100, 150 200))",
                    "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                    "MULTIPOLYGON (((100 200, 200 200, 200 100, 100 100, 100 200)), ((300 200, 400 200, 400 100, 300 100, 300 200)))",
                    "GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), LINESTRING (250 100, 350 200), POINT (350 150))"
                    ,
                };

            var srids = new[] {4326, 31467, 3857};
            
            const int numJobs = 30;
            var waitHandles = new WaitHandle[numJobs];
            for (var i = 0; i < numJobs; i++)
            {
                waitHandles[i] = new AutoResetEvent(false);
                ThreadPool.QueueUserWorkItem(TestReaderInThreadedContext, new object[] {wkts, waitHandles[i], srids, i});
            }

            WaitHandle.WaitAll(waitHandles);
            Assert.AreEqual(1 + srids.Length, ((NtsGeometryServices)GeoAPI.GeometryServiceProvider.Instance).NumFactories); 
        }

        private static readonly Random Rnd = new Random();
        private static void TestReaderInThreadedContext(object info)
        {
            var parameters = (object[]) info;
            var wkts = (string[]) parameters[0];
            var waitHandle = (AutoResetEvent) parameters[1];
            var srids = (int[]) parameters[2];
            var jobId = (int) parameters[3];

            for (var i = 0; i < 1000; i++)
            {
                var wkt = wkts[Rnd.Next(0, wkts.Length)];
                var reader = new WKTReader();
                var geom = reader.Read(wkt);
                Assert.NotNull(geom);
                foreach (var srid in srids)
                {
                    geom.SRID = srid;
                    Assert.AreEqual(geom.SRID, srid);
                    Assert.AreEqual(geom.Factory.SRID, srid);
                }
            }

            Console.WriteLine("ThreadId {0} finished Job {1}", Thread.CurrentThread.ManagedThreadId, jobId);
            waitHandle.Set();
        }

    }
}
