#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test spatial predicate optimizations for rectangles.
     */
    [TestFixture]
    public class RectanglePredicateTest
    {
        private WKTReader rdr = new WKTReader();
        private GeometryFactory fact = new GeometryFactory();

        [Test]
        public void TestShortAngleOnBoundary()
        {
            string[] onBoundary =
            { "POLYGON ((10 10, 30 10, 30 30, 10 30, 10 10))",
                "LINESTRING (10 25, 10 10, 25 10)" };
            RunRectanglePred(onBoundary);
        }

        [Test]
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

        private void RunRectanglePred(Geometry rect, Geometry testGeom)
        {
            bool intersectsValue = rect.Intersects(testGeom);
            bool relateIntersectsValue = rect.Relate(testGeom).IsIntersects();
            bool intersectsOK = intersectsValue == relateIntersectsValue;

            bool containsValue = rect.Contains(testGeom);
            bool relateContainsValue = rect.Relate(testGeom).IsContains();
            bool containsOK = containsValue == relateContainsValue;

            ////System.Console.WriteLine(testGeom);
            //if (!intersectsOK || !containsOK)
            //    Console.WriteLine(testGeom);

            Assert.IsTrue(intersectsOK);
            Assert.IsTrue(containsOK);
        }
    }
}