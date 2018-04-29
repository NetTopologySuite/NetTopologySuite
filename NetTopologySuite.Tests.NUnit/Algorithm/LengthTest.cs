using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class LengthTest : GeometryTestCase
    {
        [TestCase]
        public void TestLength()
        {
            CheckLengthOfLine("LINESTRING (100 200, 200 200, 200 100, 100 100, 100 200)", 400.0);
        }
        void CheckLengthOfLine(String wkt, double expectedLen)
        {
            var ring = (ILineString) Read(wkt);
            var pts = ring.CoordinateSequence;
            var actual = Length.OfLine(pts);
            Assert.AreEqual(actual, expectedLen);
        }
    }
}
