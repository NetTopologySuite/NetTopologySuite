using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    public class AreaNodeTest : GeometryTestCase
    {


        [Test]
        public void TestCrossing()
        {
            CheckValid("LINESTRING (500 1000, 1000 1000, 1000 1500)",
                "LINESTRING (1000 500, 1000 1000, 500 1500)", false);
        }

        [Test]
        public void TestValidQuadrant2()
        {
            CheckValid("LINESTRING (500 1000, 1000 1000, 1000 1500)",
                "LINESTRING (300 1200, 1000 1000, 500 1500)");
        }

        [Test]
        public void TestValidQuadrant4()
        {
            CheckValid("LINESTRING (500 1000, 1000 1000, 1000 1500)",
                "LINESTRING (1000 500, 1000 1000, 1500 1000)");
        }

        private void CheckValid(string wktA, string wktB)
        {
            CheckValid(wktA, wktB, true);
        }

        private void CheckValid(string wktA, string wktB, bool isExpectedValid)
        {
            var a = ReadPts(wktA);
            var b = ReadPts(wktB);
            // assert: a[1] = b[1]
            bool isValid = !AreaNode.IsCrossing(a[1], a[0], a[2], b[0], b[2]);
            Assert.That(isValid, Is.EqualTo(isExpectedValid));
        }

        private Coordinate[] ReadPts(string wkt)
        {
            var line = (LineString) Read(wkt);
            return line.Coordinates;
        }
    }
}
