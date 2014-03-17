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
            Coordinate badCoord = new Coordinate(1.0, Double.NaN);
            Coordinate[] pts = { new Coordinate(0.0, 0.0), badCoord };
            IGeometry line = geometryFactory.CreateLineString(pts);
            IsValidOp isValidOp = new IsValidOp(line);
            bool valid = isValidOp.IsValid;
            TopologyValidationError err = isValidOp.ValidationError;
            Coordinate errCoord = err.Coordinate;

            Assert.AreEqual(TopologyValidationErrors.InvalidCoordinate, err.ErrorType);
            Assert.IsTrue(Double.IsNaN(errCoord.Y));
            Assert.AreEqual(false, valid);
        }
    }
}
