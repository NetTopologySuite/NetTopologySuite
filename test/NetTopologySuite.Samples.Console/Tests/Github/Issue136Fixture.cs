#nullable disable
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue136Fixture
    {
        [Test]
        public void angle_utility_handles_orientation()
        {
            var lineString1 = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) });
            var lineString2 = new LineString(new[] { new Coordinate(2, 2), new Coordinate(1, 1) });
            double angle1 = AngleUtility.ToDegrees(
                AngleUtility.Angle(lineString1.StartPoint.Coordinate, lineString1.EndPoint.Coordinate));
            double angle2 = AngleUtility.ToDegrees(
                AngleUtility.Angle(lineString2.StartPoint.Coordinate, lineString2.EndPoint.Coordinate));
            Assert.AreEqual(45d, angle1);
            Assert.AreEqual(-135d, angle2);
        }

        [Test]
        public void linesegment_handles_orientation()
        {
            var lineSegment1 = new LineSegment(new Coordinate(1, 1), new Coordinate(2, 2));
            var lineSegment2 = new LineSegment(new Coordinate(2, 2), new Coordinate(1, 1));
            double angle1 = AngleUtility.ToDegrees(lineSegment1.Angle);
            double angle2 = AngleUtility.ToDegrees(lineSegment2.Angle);
            Assert.AreEqual(45d, angle1);
            Assert.AreEqual(-135d, angle2);
        }
    }
}
