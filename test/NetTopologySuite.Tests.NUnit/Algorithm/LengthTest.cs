#nullable disable
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
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

        void CheckLengthOfLine(string wkt, double expectedLen)
        {
            var ring = (LineString) Read(wkt);

            var pts = ring.CoordinateSequence;
            double actual = Length.OfLine(pts);
            Assert.AreEqual(actual, expectedLen);
        }
    }
}