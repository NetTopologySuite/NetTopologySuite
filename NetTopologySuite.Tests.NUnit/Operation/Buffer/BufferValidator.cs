using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using NetTopologySuite.Operation.Buffer.Validate;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    public class BufferValidator
    {
        private delegate void TestMethod();

        private class TestCase : IComparable<TestCase>
        {
            private readonly string _name;
            private readonly int _priority;

            public TestCase(string name)
                : this(name, 2)
            {
            }

            public TestCase(string name, int priority)
            {
                _name = name;
                _priority = priority;
            }

            public string Name
            {
                get { return _name; }
            }

            public int Priority
            {
                get { return _priority; }
            }

            public TestMethod TestMethod { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public int CompareTo(TestCase other)
            {
                return _priority - other.Priority;
            }
        }


        private IGeometry _original;
        private readonly double _bufferDistance;
        private readonly Dictionary<string, TestCase> _nameToTestMap = new Dictionary<string, TestCase>();
        private IGeometry _buffer;
        private const int QuadrantSegments1 = 100;
        private const int QuadrantSegments2 = 50;
        private readonly String _wkt;
        private readonly WKTWriter _wktWriter = new WKTWriter();
        private WKTReader _wktReader;

        //public BufferValidator()
        //{
        //    IGeometry g =
        //        new WKTReader().Read(
        //        "MULTILINESTRING (( 635074.5418406526 6184832.4888257105, 635074.5681951842 6184832.571842485, 635074.6472587794 6184832.575795664 ), ( 635074.6657069515 6184832.53889932, 635074.6933792098 6184832.451929366, 635074.5642420045 6184832.474330718 ))");
        //    Console.WriteLine(g);
        //    Console.WriteLine(g.Buffer(0.01, 100));
        //    Console.WriteLine("END");
        //}

        public BufferValidator(double bufferDistance, String wkt)
            : this(bufferDistance, wkt, true)
        {
        }

        public BufferValidator(double bufferDistance, String wkt, bool addContainsTest)
        {
            // SRID = 888 is to test that SRID is preserved in computed buffers
            SetFactory(new PrecisionModel(), 888);
            _bufferDistance = bufferDistance;
            _wkt = wkt;
            if (addContainsTest) AddTestContains();
            //addBufferResultValidatorTest();
        }

        private String Supplement(String message)
        {
            String newMessage = "\n" + message + "\n";
            newMessage += "Original: " + _wktWriter.WriteFormatted(GetOriginal()) + "\n";
            newMessage += "Buffer Distance: " + _bufferDistance + "\n";
            newMessage += "Buffer: " + _wktWriter.WriteFormatted(GetBuffer()) + "\n";
            return newMessage.Substring(0, newMessage.Length - 1);
        }

        private BufferValidator AddTest(TestCase test)
        {
            _nameToTestMap.Add(test.Name, test);
            return this;
        }

        public BufferValidator TestExpectedArea(double expectedArea)
        {
            try
            {
                double tolerance =
                    Math.Abs(
                        GetBuffer().Area
                        - GetOriginal()
                              .Buffer(
                                  _bufferDistance,
                                  QuadrantSegments1 - QuadrantSegments2)
                              .Area);

                Assert.AreEqual(expectedArea, GetBuffer().Area, tolerance, "Area Test");
            }
            catch (Exception e)
            {
                throw new Exception(
                    Supplement(e.ToString()) + e.StackTrace);
            }

            return this;
        }

        public BufferValidator TestEmptyBufferExpected(bool emptyBufferExpected)
        {
            Assert.IsTrue(
                emptyBufferExpected == GetBuffer().IsEmpty,
                Supplement(
                    "Expected buffer "
                    + (emptyBufferExpected ? "" : "not ")
                    + "to be empty")
                );

            return this;
        }

        public BufferValidator TestBufferHolesExpected(bool bufferHolesExpected)
        {
            Assert.IsTrue(
                HasHoles(GetBuffer()) == bufferHolesExpected,
                Supplement(
                    "Expected buffer "
                    + (bufferHolesExpected ? "" : "not ")
                    + "to have holes")
                );

            return this;
        }

        private static bool HasHoles(IGeometry buffer)
        {
            if (buffer.IsEmpty)
            {
                return false;
            }
            var polygon = buffer as IPolygon;
            if (polygon != null)
                return polygon.NumInteriorRings > 0;

            var multiPolygon = (IMultiPolygon) buffer;
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                if (HasHoles(multiPolygon.GetGeometryN(i)))
                {
                    return true;
                }
            }
            return false;
        }

        private IGeometry GetOriginal()
        {
            return _original ?? (_original = _wktReader.Read(_wkt));
        }


        public BufferValidator SetPrecisionModel(PrecisionModel precisionModel)
        {
            _wktReader = new WKTReader(new GeometryFactory(precisionModel));
            return this;
        }

        public BufferValidator SetFactory(PrecisionModel precisionModel, int srid)
        {
            _wktReader = new WKTReader(new GeometryFactory(precisionModel, srid));
            return this;
        }

        private IGeometry GetBuffer()
        {
            if (_buffer == null)
            {
                _buffer = GetOriginal().Buffer(_bufferDistance, QuadrantSegments1);
                if (GetBuffer() is GeometryCollection && GetBuffer().IsEmpty)
                {
                    try
                    {
                        //#contains doesn't work with GeometryCollections [Jon Aquino
                        // 10/29/2003]
                        _buffer = _wktReader.Read("POINT EMPTY");
                    }
                    catch (GeoAPI.IO.ParseException e)
                    {
                        NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
                    }
                }
            }
            return _buffer;
        }

        private void AddTestContains()
        {
            AddTest(
                new TestCase("Contains", 10)
                    {
                        TestMethod = () =>
                            {
                                if (GetOriginal() is GeometryCollection)
                                    return;

                                Assert.IsTrue(GetOriginal().IsValid);
                                if (_bufferDistance > 0)
                                {
                                    Assert.IsTrue(
                                        Contains(GetBuffer(), GetOriginal()),
                                        Supplement("Expected buffer to contain original"));
                                }
                                else
                                {
                                    Assert.IsTrue(
                                        Contains(GetOriginal(), GetBuffer()),
                                        Supplement("Expected original to contain buffer"));
                                }
                            }
                    });

        }

        public BufferValidator TestContains()
        {
            if (GetOriginal() is GeometryCollection)
            {
                return this;
            }
            Assert.IsTrue(GetOriginal().IsValid);
            if (_bufferDistance > 0)
            {
                Assert.IsTrue(
                    Contains(GetBuffer(), GetOriginal()),
                    Supplement("Expected buffer to contain original"));
            }
            else
            {
                Assert.IsTrue(
                    Contains(GetOriginal(), GetBuffer()),
                    Supplement("Expected original to contain buffer"));
            }
            return this;
        }

        private static bool Contains(IGeometry a, IGeometry b)
        {
            //JTS doesn't currently handle empty geometries correctly [Jon Aquino
            // 10/29/2003]
            if (b.IsEmpty)
            {
                return true;
            }
            bool isContained = a.Contains(b);
            return isContained;
        }

        private void AddBufferResultValidatorTest()
        {
            AddTest(new TestCase("Buffer result validator", 20)
                {
                    TestMethod = () =>
                        {
                            if (GetOriginal() is GeometryCollection)
                            {
                                return;
                            }
                            Assert.IsTrue(
                                BufferResultValidator.IsValid(GetOriginal(), _bufferDistance, GetBuffer()),
                                Supplement("BufferResultValidator failure"));
                        }
                });
        }

        /// <summary>
        /// Method to perform all registered Tests
        /// </summary>
        public void Test()
        {
            foreach (var testCase in _nameToTestMap.Values)
            {
                testCase.TestMethod();
            }
        }
    }
}
