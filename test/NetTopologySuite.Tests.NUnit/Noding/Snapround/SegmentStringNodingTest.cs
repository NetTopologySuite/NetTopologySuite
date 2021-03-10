using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding.Snaparound
{
    /**
     * Test for correctly created Noded Segment Strings
     * under an extreme usage of SnapRounding.
     * This test reveals a bug in SegmentNodeList.createSplitEdge()
     * which can create 1-point Segment Strings
     * if the input is incorrectly noded due to robustness issues.
     * It also reveals a limitation in SegmentNode sorting which
     * can cause nodes to sort wrongly if their coordinates are very close 
     * and they are relatively far off the line segment containing them.
     * This is actually outside of the operating regime of the SegmentNode comparison,
     * but in there is a simple fix which handles some cases like these.
     * 
     * See https://github.com/locationtech/jts/pull/395
     *
     * @version 1.17
     */
    public class SegmentStringNodingTest
    {

        WKTReader rdr = new WKTReader();


        [Test]
        public void TestThinTriangle()
        {
            const string wkt =
                "LINESTRING ( 55121.54481117887 42694.49730855581, 55121.54481117887 42694.4973085558, 55121.458748617406 42694.419143944244, 55121.54481117887 42694.49730855581 )";
            var pm = new PrecisionModel(1.1131949079327356E11);
            CheckNodedStrings(wkt, pm);
        }

        [Test]
        public void TestSegmentLength1Failure()
        {
            const string wkt =
                "LINESTRING ( -1677607.6366504875 -588231.47100446, -1674050.1010869485 -587435.2186255794, -1670493.6527468169 -586636.7948791061, -1424286.3681743187 -525586.1397894835, -1670493.6527468169 -586636.7948791061, -1674050.1010869485 -587435.2186255795, -1677607.6366504875 -588231.47100446)";
            var pm = new PrecisionModel(1.11E10);
            CheckNodedStrings(wkt, pm);
        }

        private void CheckNodedStrings(string wkt, PrecisionModel pm)
        {
            var g = rdr.Read(wkt);
            var strings = new List<ISegmentString>();
            strings.Add(new NodedSegmentString(g.Coordinates, null));
            new SnapRoundingNoder(pm).ComputeNodes(strings);

            var noded = NodedSegmentString.GetNodedSubstrings(strings);
            foreach (var s in noded) {
                Assert.That(s.Count, Is.GreaterThanOrEqualTo(2), "Found a 1-point segmentstring");
                Assert.That(IsCollapsed(s), Is.False, "Found a collapsed edge");
            }
        }

        /// <summary>
        /// Test if the segmentString is a collapsed edge
        /// of the form ABA.
        /// These should not be returned by noding. 
        /// </summary>
        /// <param name="s">A segment string</param>
        /// <returns><c>true</c> if the segment string is collapsed</returns>
        private bool IsCollapsed(ISegmentString s)
        {
            if (s.Count != 3) return false;
            bool isEndsEqual = s.Coordinates[0].Equals2D(s.Coordinates[2]);
            bool isMiddleDifferent = !s.Coordinates[0].Equals2D(s.Coordinates[1]);
            bool isCollapsed = isEndsEqual && isMiddleDifferent;
            return isCollapsed;
        }


    }
}
