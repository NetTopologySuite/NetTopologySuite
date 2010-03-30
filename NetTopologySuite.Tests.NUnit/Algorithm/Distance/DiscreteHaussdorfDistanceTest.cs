using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Distance;
using NUnit.Framework;

using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Distance
{
    [TestFixture]
    public class DiscreteHaussdorfDistanceTest
    {

  [Test]
        public void TestLineSegments()
  {
    runTest("LINESTRING (0 0, 2 1)", "LINESTRING (0 0, 2 0)", 1.0);
  }
  
  [Test]
  public void TestLineSegments2() 
  {
    runTest("LINESTRING (0 0, 2 0)", "LINESTRING (0 1, 1 2, 2 1)", 2.0);
  }
  
  [Test]
  public void TestLinePoints() 
  {
    runTest("LINESTRING (0 0, 2 0)", "MULTIPOINT (0 1, 1 0, 2 1)", 1.0);
  }
  
  /**
   * Shows effects of limiting HD to vertices
   * Answer is not true Hausdorff distance.
   * 
   * @throws Exception
   */
  [Test]
  public void TestLinesShowingDiscretenessEffect() 
  {
    runTest("LINESTRING (130 0, 0 0, 0 150)", "LINESTRING (10 10, 10 150, 130 10)", 14.142135623730951);
    // densifying provides accurate HD
    runTest("LINESTRING (130 0, 0 0, 0 150)", "LINESTRING (10 10, 10 150, 130 10)", 0.5, 70.0);
  }
  
  private const double TOLERANCE = 0.00001;
  
  private void runTest(String wkt1, String wkt2, double expectedDistance) 
  {
    IGeometry<Coord> g1 = GeometryUtils.ReadWKT(wkt1);
    IGeometry<Coord> g2 = GeometryUtils.ReadWKT(wkt2);
    
    double distance = DiscreteHausdorffDistance<Coord>.Distance(g1, g2);
    Assert.AreEqual(distance, expectedDistance, TOLERANCE);
  }
  private void runTest(String wkt1, String wkt2, double densifyFrac, double expectedDistance) 
  {
    IGeometry<Coord> g1 = GeometryUtils.ReadWKT(wkt1);
    IGeometry<Coord> g2 = GeometryUtils.ReadWKT(wkt2);
    
    double distance = DiscreteHausdorffDistance<Coord>.Distance(g1, g2, densifyFrac);
    Assert.AreEqual(distance, expectedDistance, TOLERANCE);
  }
    }
}