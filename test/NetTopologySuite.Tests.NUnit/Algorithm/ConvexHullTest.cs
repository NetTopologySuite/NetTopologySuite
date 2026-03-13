using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class ConvexHullTest : GeometryTestCase
    {
        private readonly GeometryFactory _geometryFactory;
        private readonly WKTReader _reader;

        public ConvexHullTest()
        {
            var gs = new NtsGeometryServices(new PrecisionModel(1000), 0);
            _geometryFactory = gs.CreateGeometryFactory();
            _reader = new WKTReader(gs);
        }

        [Test]
        public void TestManyIdenticalPoints()
        {
            var pts = new Coordinate[100];
            for (int i = 0; i < 99; i++)
                pts[i] = new Coordinate(0, 0);
            pts[99] = new Coordinate(1, 1);
            var ch = new ConvexHull(pts, _geometryFactory);
            var actualGeometry = ch.GetConvexHull();
            var expectedGeometry = _reader.Read("LINESTRING (0 0, 1 1)");
            Assert.IsTrue(actualGeometry.EqualsExact(expectedGeometry));
        }

        [Test]
        public void TestAllIdenticalPoints()
        {
            var pts = new Coordinate[100];
            for (int i = 0; i < 100; i++)
                pts[i] = new Coordinate(0, 0);
            var ch = new ConvexHull(pts, _geometryFactory);
            var actualGeometry = ch.GetConvexHull();
            var expectedGeometry = _reader.Read("POINT (0 0)");
            Assert.IsTrue(expectedGeometry.EqualsExact(actualGeometry));
        }

        [Test]
        public void TestLineCollinear()
        {
            CheckConvexHull(
                "LINESTRING (30 220, 240 220, 240 220)",
                "LINESTRING (30 220, 240 220)");
        }

        [Test]
        public void TestLineCollinear2()
        {
            CheckConvexHull(
                "MULTIPOINT (130 240, 130 240, 130 240, 570 240, 570 240, 570 240, 650 240)",
                "LINESTRING (130 240, 650 240)");
        }

        [Test]
        public void TestMultiCollinearEqual12()
        {
            CheckConvexHull(
                "MULTIPOINT (0 0, 0 0, 10 0)",
                "LINESTRING (0 0, 10 0)");
        }

        [Test]
        public void TestMultiPointCollinearEqual23()
        {
            CheckConvexHull(
                "MULTIPOINT (0 0, 10 0, 10 0)",
                "LINESTRING (0 0, 10 0)");
        }

        [Test]
        public void TestMultiPointCollinearEqualNone()
        {
            CheckConvexHull(
                "MULTIPOINT (0 0, 5 0, 10 0)",
                "LINESTRING (0 0, 10 0)");
        }

        [Test]
        public void TestMultiPoint()
        {
            CheckConvexHull(
                "MULTIPOINT (0 0, 5 1, 10 0)",
                "POLYGON ((0 0, 5 1, 10 0, 0 0))");
        }

        [Test]
        public void TestMultiPointLinear()
        {
            CheckConvexHull(
                "MULTIPOINT (0 0, 0 0, 5 0, 5 0, 10 0, 10 0)",
                "LINESTRING (0 0, 10 0)");
        }

        [Test]
        public void TestCollinearPoints()
        {
            CheckConvexHull(
                "MULTIPOINT ((-0.2 -0.1), (0 -0.1), (0.2 -0.1), (0 -0.1), (-0.2 0.1), (0 0.1), (0.2 0.1), (0 0.1))",
                "POLYGON ((-0.2 -0.1, -0.2 0.1, 0.2 0.1, 0.2 -0.1, -0.2 -0.1))");
        }
        /**
         * See https://trac.osgeo.org/geos/ticket/850
         */
        [Test]
        public void TestGEOS_850()
        {
            CheckConvexHull("01040000001100000001010000002bd3a24002bcb0417ff59d2051e25c4101010000003aebcec70a8b3cbfdb123fe713a2e8be0101000000afa0bb8638b770bf7fc1d77d0dda1cbf01010000009519cb944ce070bf1a46cd7df4201dbf010100000079444b4cd1937cbfa6ca29ada6a928bf010100000083323f09e16c7cbfd36d07ee0b8828bf01010000009081b8f066967ebf915fbc9ebe652abf0101000000134cf280633bc1bf37b754972dbe6dbf0101000000ea992c094df585bf1bbabc8a42f332bf0101000000c0a13c7fb31186bf9af7b10cc50b33bf0101000000a0bba15a0a7188bf8fba7870e91735bf01010000000fc8701903db93bf93bdbe93b52241bf01010000007701a73b29cc90bfb770bc3732fe3cbf010100000036fa45b75b8b8cbf1cfca5bf59a238bf0101000000a54e773f7f287ebf910d4621e5062abf01010000004b5b5dc4196f55bfa51f0579717f02bf01010000007e549489513a5fbfa57bacea34f30abf",
                "POLYGON ((-0.1346248988744213 -0.0036307230426677, -0.0019059940589774 -0.0000514030956167, 280756800.63603467 7571780.50964105, -0.1346248988744213 -0.0036307230426677))",
                0.000000000001);
        }

        /**
         * Tests robustness issue in radial sort.
         * See https://github.com/libgeos/geos/issues/722
         */
        [Test]
        public void TestCollinearPointsTinyX()
        {
            CheckConvexHull(
                "MULTIPOINT (-0.2 -0.1, 1.38777878e-17 -0.1, 0.2 -0.1, -1.38777878e-17 -0.1, -0.2 0.1, 1.38777878e-17 0.1, 0.2 0.1, -1.38777878e-17 0.1)",
                "POLYGON ((-0.2 -0.1, -0.2 0.1, 0.2 0.1, 0.2 -0.1, -0.2 -0.1))");
        }

        [Test]
        public void TestCollinearPointsLessTinyX()
        {
            CheckConvexHull(
                "MULTIPOINT (-0.2 -0.1, 1.38777878e-7 -0.1, 0.2 -0.1, -1.38777878e-7 -0.1, -0.2 0.1, 1.38777878e-7 0.1, 0.2 0.1, -1.38777878e-7 0.1)",
                "POLYGON ((-0.2 -0.1, -0.2 0.1, 0.2 0.1, 0.2 -0.1, -0.2 -0.1))");
        }

        /**
         * Test case fails in GEOS due to incorrect fix to radial sorting.
         * This did not trigger a failure in JTS, probably because the sorting
         * is less strict.
         */
        [Test]
        public void TestGEOSSortFailure()
        {
            CheckConvexHull("MULTIPOINT ((140 350), (510 140), (110 140), (250 290), (250 50), (300 370), (450 310), (440 160), (290 280), (220 160), (100 260), (320 230), (200 280), (360 130), (330 210), (380 80), (220 210), (380 310), (260 150), (260 110), (170 130))",
                "POLYGON ((100 260, 140 350, 300 370, 450 310, 510 140, 380 80, 250 50, 110 140, 100 260))");
        }

        //==========================================================

        /// <summary>
        /// Tests a bug which modified the input point array in some cases
        /// (larger geometries which are nearly linear).
        /// See https://github.com/locationtech/jts/discussions/1082
        /// </summary>
        [Test]
        public void TestInputUnmodified()
        {
            // test case is a large nearly-linear geometry
            const string line = "LINESTRING (74.20888 -245.64678, 16.0148 -54.30037, 15.9566 -54.10903, 15.86931 -53.82201, 15.81112 -53.63066, 15.75292 -53.43931, 15.69473 -53.24797, 15.63654 -53.05662, 15.57834 -52.86528, 15.52015 -52.67393, 15.46195 -52.48258, 15.40376 -52.29124, 15.34557 -52.09989, 15.28737 -51.90854, 15.22918 -51.7172, 15.17098 -51.52585, 15.11279 -51.3345, 15.05459 -51.14316, 14.9964 -50.95181, 14.93821 -50.76046, 14.88001 -50.56912, 14.82182 -50.37777, 14.76362 -50.18643, 14.70543 -49.99508, 14.64724 -49.80373, 14.58904 -49.61239, 14.53085 -49.42104, 14.47265 -49.22969, 14.41446 -49.03835, 14.35627 -48.847, 14.29807 -48.65565, 14.23988 -48.46431, 14.18168 -48.27296, 14.12349 -48.08161, 14.0653 -47.89027, 14.0071 -47.69892, 13.94891 -47.50758, 13.89071 -47.31623, 13.83252 -47.12488, 13.77432 -46.93354, 13.71613 -46.74219, 13.65794 -46.55084, 13.59974 -46.3595, 13.54155 -46.16815, -63.14605 260.17393, -63.18794 260.3695, -63.22983 260.56506, -63.27173 260.76062, -63.31362 260.95619, -63.35551 261.15175, -63.39741 261.34731, -63.4393 261.54288, -63.4812 261.73844, -63.52309 261.934, -63.56498 262.12956, -63.60688 262.32513, -63.64877 262.52069, -63.69067 262.71625, -63.73256 262.91182, -63.77445 263.10738, -63.81635 263.30294, -63.85824 263.49851, -63.90014 263.69407, -63.94203 263.88963, -63.98392 264.0852, -64.02582 264.28076, -64.06771 264.47632, -64.10961 264.67188, -64.1515 264.86745, -64.19339 265.06301, -64.23529 265.25857, -64.27718 265.45414, -64.31907 265.6497, -64.36097 265.84526, -64.40286 266.04083, -64.44476 266.23639, -64.48665 266.43195, -64.52854 266.62751, -64.57044 266.82308, -110.60097 481.69601)";
            var geom = Read(line);
            var geomCopy = geom.Copy();
            geom.ConvexHull();
            bool isUnmodified = geomCopy.EqualsExact(geom);
            Assert.That(isUnmodified, Is.True, "Input geometry has been modified");
        }

        //==========================================================

        private void CheckConvexHull(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = geom.ConvexHull();
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }
        private void CheckConvexHull(string wkt, string wktExpected, double tolerance)
        {
            var geom = Read(wkt);
            var actual = geom.ConvexHull();
            var expected = Read(wktExpected);
            CheckEqual(expected, actual, tolerance);
        }

    }
}
