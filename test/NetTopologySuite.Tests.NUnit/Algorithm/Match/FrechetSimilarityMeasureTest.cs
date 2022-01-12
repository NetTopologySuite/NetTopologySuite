using NetTopologySuite.Algorithm.Match;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Match
{
    public class FrechetSimilarityMeasureTest : GeometryTestCase
    {

        [Test]
        public void TestDifferentGeometryTypesThrowIAE()
        {
            var g1 = Read("POINT(1 1)");
            var g2 = Read("LINESTRING(1 1, 2 1)");

            try
            {
                var sm = new FrechetSimilarityMeasure();
                sm.Measure(g1, g2);
                Assert.Fail("Different geometry types should fail!");
            }
            catch (Exception e)
            {
                Assert.True(true);
            }
        }

        [Test]
        public void TestEqualGeometriesReturn1()
        {
            var g1 = Read("POINT(1 1)");
            var g2 = Read("POINT(1 1)");
            Assert.AreEqual(1d, new FrechetSimilarityMeasure().Measure(g1, g2), "Point");

            g1 = Read("LINESTRING(1 1, 2 1)");
            g2 = Read("LINESTRING(1 1, 2 1)");
            Assert.AreEqual(1d, new FrechetSimilarityMeasure().Measure(g1, g2), "LineString");

            g1 = Read("POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 7.58 1, 1 7.58, 1 1))");
            g2 = Read("POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 7.58 1, 1 7.58, 1 1))");
            Assert.AreEqual(1d, new FrechetSimilarityMeasure().Measure(g1, g2), "Polygon");
        }

        [Test]
        public void TestGreaterFrechetDistanceReturnsPoorerSimilarity()
        {
            var g1 = Read("LINESTRING(1 1, 2 1.0, 3 1)");
            var g2 = Read("LINESTRING(1 1, 2 1.1, 3 1)");
            var g3 = Read("LINESTRING(1 1, 2 1.2, 3 1)");

            var sm = new FrechetSimilarityMeasure();
            double m12 = sm.Measure(g1, g2);
            double m13 = sm.Measure(g1, g3);

            Assert.True(m13 < m12, "Greater distance, poorer similarity");
        }
    }
}
