using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Relate;
using NUnit.Framework;
using System;

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
         * The original failure is caused by the intersection computed
         * during noding not lying exactly on each original line segment.
         * This is due to numerical error in the FP intersection algorithm.
         * This is fixed by using DD intersection calculation.
         *
         * @throws Exception
         */

        [Test]
        public void TestContainsNoding()
        {
            string a = "LINESTRING (1 0, 0 2, 0 0, 2 2)";
            string b = "LINESTRING (0 0, 2 2)";
            RunRelateTest(a, b, "101F00FF2");
        }

        /**
         * From GEOS https://github.com/libgeos/geos/issues/933
         * 
         * The original failure is caused by the intersection computed
         * during noding not lying exactly on each original line segment.
         * This is due to numerical error in the FP intersection algorithm.
         * This is fixed by using DD intersection calculation.
         */
        [Test]
        public void TestContainsNoding2()
        {
            string a = "MULTILINESTRING ((0 0, 1 1), (0.5 0.5, 1 0.1, -1 0.1))";
            string b = "LINESTRING (0 0, 1 1)";

            RunRelateTest(a, b, "1F1000FF2");
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
            //TestContext.WriteLine("expected: {0}", expectedIM);
            //TestContext.WriteLine("result:   {0}", imStr);
            Assert.That(im.ToString(), Is.EqualTo(expectedIM));
        }
    }
}
