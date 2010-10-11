using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Operation.Linemerge
{
    [TestFixture]
    public class LineSequencerTest : BaseSamples
    {
        //private static IWktGeometryReader rdr = new WktReader<BufferedCoordinate>(
        //    GeometryFactory<BufferedCoordinate>.CreateFixedPrecision(
        //        new BufferedCoordinateSequenceFactory()), null);

        public LineSequencerTest() :
            base(GeometryServices.GetGeometryFactory(PrecisionModelType.Fixed))
        {
        }

        private void RunLineSequencer(String[] inputWkt, String expectedWkt)
        {
            try
            {
                IList<IGeometry> inputGeoms = fromWkt(inputWkt);
                LineSequencer<BufferedCoordinate> sequencer
                    = new LineSequencer<BufferedCoordinate>();
                sequencer.Add(inputGeoms);

                if (!sequencer.IsSequenceable())
                {
                    Assert.IsNull(expectedWkt);
                }
                else
                {
                    IGeometry<BufferedCoordinate> expected
                        = Reader.Read(expectedWkt);
                    IGeometry<BufferedCoordinate> result = sequencer.GetSequencedLineStrings();
                    //Boolean isTrue = expected.EqualsExact(result);
                    Boolean isTrue = expected.Equals(result);
                    Assert.IsTrue(isTrue);

                    Boolean isSequenced = LineSequencer<BufferedCoordinate>.IsSequenced(result);
                    Assert.IsTrue(isSequenced);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private void RunIsSequenced(String inputWkt, Boolean expected)
        {
            try
            {
                IGeometry<BufferedCoordinate> g
                    = Reader.Read(inputWkt);
                Boolean isSequenced = LineSequencer<BufferedCoordinate>.IsSequenced(g);
                Assert.IsTrue(isSequenced == expected);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private IList<IGeometry> fromWkt(String[] wkts)
        {
            IList<IGeometry> geometryList = new List<IGeometry>();

            foreach (String wkt in wkts)
            {
                try
                {
                    geometryList.Add(Reader.Read(wkt));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    throw;
                }
            }

            return geometryList;
        }

        [Test]
        public void BadLineSequence()
        {
            String wkt = "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 1, 0 4) )";
            RunIsSequenced(wkt, false);
        }

        [Test]
        public void LineSequence()
        {
            String wkt = "LINESTRING ( 0 0, 0 10 )";
            RunIsSequenced(wkt, true);
        }

        [Test]
        public void LineWithRing()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                    "LINESTRING ( 0 30, 0 20 )",
                    "LINESTRING ( 0 20, 0 10 )",
                };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void MultipleGraphsWithMultipeRings()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                    "LINESTRING ( 0 10, 40 40, 40 20, 0 10 )",
                    "LINESTRING ( 0 30, 0 20 )",
                    "LINESTRING ( 0 20, 0 10 )",
                    "LINESTRING ( 0 60, 0 50 )",
                    "LINESTRING ( 0 40, 0 50 )",
                };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 40 40, 40 20, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void MultipleGraphsWithRing()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                    "LINESTRING ( 0 30, 0 20 )",
                    "LINESTRING ( 0 20, 0 10 )",
                    "LINESTRING ( 0 60, 0 50 )",
                    "LINESTRING ( 0 40, 0 50 )",
                };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void Simple()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 20, 0 30 )",
                    "LINESTRING ( 0 10, 0 20 )",
                };

            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleBigLoop()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 20, 0 30 )",
                    "LINESTRING ( 0 30, 0 00 )",
                    "LINESTRING ( 0 10, 0 20 )",
                };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleLoop()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 10, 0 0 )",
                };

            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleLoopWithTail()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 10, 10 10 )",
                    "LINESTRING ( 10 10, 10 20, 0 10 )",
                };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10), (10 10, 10 20, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleVeryBigLoop()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 30, 0 40 )",
                    "LINESTRING ( 0 40, 0 00 )",
                    "LINESTRING ( 0 20, 0 30 )",
                    "LINESTRING ( 0 10, 0 20 )",
                };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 40), (0 40, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleVeryVeryBigLoop()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 30, 0 40 )",
                    "LINESTRING ( 0 50, 0 00 )",
                    "LINESTRING ( 0 20, 0 30 )",
                    "LINESTRING ( 0 30, 0 40 )",
                    "LINESTRING ( 0 10, 0 20 )",
                };
            String result =
                "MULTILINESTRING ((0 50, 0 0), (0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 40), (0 40, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SplitLineSequence()
        {
            String wkt = "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 3, 0 4) )";
            RunIsSequenced(wkt, true);
        }

        [Test]
        public void TwoSimpleLoops()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 0 10, 0 0 )",
                    "LINESTRING ( 0 0, 0 20 )",
                    "LINESTRING ( 0 20, 0 0 )",
                };
            String result = "MULTILINESTRING ((0 10, 0 0), (0 0, 0 20), (0 20, 0 0), (0 0, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void Wide8WithTail()
        {
            String[] wkt =
                {
                    "LINESTRING ( 0 0, 0 10 )",
                    "LINESTRING ( 10 0, 10 10 )",
                    "LINESTRING ( 0 0, 10 0 )",
                    "LINESTRING ( 0 10, 10 10 )",
                    "LINESTRING ( 0 10, 0 20 )",
                    "LINESTRING ( 10 10, 10 20 )",
                    "LINESTRING ( 0 20, 10 20 )",
                    "LINESTRING ( 10 20, 30 30 )",
                };
            String result = null;
            RunLineSequencer(wkt, result);
        }
    }
}