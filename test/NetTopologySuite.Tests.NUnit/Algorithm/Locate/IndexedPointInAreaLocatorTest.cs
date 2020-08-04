using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Locate
{
    /// <summary>
    /// Tests IndexedPointInAreaLocator algorithms
    /// </summary>
    public class IndexedPointInAreaLocatorTest : AbstractPointInRingTest
    {

        private readonly WKTReader _reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, string wkt)
        {
            var geom = _reader.Read(wkt);
            var loc = new IndexedPointInAreaLocator(geom);
            var result = loc.Locate(pt);
            Assert.AreEqual(expectedLoc, result);
        }

        /**
         * See JTS GH Issue #19.
         * Used to infinite-loop on empty geometries.
         */
        [Test]
        public void TestEmpty()
        {
            RunPtInRing(Location.Exterior, new Coordinate(0,0), "POLYGON EMPTY");
        }
    }
}
