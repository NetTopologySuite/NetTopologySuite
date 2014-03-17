using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class IsCCWTest
    {
        private WKTReader reader = new WKTReader();

        [TestAttribute]
        public void TestCCW()
        {
            Coordinate[] pts = GetCoordinates("POLYGON ((60 180, 140 240, 140 240, 140 240, 200 180, 120 120, 60 180))");
            Assert.AreEqual(CGAlgorithms.IsCCW(pts), false);

            Coordinate[] pts2 = GetCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
            Assert.AreEqual(CGAlgorithms.IsCCW(pts2), true);
            // same pts list with duplicate top point - check that isCCW still works
            Coordinate[] pts2x = GetCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
            Assert.AreEqual(CGAlgorithms.IsCCW(pts2x), true);
        }

        private Coordinate[] GetCoordinates(String wkt)
        {
            IGeometry geom = reader.Read(wkt);
            return geom.Coordinates;
        }
    }
}
