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
                10,
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))"
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
