using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class PointLocatorTest
    {
        private WKTReader reader = new WKTReader();

        [Test]
        public void TestBox()
        {
            RunPtLocator(Locations.Interior, new Coordinate(10, 10),
                "POLYGON ((0 0, 0 20, 20 20, 20 0, 0 0))");
        }

        [Test]
        public void TestComplexRing()
        {
            RunPtLocator(Locations.Interior, new Coordinate(0, 0),
                "POLYGON ((-40 80, -40 -80, 20 0, 20 -100, 40 40, 80 -80, 100 80, 140 -20, 120 140, 40 180,     60 40, 0 120, -20 -20, -40 80))");
        }

        [Test]
        public void TestPointLocatorLinearRingLineString()
        {
            RunPtLocator(Locations.Boundary, new Coordinate(0, 0),
                "GEOMETRYCOLLECTION( LINESTRING(0 0, 10 10), LINEARRING(10 10, 10 20, 20 10, 10 10))");
        }

        [Test]
        public void TestPointLocatorPointInsideLinearRing()
        {
            RunPtLocator(Locations.Exterior, new Coordinate(11, 11),
                "LINEARRING(10 10, 10 20, 20 10, 10 10)");
        }

        private void RunPtLocator(Locations expected, ICoordinate pt, String wkt)
        {
            IGeometry geom = reader.Read(wkt);
            PointLocator pointLocator = new PointLocator();
            Locations loc = pointLocator.Locate(pt, geom);
            Assert.AreEqual(expected, loc);
        }
    }
}