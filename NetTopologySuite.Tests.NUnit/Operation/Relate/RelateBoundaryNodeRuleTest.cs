using System;
using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Operation.Relate;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Relate
{
    /**
     * Tests {@link Geometry#relate} with different {@link BoundaryNodeRule}s.
     *
     * @author Martin Davis
     * @version 1.7
     */
    [TestFixture]
    public class RelateBoundaryNodeRuleTest
    {

        [Test]
        public void TestMultiLineStringSelfIntTouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((20 20, 100 100, 100 20, 20 100), (60 60, 60 140))";
            String b = "LINESTRING (60 60, 20 60)";

            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelateTest(a, b, new EndPointBoundaryNodeRule(), "FF1F00102");
        }
        [Test]
        public void TestLineStringSelfIntTouchAtEndpoint()
        {
            String a = "LINESTRING (20 20, 100 100, 100 20, 20 100)";
            String b = "LINESTRING (60 60, 20 60)";

            // results for both rules are the same
            RunRelateTest(a, b, new Mod2BoundaryNodeRule(), "F01FF0102");
            RunRelateTest(a, b, new EndPointBoundaryNodeRule(), "F01FF0102");
        }

        [Test]
        public void TestMultiLineStringTouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((0 0, 10 10), (10 10, 20 20))";
            String b = "LINESTRING (10 10, 20 0)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    RunRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelateTest(a, b, new EndPointBoundaryNodeRule(), "FF1F00102");
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            //    RunRelateTest(a, b,  BoundaryNodeRule.MULTIVALENT_ENDPOINT_BOUNDARY_RULE,  "0F1FFF1F2"    );
        }
        [Test]
        public void TestLineRingTouchAtEndpoints()
        {
            String a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            String b = "LINESTRING (20 20, 20 100)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    RunRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            //    RunRelateTest(a, b,  BoundaryNodeRule.ENDPOINT_BOUNDARY_RULE,  "FF1F0F102"    );
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            RunRelateTest(a, b, new MultiValentEndPointBoundaryNodeRule(), "0F1FFF1F2");
        }
        [Test]
        public void TestLineRingTouchAtEndpointAndInterior()
        {
            String a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            String b = "LINESTRING (20 20, 40 100)";

            // this is the same result as for the above Test
            RunRelateTest(a, b, new Mod2BoundaryNodeRule(), "F01FFF102");
            // this result is different - the A node is now on the boundary, so A.bdy/B.ext = 0
            RunRelateTest(a, b, new EndPointBoundaryNodeRule(), "F01FF0102");
        }

        static void RunRelateTest(String wkt1, String wkt2, IBoundaryNodeRule bnRule, String expectedIM)
        {
            IGeometry<Coordinate> g1 = GeometryUtils.ReadWKT(wkt1);
            IGeometry<Coordinate> g2 = GeometryUtils.ReadWKT(wkt2);
            IntersectionMatrix im = RelateOp<Coordinate>.Relate(g1, g2, bnRule);
            String imStr = im.ToString();
            Console.WriteLine(imStr);
            Assert.IsTrue(im.Matches(expectedIM));
        }
    }
}