using NetTopologySuite.Algorithm.Match;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Geometries
{
    [TestFixture]
    public class OctagonalEnvelopeTest : GeometryTestCase
    {
        [Test]
        public void TestCircle()
        {
            CheckOctagonalEnvelope(
                "POLYGON ((110 100, 110 98, 109 96, 108 94, 107 93, 106 92, 104 91, 102 90, 100 90, 98 90, 96 91, 94 92, 93 93, 92 94, 91 96, 90 98, 90 100, 90 102, 91 104, 92 106, 93 107, 94 108, 96 109, 98 110, 100 110, 102 110, 104 109, 106 108, 107 107, 108 106, 109 104, 110 102, 110 100))",
                "POLYGON ((90 96, 90 104, 96 110, 104 110, 110 104, 110 96, 104 90, 96 90, 90 96))");
        }

        [Test, Category("FailureCase")]
        public void TestRobust()
        {
            CheckOctagonalEnvelope(
                "POLYGON ((897136.58 1669979.8879999965, 901100.2509999997 1650018.9820000022, 338254.7099999995 1600889.083, 273366.8019999984 1735720.873999999, 897136.58 1669979.8879999965))",
                "POLYGON ((273366.8019999984 1665776.9910000013, 273366.8019999984 1735720.873999999, 831395.5939999977 1735720.873999999, 901100.2509999997 1666016.216999997, 901100.2509999997 1650018.9820000022, 851970.3519999976 1600889.083, 338254.7099999995 1600889.083, 273366.8019999984 1665776.9910000013))");
        }

        private void CheckOctagonalEnvelope(string wkt, string wktExpected)
        {
            var input = Read(wkt);
            if (!input.IsValid) throw new IgnoreException("input geometry not valid");
            var expected = Read(wktExpected);
            if (!expected.IsValid) throw new IgnoreException("expected geometry not valid");

            var octEnv = OctagonalEnvelope.GetOctagonalEnvelope(input);
            var isEqual = octEnv.EqualsNormalized(expected);

            var hsmDiff = 1d;
            if (!isEqual)
            {
                var hsm = new HausdorffSimilarityMeasure();
                hsmDiff = hsm.Measure(octEnv, expected);
            }
            Assert.IsTrue(isEqual, $"Failed, Hausdorff Similarity Measure value = {hsmDiff:R}.");
        }
    }
}
