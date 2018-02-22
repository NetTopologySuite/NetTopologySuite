using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /// <summary>
    /// Tests <see cref="PointLocation.IsOnLine(Coordinate, Coordinate[])"/> and
    /// <see cref="PointLocation.IsOnLine(Coordinate, ICoordinateSequence)"/>
    /// </summary>
    /// <version>1.15</version>
    public class PointLocationOnLineTest : GeometryTestCase
    {
        [Test]
        public void TestOnVertex()
        {
            CheckOnLine(20, 20, "LINESTRING (0 00, 20 20, 30 30)", true);
        }

        [Test]
        public void TestOnSegment()
        {
            CheckOnLine(10, 10, "LINESTRING (0 0, 20 20, 0 40)", true);
            CheckOnLine(10, 30, "LINESTRING (0 0, 20 20, 0 40)", true);
        }

        [Test]
        public void TestNotOnLine()
        {
            CheckOnLine(0, 100, "LINESTRING (10 10, 20 10, 30 10)", false);
        }

        void CheckOnLine(double x, double y, String wktLine, bool expected)
        {
            var line = (ILineString) Read(wktLine);

            Assert.AreEqual(expected, PointLocation.IsOnLine(new Coordinate(x, y), line.Coordinates));
            Assert.AreEqual(expected, PointLocation.IsOnLine(new Coordinate(x, y), line.CoordinateSequence));
        }

    }
}