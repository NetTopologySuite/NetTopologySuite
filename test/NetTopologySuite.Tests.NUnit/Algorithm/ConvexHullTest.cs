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
