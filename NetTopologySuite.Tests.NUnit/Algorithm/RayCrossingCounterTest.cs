using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    //Tests are exposed by AbstractPointInRingTest type
    public class RayCrossingCounterTest : AbstractPointInRingTest
    {
        private WKTReader reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, String wkt)
        {
            IGeometry geom = reader.Read(wkt);
            Assert.AreEqual(expectedLoc, RayCrossingCounter.LocatePointInRing(pt, geom.Coordinates));
        }

    }
}
