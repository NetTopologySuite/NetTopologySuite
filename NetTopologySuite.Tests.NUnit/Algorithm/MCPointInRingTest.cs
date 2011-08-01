using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    //Tests are exposed by AbstractPointInRingTest type
    public class MCPointInRingTest : AbstractPointInRingTest
    {
        private WKTReader reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, String wkt)
        {
  	        // isPointInRing is not defined for pts on boundary
  	        if (expectedLoc == Location.Boundary)
  		        return;
  	 
            IGeometry geom = reader.Read(wkt);
            if (!(geom is Polygon))
                return;
    
            LinearRing ring = (LinearRing)((Polygon) geom).ExteriorRing;
            bool expected = expectedLoc == Location.Interior;
            MCPointInRing pir = new MCPointInRing(ring);
            bool result = pir.IsInside(pt);
            Assert.AreEqual(expected, result);
        }

    }
}
