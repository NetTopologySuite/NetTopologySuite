using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    [TestFixture]
    public class LengthLocationMapTests : GeometryTestCase
    {
        [Test]
        public void TestLengthAtPosition30()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 30)";

            CheckLlm(line, point, 30);
        }

        [Test]
        public void TestLengthAtPosition50()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 50)";

            CheckLlm(line, point, 50);
        }

        [Test]
        public void TestLengthAtPosition60()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 60)";

            CheckLlm(line, point, 60);
        }

        [Test]
        public void TestLengthAtPosition100()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 100)";

            CheckLlm(line, point, 100);
        }

        [Test]
        public void TestLengthAtPosition101()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 100)";

            CheckLlm(line, point, 100);
        }

        [Test]
        public void TestLengthAtPosition0()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 0)";

            CheckLlm(line, point, 0);
        }


        [Test]
        public void TestLengthAtPositionMinus1()
        {
            string line = "LINESTRING (0 0, 0 100)";
            string point = "POINT (0 -1)";

            CheckLlm(line, point, 0);
        }

        [Test]
        public void TestMultiLineLengthPosition30()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 30)";

            CheckLlm(line, point, 30);
        }

        [Test]
        public void TestMultiLineLengthPosition50()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 50)";

            CheckLlm(line, point, 50);
        }

        [Test]
        public void TestMultiLineLengthPosition60()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 60)";

            CheckLlm(line, point, 60);
        }

        [Test]
        public void TestMultiLineLengthAtPosition100()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 100)";

            CheckLlm(line, point, 100);
        }

        [Test]
        public void TestMultiLineLengthAtPosition101()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 101)";

            CheckLlm(line, point, 100);
        }

        [Test]
        public void TestMultiLineLengthAtPosition0()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 0)";

            CheckLlm(line, point, 0);
        }

        [Test]
        public void TestMultiLineLengthAtPositionMinus1()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 50, 0 100))";
            string point = "POINT (0 -1)";

            CheckLlm(line, point, 0);
        }

        [Test]
        public void TestMultiLineHoleLengthPosition30()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 30)";

            CheckLlm(line, point, 30);
        }

        [Test]
        public void TestMultiLineHoleLengthPosition50()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 50)";

            CheckLlm(line, point, 50);
        }

        [Test]
        public void TestMultiLineHoleLengthPosition60()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 60)";

            CheckLlm(line, point, 59);
        }

        [Test]
        public void TestMultiLineHoleLengthAtPosition100()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 100)";

            CheckLlm(line, point, 99);
        }

        [Test]
        public void TestMultiLineHoleLengthAtPosition101()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 101)";

            CheckLlm(line, point, 99);
        }

        [Test]
        public void TestMultiLineHoleLengthAtPosition0()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 0)";

            CheckLlm(line, point, 0);
        }


        [Test]
        public void TestMultiLineHoleLengthAtPositionMinus1()
        {
            string line = "MULTILINESTRING((0 0, 0 50), (0 51, 0 100))";
            string point = "POINT (0 -1)";

            CheckLlm(line, point, 0);
        }

        private void CheckLlm(string wkt0, string wkt1, double expectedDistance)
        {
            var lineal = (ILineal)Read(wkt0);
            var point = (Point)Read(wkt1);
            if (lineal is LineString line)
                CheckLlm(line, point, expectedDistance);
            else
                CheckLlm((MultiLineString)lineal, point, expectedDistance);
        }

        private void CheckLlm(LineString geom0, Point geom1, double expectedDistance)
        {
            var loc = LocationIndexOfPoint.IndexOf(geom0, geom1.Coordinate);
            Assert.That(LengthLocationMap.GetLength(geom0, loc), Is.EqualTo(expectedDistance));
        }

        private void CheckLlm(MultiLineString geom0, Point geom1, double expectedDistance)
        {
            var loc = LocationIndexOfPoint.IndexOf(geom0, geom1.Coordinate);
            Assert.That(LengthLocationMap.GetLength(geom0, loc), Is.EqualTo(expectedDistance));
        }
    }
}
