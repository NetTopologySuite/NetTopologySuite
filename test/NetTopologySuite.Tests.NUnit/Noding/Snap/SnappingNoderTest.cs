using NetTopologySuite.Geometries;
using NetTopologySuite.Noding.Snap;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding.Snap
{
    public class SnappingNoderTest : GeometryTestCase
    {
        [Test]
        public void TestOverlappingLinesWithNearVertex()
        {
            string wkt1 = "LINESTRING (100 100, 300 100)";
            string wkt2 = "LINESTRING (200 100.1, 400 100)";
            string expected =
                "MULTILINESTRING ((100 100, 200 100.1), (200 100.1, 300 100), (200 100.1, 300 100), (300 100, 400 100))";
            checkRounding(wkt1, wkt2, 1, expected);
        }

        [Test]
        public void TestSnappedVertex()
        {
            string wkt1 = "LINESTRING (100 100, 200 100, 300 100)";
            string wkt2 = "LINESTRING (200 100.3, 400 110)";
            string expected = "MULTILINESTRING ((100 100, 200 100), (200 100, 300 100), (200 100, 400 110))";
            checkRounding(wkt1, wkt2, 1, expected);
        }

        [Test]
        public void TestSelfSnap()
        {
            string wkt1 = "LINESTRING (100 200, 100 100, 300 100, 200 99.3, 200 0)";
            string expected =
                "MULTILINESTRING ((100 200, 100 100, 200 99.3), (200 99.3, 300 100), (300 100, 200 99.3), (200 99.3, 200 0))";
            checkRounding(wkt1, null, 1, expected);
        }

        [Test]
        public void TestLineCondensePoints()
        {
            string wkt1 = "LINESTRING (1 1, 1.3 1, 1.6 1, 1.9 1, 2.2 1, 2.5 1, 2.8 1, 3.1 1, 3.5 1, 4 1)";
            string expected = "LINESTRING (1 1, 2.2 1, 3.5 1)";
            checkRounding(wkt1, null, 1, expected);
        }

        [Test]
        public void TestLineDensePointsSelfSnap()
        {
            string wkt1 = "LINESTRING (1 1, 1.3 1, 1.6 1, 1.9 1, 2.2 1, 2.5 1, 2.8 1, 3.1 1, 3.5 1, 4.8 1, 3.8 3.1, 2.5 1.1, 0.5 3.1)";
            string expected = "MULTILINESTRING ((1 1, 2.2 1), (2.2 1, 3.5 1, 4.8 1, 3.8 3.1, 2.2 1), (2.2 1, 1 1), (1 1, 0.5 3.1))";
            checkRounding(wkt1, null, 1, expected);
        }

        void checkRounding(string wkt1, string wkt2, double snapDist, string expectedWKT)
        {
            var geom1 = Read(wkt1);
            Geometry geom2 = null;
            if (wkt2 != null)
                geom2 = Read(wkt2);

            var noder = new SnappingNoder(snapDist);
            var result = NodingTestUtility.NodeValidated(geom1, geom2, noder);

            // only check if expected was provided
            if (expectedWKT == null) return;
            var expected = Read(expectedWKT);
            CheckEqual(expected, result);
        }


    }
}
