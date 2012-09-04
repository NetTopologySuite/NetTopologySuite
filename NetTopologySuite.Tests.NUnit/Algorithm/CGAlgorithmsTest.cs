using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{

    [TestFixture]
    public class CGAlgorithmsTest
    {
        [Test]
        public void TestDistancePointLinePerpendicular()
        {
            Assert.AreEqual(0.5, CGAlgorithms.DistancePointLinePerpendicular(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(0.5, CGAlgorithms.DistancePointLinePerpendicular(
                new Coordinate(3.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(0.707106, CGAlgorithms.DistancePointLinePerpendicular(
                new Coordinate(1, 0), new Coordinate(0, 0), new Coordinate(1, 1)), 0.000001);
        }

        [Test]
        public void TestDistancePointLine()
        {
            Assert.AreEqual(0.5, CGAlgorithms.DistancePointLine(
                new Coordinate(0.5, 0.5), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
            Assert.AreEqual(1.0, CGAlgorithms.DistancePointLine(
                new Coordinate(2, 0), new Coordinate(0, 0), new Coordinate(1, 0)), 0.000001);
        }

        [Test]
        public void TestDistanceLineLineDisjointCollinear()
        {
            Assert.AreEqual(1.999699, CGAlgorithms.DistanceLineLine(
                new Coordinate(0, 0), new Coordinate(9.9, 1.4),
                new Coordinate(11.88, 1.68), new Coordinate(21.78, 3.08)), 0.000001);
        }


    }
}