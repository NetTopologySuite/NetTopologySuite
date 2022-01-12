using NetTopologySuite.Algorithm.Match;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Match
{
    public class HausdorffSimilarityMeasureTest : GeometryTestCase
    {

        [Test]
        public void TestEqualGeometriesReturn1()
        {
            var g1 = Read("POINT(1 1)");
            var g2 = Read("POINT(1 1)");
            Assert.AreEqual(1d, new HausdorffSimilarityMeasure().Measure(g1, g2), "Point");

            g1 = Read("LINESTRING(1 1, 2 1)");
            g2 = Read("LINESTRING(1 1, 2 1)");
            Assert.AreEqual(1d, new HausdorffSimilarityMeasure().Measure(g1, g2), "LineString");

            g1 = Read("POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 7.58 1, 1 7.58, 1 1))");
            g2 = Read("POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 7.58 1, 1 7.58, 1 1))");
            Assert.AreEqual(1d, new HausdorffSimilarityMeasure().Measure(g1, g2), "POLYGON");
        }

        [Test]
        public void TestGreaterHausdorffDistanceReturnsPoorerSimilarity()
        {
            var g1 = Read("LINESTRING(1 1, 2 1.0, 3 1)");
            var g2 = Read("LINESTRING(1 1, 2 1.1, 3 1)");
            var g3 = Read("LINESTRING(1 1, 2 1.2, 3 1)");

            var sm = new HausdorffSimilarityMeasure();
            double m12 = sm.Measure(g1, g2);
            double m13 = sm.Measure(g1, g3);

            Assert.IsTrue(m13 < m12, "Greater distance, poorer similarity");
        }

    }
}
