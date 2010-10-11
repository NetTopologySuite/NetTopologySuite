using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
public class IsCCWTest
{


[Test]
  public void TestCCW()
  {
    Coordinate[] pts = getCoordinates("POLYGON ((60 180, 140 240, 140 240, 140 240, 200 180, 120 120, 60 180))");
    Assert.IsFalse(CGAlgorithms<Coordinate>.IsCCW(pts));

    Coordinate[] pts2 = getCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 60 180))");
    Assert.IsTrue(CGAlgorithms<Coordinate>.IsCCW(pts2));
    // same pts list with duplicate top point - check that isCCW still works
    Coordinate[] pts2x = getCoordinates("POLYGON ((60 180, 140 120, 100 180, 140 240, 140 240, 60 180))");
    Assert.IsTrue(CGAlgorithms<Coordinate>.IsCCW(pts2x));
  }

  private Coordinate[] getCoordinates(String wkt)
  {
    IGeometry<Coordinate> geom = GeometryUtils.ReadWKT(wkt);
    return geom.Coordinates.ToArray();
  }
}}