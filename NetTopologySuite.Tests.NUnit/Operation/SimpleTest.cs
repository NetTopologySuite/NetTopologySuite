using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation
{
    /**
     * Tests {@link IsSimpleOp} with different {@link BoundaryNodeRule}s.
     *
     * @author Martin Davis
     * @version 1.7
     */
    public class IsSimpleTest
    {
        private const double Tolerance = 0.00005;

        private static readonly IGeometryFactory fact = new GeometryFactory();
        private static readonly WKTReader rdr = new WKTReader(fact);

        ///<summary>2 LineStrings touching at an endpoint</summary>
        [Test]
        public void Test2TouchAtEndpoint()
        {
            String a = "MULTILINESTRING((0 1, 1 1, 2 1), (0 0, 1 0, 2 1))";
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, true,
                    new Coordinate(2, 1));
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true,
                    new Coordinate(2, 1));
        }

        /**
         * 
         * 
         * @throws Exception
         */
        ///<summary>3 LineStrings touching at an endpoint.</summary>
        [Test]
        public void Test3TouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((0 1, 1 1, 2 1),   (0 0, 1 0, 2 1),  (0 2, 1 2, 2 1))";

            // rings are simple under all rules
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, true,
                    new Coordinate(2, 1));
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true,
                    new Coordinate(2, 1));
        }
        [Test]
        public void TestCross()
        {
            String a = "MULTILINESTRING ((20 120, 120 20), (20 20, 120 120))";
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, false,
                    new Coordinate(70, 70));
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, false,
                    new Coordinate(70, 70));
        }

        [Test]
        public void TestMultiLineStringWithRingTouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((100 100, 20 20, 200 20, 100 100), (100 200, 100 100))";

            // under Mod-2, the ring has no boundary, so the line intersects the interior ==> not simple
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, false, new Coordinate(100, 100));
            // under Endpoint, the ring has a boundary point, so the line does NOT intersect the interior ==> simple
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true);
        }
        [Test]
        public void TestRing()
        {
            String a = "LINESTRING (100 100, 20 20, 200 20, 100 100)";

            // rings are simple under all rules
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, true);
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true);
        }


        private static void RunIsSimpleTest(String wkt, IBoundaryNodeRule bnRule, bool expectedResult)
        {
            RunIsSimpleTest(wkt, bnRule, expectedResult, null);
        }

        private static void RunIsSimpleTest(String wkt, IBoundaryNodeRule bnRule, bool expectedResult,
                                     ICoordinate expectedLocation)
        {
            IGeometry g = rdr.Read(wkt);
            IsSimpleOp op = new IsSimpleOp(g, bnRule);
            bool isSimple = op.IsSimple();
            ICoordinate nonSimpleLoc = op.NonSimpleLocation;

            // if geom is not simple, should have a valid location
            Assert.IsTrue(isSimple || nonSimpleLoc != null);

            Assert.IsTrue(expectedResult == isSimple);

            if (!isSimple && expectedLocation != null)
            {
                Assert.IsTrue(expectedLocation.Distance(nonSimpleLoc) < Tolerance);
            }
        }
    }
}