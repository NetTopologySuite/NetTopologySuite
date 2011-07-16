using System;
using NetTopologySuite.Algorithm;
using GeoAPI.Geometries;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    
    public class PointInRingTest : AbstractPointInRingTest
    {
        protected override void RunPtInRing(Locations expectedLoc, Coord pt, string wkt)
        {
            // isPointInRing is not defined for pts on boundary
            if (expectedLoc == Locations.Boundary)
                return;

            IGeometry<Coord> geom = GeometryUtils.Reader.Read(wkt);
            Boolean expected = expectedLoc == Locations.Interior;
            Assert.AreEqual(expected, CGAlgorithms<Coord>.IsPointInRing(pt, geom.Coordinates));
        }
    }
}