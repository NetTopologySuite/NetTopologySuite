using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using NetTopologySuite.Operation.Linemerge;

namespace NetTopologySuite.Tests.NUnit.Operation.LineMerge
{
    /// <summary>
    /// LineSequencer tests
    /// </summary>
    [TestFixtureAttribute]
    public class LineSequencerTest
    {
        private static readonly WKTReader Rdr =
            new WKTReader();
            //new WKTReader(new GeometryFactory(new PrecisionModel(PrecisionModels.Fixed)));

        [TestAttribute]
        public void TestSimple()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 10, 0 20 )"
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void TestSimpleLoop()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 0 0 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void TestSimpleBigLoop()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 30, 0 00 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute /*, Ignore("Degenerate loop")*/]
        public void Test2SimpleLoops()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 0 0 )",
                "LINESTRING ( 0 0, 0 20 )",
                "LINESTRING ( 0 20, 0 0 )",
            };
            string result =
                "MULTILINESTRING ((0 10, 0 0), (0 0, 0 20), (0 20, 0 0), (0 0, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void Test2SimpleLoops2()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 20 10, 20 0 )",
                "LINESTRING ( 20 0, 0 0 )",
                "LINESTRING ( 0 10, 20 10 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 20 10), (20 10, 20 0), (20 0, 0 0))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void TestWide8WithTail()
        {
            string[] wkt = {
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

        [TestAttribute]
        public void TestSimpleLoopWithTail()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10 )",
                "LINESTRING ( 10 10, 10 20, 0 10 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10), (10 10, 10 20, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void TestLineWithRing()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void TestMultipleGraphsWithRing()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
                "LINESTRING ( 0 60, 0 50 )",
                "LINESTRING ( 0 40, 0 50 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        [TestAttribute]
        public void TestMultipleGraphsWithMultipeRings()
        {
            string[] wkt = {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 10, 40 40, 40 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
                "LINESTRING ( 0 60, 0 50 )",
                "LINESTRING ( 0 40, 0 50 )",
            };
            string result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 40 40, 40 20, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        // isSequenced tests ==========================================================

        [TestAttribute]
        public void TestLineSequence()
        {
            string wkt =
                "LINESTRING ( 0 0, 0 10 )";
            RunIsSequenced(wkt, true);
        }

        [TestAttribute]
        public void TestSplitLineSequence()
        {
            string wkt =
                "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 3, 0 4) )";
            RunIsSequenced(wkt, true);
        }

        [TestAttribute]
        public void TestBadLineSequence()
        {
            string wkt =
                "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 1, 0 4) )";
            RunIsSequenced(wkt, false);
        }

        //==========================================================

        private static void RunLineSequencer(string[] inputWKT, string expectedWKT)
        {
            var inputGeoms = FromWKT(inputWKT);
            var sequencer = new LineSequencer();
            sequencer.Add(inputGeoms);

            var isCorrect = false;
            if (!sequencer.IsSequenceable())
            {
                Assert.IsTrue(expectedWKT == null);
            }
            else
            {
                var expected = Rdr.Read(expectedWKT);
                var result = sequencer.GetSequencedLineStrings();
                var isOK = expected.EqualsNormalized(result);
                if (! isOK) {
                    Console.WriteLine("ERROR - Expected: " + expected);
                    Console.WriteLine("          Actual: " + result);
                }

                var isSequenced = LineSequencer.IsSequenced(result);
                Assert.IsTrue(isOK, "Result does not match expected (using EqualsNormalized)!");
                Assert.IsTrue(isSequenced, "Result geometry is not sequenced!");
            }
        }

        private static void RunIsSequenced(string inputWKT, bool expected)
        {
            var g = Rdr.Read(inputWKT);
            var isSequenced = LineSequencer.IsSequenced(g);
            Assert.IsTrue(isSequenced == expected);
        }

        private static List<IGeometry> FromWKT(string[] wkts)
        {
            var geomList = new List<IGeometry>();
            foreach (var wkt in wkts)
            {
                try
                {
                    geomList.Add(Rdr.Read(wkt));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return geomList;
        }
    }
}