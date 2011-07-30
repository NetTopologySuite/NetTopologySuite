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

        protected override void RunPtInRing(Locations expectedLoc, Coordinate pt, String wkt)
        {
            // isPointInRing is not defined for pts on boundary
            if (expectedLoc == Locations.Boundary)
  	            return;
  	 
            IGeometry geom = reader.Read(wkt);
            bool expected = expectedLoc == Locations.Interior;
            Assert.AreEqual(expected, CGAlgorithms.IsPointInRing(pt, geom.Coordinates));
        }

    }
}
