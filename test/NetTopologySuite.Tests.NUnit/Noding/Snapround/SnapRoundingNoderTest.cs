using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding.Snaparound
{
    /// <summary>
    /// Test Snap Rounding
    /// </summary>
    public class SnapRoundingNoderTest : GeometryTestCase
    {
        private static INoder GetSnapRounder(PrecisionModel pm)
        {
            return new SnapRoundingNoder(pm);
        }

        [Test]
        public void TestSimple()
        {
            string wkt = "MULTILINESTRING ((1 1, 9 2), (3 3, 3 0))";
            string expected = "MULTILINESTRING ((1 1, 3 1), (3 1, 9 2), (3 3, 3 1), (3 1, 3 0))";
            CheckRounding(wkt, 1, expected);
        }


        /**
         * A diagonal line is snapped to a vertex half a grid cell away
         */
        [Test]
        public void TestSnappedDiagonalLine()
        {
            string wkt = "LINESTRING (2 3, 3 3, 3 2, 2 3)";
            string expected = "MULTILINESTRING ((2 3, 3 3), (2 3, 3 3), (3 2, 3 3), (3 2, 3 3))";
            CheckRounding(wkt, 1.0, expected);
        }
        /// <summary>
        /// This test checks the HotPixel test for overlapping horizontal line
        /// </summary>
        [Test]
        public void TestHorizontalLinesWithMiddleNode()
        {
            string wkt =
                "MULTILINESTRING ((2.5117493 49.0278625,                      2.5144958 49.0278625), (2.511749 49.027863, 2.513123 49.027863, 2.514496 49.027863))";
            string expected =
                "MULTILINESTRING ((2.511749 49.027863, 2.513123 49.027863), (2.511749 49.027863, 2.513123 49.027863), (2.513123 49.027863, 2.514496 49.027863), (2.513123 49.027863, 2.514496 49.027863))";
            CheckRounding(wkt, 1_000_000.0, expected);
        }

        [Test]
        public void TestSlantAndHorizontalLineWithMiddleNode()
        {
            string wkt =
                "MULTILINESTRING ((0.1565552 49.5277405, 0.1579285 49.5277405, 0.1593018 49.5277405), (0.1568985 49.5280838, 0.1589584 49.5273972))";
            string expected =
                "MULTILINESTRING ((0.156555 49.527741, 0.157928 49.527741), (0.156899 49.528084, 0.157928 49.527741), (0.157928 49.527741, 0.157929 49.527741, 0.159302 49.527741), (0.157928 49.527741, 0.158958 49.527397))";
            CheckRounding(wkt, 1_000_000.0, expected);
        }

        [Test]
        public void TestNearbyCorner()
        {

            string wkt = "MULTILINESTRING ((0.2 1.1, 1.6 1.4, 1.9 2.9), (0.9 0.9, 2.3 1.7))";
            string expected =
                "MULTILINESTRING ((0 1, 1 1), (1 1, 2 1), (1 1, 2 1), (2 1, 2 2), (2 1, 2 2), (2 2, 2 3))";
            CheckRounding(wkt, 1.0, expected);
        }

        [Test]
        public void TestNearbyShape()
        {

            string wkt = "MULTILINESTRING ((1.3 0.1, 2.4 3.9), (0 1, 1.53 1.48, 0 4))";
            string expected = "MULTILINESTRING ((1 0, 2 1), (2 1, 2 4), (0 1, 2 1), (2 1, 0 4))";
            CheckRounding(wkt, 1.0, expected);
        }

        /**
         * Currently fails, perhaps due to intersection lying right on a grid cell corner?
         * Fixed by ensuring intersections are forced into segments
         */
        [Test]
        public void TestIntOnGridCorner()
        {

            string wkt = "MULTILINESTRING ((4.30166242 45.53438188, 4.30166243 45.53438187), (4.3011475 45.5328371, 4.3018341 45.5348969))";
            string expected = null;
            CheckRounding(wkt, 100000000, expected);
        }

        /**
         * Currently fails, does not node correctly
         */
        [Test]
        public void TestVertexCrossesLine()
        {

            string wkt =
                "MULTILINESTRING ((2.2164917 48.8864136, 2.2175217 48.8867569), (2.2175217 48.8867569, 2.2182083 48.8874435), (2.2182083 48.8874435, 2.2161484 48.8853836))";
            string expected = null;
            CheckRounding(wkt, 1000000, expected);
        }

        /**
         * Currently fails, does not node correctly.
         * Fixed by NOT rounding lines extracted by Overlay
         */
        [Test]
        public void TestVertexCrossesLine2()
        {

            string wkt =
                "MULTILINESTRING ((2.276916574988164 49.06082147500638, 2.2769165 49.0608215), (2.2769165 49.0608215, 2.2755432 49.0608215), (2.2762299 49.0615082, 2.276916574988164 49.06082147500638))";
            string expected = null;
            CheckRounding(wkt, 1000000, expected);
        }

        /**
         * Looks like a very short line is stretched between two grid points, 
         * and for some reason the node at one end is not inserted in a line snapped to it
         */
        [Test]
        public void TestShortLineNodeNotAdded()
        {

            string wkt =
                "LINESTRING (2.1279144 48.8445282, 2.126884443750796 48.84555818124935, 2.1268845 48.8455582, 2.1268845 48.8462448)";
            string expected =
                "MULTILINESTRING ((2.127914 48.844528, 2.126885 48.845558), (2.126885 48.845558, 2.126884 48.845558), (2.126884 48.845558, 2.126885 48.845558), (2.126885 48.845558, 2.126885 48.846245))";
            CheckRounding(wkt, 1000000, expected);
        }

        /**
         * This test will fail if the diagonals of hot pixels are not checked.
         * Note that the nearby vertex is far enough from the long segment
         * to avoid being snapped as an intersection.
         */
        [Test]
        public void TestDiagonalNotNodedRightUp()
        {

            string wkt = "MULTILINESTRING ((0 0, 10 10), ( 0 2, 4.55 5.4, 9 10 ))";
            string expected = null;
            CheckRounding(wkt, 1, expected);
        }

        /**
         * Same diagonal test but flipped to test other diagonal
         */
        [Test]
        public void TestDiagonalNotNodedLeftUp()
        {

            string wkt = "MULTILINESTRING ((10 0, 0 10), ( 10 2, 5.45 5.45, 1 10 ))";
            string expected = null;
            CheckRounding(wkt, 1, expected);
        }

        /**
         * Original full-precision diagonal line case
         */
        [Test]
        public void TestDiagonalNotNodedOriginal()
        {

            string wkt =
                "MULTILINESTRING (( 2.45167 48.96709, 2.45768 48.9731 ), (2.4526978 48.968811, 2.4537277 48.9691544, 2.4578476 48.9732742))";
            string expected = null;
            CheckRounding(wkt, 100000, expected);
        }

        /**
         * An A vertex lies very close to a B segment.
         * The vertex is snapped across the segment, but the segment is not noded.
         * FIXED by adding intersection detection for near vertices to segments
         */
        [Test]
        public void TestNearVertexNotNoded()
        {
            string wkt =
                "MULTILINESTRING ((2.4829102 48.8726807, 2.4830818249999997 48.873195575, 2.4839401 48.8723373), ( 2.4829102 48.8726807, 2.4832535 48.8737106 ))";
            string expected = null;
            CheckRounding(wkt, 100000000, expected);
        }

        [Test, Ignore("Investigate")]
        public void TestLoopBackCreatesNode()
        {
            string wkt = "LINESTRING (2 2, 5 2, 8 4, 5 6, 4.8 2.3, 2 5)";
            string expected = "MULTILINESTRING ((2 2, 5 2), (5 2, 8 4, 5 6, 5 2), (5 2, 2 5))";
            CheckRounding(wkt, 1, expected);
        }

        private void CheckRounding(string wkt, double scale, string expectedWKT)
        {
            var geom = Read(wkt);
            var pm = new PrecisionModel(scale);
            var noder = GetSnapRounder(pm);
            var result = NodingTestUtility.NodeValidated(geom, null, noder);

            // only check if expected was provided
            if (expectedWKT == null) return;

            var expected = Read(expectedWKT);
            CheckEqual(expected, result);
        }
    }
}
