using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class DistanceComputerTest : GeometryTestCase
    {
        [Test]
        public void TestDistancePointLinePerpendicular()
        {
            Assert.AreEqual(0.5, DistanceComputer.PointToLinePerpendicular(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(0.5, DistanceComputer.PointToLinePerpendicular(
                new Coordinate(3.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(0.707106, DistanceComputer.PointToLinePerpendicular(
                new Coordinate(1, 0), new Coordinate(0, 0), new Coordinate(1, 1)), 0.000001);
        }

        [Test]
        public void TestDistancePointLine()
        {
            Assert.AreEqual(0.5, DistanceComputer.PointToSegment(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(1.0, DistanceComputer.PointToSegment(
                new Coordinate(2, 0), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
        }

        [Test]
        public void TestDistanceLineLineDisjointCollinear()
        {
            Assert.AreEqual(1.999699, DistanceComputer.SegmentToSegment(
                new Coordinate(0, 0), new Coordinate(9.9, 1.4),
                new Coordinate(11.88, 1.68), new Coordinate(21.78, 3.08)), 0.000001);
        }

        [Test]
        public void TestDistancePointToSegmentStringConsistency()
        {
            Coordinate[] coords =
            {
                new Coordinate(24824.045318333192,38536.15071012041),
                new Coordinate(26157.378651666528,37567.42733944659),
                new Coordinate(26666.666666666668,36000.0),
                new Coordinate(26157.378651666528,34432.57266055341),
                new Coordinate(24824.045318333192,33463.84928987959),
                new Coordinate(23175.954681666804,33463.84928987959),
            };

            var pt = new Coordinate(21842.621348333472, 34432.57266055341);

            double dist1 = DistanceComputer.PointToSegmentString(pt, coords);
            double dist2 = DistanceComputer.PointToSegmentString(pt, new CoordinateArraySequence(coords));
            Assert.That(dist1, Is.EqualTo(dist2));
        }
    }
}
