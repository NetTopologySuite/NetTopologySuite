using System;
using System.Threading;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Test for <see cref="WKTReader" />
    /// </summary>
    [TestFixtureAttribute]
    public class WKTReaderTest
    {
        readonly WKTWriter _writer = new WKTWriter();
        private readonly IPrecisionModel _precisionModel;
        private readonly IGeometryFactory _geometryFactory;
        readonly WKTReader _reader;

        public WKTReaderTest()
        {
            _precisionModel = new PrecisionModel(1);
            _geometryFactory = new GeometryFactory(_precisionModel, 0);
            _reader = new WKTReader(_geometryFactory);
        }

        [TestAttribute]
        public void TestReadNaN()
        {
            Assert.AreEqual("POINT (10 10)", _writer.Write(_reader.Read("POINT (10 10 NaN)")));
            Assert.AreEqual("POINT (10 10)", _writer.Write(_reader.Read("POINT (10 10 nan)")));
            Assert.AreEqual("POINT (10 10)", _writer.Write(_reader.Read("POINT (10 10 NAN)")));
        }

        [TestAttribute]
        public void TestReadPointZ()
        {
            var pt = _reader.Read("POINT Z(10 10 10)");
            Assert.IsNotNull(pt);
            Assert.IsInstanceOf(typeof(Point), pt);
            Assert.AreEqual(10, pt.Coordinate.X);
            Assert.AreEqual(10, pt.Coordinate.Y);
            Assert.AreEqual(10, pt.Coordinate.Z);
        }

        [TestAttribute]
        public void TestReadPoint()
        {
            Assert.AreEqual("POINT (10 10)", _writer.Write(_reader.Read("POINT (10 10)")));
            Assert.AreEqual("POINT EMPTY", _writer.Write(_reader.Read("POINT EMPTY")));
        }

        [TestAttribute]
        public void TestReadLineString()
        {
            Assert.AreEqual("LINESTRING (10 10, 20 20, 30 40)", _writer.Write(_reader.Read("LINESTRING (10 10, 20 20, 30 40)")));
            Assert.AreEqual("LINESTRING EMPTY", _writer.Write(_reader.Read("LINESTRING EMPTY")));
        }

        [TestAttribute]
        public void TestReadLinearRing()
        {
            try
            {
                _reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 99)");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.IndexOf("must form a closed linestring", StringComparison.Ordinal) > -1);
            }
            Assert.AreEqual("LINEARRING (10 10, 20 20, 30 40, 10 10)", _writer.Write(_reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 10)")));
            Assert.AreEqual("LINEARRING EMPTY", _writer.Write(_reader.Read("LINEARRING EMPTY")));
        }

        [TestAttribute]
        public void TestReadPolygon()
        {
            Assert.AreEqual("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))", _writer.Write(_reader.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))")));
            Assert.AreEqual("POLYGON EMPTY", _writer.Write(_reader.Read("POLYGON EMPTY")));
        }

        [TestAttribute]
        public void TestReadMultiPoint()
        {
            Assert.AreEqual("MULTIPOINT ((10 10), (20 20))", _writer.Write(_reader.Read("MULTIPOINT ((10 10), (20 20))")));
            Assert.AreEqual("MULTIPOINT EMPTY", _writer.Write(_reader.Read("MULTIPOINT EMPTY")));
        }

        [TestAttribute]
        public void TestReadMultiLineString()
        {
            Assert.AreEqual("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))", _writer.Write(_reader.Read("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))")));
            Assert.AreEqual("MULTILINESTRING EMPTY", _writer.Write(_reader.Read("MULTILINESTRING EMPTY")));
        }

        [TestAttribute]
        public void TestReadMultiPolygon()
        {
            Assert.AreEqual("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))", _writer.Write(_reader.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))")));
            Assert.AreEqual("MULTIPOLYGON EMPTY", _writer.Write(_reader.Read("MULTIPOLYGON EMPTY")));
        }

        [TestAttribute]
        public void TestReadGeometryCollection()
        {
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))", _writer.Write(_reader.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))", _writer.Write(_reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))", _writer.Write(_reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION EMPTY", _writer.Write(_reader.Read("GEOMETRYCOLLECTION EMPTY")));
        }

        [TestAttribute]
        public void TestReadZ()
        {
            Assert.AreEqual(new Coordinate(1, 2, 3), _reader.Read("POINT (1 2 3)").Coordinate);
        }

        [TestAttribute]
        public void TestReadLargeNumbers()
        {
            var precisionModel = new PrecisionModel(1E9);
            var geometryFactory = new GeometryFactory(precisionModel, 0);
            var reader = new WKTReader(geometryFactory);
            var point1 = reader.Read("POINT (123456789.01234567890 10)");
            var point2 = geometryFactory.CreatePoint(new Coordinate(123456789.01234567890, 10));
            Assert.AreEqual(point1.Coordinate.X, point2.Coordinate.X, 1E-7);
            Assert.AreEqual(point1.Coordinate.Y, point2.Coordinate.Y, 1E-7);
        }

        [TestAttribute]
        [Explicit("doesn't works on my machine")]
        public void RepeatedTestThreading()
        {
            for (int i = 0; i < 10; i++)
                ThreadPool.QueueUserWorkItem(o => DoTestThreading(), i);
        }

        [TestAttribute]
        [Explicit("doesn't works on my machine")]
        public void TestThreading()
        {
            DoTestThreading();
        }

        public void DoTestThreading()
        {
            IGeometryServices services = GeoAPI.GeometryServiceProvider.Instance;
            services.CreateGeometryFactory();
            int before = ((NtsGeometryServices) services).NumFactories;

            System.Diagnostics.Debug.WriteLine("{0} factories already created", before);

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
            int[] srids = {4326, 31467, 3857};
            
            const int numJobs = 30;
            var waitHandles = new WaitHandle[numJobs];
            for (int i = 0; i < numJobs; i++)
            {
                waitHandles[i] = new AutoResetEvent(false);
                ThreadPool.QueueUserWorkItem(TestReaderInThreadedContext, new object[] {wkts, waitHandles[i], srids, i});
            }

            WaitHandle.WaitAll(waitHandles, 10000);

            int after = ((NtsGeometryServices)services).NumFactories; 
            Console.WriteLine("Now {0} factories created", after);
            Assert.LessOrEqual(after, before + srids.Length); 
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
