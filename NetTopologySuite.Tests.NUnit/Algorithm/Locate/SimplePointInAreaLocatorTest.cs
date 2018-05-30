using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Locate
{
    /// <summary>
    /// Tests IndexedPointInAreaLocator algorithms
    /// </summary>
    public class SimplePointInAreaLocatorTest : AbstractPointInRingTest
    {

        private readonly WKTReader _reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, String wkt)
        {
            var geom = _reader.Read(wkt);
            var loc = new SimplePointInAreaLocator(geom);
            var result = loc.Locate(pt);
            Assert.AreEqual(expectedLoc, result);
        }

    }
}