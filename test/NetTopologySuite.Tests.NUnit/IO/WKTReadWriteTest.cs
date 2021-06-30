using System;
using System.Diagnostics;
using System.Threading;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    [TestFixture]
    public class WKTReadWriteTest
    {
        private readonly CoordinateSequenceFactory _csFactory;
        private readonly WKTReader _reader;

        private readonly WKTWriter _writer;

        public WKTReadWriteTest()
        {
            // We deliberately chose a coordinate sequence factory that can handle 4 dimensions
            _csFactory = PackedCoordinateSequenceFactory.DoubleFactory;
            var gs = new NtsGeometryServices(_csFactory, PrecisionModel.Floating.Value, 0);
            _reader = new WKTReader(gs);

            _writer = new WKTWriter(4) { OutputOrdinates = Ordinates.XY };
        }

        [Test]
        public void TestReadNaN()
        {
            const string pt = "POINT (10 10)";
            Assert.AreEqual(pt, _writer.Write(_reader.Read("POINT (10 10 NaN)")));
            Assert.AreEqual(pt, _writer.Write(_reader.Read("POINT (10 10 nan)")));
            Assert.AreEqual(pt, _writer.Write(_reader.Read("POINT (10 10 NAN)")));
        }

        [Test]
        public void TestReadPointZ()
        {
            var pt = _reader.Read("POINT Z(10 10 10)");
            Assert.IsNotNull(pt);
            Assert.IsInstanceOf(typeof(Point), pt);
            Assert.AreEqual(10, pt.Coordinate.X);
            Assert.AreEqual(10, pt.Coordinate.Y);
            Assert.AreEqual(10, pt.Coordinate.Z);
            Assert.IsTrue(((Point)pt).CoordinateSequence.Ordinates == Ordinates.XYZ);
            _reader.IsOldNtsCoordinateSyntaxAllowed = false;
            pt = _reader.Read("POINT(10 10)");
            Assert.IsTrue(((Point)pt).CoordinateSequence.Ordinates == Ordinates.XY);
        }

        [Test]
        public void TestReaderOrdinateSequenceDimension()
        {
            _reader.IsOldNtsCoordinateSyntaxAllowed = false;
            var pt = (Point)_reader.Read("POINT(10 10)");
            Assert.IsTrue(pt.CoordinateSequence.Ordinates == Ordinates.XY);

            _reader.IsOldNtsCoordinateSyntaxAllowed = true;
            pt = (Point)_reader.Read("POINT(10 10 NAN)");
            Assert.IsTrue(pt.CoordinateSequence.Ordinates == Ordinates.XYZ);

            pt = (Point)_reader.Read("POINT(10 10 5)");
            Assert.IsTrue(pt.CoordinateSequence.Ordinates == Ordinates.XYZ);

            _reader.IsOldNtsCoordinateSyntaxAllowed = false;
            var ls = (LineString)_reader.Read("LINESTRING(10 10, 10 20)");
            Assert.IsTrue(ls.CoordinateSequence.Ordinates == Ordinates.XY);

            _reader.IsOldNtsCoordinateSyntaxAllowed = true;
            ls = (LineString)_reader.Read("LINESTRING(10 10 NAN, 10 20 NAN)");
            Assert.IsTrue(ls.CoordinateSequence.Ordinates == Ordinates.XYZ);

            ls = (LineString)_reader.Read("LINESTRING(10 10 1, 10 20 2)");
            Assert.IsTrue(ls.CoordinateSequence.Ordinates == Ordinates.XYZ);
        }

        [Test]
        public void TestReadPoint()
        {
            Assert.AreEqual("POINT (10 10)", _writer.Write(_reader.Read("POINT (10 10)")));
            Assert.AreEqual("POINT EMPTY", _writer.Write(_reader.Read("POINT EMPTY")));
        }

        [Test]
        public void TestReadLineString()
        {
            Assert.AreEqual("LINESTRING (10 10, 20 20, 30 40)", _writer.Write(_reader.Read("LINESTRING (10 10, 20 20, 30 40)")));
            Assert.AreEqual("LINESTRING EMPTY", _writer.Write(_reader.Read("LINESTRING EMPTY")));
        }

        [Test]
        public void TestReadLinearRing()
        {
            try
            {
                _reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 99)");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains("must form a closed linestring"));
            }
            Assert.AreEqual("LINEARRING (10 10, 20 20, 30 40, 10 10)", _writer.Write(_reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 10)")));
            Assert.AreEqual("LINEARRING EMPTY", _writer.Write(_reader.Read("LINEARRING EMPTY")));
        }

        [Test]
        public void TestReadPolygon()
        {
            Assert.AreEqual("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))", _writer.Write(_reader.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))")));
            Assert.AreEqual("POLYGON EMPTY", _writer.Write(_reader.Read("POLYGON EMPTY")));
        }

        [Test]
        public void TestReadMultiPoint()
        {
            Assert.AreEqual("MULTIPOINT ((10 10), (20 20))", _writer.Write(_reader.Read("MULTIPOINT ((10 10), (20 20))")));
            Assert.AreEqual("MULTIPOINT EMPTY", _writer.Write(_reader.Read("MULTIPOINT EMPTY")));
            Assert.AreEqual("MULTIPOINT (EMPTY, EMPTY)", _writer.Write(_reader.Read("MULTIPOINT (EMPTY, EMPTY)")));
        }

        [Test]
        public void TestReadMultiLineString()
        {
            Assert.AreEqual("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))", _writer.Write(_reader.Read("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))")));
            Assert.AreEqual("MULTILINESTRING EMPTY", _writer.Write(_reader.Read("MULTILINESTRING EMPTY")));
            Assert.AreEqual("MULTILINESTRING (EMPTY, EMPTY)", _writer.Write(_reader.Read("MULTILINESTRING (EMPTY, EMPTY)")));
        }

        [Test]
        public void TestReadMultiPolygon()
        {
            Assert.AreEqual("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))", _writer.Write(_reader.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))")));
            Assert.AreEqual("MULTIPOLYGON EMPTY", _writer.Write(_reader.Read("MULTIPOLYGON EMPTY")));
            Assert.AreEqual("MULTIPOLYGON (EMPTY, EMPTY)", _writer.Write(_reader.Read("MULTIPOLYGON (EMPTY, EMPTY)")));
        }

        [Test]
        public void TestReadGeometryCollection()
        {
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))", _writer.Write(_reader.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))", _writer.Write(_reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))", _writer.Write(_reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION EMPTY", _writer.Write(_reader.Read("GEOMETRYCOLLECTION EMPTY")));
        }

        [Test]
        public void TestReadGeometryCollectionEmptyWithElements()
        {
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT EMPTY)",
                _writer.Write(_reader.Read("GEOMETRYCOLLECTION ( POINT EMPTY )")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT EMPTY, LINESTRING EMPTY)",
                _writer.Write(_reader.Read("GEOMETRYCOLLECTION ( POINT EMPTY, LINESTRING EMPTY )")));
        }

        [Test]
        [Explicit("doesn't works on my machine")]
        public void RepeatedTestThreading()
        {
            for (int i = 0; i < 10; i++)
                ThreadPool.QueueUserWorkItem(o => DoTestThreading(), i);
        }

        [Test]
        [Explicit("doesn't works on my machine")]
        public void TestThreading()
        {
            DoTestThreading();
        }

        public void DoTestThreading()
        {
            var services = NtsGeometryServices.Instance;
            services.CreateGeometryFactory();
            int before = ((NtsGeometryServices) services).NumFactories;

            Debug.WriteLine("{0} factories already created", before);

            string[] wkts = new[]
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
            TestContext.WriteLine("Now {0} factories created", after);
            Assert.LessOrEqual(after, before + srids.Length);
        }

        private static readonly Random Rnd = new Random();
        private static void TestReaderInThreadedContext(object info)
        {
            object[] parameters = (object[]) info;
            string[] wkts = (string[]) parameters[0];
            var waitHandle = (AutoResetEvent) parameters[1];
            int[] srids = (int[]) parameters[2];
            int jobId = (int) parameters[3];

            for (int i = 0; i < 1000; i++)
            {
                string wkt = wkts[Rnd.Next(0, wkts.Length)];
                var reader = new WKTReader();
                var geom = reader.Read(wkt);
                Assert.NotNull(geom);
                foreach (int srid in srids)
                {
                    geom.SRID = srid;
                    Assert.AreEqual(geom.SRID, srid);
                    Assert.AreEqual(geom.Factory.SRID, srid);
                }
            }

            TestContext.WriteLine("ThreadId {0} finished Job {1}", Thread.CurrentThread.ManagedThreadId, jobId);
            waitHandle.Set();
        }

    }
}
