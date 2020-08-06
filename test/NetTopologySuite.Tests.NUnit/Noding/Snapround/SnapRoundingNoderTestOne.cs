using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding.Snaparound
{
    /**
     * Test Snap Rounding
     *
     * @version 1.17
     */
    public class SnapRoundingNoderTestOne : GeometryTestCase
    {


        private static INoder GetSnapRounder(PrecisionModel pm)
        {
            return new SnapRoundingNoder(pm);
        }

        [Test]
        public void TestSlantAndHorizontalLineWithMiddleNode()
        {
            string wkt =
                "MULTILINESTRING ((0.1565552 49.5277405, 0.1579285 49.5277405, 0.1593018 49.5277405), (0.1568985 49.5280838, 0.1589584 49.5273972))";
            string expected =
                "MULTILINESTRING ((0.156555 49.527741, 0.157928 49.527741), (0.156899 49.528084, 0.157928 49.527741), (0.157928 49.527741, 0.157929 49.527741), (0.157928 49.527741, 0.158958 49.527397), (0.157929 49.527741, 0.159302 49.527741))";
            CheckRounding(wkt, 1_000_000.0, expected);
        }

        [Test, Ignore("")]
        public void TestFlatLinesWithMiddleNode()
        {
            string wkt =
                "MULTILINESTRING ((2.5117493 49.0278625,                      2.5144958 49.0278625), (2.511749 49.027863, 2.513123 49.027863, 2.514496 49.027863))";
            string expected =
                "MULTILINESTRING ((2.511749 49.027863, 2.513123 49.027863), (2.511749 49.027863, 2.513123 49.027863), (2.513123 49.027863, 2.514496 49.027863), (2.513123 49.027863, 2.514496 49.027863))";
            CheckRounding(wkt, 1_000_000.0, expected);
        }

        [Test, Ignore("")]
        public void TestNearbyCorner()
        {

            string wkt = "MULTILINESTRING ((0.2 1.1, 1.6 1.4, 1.9 2.9), (0.9 0.9, 2.3 1.7))";
            string expected =
                "MULTILINESTRING ((0 1, 1 1), (1 1, 2 1), (1 1, 2 1), (2 1, 2 2), (2 1, 2 2), (2 2, 2 3))";
            CheckRounding(wkt, 1.0, expected);
        }

        [Test, Ignore("")]
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
        [Test, Ignore("")]
        public void TestIntOnGridCorner()
        {

            string wkt =
                "MULTILINESTRING ((4.30166242 45.53438188, 4.30166243 45.53438187), (4.3011475 45.5328371, 4.3018341 45.5348969))";
            string expected = null;
            CheckRounding(wkt, 100000000, expected);
        }

        /**
         * Currently fails, does not node correctly
         * Fixed by not snapping line segments when testing against hot pixel
         */
        [Test, Ignore("")]
        public void TestVertexCrossesLine()
        {

            string wkt =
                "MULTILINESTRING ((2.2164917 48.8864136, 2.2175217 48.8867569), (2.2175217 48.8867569, 2.2182083 48.8874435), (2.2182083 48.8874435, 2.2161484 48.8853836))";
            string expected = null;
            CheckRounding(wkt, 1000000, expected);
        }

        /**
         * Currently fails, does not node correctly.
         * 
         * FIXED by NOT rounding lines extracted by Overlay
         */
        [Test, Ignore("")]
        public void TestVertexCrossesLine2()
        {

            string wkt =
                "MULTILINESTRING ((2.276916574988164 49.06082147500638, 2.2769165 49.0608215), (2.2769165 49.0608215, 2.2755432 49.0608215), (2.2762299 49.0615082, 2.276916574988164 49.06082147500638))";
            string expected = null;
            CheckRounding(wkt, 1000000, expected);
        }

        /**
         * Looks like a very short line is stretched between two grid points, 
         * and for some reason the node at one end is not inserted in a line snapped to it.
         * 
         * FIXED by ensuring that HotPixel intersection tests whether segment
         * endpoints lie inside pixel.
         */
        [Test, Ignore("")]
        public void TestShortLineNodeNotAdded()
        {

            string wkt =
                "LINESTRING (2.1279144 48.8445282, 2.126884443750796 48.84555818124935, 2.1268845 48.8455582, 2.1268845 48.8462448)";
            string expected =
                "MULTILINESTRING ((2.127914 48.844528, 2.126885 48.845558), (2.126885 48.845558, 2.126884 48.845558), (2.126884 48.845558, 2.126885 48.845558), (2.126885 48.845558, 2.126885 48.846245))";
            CheckRounding(wkt, 1000000, expected);
        }

        /**
         * An A vertex lies very close to a B segment.
         * The vertex is snapped across the segment, but the segment is not noded.
         * FIXED by adding intersection detection for near vertices to segments
         */
        [Test, Ignore("")]
        public void TestNearVertexNotNoded()
        {
            string wkt =
                "MULTILINESTRING ((2.4829102 48.8726807, 2.4830818249999997 48.873195575, 2.4839401 48.8723373), ( 2.4829102 48.8726807, 2.4832535 48.8737106 ))";
            string expected = null;
            CheckRounding(wkt, 100000000, expected);
        }

        /**
         * A vertex lies near interior of horizontal segment.  
         * Both are moved by rounding, and vertex ends up coincident with segment,
         * but node is not created.
         * This is very subtle, since because the segment is horizontal the vertex lies exactly on it
         * and thus still reports as valid geometry (although a noding check reports failure).
         * This is caused by the indexing used in Snap-rounding using exact envelopes.
         * What is needed is a small expansion amount to ensure segments within snap distance are tested
         * (in MCIndexNoder)
         */
        [Test, Ignore("")]
        public void TestVertexNearHorizSegNotNoded()
        {
            string wkt =
                "MULTILINESTRING (( 2.5096893 48.9530182, 2.50762932500455 48.95233152500091, 2.5055695 48.9530182 ), ( 2.5090027 48.9523315, 2.5035095 48.9523315 ))";
            string expected = null;
            CheckRounding(wkt, 1000000, expected);
        }

        void CheckRounding(string wkt, double scale, string expectedWKT)
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
