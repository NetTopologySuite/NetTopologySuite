using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class PointLocationOn4DLineTest : GeometryTestCase
    {
        [Test]
        public void TestOnVertex()
        {
            this.CheckOnLine(20, 20, "LINESTRINGZM (0 0 0 0, 20 20 20 20, 30 30 30 30)", true);
        }

        [Test]
        public void TestOnSegment()
        {
            this.CheckOnLine(10, 10, "LINESTRINGZM (0 0 0 0, 20 20 20 20, 0 40 40 40)", true);
            this.CheckOnLine(10, 30, "LINESTRINGZM (0 0 0 0, 20 20 20 20, 0 40 40 40)", true);
        }

        [Test]
        public void TestNotOnLine()
        {
            this.CheckOnLine(0, 100, "LINESTRINGZM (10 10 10 10, 20 10 10 10, 30 10 10 10)", false);
        }

        private void CheckOnLine(double x, double y, string wktLine, bool expected)
        {
            var line = (ILineString)this.Read(wktLine);
            Assert.That(PointLocation.IsOnLine(new Coordinate(x, y), line.Coordinates), Is.EqualTo(expected));
            Assert.That(PointLocation.IsOnLine(new Coordinate(x, y), line.CoordinateSequence), Is.EqualTo(expected));
        }
    }
}
