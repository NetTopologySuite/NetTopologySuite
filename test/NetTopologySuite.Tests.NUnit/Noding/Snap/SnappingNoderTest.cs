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
            CheckRounding(wkt1, wkt2, 1, expected);
        }

        [Test]
        public void TestSnappedVertex()
        {
            string wkt1 = "LINESTRING (100 100, 200 100, 300 100)";
            string wkt2 = "LINESTRING (200 100.3, 400 110)";
            string expected = "MULTILINESTRING ((100 100, 200 100), (200 100, 300 100), (200 100, 400 110))";
            CheckRounding(wkt1, wkt2, 1, expected);
        }

        [Test]
        public void TestSelfSnap()
        {
            string wkt1 = "LINESTRING (100 200, 100 100, 300 100, 200 99.3, 200 0)";
            string expected =
                "MULTILINESTRING ((100 200, 100 100, 200 99.3), (200 99.3, 300 100), (300 100, 200 99.3), (200 99.3, 200 0))";
            CheckRounding(wkt1, null, 1, expected);
        }

        [Test]
        public void TestLineCondensePoints()
        {
            string wkt1 = "LINESTRING (1 1, 1.3 1, 1.6 1, 1.9 1, 2.2 1, 2.5 1, 2.8 1, 3.1 1, 3.5 1, 4 1)";
            string expected = "LINESTRING (1 1, 2.2 1, 3.5 1)";
            CheckRounding(wkt1, null, 1, expected);
        }

        [Test]
        public void TestLineDensePointsSelfSnap()
        {
            string wkt1 = "LINESTRING (1 1, 1.3 1, 1.6 1, 1.9 1, 2.2 1, 2.5 1, 2.8 1, 3.1 1, 3.5 1, 4.8 1, 3.8 3.1, 2.5 1.1, 0.5 3.1)";
            string expected = "MULTILINESTRING ((1 1, 2.2 1), (2.2 1, 3.5 1, 4.8 1, 3.8 3.1, 2.2 1), (2.2 1, 1 1), (1 1, 0.5 3.1))";
            CheckRounding(wkt1, null, 1, expected);
        }

        /**
         * Two rings with edges which are almost coincident.
         * Edegs are snapped to produce same segment
         */
        [Test]
        public void TestAlmostCoincidentEdge()
        {
            string wkt1 = "MULTILINESTRING ((698400.5682737827 2388494.3828697307, 698402.3209180075 2388497.0819257903, 698415.3598714538 2388498.764371397, 698413.5003455497 2388495.90071853, 698400.5682737827 2388494.3828697307), (698231.847335025 2388474.57994264, 698440.416211779 2388499.05985776, 698432.582638943 2388300.28294705, 698386.666515791 2388303.40346027, 698328.29462841 2388312.88889197, 698231.847335025 2388474.57994264))";
            string expected = "MULTILINESTRING ((698231.847335025 2388474.57994264, 698328.29462841 2388312.88889197, 698386.666515791 2388303.40346027, 698432.582638943 2388300.28294705, 698440.416211779 2388499.05985776, 698413.5003455497 2388495.90071853), (698231.847335025 2388474.57994264, 698400.5682737827 2388494.3828697307), (698400.5682737827 2388494.3828697307, 698402.3209180075 2388497.0819257903, 698415.3598714538 2388498.764371397, 698413.5003455497 2388495.90071853), (698400.5682737827 2388494.3828697307, 698413.5003455497 2388495.90071853), (698400.5682737827 2388494.3828697307, 698413.5003455497 2388495.90071853))";
            CheckRounding(wkt1, null, 1, expected);
        }

        /**
         * Extract from previous test
         */
        [Test]
        public void TestAlmostCoincidentines()
        {
            string wkt1 = "MULTILINESTRING ((698413.5003455497 2388495.90071853, 698400.5682737827 2388494.3828697307), (698231.847335025 2388474.57994264, 698440.416211779 2388499.05985776))";
            string expected = "MULTILINESTRING ((698231.847335025 2388474.57994264, 698400.5682737827 2388494.3828697307), (698400.5682737827 2388494.3828697307, 698413.5003455497 2388495.90071853), (698400.5682737827 2388494.3828697307, 698413.5003455497 2388495.90071853), (698413.5003455497 2388495.90071853, 698440.416211779 2388499.05985776))";
            CheckRounding(wkt1, null, 1, expected);
        }


        void CheckRounding(string wkt1, string wkt2, double snapDist, string expectedWKT)
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
