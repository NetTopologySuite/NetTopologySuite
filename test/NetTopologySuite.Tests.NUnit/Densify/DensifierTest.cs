using NetTopologySuite.Densify;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Densify
{
    public class DensifierTest : GeometryTestCase
    {
        private const double TOLERANCE = 1e-6;

        [Test]
        public void TestLine()
        {
            CheckDensify("LINESTRING (0 0, 30 40, 35 35)",
                10,
                "LINESTRING (0 0, 5 6.666666666666668, 10 13.333333333333336, 15 20, 20 26.66666666666667, 25 33.33333333333334, 30 40, 35 35)");
        }

        [Test]
        public void TestBox()
        {
            CheckDensify("POLYGON ((10 30, 30 30, 30 10, 10 10, 10 30))",
                10,
                "POLYGON ((10 30, 16.666666666666668 30, 23.333333333333336 30, 30 30, 30 23.333333333333332, 30 16.666666666666664, 30 10, 23.333333333333332 10, 16.666666666666664 10, 10 10, 10 16.666666666666668, 10 23.333333333333336, 10 30))");
        }

        [Test]
        public void TestBoxNoValidate()
        {
            CheckDensifyNoValidate("POLYGON ((10 30, 30 30, 30 10, 10 10, 10 30))",
                10,
                "POLYGON ((10 30, 16.666666666666668 30, 23.333333333333336 30, 30 30, 30 23.333333333333332, 30 16.666666666666664, 30 10, 23.333333333333332 10, 16.666666666666664 10, 10 10, 10 16.666666666666668, 10 23.333333333333336, 10 30))");
        }

        private void CheckDensify(string wkt, double distanceTolerance, string wktExpected)
        {
            var geom = Read(wkt);
            var expected = Read(wktExpected);
            var actual = Densifier.Densify(geom, distanceTolerance);
            CheckEqual(expected, actual, TOLERANCE);
        }

        /*
         * Note: it's hard to construct a geometry which would actually be invalid when densified.
         * This test just checks that the code path executes.
         */
        private void CheckDensifyNoValidate(string wkt, double distanceTolerance, string wktExpected)
        {
            var geom = Read(wkt);
            var expected = Read(wktExpected);
            var den = new Densifier(geom);
            den.DistanceTolerance = distanceTolerance;
            den.Validate = false;
            var actual = den.GetResultGeometry();
            CheckEqual(expected, actual, TOLERANCE);
        }

    }
}
