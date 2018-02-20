using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class CGAlgorithmsTest
    {
        [Test]
        public void TestDistancePointLinePerpendicular()
        {
            Assert.AreEqual(0.5, CGAlgorithms.DistancePointLinePerpendicular(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(0.5, CGAlgorithms.DistancePointLinePerpendicular(
                new Coordinate(3.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(0.707106, CGAlgorithms.DistancePointLinePerpendicular(
                new Coordinate(1, 0), new Coordinate(0, 0), new Coordinate(1, 1)), 0.000001);
        }

        [Test]
        public void TestDistancePointLine()
        {
            Assert.AreEqual(0.5, CGAlgorithms.DistancePointLine(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(1.0, CGAlgorithms.DistancePointLine(
                new Coordinate(2, 0), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
        }

        [Test]
        public void TestDistanceLineLineDisjointCollinear()
        {
            Assert.AreEqual(1.999699, CGAlgorithms.DistanceLineLine(
                new Coordinate(0, 0), new Coordinate(9.9, 1.4),
                new Coordinate(11.88, 1.68), new Coordinate(21.78, 3.08)), 0.000001);
        }

        [Test]
        public void TestOrientationIndexRobust()
        {
            Coordinate p0 = new Coordinate(219.3649559090992, 140.84159161824724);
            Coordinate p1 = new Coordinate(168.9018919682399, -5.713787599646864);
            Coordinate p = new Coordinate(186.80814046338352, 46.28973405831556);
            int orient = (int)OrientationFunctions.Index(p0, p1, p);
            int orientInv = (int)OrientationFunctions.Index(p1, p0, p);
            Assert.That(orient, Is.Not.EqualTo(orientInv));
        }
    }
}