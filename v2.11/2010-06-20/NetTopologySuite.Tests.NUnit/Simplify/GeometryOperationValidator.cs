using System;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    public class GeometryOperationValidator
    {
        private static IWktGeometryReader<Coordinate> _reader = GeometryUtils.GeometryFactory.WktReader;

        private IGeometry<Coordinate>[] _ioGeometry;
        private Boolean _expectedSameStructure;
        private String _wktExpected = null;

        public GeometryOperationValidator(IGeometry<Coordinate>[] ioGeometry)
        {
            _ioGeometry = ioGeometry;
        }

        public string WKTExpected
        {
            get { return _wktExpected; }
            set { _wktExpected = value; }
        }

        public GeometryOperationValidator SetExpectedResult(String wktExpected)
        {
            _wktExpected = wktExpected;
            return this;
        }

        public GeometryOperationValidator SetExpectedSameStructure()
        {
            _expectedSameStructure = true;
            return this;
        }

        public void Test()
        {
            //    inputGeom = rdr.read(wkt);
            //    simplifiedGeom = TopologyPreservingSimplifier.simplify(inputGeom, tolerance);
            //    System.out.println(simplifiedGeom);
            TestSameStructure();
            TestValid();
            TestExpectedResult();
        }

        public GeometryOperationValidator TestSameStructure()
        {
            if (!_expectedSameStructure)
                return this;
            Assert.IsTrue(SameStructureTester.IsSameStructure(_ioGeometry[0], _ioGeometry[1]),"simplified geometry has different structure than input");
            return this;
        }

        public GeometryOperationValidator TestValid()
        {
            Assert.IsTrue(_ioGeometry[1].IsValid, "simplified geometry is not valid");
            return this;
        }

        public GeometryOperationValidator TestEmpty(Boolean isEmpty)
        {
            String failureCondition = isEmpty ? "not empty" : "empty";
            Assert.IsTrue(_ioGeometry[1].IsEmpty == isEmpty, "simplified geometry is " + failureCondition);
            return this;
        }

        private void TestExpectedResult()
        {
            if (String.IsNullOrEmpty(_wktExpected)) return;

            IGeometry<Coordinate> expectedGeom = _reader.Read(_wktExpected);
            Assert.IsTrue(expectedGeom.EqualsExact(_ioGeometry[1]), "Expected result not found");

        }

    }
}