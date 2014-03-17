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
            String[] onBoundary =
            { "POLYGON ((10 10, 30 10, 30 30, 10 30, 10 10))",
                "LINESTRING (10 25, 10 10, 25 10)" };
            RunRectanglePred(onBoundary);
        }

        [TestAttribute]
        public void TestAngleOnBoundary()
        {
            String[] onBoundary =
            { "POLYGON ((10 10, 30 10, 30 30, 10 30, 10 10))",
                "LINESTRING (10 30, 10 10, 30 10)" };
            RunRectanglePred(onBoundary);
        }

        private void RunRectanglePred(String[] wkt)
        {
            IGeometry rect = rdr.Read(wkt[0]);
            IGeometry b = rdr.Read(wkt[1]);
            RunRectanglePred(rect, b);
        }

        private void RunRectanglePred(IGeometry rect, IGeometry testGeom)
        {
            bool intersectsValue = rect.Intersects(testGeom);
            bool relateIntersectsValue = rect.Relate(testGeom).IsIntersects();
            bool intersectsOK = intersectsValue == relateIntersectsValue;

            bool containsValue = rect.Contains(testGeom);
            bool relateContainsValue = rect.Relate(testGeom).IsContains();
            bool containsOK = containsValue == relateContainsValue;

            //System.out.println(testGeom);
            if (!intersectsOK || !containsOK)
            {
                Console.WriteLine(testGeom);
            }
            Assert.IsTrue(intersectsOK);
            Assert.IsTrue(containsOK);
        }
    }
}