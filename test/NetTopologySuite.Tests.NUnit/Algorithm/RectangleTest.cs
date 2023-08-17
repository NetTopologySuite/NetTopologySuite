using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rectangle = NetTopologySuite.Algorithm.Rectangle;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class RectangleTest : GeometryTestCase
    {
        private const double TOL = 1e-10;

        [Test]
        public void TestOrthogonal()
        {
            checkRectangle("LINESTRING (9 1, 1 1, 0 5, 7 10, 10 6)",
                "POLYGON ((0 1, 0 10, 10 10, 10 1, 0 1))");
        }

        [Test]
        public void Test45()
        {
            checkRectangle("LINESTRING (10 5, 5 0, 2 1, 2 7, 9 9)",
                "POLYGON ((-1 4, 6.5 11.5, 11.5 6.5, 4 -1, -1 4))");
        }

        [Test]
        public void TestCoincidentBaseSides()
        {
            checkRectangle("LINESTRING (10 5, 7 0, 7 0, 2 7, 10 5)",
                "POLYGON ((0.2352941176470591 4.0588235294117645, 3.2352941176470598 9.058823529411764, 10 5, 7 0, 0.2352941176470591 4.0588235294117645))");
        }

        private void checkRectangle(string wkt, string wktExpected)
        {
            var line = (LineString)Read(wkt);
            var baseRightPt = line.GetCoordinateN(0);
            var baseLeftPt = line.GetCoordinateN(1);
            var leftSidePt = line.GetCoordinateN(2);
            var oppositePt = line.GetCoordinateN(3);
            var rightSidePt = line.GetCoordinateN(4);
                var actual = Rectangle.CreateFromSidePts(baseRightPt, baseLeftPt,
                oppositePt, leftSidePt, rightSidePt, line.Factory);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual, TOL);
        }
    }
}
