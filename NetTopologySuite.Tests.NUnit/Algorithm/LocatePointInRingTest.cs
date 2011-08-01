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
            IGeometry geom = reader.Read(wkt);
            Assert.AreEqual(expectedLoc, CGAlgorithms.LocatePointInRing(pt, geom.Coordinates));

        }

    }
}
