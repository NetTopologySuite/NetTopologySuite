using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test spatial predicate optimizations for rectangles.
     */
    [TestFixtureAttribute]
    public class RectanglePredicateTest
    {
        private WKTReader rdr = new WKTReader();
        private GeometryFactory fact = new GeometryFactory();
        [TestAttribute]
        public void TestShortAngleOnBoundary()
        {
            string[] onBoundary =
            { "POLYGON ((10 10, 30 10, 30 30, 10 30, 10 10))",
                "LINESTRING (10 25, 10 10, 25 10)" };
            RunRectanglePred(onBoundary);
        }
        [TestAttribute]
        public void TestAngleOnBoundary()
        {
            string[] onBoundary =
            { "POLYGON ((10 10, 30 10, 30 30, 10 30, 10 10))",
                "LINESTRING (10 30, 10 10, 30 10)" };
            RunRectanglePred(onBoundary);
        }
        private void RunRectanglePred(string[] wkt)
        {
            var rect = rdr.Read(wkt[0]);
            var b = rdr.Read(wkt[1]);
            RunRectanglePred(rect, b);
        }
        private void RunRectanglePred(IGeometry rect, IGeometry testGeom)
        {
            var intersectsValue = rect.Intersects(testGeom);
            var relateIntersectsValue = rect.Relate(testGeom).IsIntersects();
            var intersectsOK = intersectsValue == relateIntersectsValue;
            var containsValue = rect.Contains(testGeom);
            var relateContainsValue = rect.Relate(testGeom).IsContains();
            var containsOK = containsValue == relateContainsValue;
            ////System.Console.WriteLine(testGeom);
            //if (!intersectsOK || !containsOK)
            //    Console.WriteLine(testGeom);
            Assert.IsTrue(intersectsOK);
            Assert.IsTrue(containsOK);
        }
    }
}
