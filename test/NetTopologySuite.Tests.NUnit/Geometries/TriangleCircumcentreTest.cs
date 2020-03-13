using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class TriangleCircumcentreTest
    {
        [Test, Ignore("This test fails due to round-off error")]
        public void TestSquareDiagonal()
        {
            var cc1 = Circumcentre(193600.80333333334, 469345.355, 193600.80333333334, 469345.0175, 193601.10666666666, 469345.0175);
            var cc2 = Circumcentre(193600.80333333334, 469345.355, 193601.10666666666, 469345.0175, 193601.10666666666, 469345.355);
            CheckCCEqual(cc1, cc2);
        }

        [Test]
        public void TestSquareDiagonalDD()
        {
            var cc1 = CircumcentreDD(193600.80333333334, 469345.355, 193600.80333333334, 469345.0175, 193601.10666666666, 469345.0175);
            var cc2 = CircumcentreDD(193600.80333333334, 469345.355, 193601.10666666666, 469345.0175, 193601.10666666666, 469345.355);
            CheckCCEqual(cc1, cc2);
        }

        private static Coordinate Circumcentre(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);
            var c = new Coordinate(cx, cy);
            return Triangle.Circumcentre(a, b, c);
        }
        private static Coordinate CircumcentreDD(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);
            var c = new Coordinate(cx, cy);
            return Triangle.CircumcentreDD(a, b, c);
        }

        private void CheckCCEqual(Coordinate cc1, Coordinate cc2)
        {
            bool isEqual = cc1.Equals2D(cc2);
            if (!isEqual)
            {
                Assert.Warn("Triangle circumcentres are not equal!");
                Assert.Warn(WKTWriter.ToPoint(cc1));
                Assert.Warn(WKTWriter.ToPoint(cc2));
                Assert.Fail();
            }
        }
    }

}
