using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class AreaTest : GeometryTestCase
    {

        [Test]
        public void TestArea()
        {
            CheckAreaOfRing("LINEARRING (100 200, 200 200, 200 100, 100 100, 100 200)", 10000.0);
        }

        [Test]
        public void TestAreaSignedCw()
        {
            CheckAreaOfRingSigned("LINEARRING (100 200, 200 200, 200 100, 100 100, 100 200)", 10000.0);
        }

        [Test]
        public void TestAreaSignedCcw()
        {
            CheckAreaOfRingSigned("LINEARRING (100 200, 100 100, 200 100, 200 200, 100 200)", -10000.0);
        }

        void CheckAreaOfRing(string wkt, double expectedArea)
        {
            var ring = (ILinearRing) Read(wkt);

            var ringPts = ring.Coordinates;
            double actual1 = Area.OfRing(ringPts);
            Assert.AreEqual(actual1, expectedArea);

            var ringSeq = ring.CoordinateSequence;
            double actual2 = Area.OfRing(ringSeq);
            Assert.AreEqual(actual2, expectedArea);
        }

        void CheckAreaOfRingSigned(string wkt, double expectedArea)
        {
            var ring = (ILinearRing) Read(wkt);

            var ringPts = ring.Coordinates;
            double actual1 = Area.OfRingSigned(ringPts);
            Assert.AreEqual(actual1, expectedArea);

            var ringSeq = ring.CoordinateSequence;
            double actual2 = Area.OfRingSigned(ringSeq);
            Assert.AreEqual(actual2, expectedArea);
        }
    }
}