using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /// <summary>
    /// Tests <see cref="PointLocation"/>
    /// </summary>
    /// <version>1.15</version>
    public class PointLocationTest : GeometryTestCase
    {
        [Test]
        public void TestOnLineOnVertex()
        {
            CheckOnLine(20, 20, "LINESTRING (0 00, 20 20, 30 30)", true);
        }

        [Test]
        public void TestOnLineInSegment()
        {
            CheckOnLine(10, 10, "LINESTRING (0 0, 20 20, 0 40)", true);
            CheckOnLine(10, 30, "LINESTRING (0 0, 20 20, 0 40)", true);
        }

        [Test]
        public void TestNotOnLine()
        {
            CheckOnLine(0, 100, "LINESTRING (10 10, 20 10, 30 10)", false);
        }

        [Test]
        public void TestOnSegment()
        {
            CheckOnSegment(5, 5, "LINESTRING(0 0, 9 9)", true);
            CheckOnSegment(0, 0, "LINESTRING(0 0, 9 9)", true);
            CheckOnSegment(9, 9, "LINESTRING(0 0, 9 9)", true);
        }

        [Test]
        public void TestNotOnSegment()
        {
            CheckOnSegment(5, 6, "LINESTRING(0 0, 9 9)", false);
            CheckOnSegment(10, 10, "LINESTRING(0 0, 9 9)", false);
            CheckOnSegment(9, 9.00001, "LINESTRING(0 0, 9 9)", false);
        }

        [Test]
        public void TestOnZeroLengthSegment()
        {
            CheckOnSegment(1, 1, "LINESTRING(1 1, 1 1)", true);
            CheckOnSegment(1, 2, "LINESTRING(1 1, 1 1)", false);
        }

        private void CheckOnSegment(double x, double y, string wktLine, bool expected)
        {
            var line = (LineString)Read(wktLine);
            var p0 = line.GetCoordinateN(0);
            var p1 = line.GetCoordinateN(1);
            Assert.That(PointLocation.IsOnSegment(new Coordinate(x, y), p0, p1), Is.EqualTo(expected));
        }

        void CheckOnLine(double x, double y, string wktLine, bool expected)
        {
            var line = (LineString) Read(wktLine);

            Assert.AreEqual(expected, PointLocation.IsOnLine(new Coordinate(x, y), line.Coordinates));
            Assert.AreEqual(expected, PointLocation.IsOnLine(new Coordinate(x, y), line.CoordinateSequence));
        }

    }
}
