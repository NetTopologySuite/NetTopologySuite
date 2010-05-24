using System;
using System.Collections.Generic;
using C5;
using GeoAPI;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    public class BufferValidator
    {
        private class Test : IComparable<Test>
        {
            public delegate void DoTest();
            //protected readonly BufferValidator Parent;
            private readonly String _name;
            private readonly int _priority;

            public Test(String name) : this(name, 2) { }
            public Test(String name, int priority)
            {
                //Parent = parent;
                _name = name;
                _priority = priority;
            }

            public String Name { get { return _name; } }
            public override String ToString() { return Name; }
            public void Do()
            {
                if (TestDelegate != null) TestDelegate();
            }
            public DoTest TestDelegate;
            //public abstract void Do();
            public int CompareTo(Test o)
            {
                return _priority - o._priority;
            }
        }


        private IGeometry<Coordinate> _original;
        private double _bufferDistance;
        private readonly Dictionary<String, Test> _nameToTestMap = new Dictionary<String, Test>();
        private IGeometry<Coordinate> _buffer;
        protected const int QuadrantSegments1 = 100;
        protected const int QuadrantSegments2 = 50;
        private String _wkt;
        private IGeometryFactory<Coordinate> _geomFact = GeometryUtils.GeometryFactory;
        private IWktGeometryWriter<Coordinate> _wktWriter = GeometryUtils.GeometryFactory.WktWriter;
        private IWktGeometryReader<Coordinate> _wktReader = GeometryUtils.GeometryFactory.WktReader;


        public BufferValidator(double bufferDistance, String wkt)
        {
            // SRID = 888 is to Test that SRID is preserved in computed buffers
            //SetFactory(new PrecisionModel(), 888);
            _bufferDistance = bufferDistance;
            _wkt = wkt;
            AddContainsTest();
            //    addBufferResultValidatorTest();
        }


        public void Do()
        {
            try
            {
                TreeSet<Test> ts = new TreeSet<Test>();
                ts.AddAll(_nameToTestMap.Values);
                foreach (Test test in ts)
                    test.Do();
            }
            catch (Exception e)
            {
                throw new Exception(
                    Supplement(e.ToString()));//+ StringUtil.GetStackTrace(e));
            }
        }

        private String Supplement(String message)
        {
            String newMessage = "\n" + message + "\n";
            newMessage += "Original: " + _wktWriter.Write(Original) + "\n";
            newMessage += "Buffer Distance: " + _bufferDistance + "\n";
            newMessage += "Buffer: " + _wktWriter.Write(Buffer) + "\n";
            return newMessage.Substring(0, newMessage.Length - 1);
        }

        private BufferValidator AddTest(Test test)
        {
            _nameToTestMap.Add(test.Name, test);
            return this;
        }

        public BufferValidator SetExpectedArea(double expectedArea)
        {
            return AddTest(new Test("Area Test")
                               {
                                   TestDelegate = delegate
                                                      {
                                                          double tolerance = Math.Abs(
                                                              ((ISurface<Coordinate>)Buffer).Area -
                                                              ((ISurface<Coordinate>)
                                                               _original.Buffer(_bufferDistance,
                                                                                QuadrantSegments1 - QuadrantSegments2)).Area);
                                                          Assert.AreEqual(expectedArea, ((ISurface<Coordinate>)Buffer).Area,
                                                                          tolerance, "Area Test");

                                                      }
                               });

        }

        public BufferValidator SetEmptyBufferExpected(Boolean emptyBufferExpected)
        {
            return AddTest(new Test("Empty Buffer Test", 1)
                               {
                                   TestDelegate = delegate
                                                      {
                                                          Assert.IsTrue(emptyBufferExpected == Buffer.IsEmpty,
                                                                        Supplement("Expected buffer " +
                                                                                   (emptyBufferExpected ? "" : "not ") +
                                                                                   "to be empty")
                                                              );
                                                      }
                               });
        }


        public BufferValidator SetBufferHolesExpected(Boolean bufferHolesExpected)
        {
            return AddTest(new Test("Buffer Holes Test")
            {
                TestDelegate = delegate
                {
                    Assert.IsTrue(HasHoles(Buffer) == bufferHolesExpected,
                      Supplement("Expected buffer " + (bufferHolesExpected ? "" : "not ") + "to have holes"));
                }
            });
        }

        private static Boolean HasHoles(IGeometry<Coordinate> buffer)
        {
            if (buffer.IsEmpty)
            {
                return false;
            }
            if (buffer is IPolygon<Coordinate>)
            {
                return ((IPolygon<Coordinate>)buffer).InteriorRingsCount > 0;
            }
            IMultiPolygon<Coordinate> multiPolygon = (IMultiPolygon<Coordinate>)buffer;
            foreach (IPolygon<Coordinate> polygon in multiPolygon)
            {
                if (HasHoles(polygon)) return true;
            }
            return false;
        }

        private IGeometry<Coordinate> Original
        {
            get
            {
                if (_original == null)
                {
                    _original = _wktReader.Read(_wkt);
                }
                return _original;
            }
        }


        public BufferValidator SetPrecisionModel(IPrecisionModel<Coordinate> precisionModel)
        {
            _wktReader = GeometryUtils.GetPrecisedFactory(precisionModel).WktReader;
            //new WKTReader(new GeometryFactory(precisionModel));
            return this;
        }

        public BufferValidator SetFactory(IPrecisionModel<Coordinate> precisionModel, int srid)
        {
            _wktReader = GeometryUtils.GetPrecisedFactory(precisionModel).WktReader;
            return this;
        }

        private IGeometry<Coordinate> Buffer
        {
            get
            {
                if (_buffer == null)
                {
                    _buffer = Original.Buffer(_bufferDistance, QuadrantSegments1);
                    if (_buffer is IGeometryCollection<Coordinate> && _buffer.IsEmpty)
                    {
                        try
                        {
                            //#contains doesn't work with GeometryCollections [Jon Aquino
                            // 10/29/2003]
                            _buffer = _wktReader.Read("POINT EMPTY");
                        }
                        catch (ParseException e)
                        {
                            throw new Exception("Should never reach here");
                        }
                    }
                }
                return _buffer;
            }
        }

        private void AddContainsTest()
        {
            AddTest(new Test("Contains Test")
                        {
                            TestDelegate = delegate
                                               {
                                                   if (Original is IGeometryCollection<Coordinate>)
                                                   {
                                                       return;
                                                   }
                                                   Assert.IsTrue(Original.IsValid);
                                                   if (_bufferDistance > 0)
                                                   {
                                                       Assert.IsTrue(Contains(Buffer, Original),
                                                                     Supplement("Expected buffer to contain original"));
                                                   }
                                                   else
                                                   {
                                                       Assert.IsTrue(Contains(Original, Buffer),
                                                                     Supplement("Expected original to contain buffer"));
                                                   }
                                               }
                        });
        }


        private Boolean Contains(IGeometry<Coordinate> a, IGeometry<Coordinate> b)
        {
            //JTS doesn't currently handle empty geometries correctly [Jon Aquino
            // 10/29/2003]
            if (b.IsEmpty)
            {
                return true;
            }
            Boolean isContained = a.Contains(b);
            return isContained;
        }





        private void AddBufferResultValidatorTest()
        {
            AddTest(new Test("BufferResultValidator Test")
                        {
                            TestDelegate = delegate
                                               {
                                                   if (_original is IGeometryCollection<Coordinate>)
                                                   {
                                                       return;
                                                   }

                                                   Assert.IsTrue(
                                                       GisSharpBlog.NetTopologySuite.Operation.Buffer.Validate.
                                                           BufferResultValidator<Coordinate>.IsValid(_original,
                                                                                                     _bufferDistance,
                                                                                                     _buffer),
                                                       Supplement("BufferResultValidator failure"));
                                               }
                        }
                );
        }
    }
}

    
