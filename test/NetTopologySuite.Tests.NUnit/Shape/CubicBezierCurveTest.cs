using NetTopologySuite.Precision;
using NetTopologySuite.Shape;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Shape
{
    internal class CubicBezierCurveTest : GeometryTestCase
    {
        public CubicBezierCurveTest()
            :base()
        {
        }

        [Test]
        public void TestLineStringNull()
        {
            CheckBezier("LINESTRING(0 0, 10 0, 10 10)", 0d,
                "LINESTRING (0 0, 0.13 0, 0.49 0, 1.04 0, 1.75 0, 2.59 0, 3.52 0, 4.5 0, 5.5 0, 6.48 0, 7.41 0, 8.25 0, 8.96 0, 9.51 0, 9.87 0, 10 0, 10 0.13, 10 0.49, 10 1.04, 10 1.75, 10 2.59, 10 3.52, 10 4.5, 10 5.5, 10 6.48, 10 7.41, 10 8.25, 10 8.96, 10 9.51, 10 9.87, 10 10)", 0d);
        }
        [Test]
        public void TestLineStringHalf()
        {
            CheckBezier("LINESTRING(0 0, 10 0, 10 10)", 0.5d,
                "LINESTRING (0 0, 0.34 -0.25, 0.82 -0.46, 1.42 -0.64, 2.12 -0.78, 2.89 -0.88, 3.71 -0.95, 4.57 -0.99, 5.43 -0.99, 6.29 -0.95, 7.11 -0.88, 7.88 -0.78, 8.58 -0.64, 9.18 -0.46, 9.66 -0.25, 10 0, 10.25 0.34, 10.46 0.82, 10.64 1.42, 10.78 2.12, 10.88 2.89, 10.95 3.71, 10.99 4.57, 10.99 5.43, 10.95 6.29, 10.88 7.11, 10.78 7.88, 10.64 8.58, 10.46 9.18, 10.25 9.66, 10 10)", 0d);
        }
        [Test]
        public void TestLineStringOne()
        {
            CheckBezier("LINESTRING(0 0, 10 0, 10 10)", 1.0d,
                "LINESTRING (0 0, 0.56 -0.49, 1.16 -0.92, 1.8 -1.27, 2.48 -1.56, 3.18 -1.77, 3.9 -1.91, 4.63 -1.98, 5.37 -1.98, 6.1 -1.91, 6.82 -1.77, 7.52 -1.56, 8.2 -1.27, 8.84 -0.92, 9.44 -0.49, 10 0, 10.49 0.56, 10.92 1.16, 11.27 1.8, 11.56 2.48, 11.77 3.18, 11.91 3.9, 11.98 4.63, 11.98 5.37, 11.91 6.1, 11.77 6.82, 11.56 7.52, 11.27 8.2, 10.92 8.84, 10.49 9.44, 10 10)", 0d);
        }

        private void CheckBezier(string orignalWkt, double alpha, string expectedWkt, double tolerance)
        {
            var original = Read(orignalWkt);
            var actual = CubicBezierCurve.Create(original, alpha);
            actual = GeometryPrecisionReducer.Reduce(actual, new NetTopologySuite.Geometries.PrecisionModel(100));
            //TestContext.WriteLine(actual);
            var expected = Read(expectedWkt);

            CheckEqual(expected, actual, tolerance);
        }
    }
}
