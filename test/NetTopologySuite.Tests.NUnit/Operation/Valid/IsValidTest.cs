using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    [TestFixture]
    public class IsValidTest
    {
        private PrecisionModel precisionModel;
        private readonly GeometryFactory geometryFactory;
        private readonly WKTReader reader;

        public IsValidTest()
        {
            precisionModel = new PrecisionModel();
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [Test]
        public void TestInvalidCoordinate()
        {
            var badCoord = new Coordinate(1.0, double.NaN);
            Coordinate[] pts = { new Coordinate(0.0, 0.0), badCoord };
            Geometry line = geometryFactory.CreateLineString(pts);
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
            var g = reader.Read("POLYGON((0 0, 0 0, 0 0, 0 0, 0 0))");
            Assert.That(g.IsValid);
        }

        [Test]
        public void TestLineString()
        {
            var g = reader.Read("LINESTRING(0 0, 0 0)");
            Assert.That(g.IsValid);
        }

    }
}
