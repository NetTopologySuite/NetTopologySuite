using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Algorithm;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{

/**
 * Tests CGAlgorithms.computeOrientation
 * @version 1.7
 */
    [TestFixtureAttribute]
    public class OrientationIndexTest
{

  private static WKTReader reader = new WKTReader();
  //private CGAlgorithms rcga = new CGAlgorithms();
        [TestAttribute]
  public void TestCCW()
  {
    Assert.IsTrue(IsAllOrientationsEqual(GetCoordinates("LINESTRING ( 0 0, 0 1, 1 1)")));
  }
  [TestAttribute]
  public void TestCCW2()
  {
    // experimental case - can't make it fail
    Coordinate[] pts2 = {
      new Coordinate(1.0000000000004998, -7.989685402102996),
      new Coordinate(10.0, -7.004368924503866),
      new Coordinate(1.0000000000005, -7.989685402102996),
    };
    Assert.IsTrue(IsAllOrientationsEqual(pts2));
  }
  
  /**
   * Tests whether the orientations around a triangle of points
   * are all equal (as is expected if the orientation predicate is correct)
   * 
   * @param pts an array of three points
   * @return true if all the orientations around the triangle are equal
   */
  public static bool IsAllOrientationsEqual(Coordinate[] pts)
  {
    int[] orient = new int[3];
    orient[0] = RobustDeterminant.OrientationIndex(pts[0], pts[1], pts[2]);
    orient[1] = RobustDeterminant.OrientationIndex(pts[1], pts[2], pts[0]);
    orient[2] = RobustDeterminant.OrientationIndex(pts[2], pts[0], pts[1]);
    return orient[0] == orient[1] && orient[0] == orient[2];
  }
  
  public static bool IsAllOrientationsEqual(
      double p0x, double p0y,
      double p1x, double p1y,
      double p2x, double p2y)
  {
    Coordinate[] pts = {
        new Coordinate(p0x, p0y),
        new Coordinate(p1x, p1y),
        new Coordinate(p2x, p2y)
    };
    return IsAllOrientationsEqual(pts);
  }
  
  public static Coordinate[] GetCoordinates(String wkt) 
  {
    var geom = reader.Read(wkt);
    return geom.Coordinates;
  }
  

}}