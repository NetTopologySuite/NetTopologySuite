using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    [TestFixture]
    public class LengthLocationMapTests
    {
        [Test]
        public void TestLengthAtPosition30()
        {
            var line = Read("LINESTRING (0 0, 0 100)");
            var point = Read("POINT (0 30)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(30, length);
        }

        [Test]
        public void TestLengthAtPosition50()
        {
            var line = Read("LINESTRING (0 0, 0 100)");
            var point = Read("POINT (0 50)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(50, length);
        }

        [Test]
        public void TestLengthAtPosition60()
        {
            var line = Read("LINESTRING (0 0, 0 100)");
            var point = Read("POINT (0 60)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(60, length);
        }

        [Test]
        public void TestLengthAtPosition100()
        {
            var line = Read("LINESTRING (0 0, 0 100)");
            var point = Read("POINT (0 100)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(100, length);
        }

        [Test]
        public void TestLengthAtPosition0()
        {
            var line = Read("LINESTRING (0 0, 0 100)");
            var point = Read("POINT (0 0)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(0, length);
        }

        [Test]
        public void TestMultiLineLengthPosition30()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 50, 0 100))");
            var point = Read("POINT (0 30)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(30, length);
        }

        [Test]
        public void TestMultiLineLengthPosition50()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 50, 0 100))");
            var point = Read("POINT (0 50)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(50, length);
        }

        [Test]
        public void TestMultiLineLengthPosition60()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 50, 0 100))");
            var point = Read("POINT (0 60)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(60, length);
        }

        [Test]
        public void TestMultiLineLengthAtPosition100()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 50, 0 100))");
            var point = Read("POINT (0 100)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(100, length);
        }

        [Test]
        public void TestMultiLineLengthAtPosition0()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 50, 0 100))");
            var point = Read("POINT (0 0)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(0, length);
        }

        [Test]
        public void TestMultiLineHoleLengthPosition30()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 51, 0 100))");
            var point = Read("POINT (0 30)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(30, length);
        }

        [Test]
        public void TestMultiLineHoleLengthPosition50()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 51, 0 100))");
            var point = Read("POINT (0 50)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(50, length);
        }

        [Test]
        public void TestMultiLineHoleLengthPosition60()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 51, 0 100))");
            var point = Read("POINT (0 60)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(59, length);
        }

        [Test]
        public void TestMultiLineHoleLengthAtPosition100()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 51, 0 100))");
            var point = Read("POINT (0 100)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(99, length);
        }

        [Test]
        public void TestMultiLineHoleLengthAtPosition0()
        {
            var line = Read("MULTILINESTRING((0 0, 0 50), (0 51, 0 100))");
            var point = Read("POINT (0 0)");

            var loc = LocationIndexOfPoint.IndexOf(line, point.Coordinate);
            var length = LengthLocationMap.GetLength(line, loc);
            Assert.AreEqual(0, length);
        }

        private WKTReader _reader = new WKTReader();

        protected Geometry Read(string wkt)
        {
            try
            {
                return _reader.Read(wkt);
            }
            catch (ParseException ex)
            {
                throw new ApplicationException("An exception occurred while reading the wkt", ex);
            }
        }
    }
}
