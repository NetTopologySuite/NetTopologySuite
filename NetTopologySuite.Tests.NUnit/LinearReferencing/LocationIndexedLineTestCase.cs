using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Tests the <see cref="LocationIndexedLine"/> class
    /// </summary>
    [TestFixture]
    public class LocationIndexedLineTestCase : AbstractIndexedLineTestCase
    {
        [Test]
        public void TestMultiLineStringSimple()
        {
            RunExtractLine("MULTILINESTRING ((0 0, 10 10), (20 20, 30 30))",
                            new LinearLocation(0, 0, .5),
                            new LinearLocation(1, 0, .5),
                            "MULTILINESTRING ((5 5, 10 10), (20 20, 25 25))");
        }

        [Test]
        public void TestMultiLineString2()
        {
            RunExtractLine("MULTILINESTRING ((0 0, 10 10), (20 20, 30 30))",
                            new LinearLocation(0, 0, 1.0),
                            new LinearLocation(1, 0, .5),
                            "MULTILINESTRING ((10 10, 10 10), (20 20, 25 25))");
        }

        private void RunExtractLine(String wkt, LinearLocation start, LinearLocation end, String expected)
        {
            IGeometry geom = Read(wkt);
            LocationIndexedLine lil = new LocationIndexedLine(geom);
            IGeometry result = lil.ExtractLine(start, end);
            //System.out.println(result);
            CheckExpected(result, expected);
        }

        protected override IGeometry IndicesOfThenExtract(IGeometry input, IGeometry subLine)
        {
            LocationIndexedLine indexedLine = new LocationIndexedLine(input);
            LinearLocation[] loc = indexedLine.IndicesOf(subLine);
            IGeometry result = indexedLine.ExtractLine(loc[0], loc[1]);
            return result;
        }

        protected override bool IndexOfAfterCheck(IGeometry linearGeom, Coordinate testPt)
        {
            LocationIndexedLine indexedLine = new LocationIndexedLine(linearGeom);

            // check locations are consecutive
            LinearLocation loc1 = indexedLine.IndexOf(testPt);
            LinearLocation loc2 = indexedLine.IndexOfAfter(testPt, loc1);
            if (loc2.CompareTo(loc1) <= 0) return false;

            // check extracted points are the same as the input
            Coordinate pt1 = indexedLine.ExtractPoint(loc1);
            Coordinate pt2 = indexedLine.ExtractPoint(loc2);
            if (!pt1.Equals2D(testPt)) return false;
            if (!pt2.Equals2D(testPt)) return false;

            return true;
        }

        //protected override Coordinate ExtractOffsetAt(IGeometry linearGeom, Coordinate testPt, double offsetDistance)
        //{
        //    LocationIndexedLine indexedLine = new LocationIndexedLine(linearGeom);
        //    LinearLocation index = indexedLine.IndexOf(testPt);

        //    return indexedLine.ExtractPoint(index, offsetDistance);
        //}
    }
}