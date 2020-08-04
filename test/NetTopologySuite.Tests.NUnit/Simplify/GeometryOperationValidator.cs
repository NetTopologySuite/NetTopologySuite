using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    /// <summary>
    /// Runs various validation tests on a the results of a geometry operation
    /// </summary>
    public class GeometryOperationValidator
    {
        private readonly WKTReader _reader = new WKTReader();
        private readonly Geometry[] _ioGeometry;
        private bool _expectedSameStructure;
        private string _wktExpected;

        public GeometryOperationValidator(Geometry[] ioGeometry)
        {
            _ioGeometry = ioGeometry;
        }

        public GeometryOperationValidator SetExpectedResult(string wktExpected)
        {
            _wktExpected = wktExpected;
            return this;
        }

        public GeometryOperationValidator SetExpectedSameStructure()
        {
            _expectedSameStructure = true;
            return this;
        }

        public bool IsAllTestsPassed()
        {
            try
            {
                Test();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tests if the result is valid.
        /// Throws an exception if result is not valid.
        /// This allows chaining multiple tests together.
        /// </summary>
        /// <exception cref="Exception">Thrown if the result is not valid.</exception>
        public void Test()
        {
            TestSameStructure();
            TestValid();
            TestExpectedResult();
        }

        public GeometryOperationValidator TestSameStructure()
        {
            if (!_expectedSameStructure)
                return this;
            bool test = SameStructureTester.IsSameStructure(_ioGeometry[0], _ioGeometry[1]);
            Assert.IsTrue(test, "simplified geometry has different structure than input");
            return this;
        }

        public GeometryOperationValidator TestValid()
        {
            bool test = _ioGeometry[1].IsValid;
            Assert.IsTrue(test, "simplified geometry is not valid");
            return this;
        }

        public GeometryOperationValidator TestEmpty(bool isEmpty)
        {
            string failureCondition = isEmpty ? "not empty" : "empty";
            bool test = _ioGeometry[1].IsEmpty == isEmpty;
            Assert.IsTrue(test, string.Format("simplified geometry is {0}", failureCondition));
            return this;
        }

        private void TestExpectedResult()
        {
            if (_wktExpected == null) return;
            var expectedGeom = _reader.Read(_wktExpected);
            bool test = expectedGeom.EqualsExact(_ioGeometry[1]);
            Assert.IsTrue(test, "Expected result not found");
        }
    }
}