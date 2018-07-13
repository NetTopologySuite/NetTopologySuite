using System;
using GeoAPI.Geometries;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Tests the <see cref="LocationIndexedLine"/> class
    /// </summary>
    [TestFixtureAttribute]
    public class LocationIndexedLineTest : AbstractIndexedLineTest
    {
        [TestAttribute]
        public override void TestOffsetStartPointRepeatedPoint()
        {
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", 1.0, "POINT (-0.7071067811865475 0.7071067811865475)");
            RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(0 0)", -1.0, "POINT (0.7071067811865475 -0.7071067811865475)");

            //// These tests work for LengthIndexedLine, but not LocationIndexedLine
            //RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", 5.0, "POINT (6.464466094067262 13.535533905932738)");
            //RunOffsetTest("LINESTRING (0 0, 10 10, 10 10, 20 20)", "POINT(10 10)", -5.0, "POINT (13.535533905932738 6.464466094067262)");
        }

        [TestAttribute]
        public void TestMultiLineStringSimple()
        {
            RunExtractLine("MULTILINESTRING ((0 0, 10 10), (20 20, 30 30))",
                            new LinearLocation(0, 0, .5),
                            new LinearLocation(1, 0, .5),
                            "MULTILINESTRING ((5 5, 10 10), (20 20, 25 25))");
        }

        [TestAttribute]
        public void TestMultiLineString2()
        {
            RunExtractLine("MULTILINESTRING ((0 0, 10 10), (20 20, 30 30))",
                            new LinearLocation(0, 0, 1.0),
                            new LinearLocation(1, 0, .5),
                            "MULTILINESTRING ((10 10, 10 10), (20 20, 25 25))");
        }

        private void RunExtractLine(string wkt, LinearLocation start, LinearLocation end, string expected)
        {
            var geom = Read(wkt);
            var lil = new LocationIndexedLine(geom);
            var result = lil.ExtractLine(start, end);
            //System.out.println(result);
            CheckExpected(result, expected);
        }

        protected override IGeometry IndicesOfThenExtract(IGeometry input, IGeometry subLine)
        {
            var indexedLine = new LocationIndexedLine(input);
            var loc = indexedLine.IndicesOf(subLine);
            var result = indexedLine.ExtractLine(loc[0], loc[1]);
            return result;
        }

        protected override bool IndexOfAfterCheck(IGeometry linearGeom, Coordinate testPt)
        {
            var indexedLine = new LocationIndexedLine(linearGeom);

            // check locations are consecutive
            var loc1 = indexedLine.IndexOf(testPt);
            var loc2 = indexedLine.IndexOfAfter(testPt, loc1);
            if (loc2.CompareTo(loc1) <= 0) return false;

            // check extracted points are the same as the input
            var pt1 = indexedLine.ExtractPoint(loc1);
            var pt2 = indexedLine.ExtractPoint(loc2);
            if (!pt1.Equals2D(testPt)) return false;
            if (!pt2.Equals2D(testPt)) return false;

            return true;
        }

        protected override Coordinate ExtractOffsetAt(IGeometry linearGeom, Coordinate testPt, double offsetDistance)
        {
            var indexedLine = new LocationIndexedLine(linearGeom);
            var index = indexedLine.IndexOf(testPt);

            return indexedLine.ExtractPoint(index, offsetDistance);
        }
    }
}