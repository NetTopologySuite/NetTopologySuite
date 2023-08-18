using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class PolygonNodeTopologyTest : GeometryTestCase
    {
        [Test]
        public void TestNonCrossing()
        {
            CheckCrossing("LINESTRING (500 1000, 1000 1000, 1000 1500)",
                "LINESTRING (1000 500, 1000 1000, 500 1500)");
        }

        [Test]
        public void TestNonCrossingQuadrant2()
        {
            CheckNonCrossing("LINESTRING (500 1000, 1000 1000, 1000 1500)",
                "LINESTRING (300 1200, 1000 1000, 500 1500)");
        }

        [Test]
        public void TestNonCrossingQuadrant4()
        {
            CheckNonCrossing("LINESTRING (500 1000, 1000 1000, 1000 1500)",
                "LINESTRING (1000 500, 1000 1000, 1500 1000)");
        }

        [Test]
        public void TestInteriorSegment()
        {
            CheckInterior("LINESTRING (5 9, 5 5, 9 5)",
                "LINESTRING (5 5, 0 0)");
        }

        [Test]
        public void TestExteriorSegment()
        {
            CheckExterior("LINESTRING (5 9, 5 5, 9 5)",
                "LINESTRING (5 5, 9 9)");
        }

        private void CheckCrossing(string wktA, string wktB)
        {
            CheckCrossing(wktA, wktB, true);
        }

        private void CheckNonCrossing(string wktA, string wktB)
        {
            CheckCrossing(wktA, wktB, false);
        }

        private void CheckCrossing(string wktA, string wktB, bool isExpectedCrossing)
        {
            var a = ReadPts(wktA);
            var b = ReadPts(wktB);
            // assert: a[1] = b[1]
            bool isCrossing = PolygonNodeTopology.IsCrossing(a[1], a[0], a[2], b[0], b[2]);
            Assert.That(isCrossing, Is.EqualTo(isExpectedCrossing));
        }

        private void CheckInterior(string wktA, string wktB)
        {
            CheckInteriorSegment(wktA, wktB, true);
        }

        private void CheckExterior(string wktA, string wktB)
        {
            CheckInteriorSegment(wktA, wktB, false);
        }

        private void CheckInteriorSegment(string wktA, string wktB, bool isExpected)
        {
            var a = ReadPts(wktA);
            var b = ReadPts(wktB);
            // assert: a[1] = b[1]
            bool isInterior = PolygonNodeTopology.IsInteriorSegment(a[1], a[0], a[2], b[1]);
            Assert.That(isInterior, Is.EqualTo(isExpected));
        }

        private Coordinate[] ReadPts(string wkt)
        {
            var line = (LineString)Read(wkt);
            return line.Coordinates;
        }
    }
}
