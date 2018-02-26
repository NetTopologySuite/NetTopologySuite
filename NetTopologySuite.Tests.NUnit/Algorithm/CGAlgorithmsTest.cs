using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class CGAlgorithmsTest
    {
        [Test]
        public void TestOrientationIndexRobust()
        {
            Coordinate p0 = new Coordinate(219.3649559090992, 140.84159161824724);
            Coordinate p1 = new Coordinate(168.9018919682399, -5.713787599646864);
            Coordinate p = new Coordinate(186.80814046338352, 46.28973405831556);
            int orient = (int)Orientation.Index(p0, p1, p);
            int orientInv = (int)Orientation.Index(p1, p0, p);
            Assert.That(orient, Is.Not.EqualTo(orientInv));
        }
    }
}