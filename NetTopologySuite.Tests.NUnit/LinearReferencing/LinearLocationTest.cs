using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.LinearReferencing
{
    /// <summary>
    /// Tests methods involving only <see cref="LinearLocation" />s
    /// </summary>
    /// <author>Martin Davis</author>
    [TestFixtureAttribute]
    public class LinearLocationTest
    {
        private WKTReader reader = new WKTReader();

        [TestAttribute]
        public void TestZeroLengthLineString()
        {
            var line = reader.Read("LINESTRING (10 0, 10 0)");
            var indexedLine = new LocationIndexedLine(line);
            var loc0 = indexedLine.IndexOf(new Coordinate(11, 0));
            Assert.IsTrue(loc0.CompareTo(new LinearLocation(0, Double.NaN)) == 0);
        }

        [TestAttribute]
        public void TestRepeatedCoordsLineString()
        {
            var line = reader.Read("LINESTRING (10 0, 10 0, 20 0)");
            var indexedLine = new LocationIndexedLine(line);
            var loc0 = indexedLine.IndexOf(new Coordinate(11, 0));
            Assert.IsTrue(loc0.CompareTo(new LinearLocation(1, 0.1)) == 0);
        }


        [TestAttribute]
        public void TestSameSegmentLineString()
        {
            IGeometry line = reader.Read("LINESTRING (0 0, 10 0, 20 0, 30 0)");
            LocationIndexedLine indexedLine = new LocationIndexedLine(line);

            LinearLocation loc0 = indexedLine.IndexOf(new Coordinate(0, 0));
            LinearLocation loc0_5 = indexedLine.IndexOf(new Coordinate(5, 0));
            LinearLocation loc1 = indexedLine.IndexOf(new Coordinate(10, 0));
            LinearLocation loc2 = indexedLine.IndexOf(new Coordinate(20, 0));
            LinearLocation loc2_5 = indexedLine.IndexOf(new Coordinate(25, 0));
            LinearLocation loc3 = indexedLine.IndexOf(new Coordinate(30, 0));

            Assert.IsTrue(loc0.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0.IsOnSameSegment(loc0_5));
            Assert.IsTrue(loc0.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2_5));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc3));

            Assert.IsTrue(loc0_5.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0_5.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc2.IsOnSameSegment(loc0));
            Assert.IsTrue(loc2.IsOnSameSegment(loc1));
            Assert.IsTrue(loc2.IsOnSameSegment(loc2));
            Assert.IsTrue(loc2.IsOnSameSegment(loc3));

            Assert.IsTrue(loc2_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc3.IsOnSameSegment(loc0));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2_5));
            Assert.IsTrue(loc3.IsOnSameSegment(loc3));
        }

        [TestAttribute]
        public void TestSameSegmentMultiLineString()
        {
            IGeometry line = reader.Read("MULTILINESTRING ((0 0, 10 0, 20 0), (20 0, 30 0))");
            LocationIndexedLine indexedLine = new LocationIndexedLine(line);

            LinearLocation loc0 = indexedLine.IndexOf(new Coordinate(0, 0));
            LinearLocation loc0_5 = indexedLine.IndexOf(new Coordinate(5, 0));
            LinearLocation loc1 = indexedLine.IndexOf(new Coordinate(10, 0));
            LinearLocation loc2 = indexedLine.IndexOf(new Coordinate(20, 0));
            LinearLocation loc2B = new LinearLocation(1, 0, 0.0);

            LinearLocation loc2_5 = indexedLine.IndexOf(new Coordinate(25, 0));
            LinearLocation loc3 = indexedLine.IndexOf(new Coordinate(30, 0));

            Assert.IsTrue(loc0.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0.IsOnSameSegment(loc0_5));
            Assert.IsTrue(loc0.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc2_5));
            Assert.IsTrue(!loc0.IsOnSameSegment(loc3));

            Assert.IsTrue(loc0_5.IsOnSameSegment(loc0));
            Assert.IsTrue(loc0_5.IsOnSameSegment(loc1));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc0_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc2.IsOnSameSegment(loc0));
            Assert.IsTrue(loc2.IsOnSameSegment(loc1));
            Assert.IsTrue(loc2.IsOnSameSegment(loc2));
            Assert.IsTrue(!loc2.IsOnSameSegment(loc3));
            Assert.IsTrue(loc2B.IsOnSameSegment(loc3));

            Assert.IsTrue(loc2_5.IsOnSameSegment(loc3));

            Assert.IsTrue(!loc3.IsOnSameSegment(loc0));
            Assert.IsTrue(!loc3.IsOnSameSegment(loc2));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2B));
            Assert.IsTrue(loc3.IsOnSameSegment(loc2_5));
            Assert.IsTrue(loc3.IsOnSameSegment(loc3));
        }

        [TestAttribute]
        public void TestGetSegmentMultiLineString()
        {
            IGeometry line = reader.Read("MULTILINESTRING ((0 0, 10 0, 20 0), (20 0, 30 0))");
            LocationIndexedLine indexedLine = new LocationIndexedLine(line);

            LinearLocation loc0 = indexedLine.IndexOf(new Coordinate(0, 0));
            LinearLocation loc0_5 = indexedLine.IndexOf(new Coordinate(5, 0));
            LinearLocation loc1 = indexedLine.IndexOf(new Coordinate(10, 0));
            LinearLocation loc2 = indexedLine.IndexOf(new Coordinate(20, 0));
            LinearLocation loc2B = new LinearLocation(1, 0, 0.0);

            LinearLocation loc2_5 = indexedLine.IndexOf(new Coordinate(25, 0));
            LinearLocation loc3 = indexedLine.IndexOf(new Coordinate(30, 0));

            LineSegment seg0 = new LineSegment(new Coordinate(0, 0), new Coordinate(10, 0));
            LineSegment seg1 = new LineSegment(new Coordinate(10, 0), new Coordinate(20, 0));
            LineSegment seg2 = new LineSegment(new Coordinate(20, 0), new Coordinate(30, 0));

            Assert.IsTrue(loc0.GetSegment(line).Equals(seg0));
            Assert.IsTrue(loc0_5.GetSegment(line).Equals(seg0));

            Assert.IsTrue(loc1.GetSegment(line).Equals(seg1));
            Assert.IsTrue(loc2.GetSegment(line).Equals(seg1));

            Assert.IsTrue(loc2_5.GetSegment(line).Equals(seg2));
            Assert.IsTrue(loc3.GetSegment(line).Equals(seg2));
        }
    }
}
