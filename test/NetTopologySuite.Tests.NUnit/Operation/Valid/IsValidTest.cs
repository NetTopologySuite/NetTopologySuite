using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    [TestFixture]
    public class IsValidTest
    {
        private readonly GeometryFactory _geometryFactory;
        private readonly WKTReader _reader;

        public IsValidTest()
        {
            var gs = new NtsGeometryServices(PrecisionModel.Floating.Value, 0);
            _geometryFactory = gs.CreateGeometryFactory();
            _reader = new WKTReader(gs);
        }

        [Test]
        public void TestInvalidCoordinate()
        {
            var badCoord = new Coordinate(1.0, double.NaN);
            Coordinate[] pts = {new Coordinate(0.0, 0.0), badCoord};
            Geometry line = _geometryFactory.CreateLineString(pts);
            var isValidOp = new IsValidOp(line);
            bool valid = isValidOp.IsValid;
            var err = isValidOp.ValidationError;
            var errCoord = err.Coordinate;

            Assert.AreEqual(TopologyValidationErrors.InvalidCoordinate, err.ErrorType);
            Assert.IsTrue(double.IsNaN(errCoord.Y));
            Assert.AreEqual(false, valid);
        }

        [Test]
        public void TestZeroAreaPolygon()
        {
            var g = _reader.Read("POLYGON((0 0, 0 0, 0 0, 0 0, 0 0))");
            Assert.That(() => g.IsValid, Throws.Nothing);
        }

        [Test]
        public void TestLineString()
        {
            var g = _reader.Read("LINESTRING(0 0, 0 0)");
            Assert.That(() => g.IsValid, Throws.Nothing);
        }

        [Test]
        public void TestLinearRingTriangle()
        {
            var g = _reader.Read(
                "LINEARRING (100 100, 150 200, 200 100, 100 100)");
            Assert.That(g.IsValid);
        }

        [Test]
        public void TestLinearRingSelfCrossing()
        {
            var g = _reader.Read(
                "LINEARRING (150 100, 300 300, 100 300, 350 100, 150 100)");
            Assert.That(!g.IsValid);
        }

        [Test]
        public void TestLinearRingSelfCrossing2()
        {
            var g = _reader.Read(
                "LINEARRING (0 0, 100 100, 100 0, 0 100, 0 0)");
            Assert.That(!g.IsValid);
        }

    }
}
