using NetTopologySuite.Hull;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Hull
{
    [TestFixture]
    public class ConcaveHullTest : GeometryTestCase
    {

        [Test]
        public void TestSimple()
        {
            CheckHull(
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))",
                150,
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))"
                );
        }

        [Test, Ignore("Not verified test case.")]
        public void TestTurf()
        {
            CheckHull(
                "LINESTRING (-63.601226 44.642643, -63.591442 44.651436, -63.580799 44.648749, -63.573589 44.641788, -63.587665 44.64533, -63.595218 44.64765)",
                0.0001,
                "POLYGON ((-63.601226 44.642643, -63.587665 44.64533, -63.573589 44.641788, -63.580799 44.648749, -63.591442 44.651436, -63.601226 44.642643))"
                );
        }


        private void CheckHull(string inputWKT, double tolerance, string expectedWKT)
        {
            var input = Read(inputWKT);
            var expected = Read(expectedWKT);
            var hull = new ConcaveHull(input, tolerance);
            var actual = hull.GetResult();
            CheckEqual(expected, actual);
        }
    }
}
