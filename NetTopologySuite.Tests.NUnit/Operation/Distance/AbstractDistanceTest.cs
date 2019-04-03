using GeoAPI.Geometries;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance
{
    [TestFixture]
    public abstract class AbstractDistanceTest : GeometryTestCase
    {
        private readonly IPrecisionModel _precisionModel;
        private readonly IGeometryFactory _geometryFactory;
        private readonly WKTReader _reader;

        protected AbstractDistanceTest()
        {
            _precisionModel = new PrecisionModel(1);
            _geometryFactory = new GeometryFactory(_precisionModel, 0);
            _reader = new WKTReader(_geometryFactory);
        }

        protected bool SkipTestsThatRelyOnCheckingPointInPolygon { get; set; }

        [Test]
        public void TestDisjointCollinearSegments()
        {
            var g1 = _reader.Read("LINESTRING (0.0 0.0, 9.9 1.4)");
            var g2 = _reader.Read("LINESTRING (11.88 1.68, 21.78 3.08)");
            Assert.That(g1.Distance(g2), Is.EqualTo(2.23606).Within(0.0001));
        }

        [Test]
        public void TestEverything()
        {
            var g1 = _reader.Read("POLYGON ((40 320, 200 380, 320 80, 40 40, 40 320),  (180 280, 80 280, 100 100, 220 140, 180 280))");
            var g2 = _reader.Read("POLYGON ((160 240, 120 240, 120 160, 160 140, 160 240))");
            Assert.That(g1.Distance(g2), Is.EqualTo(18.97366596).Within(1E-5));

            g2 = _reader.Read("POLYGON ((160 240, 120 240, 120 160, 180 100, 160 240))");
            Assert.That(g1.Distance(g2), Is.Zero.Within(1E-5));

            var l1 = (ILineString)_reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            var l2 = (ILineString)_reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            Assert.That(l1.Distance(l2), Is.Zero.Within(1E-5));
        }

        [Test]
        public void TestEmpty()
        {
            var g1 = _reader.Read("POINT (0 0)");
            var g2 = _reader.Read("POLYGON EMPTY");
            Assert.That(g1.Distance(g2), Is.Zero);
        }

        [TestCase("POLYGON ((200 180, 60 140, 60 260, 200 180))", "POINT (140 280)", 57.05597791103589, 111.6923076923077, 230.46153846153845, 140, 280, false)]
        [TestCase("POLYGON ((200 180, 60 140, 60 260, 200 180))", "MULTIPOINT ((140 280), (140 320))", 57.05597791103589, 111.6923076923077, 230.46153846153845, 140, 280, false)]
        [TestCase("LINESTRING (100 100, 200 100, 200 200, 100 200, 100 100)", "POINT (10 10)", 127.27922061357856, 100, 100, 10, 10, false)]
        [TestCase("LINESTRING (100 100, 200 200)", "LINESTRING (100 200, 200 100)", 0, 150, 150, 150, 150, false)]
        [TestCase("LINESTRING (100 100, 200 200)", "LINESTRING (150 121, 200 0)", 20.506096654409877, 135.5, 135.5, 150, 121, false)]
        [TestCase("POLYGON ((76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185), (267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237))", "LINESTRING (153 204, 185 224, 209 207, 238 222, 254 186)", 13.788860460124573, 139.4956500724988, 206.78661188980183, 153, 204, false)]
        [TestCase("POLYGON ((76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185), (267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237))", "LINESTRING (120 215, 185 224, 209 207, 238 222, 254 186)", 0, 120, 215, 120, 215, true)]
        public void TestClosestPoints(string wkt0, string wkt1, double distance, double p0X, double p0Y, double p1X, double p1Y, bool reliesOnCheckingPointInPolygon)
        {
            if (reliesOnCheckingPointInPolygon && this.SkipTestsThatRelyOnCheckingPointInPolygon)
            {
                Assert.Ignore("skip this test for now, since it relies on checking point-in-polygon");
            }

            var g0 = Read(wkt0);
            var g1 = Read(wkt1);

            var nearestPoints = NearestPoints(g0, g1);

            const double Tolerance = 1E-10;
            Assert.That(nearestPoints[0].Distance(nearestPoints[1]), Is.EqualTo(distance).Within(Tolerance));

            Assert.That(nearestPoints[0].X, Is.EqualTo(p0X).Within(Tolerance));
            Assert.That(nearestPoints[0].Y, Is.EqualTo(p0Y).Within(Tolerance));
            Assert.That(nearestPoints[1].X, Is.EqualTo(p1X).Within(Tolerance));
            Assert.That(nearestPoints[1].Y, Is.EqualTo(p1Y).Within(Tolerance));
        }

        protected abstract Coordinate[] NearestPoints(IGeometry g1, IGeometry g2);
    }
}
