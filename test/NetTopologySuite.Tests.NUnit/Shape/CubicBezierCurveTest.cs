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
            CheckCurve("LINESTRING(0 0, 10 0, 10 10)", 0d,
                "LINESTRING (0 0, 0.13 0, 0.49 0, 1.04 0, 1.75 0, 2.59 0, 3.52 0, 4.5 0, 5.5 0, 6.48 0, 7.41 0, 8.25 0, 8.96 0, 9.51 0, 9.87 0, 10 0, 10 0.13, 10 0.49, 10 1.04, 10 1.75, 10 2.59, 10 3.52, 10 4.5, 10 5.5, 10 6.48, 10 7.41, 10 8.25, 10 8.96, 10 9.51, 10 9.87, 10 10)", 0d);
        }
        [Test]
        public void TestLineStringHalf()
        {
            CheckCurve("LINESTRING(0 0, 10 0, 10 10)", 0.5d,
                "LINESTRING (0 0, 0.34 -0.25, 0.82 -0.46, 1.42 -0.64, 2.12 -0.78, 2.89 -0.88, 3.71 -0.95, 4.57 -0.99, 5.43 -0.99, 6.29 -0.95, 7.11 -0.88, 7.88 -0.78, 8.58 -0.64, 9.18 -0.46, 9.66 -0.25, 10 0, 10.25 0.34, 10.46 0.82, 10.64 1.42, 10.78 2.12, 10.88 2.89, 10.95 3.71, 10.99 4.57, 10.99 5.43, 10.95 6.29, 10.88 7.11, 10.78 7.88, 10.64 8.58, 10.46 9.18, 10.25 9.66, 10 10)", 0d);
        }
        [Test]
        public void TestLineStringOne()
        {
            CheckCurve("LINESTRING(0 0, 10 0, 10 10)", 1.0d,
                "LINESTRING (0 0, 0.56 -0.49, 1.16 -0.92, 1.8 -1.27, 2.48 -1.56, 3.18 -1.77, 3.9 -1.91, 4.63 -1.98, 5.37 -1.98, 6.1 -1.91, 6.82 -1.77, 7.52 -1.56, 8.2 -1.27, 8.84 -0.92, 9.44 -0.49, 10 0, 10.49 0.56, 10.92 1.16, 11.27 1.8, 11.56 2.48, 11.77 3.18, 11.91 3.9, 11.98 4.63, 11.98 5.37, 11.91 6.1, 11.77 6.82, 11.56 7.52, 11.27 8.2, 10.92 8.84, 10.49 9.44, 10 10)", 0d);
        }

        [Test]
        public void TestAlphaSquare()
        {
            CheckCurve("POLYGON ((40 60, 60 60, 60 40, 40 40, 40 60))", 1,
                "POLYGON ((40 60, 41.1 61, 42.3 61.8, 43.6 62.5, 45 63.1, 46.4 63.5, 47.8 63.8, 49.3 64, 50.7 64, 52.2 63.8, 53.6 63.5, 55 63.1, 56.4 62.5, 57.7 61.8, 58.9 61, 60 60, 61 58.9, 61.8 57.7, 62.5 56.4, 63.1 55, 63.5 53.6, 63.8 52.2, 64 50.7, 64 49.3, 63.8 47.8, 63.5 46.4, 63.1 45, 62.5 43.6, 61.8 42.3, 61 41.1, 60 40, 58.9 39, 57.7 38.2, 56.4 37.5, 55 36.9, 53.6 36.5, 52.2 36.2, 50.7 36, 49.3 36, 47.8 36.2, 46.4 36.5, 45 36.9, 43.6 37.5, 42.3 38.2, 41.1 39, 40 40, 39 41.1, 38.2 42.3, 37.5 43.6, 36.9 45, 36.5 46.4, 36.2 47.8, 36 49.3, 36 50.7, 36.2 52.2, 36.5 53.6, 36.9 55, 37.5 56.4, 38.2 57.7, 39 58.9, 40 60))");
        }

        [Test]
        public void TestAlphaRightAngle()
        {
            CheckCurve("LINESTRING (30 40, 40 50, 50 40)", 1,
                "LINESTRING (30 40, 30.1 41.1, 30.2 42.1, 30.5 43.1, 30.9 44, 31.4 44.9, 32 45.8, 32.7 46.6, 33.4 47.3, 34.2 48, 35.1 48.6, 36 49.1, 36.9 49.5, 37.9 49.8, 38.9 49.9, 40 50, 41.1 49.9, 42.1 49.8, 43.1 49.5, 44 49.1, 44.9 48.6, 45.8 48, 46.6 47.3, 47.3 46.6, 48 45.8, 48.6 44.9, 49.1 44, 49.5 43.1, 49.8 42.1, 49.9 41.1, 50 40)");
        }

        [Test]
        public void TestAlphaRightZigzag()
        {
            CheckCurve("LINESTRING (10 10, 20 19, 30 10, 40 20)", 1,
                "LINESTRING (10 10, 10.2 11, 10.4 11.9, 10.8 12.9, 11.2 13.7, 11.7 14.6, 12.3 15.3, 13 16, 13.7 16.7, 14.5 17.3, 15.3 17.8, 16.2 18.2, 17.1 18.5, 18 18.8, 19 18.9, 20 19, 20.9 18.9, 21.8 18.6, 22.5 18.1, 23.1 17.4, 23.7 16.6, 24.2 15.8, 24.8 14.9, 25.2 14, 25.8 13.1, 26.3 12.3, 26.9 11.5, 27.5 10.9, 28.2 10.4, 29.1 10.1, 30 10, 31 10.1, 32 10.3, 33 10.6, 33.9 11, 34.8 11.5, 35.7 12.1, 36.5 12.8, 37.2 13.5, 37.9 14.3, 38.5 15.2, 39 16.1, 39.4 17, 39.7 18, 39.9 19, 40 20)");
        }

        [Test]
        public void TestCtrlRightZigzag()
        {
            CheckCurve("LINESTRING (10 10, 20 20, 30 10, 40 20)",
                "LINESTRING (10 15, 15 20, 25 20, 28 10, 32 10, 40 25)",
                "LINESTRING (10 10, 10.1 11, 10.3 12, 10.6 13, 11 13.9, 11.5 14.8, 12.1 15.7, 12.8 16.5, 13.5 17.2, 14.3 17.9, 15.2 18.5, 16.1 19, 17 19.4, 18 19.7, 19 19.9, 20 20, 21 19.9, 21.9 19.5, 22.8 19, 23.6 18.2, 24.4 17.4, 25.1 16.5, 25.8 15.5, 26.4 14.5, 27.1 13.5, 27.6 12.6, 28.2 11.8, 28.7 11, 29.1 10.5, 29.6 10.1, 30 10, 30.5 10.2, 31.1 10.7, 31.8 11.5, 32.6 12.5, 33.5 13.7, 34.4 15, 35.3 16.2, 36.2 17.5, 37.1 18.6, 37.9 19.6, 38.6 20.4, 39.2 20.9, 39.6 21, 39.9 20.7, 40 20)");
        }

        private void CheckCurve(string orignalWkt, double alpha, string expectedWkt, double tolerance = 0.5d)
        {
            var original = Read(orignalWkt);
            var actual = CubicBezierCurve.Create(original, alpha);
            actual = GeometryPrecisionReducer.Reduce(actual, new NetTopologySuite.Geometries.PrecisionModel(100));
            //TestContext.WriteLine(actual);
            var expected = Read(expectedWkt);

            CheckEqual(expected, actual, tolerance);
        }


        private void CheckCurve(string wkt, string wktCtrl, string wktExpected)
        {
            var geom = Read(wkt);
            var ctrl = Read(wktCtrl);
            var actual = CubicBezierCurve.Create(geom, ctrl);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual, 0.5);
        }

    }
}
