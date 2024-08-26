using NetTopologySuite.Algorithm;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    /**
     * Tests {@link RelateNG} with {@link BoundaryNodeRule}s.
     *
     * @author Martin Davis
     * @version 1.7
     */
    public class RelateNGBoundaryNodeRuleTest : GeometryTestCase
    {
        [Test]
        public void TestMultiLineStringSelfIntTouchAtEndpoint()
        {
            const string a = "MULTILINESTRING ((20 20, 100 100, 100 20, 20 100), (60 60, 60 140))";
            const string b = "LINESTRING (60 60, 20 60)";

            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F00102");
        }

        [Test]
        public void TestLineStringSelfIntTouchAtEndpoint()
        {
            const string a = "LINESTRING (20 20, 100 100, 100 20, 20 100)";
            const string b = "LINESTRING (60 60, 20 60)";

            // results for both rules are the same
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FF0102");
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "F01FF0102");
        }

        [Test]
        public void TestMultiLineStringTouchAtEndpoint()
        {
            const string a = "MULTILINESTRING ((0 0, 10 10), (10 10, 20 20))";
            const string b = "LINESTRING (10 10, 20 0)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F00102");
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            //    runRelateTest(a, b,  BoundaryNodeRule.MULTIVALENT_ENDPOINT_BOUNDARY_RULE,  "0F1FFF1F2"    );
        }

        [Test]
        public void TestLineRingTouchAtEndpoints()
        {
            const string a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            const string b = "LINESTRING (20 20, 20 100)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FFF102");
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F0F102");
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            RunRelate(a, b, BoundaryNodeRules.MultivalentEndpointBoundaryRule, "FF10FF1F2");
        }

        [Test]
        public void TestLineRingTouchAtEndpointAndInterior()
        {
            const string a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            const string b = "LINESTRING (20 20, 40 100)";

            // this is the same result as for the above test
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FFF102");
            // this result is different - the A node is now on the boundary, so A.bdy/B.ext = 0
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "F01FF0102");
        }

        [Test]
        public void TestPolygonEmptyRing()
        {
            const string a = "POLYGON EMPTY";
            const string b = "LINESTRING (20 100, 20 220, 120 100, 20 100)";

            // closed line has no boundary under SFS rule
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "FFFFFF1F2");

            // closed line has boundary under ENDPOINT rule
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FFFFFF102");
        }

        [Test]
        public void TestPolygonEmptyMultiLineStringClosed()
        {
            const string a = "POLYGON EMPTY";
            const string b = "MULTILINESTRING ((0 0, 0 1), (0 1, 1 1, 1 0, 0 0))";

            // closed line has no boundary under SFS rule
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "FFFFFF1F2");

            // closed line has boundary under ENDPOINT rule
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FFFFFF102");
        }

        [Test]
        public void TestPolygonEqualRotated()
        {
            const string a = "POLYGON ((0 0, 140 0, 140 140, 0 140, 0 0))";
            const string b = "POLYGON ((140 0, 0 0, 0 140, 140 140, 140 0))";

            // BNR only considers linear endpoints, so results are equal for all rules
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "2FFF1FFF2");
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "2FFF1FFF2");
            RunRelate(a, b, BoundaryNodeRules.MonoValentEndpointBoundaryRule, "2FFF1FFF2");
            RunRelate(a, b, BoundaryNodeRules.MultivalentEndpointBoundaryRule, "2FFF1FFF2");
        }

        [Test]
        public void TestLineStringInteriorTouchMultivalent()
        {
            const string a = "POLYGON EMPTY";
            const string b = "MULTILINESTRING ((0 0, 0 1), (0 1, 1 1, 1 0, 0 0))";

            // closed line has no boundary under SFS rule
            RunRelate(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "FFFFFF1F2");

            // closed line has boundary under ENDPOINT rule
            RunRelate(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FFFFFF102");
        }


        private void RunRelate(string wkt1, string wkt2, IBoundaryNodeRule bnRule, string expectedIM)
        {
            var g1 = Read(wkt1);
            var g2 = Read(wkt2);
            var im = NetTopologySuite.Operation.RelateNG.RelateNG.Relate(g1, g2, bnRule);
            Assert.That(im.Matches(expectedIM), $"Expected {expectedIM}, found {im}");
        }

    }
}
