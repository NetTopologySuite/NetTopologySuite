using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    public class SegmentStringTest : GeometryTestCase
    {

        [Test]
        public void TestNextInRing()
        {
            var ss = Create("LINESTRING(0 0, 1 2, 3 1, 0 0)");
            Assert.That(ss.IsClosed, Is.True);
            CheckEqualXY(ss.NextInRing(0), new Coordinate(1, 2));
            CheckEqualXY(ss.NextInRing(1), new Coordinate(3, 1));
            CheckEqualXY(ss.NextInRing(2), new Coordinate(0, 0));
            CheckEqualXY(ss.NextInRing(3), new Coordinate(1, 2));
        }

        [Test]
        public void TestPrevInRing()
        {
            var ss = Create("LINESTRING(0 0, 1 2, 3 1, 0 0)");
            Assert.That(ss.IsClosed, Is.True);
            CheckEqualXY(ss.PrevInRing(0), new Coordinate(3, 1));
            CheckEqualXY(ss.PrevInRing(1), new Coordinate(0, 0));
            CheckEqualXY(ss.PrevInRing(2), new Coordinate(1, 2));
            CheckEqualXY(ss.PrevInRing(3), new Coordinate(3, 1));
        }

        private ISegmentString Create(string wkt)
        {
            var geom = Read(wkt);
            return new BasicSegmentString(geom.Coordinates, null);
        }

    }
}
