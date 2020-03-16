using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class CGAlgorithmsTest
    {
        [Test]
        public void TestIntersection()
        {
            var p1 = new Coordinate(413219.0849208352, 990424.3721256976);
            var p2 = new Coordinate(413217.6678330222, 990524.3620845041);
            var q1 = new Coordinate(413216.2715430226, 990404.8197292066);
            var q2 = new Coordinate(413186.72067708324, 990309.2857231029);
            var intPt = CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
            Assert.IsTrue(intPt != null);
        }

        [Test]
        public void TestOrientationIndexRobust()
        {
            var p0 = new Coordinate(219.3649559090992, 140.84159161824724);
            var p1 = new Coordinate(168.9018919682399, -5.713787599646864);
            var p = new Coordinate(186.80814046338352, 46.28973405831556);
            int orient = (int)Orientation.Index(p0, p1, p);
            int orientInv = (int)Orientation.Index(p1, p0, p);
            Assert.That(orient, Is.Not.EqualTo(orientInv));
        }
    }
}
