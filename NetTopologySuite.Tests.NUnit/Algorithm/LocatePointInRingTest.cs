using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    // Tests are exposed by AbstractPointInRingTest type
    public class LocatePointInRingTest : AbstractPointInRingTest
    {
        private WKTReader reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, string wkt)
        {
            var geom = reader.Read(wkt);
            Assert.AreEqual(expectedLoc, PointLocation.LocateInRing(pt, geom.Coordinates));
            var poly = geom as IPolygon;
            if (poly == null)
                return;

            Assert.AreEqual(expectedLoc, PointLocation.LocateInRing(pt, poly.ExteriorRing.CoordinateSequence));
        }

    }
}
