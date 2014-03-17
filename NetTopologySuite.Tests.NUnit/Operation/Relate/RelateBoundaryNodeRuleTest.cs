using System;
using GeoAPI.Geometries;
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
    [TestFixtureAttribute]
    public class RelateBoundaryNodeRuleTest
    {
        private GeometryFactory fact;
        private WKTReader rdr;

        public RelateBoundaryNodeRuleTest()
        {
            fact = new GeometryFactory();
            rdr = new WKTReader(fact);
        }

        [TestAttribute]
        public void TestMultiLineStringSelfIntTouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((20 20, 100 100, 100 20, 20 100), (60 60, 60 140))";
            String b = "LINESTRING (60 60, 20 60)";

            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F00102");
        }

        [TestAttribute]
        public void TestLineStringSelfIntTouchAtEndpoint()
        {
            String a = "LINESTRING (20 20, 100 100, 100 20, 20 100)";
            String b = "LINESTRING (60 60, 20 60)";

            // results for both rules are the same
            RunRelateTest(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FF0102");
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "F01FF0102");
        }

        [TestAttribute]
        public void TestMultiLineStringTouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((0 0, 10 10), (10 10, 20 20))";
            String b = "LINESTRING (10 10, 20 0)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "FF1F00102");
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            //    runRelateTest(a, b,  BoundaryNodeRule.MULTIVALENT_ENDPOINT_BOUNDARY_RULE,  "0F1FFF1F2"    );
        }

        [TestAttribute]
        public void TestLineRingTouchAtEndpoints()
        {
            String a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            String b = "LINESTRING (20 20, 20 100)";

            // under Mod2, A has no boundary - A.int / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.OGC_SFS_BOUNDARY_RULE,   "F01FFF102"    );
            // under EndPoint, A has a boundary node - A.bdy / B.bdy = 0
            //    runRelateTest(a, b,  BoundaryNodeRule.ENDPOINT_BOUNDARY_RULE,  "FF1F0F102"    );
            // under MultiValent, A has a boundary node but B does not - A.bdy / B.bdy = F and A.int
            RunRelateTest(a, b, BoundaryNodeRules.MultivalentEndpointBoundaryRule, "0F1FFF1F2");
        }

        [TestAttribute]
        public void TestLineRingTouchAtEndpointAndInterior()
        {
            String a = "LINESTRING (20 100, 20 220, 120 100, 20 100)";
            String b = "LINESTRING (20 20, 40 100)";

            // this is the same result as for the above test
            RunRelateTest(a, b, BoundaryNodeRules.OgcSfsBoundaryRule, "F01FFF102");
            // this result is different - the A node is now on the boundary, so A.bdy/B.ext = 0
            RunRelateTest(a, b, BoundaryNodeRules.EndpointBoundaryRule, "F01FF0102");
        }

        void RunRelateTest(String wkt1, String wkt2, IBoundaryNodeRule bnRule, String expectedIM)
        {
            IGeometry g1 = rdr.Read(wkt1);
            IGeometry g2 = rdr.Read(wkt2);
            IntersectionMatrix im = RelateOp.Relate(g1, g2, bnRule);
            String imStr = im.ToString();
            Console.WriteLine(imStr);
            Assert.IsTrue(im.Matches(expectedIM));
        }
    }
}