using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.Operation.Relate
{
public class ContainsTest
{
  private readonly WKTReader _reader = new WKTReader(new GeometryFactory());

  /**
   * From GEOS #572
   * 
   * The cause is that the longer line nodes the single-segment line.
   * The node then tests as not lying precisely on the original longer line.
   * 
   * The solution is to change the relate algorithm so that it never computes
   * new intersection points, only ones which occur at existing vertices.
   * (The topology of the implicit intersections can still be computed
   * to contribute to the intersection matrix result).
   * This will require a complete reworking of the relate algorithm. 
   */
    [Test, Ignore("Known to fail")]
    public void TestContainsIncorrect()
    {
        var a = _reader.Read("LINESTRING (1 0, 0 2, 0 0, 2 2)");
        var b = _reader.Read("LINESTRING (0 0, 2 2)");

        // actual matrix is 001F001F2
        Assert.IsTrue(a.Contains(b));
    }
}}