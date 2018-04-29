using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Base class for linear referencing class unit tests
    /// </summary>
    [TestFixtureAttribute]
    public abstract class AbstractIndexedLineTest
    {
        private readonly WKTReader _reader = new WKTReader();
        [TestAttribute]
        public void TestFirst()
        {
            RunOffsetTest("LINESTRING (0 0, 20 20)", "POINT(20 20)", 0.0, "POINT (20 20)");
        }
        [TestAttribute]
        public void TestML()
        {
            RunIndicesOfThenExtract("MULTILINESTRING ((0 0, 10 10), (20 20, 30 30))",
                "MULTILINESTRING ((1 1, 10 10), (20 20, 25 25))");
        }
        [TestAttribute]
        public void TestPartOfSegmentNoVertex()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 10 10, 20 20)",
                "LINESTRING (1 1, 9 9)");
        }
        [TestAttribute]
        public void TestPartOfSegmentContainingVertex()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 10 10, 20 20)",
                "LINESTRING (5 5, 10 10, 15 15)");
        }
        /// <summary>
        /// Tests that duplicate coordinates are handled correctly.
        /// </summary>
        [TestAttribute]
        public void TestPartOfSegmentContainingDuplicateCoords()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 10 10, 10 10, 20 20)",
                "LINESTRING (5 5, 10 10, 10 10, 15 15)");
        }
        /// <summary>
        /// Following tests check that correct portion of loop is identified.
        /// This requires that the correct vertex for (0,0) is selected.
        /// </summary>
        [TestAttribute]
        public void TestLoopWithStartSubLine()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (0 0, 0 10, 10 10)");
        }
        [TestAttribute]
        public void TestLoopWithEndingSubLine()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (10 10, 10 0, 0 0)");
        }
        // test a subline equal to the parent loop
        [TestAttribute]
        public void TestLoopWithIdenticalSubLine()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)");
        }
        // test a zero-length subline equal to the start point
        [TestAttribute]
        public void TestZeroLenSubLineAtStart()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (0 0, 0 0)");
        }
        // test a zero-length subline equal to a mid point
        [TestAttribute]
        public void TestZeroLenSubLineAtMidVertex()
        {
            RunIndicesOfThenExtract("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "LINESTRING (10 10, 10 10)");
        }
        [TestAttribute]
        public void TestIndexOfAfterSquare()
        {
            RunIndexOfAfterTest("LINESTRING (0 0, 0 10, 10 10, 10 0, 0 0)",
                "POINT (0 0)");
        }
        [TestAttribute]
        public void TestIndexOfAfterRibbon()
        {
            RunIndexOfAfterTest("LINESTRING (0 0, 0 60, 50 60, 50 20, -20 20)",
                "POINT (0 20)");
        }
        [TestAttribute]
        public void TestOffsetStartPoint()
        {
            RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }
        [TestAttribute]
        public virtual void TestOffsetStartPointRepeatedPoint()
        {
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");
            // These tests work for LengthIndexedLine, but not LocationIndexedLine
            //RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            //RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }
        [TestAttribute]
        public void TestOffsetEndPoint()
        {
            RunOffsetTest("LINESTRING (0 0, 20 20)", "POINT(20 20)", 0.0, "POINT (20 20)");
            RunOffsetTest("LINESTRING (0 0, 13 13, 20 20)", "POINT(20 20)", 0.0, "POINT (20 20)");
            RunOffsetTest("LINESTRING (0 0, 10 0, 20 0)", "POINT(20 0)", 1.0, "POINT (20 1)");
            RunOffsetTest("LINESTRING (0 0, 20 0)", "POINT(10 0)", 1.0, "POINT (10 1)"); // point on last segment
            RunOffsetTest("MULTILINESTRING ((0 0, 10 0), (10 0, 20 0))", "POINT(10 0)", -1.0, "POINT (10 -1)");
            RunOffsetTest("MULTILINESTRING ((0 0, 10 0), (10 0, 20 0))", "POINT(20 0)", 1.0, "POINT (20 1)");
        }
        protected IGeometry Read(string wkt)
        {
            try
            {
                return _reader.Read(wkt);
            }
            catch (GeoAPI.IO.ParseException ex)
            {
                throw new ApplicationException("An exception occured while reading the wkt", ex);
            }
        }
        protected void RunIndicesOfThenExtract(string inputStr, string subLineStr)
        {
            var input = Read(inputStr);
            var subLine = Read(subLineStr);
            var result = IndicesOfThenExtract(input, subLine);
            CheckExpected(result, subLineStr);
        }
        protected void CheckExpected(IGeometry result, string expected)
        {
            var subLine = Read(expected);
            var isEqual = result.EqualsExact(subLine, 1.0e-5);
            if (!isEqual)
                Console.WriteLine("Computed result is: " + result);
            Assert.IsTrue(isEqual);
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
        protected void RunIndexOfAfterTest(string inputStr, string testPtWKT)
        {
            var input = Read(inputStr);
            var testPoint = Read(testPtWKT);
            var testPt = testPoint.Coordinate;
            var resultOk = IndexOfAfterCheck(input, testPt);
            Assert.IsTrue(resultOk);
        }
        protected abstract bool IndexOfAfterCheck(IGeometry input, Coordinate testPt);
        private const double ToleranceDist = 0.001;
        protected void RunOffsetTest(string inputWKT, string testPtWKT, double offsetDistance, string expectedPtWKT)
        {
            var input = Read(inputWKT);
            var testPoint = Read(testPtWKT);
            var expectedPoint = Read(expectedPtWKT);
            var testPt = testPoint.Coordinate;
            var expectedPt = expectedPoint.Coordinate;
            var offsetPt = ExtractOffsetAt(input, testPt, offsetDistance);
            var isOk = offsetPt.Distance(expectedPt) < ToleranceDist;
            if (!isOk)
                Console.WriteLine("Expected = " + expectedPoint + "  Actual = " + offsetPt);
            Assert.IsTrue(isOk);
        }
        protected abstract Coordinate ExtractOffsetAt(IGeometry input, Coordinate testPt, double offsetDistance);
    }
}
