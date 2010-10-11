using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class LocatePointInRingTest : AbstractPointInRingTest
    {
        protected override void RunPtInRing(Locations expectedLoc, Coordinate pt, string wkt)
        {
    IGeometry<Coordinate> geom = GeometryUtils.ReadWKT(wkt);
    Assert.AreEqual(expectedLoc, CGAlgorithms<Coordinate>.LocatePointInRing(pt, geom.Coordinates));
        }
    }
}