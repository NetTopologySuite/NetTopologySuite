using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryCompareToTest : GeometryTestCase
    {
        [Test]
        public void TestPoints()
        {
            checkCompareTo(-1, "POINT (0 0)", "POINT (1 0)");
            checkCompareTo(-1, "POINT (0 0)", "POINT (0 1)");
            checkCompareTo(1, "POINT (1 0)", "POINT (0 1)");
        }

        [Test]
        public void TestLines()
        {
            checkCompareTo(-1,
                "LINESTRING ( 0 0, 1 1, 0 1)",
                "LINESTRING ( 0 0, 1 1, 0 2)");
        }

        [Test]
        public void TestPolygonToPolygonWithHole()
        {
            checkCompareTo(-1, GeometryTestData.WKT_POLY, GeometryTestData.WKT_POLY_HOLE);
        }

        [Test]
        public void TestEqual()
        {
            checkCompareTo(0, GeometryTestData.WKT_POINT, GeometryTestData.WKT_POINT);
            checkCompareTo(0, GeometryTestData.WKT_LINESTRING, GeometryTestData.WKT_LINESTRING);
            checkCompareTo(0, GeometryTestData.WKT_POLY, GeometryTestData.WKT_POLY);
            checkCompareTo(0, GeometryTestData.WKT_POLY_HOLE, GeometryTestData.WKT_POLY_HOLE);
        }

        [Test]
        public void TestOrdering()
        {
            checkCompareTo(-1, GeometryTestData.WKT_POINT, GeometryTestData.WKT_MULTIPOINT);
            checkCompareTo(-1, GeometryTestData.WKT_MULTIPOINT, GeometryTestData.WKT_LINESTRING);
            checkCompareTo(-1, GeometryTestData.WKT_LINESTRING, GeometryTestData.WKT_LINEARRING);
            checkCompareTo(-1, GeometryTestData.WKT_LINEARRING, GeometryTestData.WKT_MULTILINESTRING);
            checkCompareTo(-1, GeometryTestData.WKT_MULTILINESTRING, GeometryTestData.WKT_POLY);
            checkCompareTo(-1, GeometryTestData.WKT_POLY, GeometryTestData.WKT_MULTIPOLYGON);
            checkCompareTo(-1, GeometryTestData.WKT_MULTIPOLYGON, GeometryTestData.WKT_GC);
        }

        private void checkCompareTo(int compExpected, string wkt1, string wkt2)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            int comp = g1.CompareTo(g2);
            Assert.That(comp, Is.EqualTo(compExpected));
        }
    }
}
