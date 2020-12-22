using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Relate;
using NUnit.Framework;

/// <summary>
/// Tests <see cref="Geometry.Relate" /> with different <see cref="BoundaryNodeRule" />s.
/// </summary>
/// <author>Martin Davis</author>
namespace NetTopologySuite.Tests.NUnit.Operation.Relate
{
    [TestFixture]
    public class RelateBoundaryNodeRuleTest
    {
        private readonly WKTReader _rdr;

        public RelateBoundaryNodeRuleTest()
        {
            _rdr = new WKTReader();
        }

        [Test]
        public void TestMultiLineStringSelfIntTouchAtEndpoint()
        {
            string a = "MULTILINESTRING ((20 20, 100 100, 100 20, 20 100), (60 60, 60 140))";
            string b = "LINESTRING (60 60, 20 60)";

            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F00102");
        }

        [Test]
        public void TestLineStringSelfIntTouchAtEndpoint()
        {
            string a = "LINESTRING (20 20, 100 100, 100 20, 20 100)";
            string b = "LINESTRING (60 60, 20 60)";

            // results for both rules are the same
            RunRelateTest(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FF0102");
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "F01FF0102");
        }

        [Test]
        public void TestMultiLineStringTouchAtEndpoint()
        {
            string a = "MULTILINESTRING ((0 0, 10 10), (10 10, 20 20))";
            string b = "LINESTRING (10 10, 20 0)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F00102");
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            //    runRelateTest(a, b,  BoundaryNodeRule.MULTIVALENT_ENDPOINT_BOUNDARY_RULE,  "0F1FFF1F2"    );
        }

        [Test]
        public void TestLineRingTouchAtEndpoints()
        {
            string a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            string b = "LINESTRING (20 20, 20 100)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.ENDPOINT_BOUNDARY_RULE,  "FF1F0F102"    );
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            RunRelateTest(a, b, BoundaryNodeRules.MultivalentEndpointBoundaryRule, "0F1FFF1F2");
        }

        [Test]
        public void TestLineRingTouchAtEndpointAndInterior()
        {
            string a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            string b = "LINESTRING (20 20, 40 100)";

            // this is the same result as for the above test
            RunRelateTest(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FFF102");
            // this result is different - the A node is now on the boundary, so A.bdy/B.ext = 0
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "F01FF0102");
        }

        void RunRelateTest(string wkt1, string wkt2, IBoundaryNodeRule bnRule, string expectedIM)
        {
            var g1 = _rdr.Read(wkt1);
            var g2 = _rdr.Read(wkt2);
            var im = RelateOp.Relate(g1, g2, bnRule);
            string imStr = im.ToString();
            //TestContext.WriteLine(imStr);
            Assert.IsTrue(im.Matches(expectedIM));
        }
    }
}
