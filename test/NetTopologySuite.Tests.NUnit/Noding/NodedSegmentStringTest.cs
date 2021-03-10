using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    public class NodedSegmentStringTest : GeometryTestCase
    {

        /**
         * Tests a case which involves nodes added when using the SnappingNoder.
         * In this case one of the added nodes is relatively "far" from its segment, 
         * and "near" the start vertex of the segment.
         * Computing the noding correctly requires the fix to {@link SegmentNode#compareTo(Object)}
         * added in https://github.com/locationtech/jts/pull/399
         * 
         * See https://trac.osgeo.org/geos/ticket/1051
         */
        [Test]
        public void TestSegmentNodeOrderingForSnappedNodes()
        {
            CheckNoding(
                "LINESTRING (655103.6628454948 1794805.456674405, 655016.20226 1794940.10998, 655014.8317182435 1794941.5196832407)",
                "MULTIPOINT((655016.29615051334 1794939.965427252), (655016.20226531825 1794940.1099718122), (655016.20226 1794940.10998), (655016.20225819293 1794940.1099794197))",
                new int[] {0, 0, 1, 1},
                "MULTILINESTRING ((655014.8317182435 1794941.5196832407, 655016.2022581929 1794940.1099794197), (655016.2022581929 1794940.1099794197, 655016.20226 1794940.10998), (655016.20226 1794940.10998, 655016.2022653183 1794940.1099718122), (655016.2022653183 1794940.1099718122, 655016.2961505133 1794939.965427252), (655016.2961505133 1794939.965427252, 655103.6628454948 1794805.456674405))");
        }

        private void CheckNoding(string wktLine, string wktNodes, int[] segmentIndex, string wktExpected)
        {
            var line = Read(wktLine);
            var pts = Read(wktNodes);

            var nss = new NodedSegmentString(line.Coordinates, null);
            var node = pts.Coordinates;

            for (int i = 0; i < node.Length; i++)
            {
                nss.AddIntersection(node[i], segmentIndex[i]);
            }

            var nodedSS = NodingTestUtility.GetNodedSubstrings(nss);
            var result = NodingTestUtility.ToLines(nodedSS, line.Factory);
            //System.out.println(result);
            var expected = Read(wktExpected);
            CheckEqual(expected, result);
        }

    }

}
