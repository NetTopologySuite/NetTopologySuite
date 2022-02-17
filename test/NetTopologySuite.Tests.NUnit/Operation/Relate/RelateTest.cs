using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Relate;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Relate
{
    /**
     * Tests {@link Geometry#relate}.
     *
     * @author Martin Davis
     * @version 1.7
     */

    public class RelateTest : GeometryTestCase
    {
        /**
         * From GEOS #572
         *
         * The cause is that the longer line nodes the single-segment line.
         * The node then tests as not lying precisely on the original longer line.
         *
         * @throws Exception
         */

        [Test, Ignore("Known bug")]
        public void TestContainsIncorrectIntersectionMatrix()
        {
            string a = "LINESTRING (1 0, 0 2, 0 0, 2 2)";
            string b = "LINESTRING (0 0, 2 2)";
            RunRelateTest(a, b, "101F00FF2");
        }

        /**
         * Tests case where segments intersect properly, but computed intersection point
         * snaps to a boundary endpoint due to roundoff.
         * Fixed by detecting that computed intersection snapped to a boundary node.
         * 
         * See https://lists.osgeo.org/pipermail/postgis-users/2022-February/045266.html
         */
        [Test]
        public void TestIntersectsSnappedEndpoint1()
        {
            string a = "LINESTRING (-29796.696826656284 138522.76848210802, -29804.3911369969 138519.3504205817)";
            string b = "LINESTRING (-29802.795222153436 138520.05937757515, -29802.23305474065 138518.7938969792)";
            RunRelateTest(a, b, "F01FF0102");
        }

        /**
         * Tests case where segments intersect properly, but computed intersection point
         * snaps to a boundary endpoint due to roundoff.
         * Fixed by detecting that computed intersection snapped to a boundary node.
         * 
         * See https://lists.osgeo.org/pipermail/postgis-users/2022-February/045277.html
         */
        [Test]
        public void TestIntersectsSnappedEndpoint2()
        {
            string a = "LINESTRING (-57.2681216 49.4063466, -57.267725199999994 49.406617499999996, -57.26747895046037 49.406750916517765)";
            string b = "LINESTRING (-57.267475399999995 49.4067465, -57.2675701 49.406864299999995, -57.267989 49.407135399999994)";
            RunRelateTest(a, b, "FF10F0102");
        }

        private void RunRelateTest(string wkt1, string wkt2, string expectedIM)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            var im = RelateOp.Relate(g1, g2);
            string imStr = im.ToString();
            //TestContext.WriteLine("expected: {0}", expectedIM);
            //TestContext.WriteLine("result:   {0}", imStr);
            Assert.IsTrue(im.Matches(expectedIM));
        }
    }
}
