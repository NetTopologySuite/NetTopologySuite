using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    /// <summary
    /// Runs various validation tests on a the results of a geometry operation
    /// </summary>
    public class GeometryOperationValidator
    {
        private static WKTReader rdr = new WKTReader();
        private IGeometry[] ioGeometry;
        private bool expectedSameStructure = false;
        private String wktExpected = null;

        public GeometryOperationValidator(IGeometry[] ioGeometry)
        {
            this.ioGeometry = ioGeometry;
        }

        public GeometryOperationValidator SetExpectedResult(String wktExpected)
        {
            this.wktExpected = wktExpected;
            return this;
        }

        public GeometryOperationValidator SetExpectedSameStructure()
        {
            this.expectedSameStructure = true;
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
            if (!expectedSameStructure)
                return this;
            Assert.IsTrue(SameStructureTester.IsSameStructure(ioGeometry[0], ioGeometry[1]),
                "simplified geometry has different structure than input");
            return this;
        }

        public GeometryOperationValidator TestValid()
        {
            Assert.IsTrue(ioGeometry[1].IsValid,
                "simplified geometry is not valid");
            return this;
        }

        public GeometryOperationValidator TestEmpty(bool isEmpty)
        {
            String failureCondition = isEmpty ? "not empty" : "empty";
            Assert.IsTrue(ioGeometry[1].IsEmpty == isEmpty,
                "simplified geometry is " + failureCondition);
            return this;
        }

        private void TestExpectedResult()
        {
            if (wktExpected == null) return;
            IGeometry expectedGeom = rdr.Read(wktExpected);
            Assert.IsTrue(expectedGeom.EqualsExact(ioGeometry[1]),
                "Expected result not found");
        }
    }
}