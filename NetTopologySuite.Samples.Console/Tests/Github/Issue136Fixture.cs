using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;
namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue136Fixture
    {
        [Test]
        public void linestring_angle_value_ignores_orientation()
        {
            var lineString1 = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) });
            var lineString2 = new LineString(new[] { new Coordinate(2, 2), new Coordinate(1, 1) });
            var angle1 = lineString1.Angle;
            var angle2 = lineString2.Angle;
            Assert.AreEqual(angle1, angle2);
            Assert.IsTrue(Math.Abs(angle1 - 45d) < 0.001);
            Assert.IsTrue(Math.Abs(angle2 - 45d) < 0.001);
        }
        [Test]
        public void angle_utility_handles_orientation()
        {
            var lineString1 = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) });
            var lineString2 = new LineString(new[] { new Coordinate(2, 2), new Coordinate(1, 1) });
            var angle1 = AngleUtility.ToDegrees(
                AngleUtility.Angle(lineString1.StartPoint.Coordinate, lineString1.EndPoint.Coordinate));
            var angle2 = AngleUtility.ToDegrees(
                AngleUtility.Angle(lineString2.StartPoint.Coordinate, lineString2.EndPoint.Coordinate));
            Assert.AreEqual(45d, angle1);
            Assert.AreEqual(-135d, angle2);
        }
        [Test]
        public void linesegment_handles_orientation()
        {
            var lineSegment1 = new LineSegment(new Coordinate(1, 1), new Coordinate(2, 2));
            var lineSegment2 = new LineSegment(new Coordinate(2, 2), new Coordinate(1, 1));
            var angle1 = AngleUtility.ToDegrees(lineSegment1.Angle);
            var angle2 = AngleUtility.ToDegrees(lineSegment2.Angle);
            Assert.AreEqual(45d, angle1);
            Assert.AreEqual(-135d, angle2);
        }
    }
}
