/**
 * Test LineSequencer
 *
 * @version 1.7
 */
using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Linemerge
{

    [TestFixture]
    public class LineSequencerTest
    {

        [Test]
        public void TestSimple()
        {
            String[] wkt = {
      "LINESTRING ( 0 0, 0 10 )",
      "LINESTRING ( 0 20, 0 30 )",
      "LINESTRING ( 0 10, 0 20 )"
    };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void TestSimpleLoop()
        {
            String[] wkt = {
      "LINESTRING ( 0 0, 0 10 )",
      "LINESTRING ( 0 10, 0 0 )",
    };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 0))";
            RunLineSequencer(wkt, result);
        }
        [Test]
        public void TestSimpleBigLoop()
        {
            String[] wkt = {
      "LINESTRING ( 0 0, 0 10 )",
      "LINESTRING ( 0 20, 0 30 )",
      "LINESTRING ( 0 30, 0 00 )",
      "LINESTRING ( 0 10, 0 20 )",
    };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 0))";
            RunLineSequencer(wkt, result);
        }
        [Test]
        public void Test2SimpleLoops()
        {
            String[] wkt = {
      "LINESTRING ( 0 0, 0 10 )",
      "LINESTRING ( 0 10, 0 0 )",
      "LINESTRING ( 0 0, 0 20 )",
      "LINESTRING ( 0 20, 0 0 )",
    };
            String result =
                "MULTILINESTRING ((0 10, 0 0), (0 0, 0 20), (0 20, 0 0), (0 0, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void TestWide8WithTail()
        {
            String[] wkt = {
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
        [Test]
        public void TestSimpleLoopWithTail()
        {
            String[] wkt = {
      "LINESTRING ( 0 0, 0 10 )",
      "LINESTRING ( 0 10, 10 10 )",
      "LINESTRING ( 10 10, 10 20, 0 10 )",
    };
            String result =
                "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10), (10 10, 10 20, 0 10))";
            RunLineSequencer(wkt, result);
        }

        [Test]
        public void TestLineWithRing()
        {
            String[] wkt = {
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
        public void TestMultipleGraphsWithRing()
        {
            String[] wkt = {
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
        public void TestMultipleGraphsWithMultipeRings()
        {
            String[] wkt = {
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

        // isSequenced tests ==========================================================

        [Test]
        public void TestLineSequence()
        {
            String wkt =
                "LINESTRING ( 0 0, 0 10 )";
            RunIsSequenced(wkt, true);
        }

        [Test]
        public void TestSplitLineSequence()
        {
            String wkt =
                "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 3, 0 4) )";
            RunIsSequenced(wkt, true);
        }

        [Test]
        public void TestBadLineSequence()
        {
            String wkt =
                "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 1, 0 4) )";
            RunIsSequenced(wkt, false);
        }

        //==========================================================

        private void RunLineSequencer(String[] inputWKT, String expectedWKT)
        {
            //IEnumerable<IGeometry<Coordinate>> inputGeoms = FromWKT(inputWKT);
            LineSequencer<Coordinate> sequencer = new LineSequencer<Coordinate>();
            foreach( string wkt in inputWKT)
                sequencer.Add(GeometryUtils.ReadWKT(wkt));

            Boolean isCorrect = false;
            if (!sequencer.IsSequenceable())
            {
                Assert.IsTrue(expectedWKT == null);
            }
            else
            {
                IGeometry<Coordinate> expected = GeometryUtils.Reader.Read(expectedWKT);
                IGeometry<Coordinate> result = sequencer.GetSequencedLineStrings();
                Assert.IsTrue(expected.EqualsExact(result));

                Boolean isSequenced = LineSequencer<Coordinate>.IsSequenced(result);
                Assert.IsTrue(isSequenced);
            }
        }

        private static void RunIsSequenced(String inputWKT, Boolean expected)
        {
            IGeometry<Coordinate> g = GeometryUtils.Reader.Read(inputWKT);
            Boolean isSequenced = LineSequencer<Coordinate>.IsSequenced(g);
            Assert.IsTrue(isSequenced == expected);
        }

        static IEnumerable<IGeometry<Coordinate>> FromWKT(String[] wkts)
        {
                return GeometryUtils.ReadWKT(wkts);
            /*
            List geomList = new ArrayList();
          for (int i = 0; i < wkts.length; i++) {
            try {
              geomList.add(rdr.read(wkts[i]));
            }
            catch (Exception ex) {
              ex.printStackTrace();
            }
          }
          return geomList;*/
        }

    }
}