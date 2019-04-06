using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
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

        public void TestDistancePointLine()
        {
            Assert.AreEqual(0.5, DistanceComputer.PointToSegment(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(1.0, DistanceComputer.PointToSegment(
                new Coordinate(2, 0), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
        }

        public void TestDistanceLineLineDisjointCollinear()
        {
            Assert.AreEqual(1.999699, DistanceComputer.SegmentToSegment(
                new Coordinate(0, 0), new Coordinate(9.9, 1.4),
                new Coordinate(11.88, 1.68), new Coordinate(21.78, 3.08)), 0.000001);
        }
    }
}