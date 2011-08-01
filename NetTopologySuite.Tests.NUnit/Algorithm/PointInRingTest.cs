using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    //Tests are exposed by AbstractPointInRingTest type
    public class PointInRingTest : AbstractPointInRingTest
    {
        private WKTReader reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, String wkt)
        {
            // isPointInRing is not defined for pts on boundary
            if (expectedLoc == Location.Boundary)
  	            return;
  	 
            IGeometry geom = reader.Read(wkt);
            bool expected = expectedLoc == Location.Interior;
            Assert.AreEqual(expected, CGAlgorithms.IsPointInRing(pt, geom.Coordinates));
        }

    }
}
