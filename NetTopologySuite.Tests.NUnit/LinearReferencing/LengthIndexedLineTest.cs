using System;
using GeoAPI.Geometries;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Tests the <see cref="LengthIndexedLine" /> class
    /// </summary>
    [TestFixtureAttribute]
    public class LengthIndexedLineTest : AbstractIndexedLineTest
    {
        [TestAttribute]
        public void TestExtractLineBeyondRange()
        {
            CheckExtractLine("LINESTRING (0 0, 10 10)", -100, 100, "LINESTRING (0 0, 10 10)");
        }

        [TestAttribute]
        public void TestExtractLineReverse()
        {
            CheckExtractLine("LINESTRING (0 0, 10 0)", 9, 1, "LINESTRING (9 0, 1 0)");
        }

        [TestAttribute]
        public void TestExtractLineReverseMulti()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                19, 1, "MULTILINESTRING ((29 0, 25 0, 20 0), (10 0, 1 0))");
        }

        [TestAttribute]
        public void TestExtractLineNegative()
        {
            CheckExtractLine("LINESTRING (0 0, 10 0)", -9, -1, "LINESTRING (1 0, 9 0)");
        }

        [TestAttribute]
        public void TestExtractLineNegativeReverse()
        {
            CheckExtractLine("LINESTRING (0 0, 10 0)", -1, -9, "LINESTRING (9 0, 1 0)");
        }

        [TestAttribute]
        public void TestExtractLineIndexAtEndpoint()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                10, -1, "LINESTRING (20 0, 25 0, 29 0)");
        }

        /**
         * Tests that leading and trailing zero-length sublines are trimmed in the computed result,
         * and that zero-length extracts return the lowest extracted zero-length line
         */

        [TestAttribute]
        public void TestExtractLineIndexAtEndpointWithZeroLenComponents()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                10, -1, "LINESTRING (20 0, 25 0, 29 0)");
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                5, 10, "LINESTRING (5 0, 10 0)");
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                10, 10, "LINESTRING (10 0, 10 0)");
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (10 0, 10 0), (10 0, 10 0), (10 0, 10 0), (20 0, 25 0, 30 0))",
                10, -10, "LINESTRING (10 0, 10 0)");
        }

        [TestAttribute]
        public void TestExtractLineBothIndicesAtEndpoint()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                10, 10, "LINESTRING (10 0, 10 0)");
        }

        [TestAttribute]
        public void TestExtractLineBothIndicesAtEndpointNegative()
        {
            CheckExtractLine("MULTILINESTRING ((0 0, 10 0), (20 0, 25 0, 30 0))",
                                -10, 10, "LINESTRING (10 0, 10 0)");
        }

        /**
         * From GEOS Ticket #323
         */
        [TestAttribute]
        public void TestProjectExtractPoint()
        {
            IGeometry linearGeom = Read("MULTILINESTRING ((0 2, 0 0), (-1 1, 1 1))");
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            var index = indexedLine.Project(new Coordinate(1, 0));
            Coordinate pt = indexedLine.ExtractPoint(index);
            Assert.IsTrue(pt.Equals(new Coordinate(0, 0)));
        }

        [TestAttribute]
        public void TestExtractPointBeyondRange()
        {
            IGeometry linearGeom = Read("LINESTRING (0 0, 10 10)");
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            Coordinate pt = indexedLine.ExtractPoint(100);
            Assert.IsTrue(pt.Equals(new Coordinate(10, 10)));

            Coordinate pt2 = indexedLine.ExtractPoint(0);
            Assert.IsTrue(pt2.Equals(new Coordinate(0, 0)));
        }

        [TestAttribute]
        public void TestProjectPointWithDuplicateCoords()
        {
            IGeometry linearGeom = Read("LINESTRING (0 0, 10 0, 10 0, 20 0)");
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            double projIndex = indexedLine.Project(new Coordinate(10, 1));
            Assert.IsTrue(projIndex == 10.0);
        }

        /// <summary>
        /// These tests work for LengthIndexedLine, but not LocationIndexedLine
        /// </summary>
        [TestAttribute]
        public void TestOffsetStartPointRepeatedPoint()
        {
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }

        /// <summary>
        /// Tests that z values are interpolated
        /// </summary>
        [TestAttribute]
        public void TestComputeZ()
        {
            IGeometry linearGeom = Read("LINESTRING (0 0 0, 10 10 10)");
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            double projIndex = indexedLine.Project(new Coordinate(5, 5));
            Coordinate projPt = indexedLine.ExtractPoint(projIndex);
            //    System.out.println(projPt);
            Assert.IsTrue(projPt.Equals3D(new Coordinate(5, 5, 5)));
        }

        /// <summary>
        /// Tests that if the input does not have Z ordinates, neither does the output.
        /// </summary>
        [TestAttribute]
        public void TestComputeZNaN()
        {
            IGeometry linearGeom = Read("LINESTRING (0 0, 10 10 10)");
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            double projIndex = indexedLine.Project(new Coordinate(5, 5));
            Coordinate projPt = indexedLine.ExtractPoint(projIndex);
            Assert.IsTrue(Double.IsNaN(projPt.Z));
        }

        private void CheckExtractLine(String wkt, double start, double end, String expected)
        {
            IGeometry linearGeom = Read(wkt);
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            IGeometry result = indexedLine.ExtractLine(start, end);
            CheckExpected(result, expected);
        }

        protected override IGeometry IndicesOfThenExtract(IGeometry linearGeom, IGeometry subLine)
        {
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            double[] loc = indexedLine.IndicesOf(subLine);
            IGeometry result = indexedLine.ExtractLine(loc[0], loc[1]);
            return result;
        }

        protected override bool IndexOfAfterCheck(IGeometry linearGeom, Coordinate testPt)
        {
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);

            // check locations are consecutive
            double loc1 = indexedLine.IndexOf(testPt);
            double loc2 = indexedLine.IndexOfAfter(testPt, loc1);
            if (loc2 <= loc1) return false;

            // check extracted points are the same as the input
            Coordinate pt1 = indexedLine.ExtractPoint(loc1);
            Coordinate pt2 = indexedLine.ExtractPoint(loc2);
            if (!pt1.Equals2D(testPt)) return false;
            if (!pt2.Equals2D(testPt)) return false;

            return true;
        }

        protected override Coordinate ExtractOffsetAt(IGeometry linearGeom, Coordinate testPt, double offsetDistance)
        {
            LengthIndexedLine indexedLine = new LengthIndexedLine(linearGeom);
            double index = indexedLine.IndexOf(testPt);
            return indexedLine.ExtractPoint(index, offsetDistance);
        }
    }
}