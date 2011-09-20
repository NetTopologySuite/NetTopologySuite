using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{

    /// <summary>
    /// Tests the <see cref="LocationIndexedLine" /> class
    /// </summary>
    [TestFixture]
    public abstract class AbstractIndexedLineTestCase
    {
        private WKTReader reader = new WKTReader();

        [Test]
        public void TestML()
        {
            RunIndicesOfThenExtract("MULTILINESTRING ((0 0, 10 10), (20 20, 30 30))",
                "MULTILINESTRING ((1 1, 10 10), (20 20, 25 25))");
        }

        [Test]
        public void TestPartOfSegmentNoVertex()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 10 10, 20 20)",
                "LINESTRING (1 1, 9 9)");
        }

        [Test]
        public void TestPartOfSegmentContainingVertex()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 10 10, 20 20)",
                "LINESTRING (5 5, 10 10, 15 15)");
        }

        /// <summary>
        /// Tests that duplicate coordinates are handled correctly.
        /// </summary>
        [Test]
        public void TestPartOfSegmentContainingDuplicateCoords()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 10 10, 10 10, 20 20)",
                "LINESTRING (5 5, 10 10, 10 10, 15 15)");
        }

        /// <summary>
        /// Following tests check that correct portion of loop is identified.
        /// This requires that the correct vertex for (0,0) is selected.
        /// </summary>
        [Test]
        public void TestLoopWithStartSubLine()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (0 0, 0 10, 10 10)");
        }

        [Test]
        public void TestLoopWithEndingSubLine()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (10 10, 10 0, 0 0)");
        }

        // test a subline equal to the parent loop
        [Test]
        public void TestLoopWithIdenticalSubLine()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)");
        }

        // test a zero-length subline equal to the start point
        [Test]
        public void TestZeroLenSubLineAtStart()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (0 0, 0 0)");
        }

        // test a zero-length subline equal to a mid point
        [Test]
        public void TestZeroLenSubLineAtMidVertex()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (10 10, 10 10)");
        }

        [Test]
        public void TestIndexOfAfterSquare()
        {
            RunIndexOfAfterTest("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "POINT (0 0)");
        }

        [Test]
        public void TestIndexOfAfterRibbon()
        {
            RunIndexOfAfterTest("LINESTRING (0 0, 0 60, 50 60, 50 20, -20 20)",
                "POINT (0 20)");
        }

        [Ignore("NTS does not have a method overload for the ExtractPoint method which takes an index and an offset distance.  Once this migrated to NTS, the following block can be uncommented")]
        public void TestOffsetStartPoint()
        {
            //TODO: Uncomment when NTS has a method overload for the ExtractPoint method which takes an index and an offset distance
            //RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            //RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");
            //RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            //RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }

        [Ignore("NTS does not have a method overload for the ExtractPoint method which takes an index and an offset distance.  Once this migrated to NTS, the following block can be uncommented")]
        public void TestOffsetStartPointRepeatedPoint()
        {
            //TODO: Uncomment when NTS has a method overload for the ExtractPoint method which takes an index and an offset distance
            //RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            //RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");
            
            
            // These tests work for LengthIndexedLine, but not LocationIndexedLine
            //runOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            //runOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }

        protected IGeometry Read(String wkt)
        {
            try
            {
                return reader.Read(wkt);
            }
            catch (ParseException ex)
            {
                throw new ApplicationException("An exception occured while reading the wkt", ex);
            }
        }

        protected void RunIndicesOfThenExtract(String inputStr, String subLineStr)
        //
        {
            IGeometry input = Read(inputStr);
            IGeometry subLine = Read(subLineStr);
            IGeometry result = IndicesOfThenExtract(input, subLine);
            CheckExpected(result, subLineStr);
        }

        protected void CheckExpected(IGeometry result, String expected)
        {
            IGeometry subLine = Read(expected);
            Assert.IsTrue(result.EqualsExact(subLine, 1.0e-5));
        }

        protected abstract IGeometry IndicesOfThenExtract(IGeometry input, IGeometry subLine);
        /*
            // example of indicesOfThenLocate method
            private Geometry indicesOfThenLocate(LineString input, LineString subLine)
            {
            LocationIndexedLine indexedLine = new LocationIndexedLine(input);
            LineStringLocation[] loc = indexedLine.indicesOf(subLine);
            Geometry result = indexedLine.locate(loc[0], loc[1]);
            return result;
        }
        */

        protected void RunIndexOfAfterTest(String inputStr, String testPtWKT)
        {
            IGeometry input = Read(inputStr);
            IGeometry testPoint = Read(testPtWKT);
            Coordinate testPt = testPoint.Coordinate;
            bool resultOK = IndexOfAfterCheck(input, testPt);
            Assert.IsTrue(resultOK);
        }

        protected abstract bool IndexOfAfterCheck(IGeometry input, Coordinate testPt);

        static double TOLERANCE_DIST = 0.001;

        //TODO: Uncomment when NTS has a method overload for the ExtractPoint method which takes an index and an offset distance
        //protected void RunOffsetTest(String inputWKT, String testPtWKT, double offsetDistance, String expectedPtWKT)
        //{
        //    IGeometry input = Read(inputWKT);
        //    IGeometry testPoint = Read(testPtWKT);
        //    IGeometry expectedPoint = Read(expectedPtWKT);
        //    Coordinate testPt = testPoint.Coordinate;
        //    Coordinate expectedPt = expectedPoint.Coordinate;
        //    Coordinate offsetPt = ExtractOffsetAt(input, testPt, offsetDistance);

        //    bool isOk = offsetPt.Distance(expectedPt) < TOLERANCE_DIST;
        //    if (!isOk)
        //        Console.WriteLine("Expected = " + expectedPoint + "  Actual = " + offsetPt);
        //    Assert.IsTrue(isOk);
        //}

        //TODO: Uncomment when NTS has a method overload for the ExtractPoint method which takes an index and an offset distance
        //protected abstract Coordinate ExtractOffsetAt(IGeometry input, Coordinate testPt, double offsetDistance);
    }
}