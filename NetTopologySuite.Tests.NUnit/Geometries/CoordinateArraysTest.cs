using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class CoordinateArraysTest
    {
        [Test]
        public void TestPtNotInList1()
        {
            Assert.IsTrue(CoordinateArrays.PointNotInList(
                new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new Coordinate[] { new Coordinate(1, 1), new Coordinate(1, 2), new Coordinate(1, 3) }
                ).Equals2D(new Coordinate(2, 2))
                );
        }

        [Test]
        public void TestPtNotInList2()
        {
            Assert.IsTrue(CoordinateArrays.PointNotInList(
                new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new Coordinate[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) }
                ) == null
                );
        }
    }
}