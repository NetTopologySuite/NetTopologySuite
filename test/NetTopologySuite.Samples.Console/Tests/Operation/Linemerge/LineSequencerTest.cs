using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Operation.Linemerge
{
    [TestFixture]
    public class LineSequencerTest : BaseSamples
    {
        private static readonly WKTReader rdr = new WKTReader(new NtsGeometryServices(new PrecisionModel(PrecisionModels.Fixed)));

        public LineSequencerTest() : base(new NtsGeometryServices(new PrecisionModel(PrecisionModels.Fixed)))
        { }

        [Test]
        public void Simple()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )" ,
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleLoop()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 0 0 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleBigLoop()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 30, 0 00 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleVeryBigLoop()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 30, 0 40 )",
                "LINESTRING ( 0 40, 0 00 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 40), (0 40, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void SimpleVeryVeryBigLoop()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 30, 0 40 )",
                "LINESTRING ( 0 50, 0 00 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 30, 0 40 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            const string result = "MULTILINESTRING ((0 50, 0 0), (0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 40), (0 40, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void TwoSimpleLoops()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 0 0 )",
                "LINESTRING ( 0 0, 0 20 )",
                "LINESTRING ( 0 20, 0 0 )",
            };
            const string result = "MULTILINESTRING ((0 10, 0 0), (0 0, 0 20), (0 20, 0 0), (0 0, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void Wide8WithTail()
        {
            string[] wkt =
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
            string result = null;
            RunLineSequencer(wkt, result);
        }

       [Test]
        public void SimpleLoopWithTail()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10 )",
                "LINESTRING ( 10 10, 10 20, 0 10 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10), (10 10, 10 20, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void LineWithRing()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void MultipleGraphsWithRing()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
                "LINESTRING ( 0 60, 0 50 )",
                "LINESTRING ( 0 40, 0 50 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void MultipleGraphsWithMultipleRings()
        {
            string[] wkt =
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 10, 40 40, 40 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
                "LINESTRING ( 0 60, 0 50 )",
                "LINESTRING ( 0 40, 0 50 )",
            };
            const string result = "MULTILINESTRING ((0 0, 0 10), (0 10, 40 40, 40 20, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void LineSequence()
        {
            const string wkt = "LINESTRING ( 0 0, 0 10 )";
            RunIsSequenced(wkt, true);
        }

        [Test]
        public void SplitLineSequence()
        {
            const string wkt = "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 3, 0 4) )";
            RunIsSequenced(wkt, true);
        }

        [Test]
        public void BadLineSequence()
        {
            const string wkt = "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 1, 0 4) )";
            RunIsSequenced(wkt, false);
        }

        private static void RunLineSequencer(string[] inputWKT, string expectedWKT)
        {
            try
            {
                var inputGeoms = FromWKT(inputWKT);
                var sequencer = new LineSequencer();
                sequencer.Add(inputGeoms);

                if (!sequencer.IsSequenceable())
                    Assert.IsNull(expectedWKT);
                else
                {
                    var expected = rdr.Read(expectedWKT);
                    var result = sequencer.GetSequencedLineStrings();
                    bool isTrue = expected.EqualsExact(result);
                    Assert.IsTrue(isTrue, "Expected " + expected + " but was " + result);

                    bool isSequenced = LineSequencer.IsSequenced(result);
                    Assert.IsTrue(isSequenced, "result is not sequenced");
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); throw; }
        }

        private static void RunIsSequenced(string inputWKT, bool expected)
        {
            try
            {
                var g = rdr.Read(inputWKT);
                bool isSequenced = LineSequencer.IsSequenced(g);
                Assert.IsTrue(isSequenced == expected);
            }
            catch(Exception ex) { Debug.WriteLine(ex.ToString()); throw ex; }
        }

        private static IEnumerable<Geometry> FromWKT(string[] wkts)
        {
            IList<Geometry> geomList = new List<Geometry>();
            foreach (string wkt in wkts)
            {
                try
                {
                    geomList.Add(rdr.Read(wkt));
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); throw; }
            }
            return geomList;
        }
    }
}

