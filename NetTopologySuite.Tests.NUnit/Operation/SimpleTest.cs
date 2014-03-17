using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation
{
    ///<summary>
    /// Tests <see cref="IsSimpleOp"/> with different <see cref="IBoundaryNodeRule"/>s.
    ///</summary>
    /// <author>Martin Davis</author>
    public class IsSimpleTest
    {
        private const double Tolerance = 0.00005;

        private static readonly IGeometryFactory Fact = new GeometryFactory();
        private static readonly WKTReader rdr = new WKTReader(Fact);

        ///<summary>
        /// 2 LineStrings touching at an endpoint
        ///</summary>
        [TestAttribute]
        public void Test2TouchAtEndpoint()
        {
            String a = "MULTILINESTRING((0 1, 1 1, 2 1), (0 0, 1 0, 2 1))";
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, true,
                    new Coordinate(2, 1));
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true,
                    new Coordinate(2, 1));
        }

        ///<summary>3 LineStrings touching at an endpoint.</summary>
        [TestAttribute]
        public void Test3TouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((0 1, 1 1, 2 1),   (0 0, 1 0, 2 1),  (0 2, 1 2, 2 1))";

            // rings are simple under all rules
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, true,
                    new Coordinate(2, 1));
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true,
                    new Coordinate(2, 1));
        }
        [TestAttribute]
        public void TestCross()
        {
            String a = "MULTILINESTRING ((20 120, 120 20), (20 20, 120 120))";
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, false,
                    new Coordinate(70, 70));
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, false,
                    new Coordinate(70, 70));
        }

        [TestAttribute]
        public void TestMultiLineStringWithRingTouchAtEndpoint()
        {
            String a = "MULTILINESTRING ((100 100, 20 20, 200 20, 100 100), (100 200, 100 100))";

            // under Mod-2, the ring has no boundary, so the line intersects the interior ==> not simple
            RunIsSimpleTest(a, BoundaryNodeRules.Mod2BoundaryRule, false, new Coordinate(100, 100));
            // under Endpoint, the ring has a boundary point, so the line does NOT intersect the interior ==> simple
            RunIsSimpleTest(a, BoundaryNodeRules.EndpointBoundaryRule, true);
        }
        [TestAttribute]
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
                                     Coordinate expectedLocation)
        {
            IGeometry g = rdr.Read(wkt);
            IsSimpleOp op = new IsSimpleOp(g, bnRule);
            bool isSimple = op.IsSimple();
            Coordinate nonSimpleLoc = op.NonSimpleLocation;

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