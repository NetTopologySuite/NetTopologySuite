using NetTopologySuite.Algorithm.Axis;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Axis
{
    public class ApproximateMedialAxisTest : GeometryTestCase
    {
        [Test]
        public void TestQuad()
        {
            CheckTree("POLYGON ((10 10, 30 30, 60 40, 90 70, 90 10, 10 10))"
                , "GEOMETRYCOLLECTION (POLYGON ((10 10, 20 40, 90 10, 10 10)), POLYGON ((90 90, 20 40, 90 10, 90 90)))");
        }

        [Test]
        public void TestRandom()
        {
            CheckTree("POLYGON ((200 100, 100 100, 150 200, 250 250, 300 300, 360 400, 500 300, 400 250, 300 200, 300 150, 200 100))"
                , "GEOMETRYCOLLECTION (POLYGON ((10 10, 20 40, 90 10, 10 10)), POLYGON ((90 90, 20 40, 90 10, 90 90)))");
        }

        private void CheckTree(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = ApproximateMedialAxis.MedialAxis(geom);
            var expected = Read(wktExpected);
            //CheckEqual(expected, actual);
        }
    }
}
