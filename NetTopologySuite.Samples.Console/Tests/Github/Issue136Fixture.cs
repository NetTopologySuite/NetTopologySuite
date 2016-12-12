using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue136Fixture
    {
        [Test]
        public void angle_value_ignores_direction()
        {
            LineString lineString1 = new LineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2) });
            LineString lineString2 = new LineString(new[] { new Coordinate(2, 2), new Coordinate(1, 1) });
            Assert.AreEqual(lineString1.Angle, lineString2.Angle);
        }
    }
}