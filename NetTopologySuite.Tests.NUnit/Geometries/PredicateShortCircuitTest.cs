using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test named predicate short-circuits
     */
    [TestFixtureAttribute]
    public class PredicateShortCircuitTest
    {
        WKTReader rdr = new WKTReader();

        String[] polyInsidePoly =
        { "POLYGON (( 0 0, 100 0, 100 100, 0 100, 0 0 ))",
          "POLYGON (( 10 10, 90 10, 90 90, 10 90, 10 10 ))" };
        String[] polyPartiallyOverlapsPoly =
        { "POLYGON (( 10 10, 100 10, 100 100, 10 100, 10 10 ))",
          "POLYGON (( 0 0, 90 0, 90 90, 0 90, 0 0 ))" };
        String[] polyTouchesPolyAtPoint =
        { "POLYGON (( 10 10, 100 10, 100 100, 10 100, 10 10 ))",
          "POLYGON (( 0 0, 10 0, 10 10, 0 10, 0 0 ))" };
        String[] polyTouchesPolyAtLine =
        { "POLYGON (( 10 10, 100 10, 100 100, 10 100, 10 10 ))",
          "POLYGON (( 10 0, 10 10, 20 10, 20 0, 10 0 ))" };
        String[] polyInsideHoleInPoly =
        { "POLYGON (( 40 40, 40 60, 60 60, 60 40, 40 40 ))",
          "POLYGON (( 0 0, 100 0, 100 100, 0 100, 0 0), ( 10 10, 90 10, 90 90, 10 90, 10 10))" };

        [TestAttribute]
        public void TestAll()
        {
            DoPredicates(polyInsidePoly);
            DoPredicates(polyPartiallyOverlapsPoly);
            DoPredicates(polyTouchesPolyAtPoint);
            DoPredicates(polyTouchesPolyAtLine);
            DoPredicates(polyInsideHoleInPoly);
        }

        public void DoPredicates(String[] wkt)
        {
            IGeometry a = rdr.Read(wkt[0]);
            IGeometry b = rdr.Read(wkt[1]);
            DoPredicates(a, b);
            DoPredicates(b, a);
        }

        public void DoPredicates(IGeometry a, IGeometry b)
        {
            Assert.IsTrue(a.Contains(b) == a.Relate(b).IsContains());
            Assert.IsTrue(a.Crosses(b) == a.Relate(b).IsCrosses(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Disjoint(b) == a.Relate(b).IsDisjoint());
            Assert.IsTrue(a.Equals(b) == a.Relate(b).IsEquals(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Intersects(b) == a.Relate(b).IsIntersects());
            Assert.IsTrue(a.Overlaps(b) == a.Relate(b).IsOverlaps(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Touches(b) == a.Relate(b).IsTouches(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Within(b) == a.Relate(b).IsWithin());
        }
    }
}