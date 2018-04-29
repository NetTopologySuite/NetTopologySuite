using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    [TestFixtureAttribute]
    public class IsValidTest
    {
        private PrecisionModel precisionModel;
        private GeometryFactory geometryFactory;
        WKTReader reader;
        public IsValidTest()
        {
            precisionModel = new PrecisionModel();
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }
        [TestAttribute]
        public void TestInvalidCoordinate()
        {
            var badCoord = new Coordinate(1.0, double.NaN);
            Coordinate[] pts = { new Coordinate(0.0, 0.0), badCoord };
            IGeometry line = geometryFactory.CreateLineString(pts);
            var isValidOp = new IsValidOp(line);
            var valid = isValidOp.IsValid;
            var err = isValidOp.ValidationError;
            var errCoord = err.Coordinate;
            Assert.AreEqual(TopologyValidationErrors.InvalidCoordinate, err.ErrorType);
            Assert.IsTrue(double.IsNaN(errCoord.Y));
            Assert.AreEqual(false, valid);
        }
    }
}
