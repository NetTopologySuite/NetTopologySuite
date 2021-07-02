using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    ///<summary>
    /// Tests <see cref="NetTopologySuite.Operation.Valid.IsSimpleOp"/> with different <see cref="IBoundaryNodeRule"/>s.
    ///</summary>
    /// <author>Martin Davis</author>
    public class IsSimpleTest : GeometryTestCase
    {
        private const double Tolerance = 0.00005;

        ///<summary>
        /// 2 LineStrings touching at an endpoint
        ///</summary>
        [Test]
        public void Test2TouchAtEndpoint()
        {
            string a = "MULTILINESTRING((0 1, 1 1, 2 1), (0 0, 1 0, 2 1))";
            CheckIsSimple(a, BoundaryNodeRules.Mod2BoundaryRule, true,
                    new Coordinate(2, 1));
            CheckIsSimple(a, BoundaryNodeRules.EndpointBoundaryRule, true,
                    new Coordinate(2, 1));
        }

        ///<summary>3 LineStrings touching at an endpoint.</summary>
        [Test]
        public void Test3TouchAtEndpoint()
        {
            string a = "MULTILINESTRING ((0 1, 1 1, 2 1),   (0 0, 1 0, 2 1),  (0 2, 1 2, 2 1))";

            // rings are simple under all rules
            CheckIsSimple(a, BoundaryNodeRules.Mod2BoundaryRule, true,
                    new Coordinate(2, 1));
            CheckIsSimple(a, BoundaryNodeRules.EndpointBoundaryRule, true,
                    new Coordinate(2, 1));
        }
        [Test]
        public void TestCross()
        {
            string a = "MULTILINESTRING ((20 120, 120 20), (20 20, 120 120))";
            CheckIsSimple(a, BoundaryNodeRules.Mod2BoundaryRule, false,
                    new Coordinate(70, 70));
            CheckIsSimple(a, BoundaryNodeRules.EndpointBoundaryRule, false,
                    new Coordinate(70, 70));
        }

        [Test]
        public void TestMultiLineStringWithRingTouchAtEndpoint()
        {
            string a = "MULTILINESTRING ((100 100, 20 20, 200 20, 100 100), (100 200, 100 100))";

            // under Mod-2, the ring has no boundary, so the line intersects the interior ==> not simple
            CheckIsSimple(a, BoundaryNodeRules.Mod2BoundaryRule, false, new Coordinate(100, 100));
            // under Endpoint, the ring has a boundary point, so the line does NOT intersect the interior ==> simple
            CheckIsSimple(a, BoundaryNodeRules.EndpointBoundaryRule, true);
        }

        [Test]
        public void TestRing()
        {
            string a = "LINESTRING (100 100, 20 20, 200 20, 100 100)";

            // rings are simple under all rules
            CheckIsSimple(a, BoundaryNodeRules.Mod2BoundaryRule, true);
            CheckIsSimple(a, BoundaryNodeRules.EndpointBoundaryRule, true);
        }

        [Test]
        public void TestLinesAll()
        {
            CheckIsSimpleAll("MULTILINESTRING ((10 20, 90 20), (10 30, 90 30), (50 40, 50 10))",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((50 20), (50 30))");
        }

        [Test]
        public void TestPolygonAll()
        {
            CheckIsSimpleAll("POLYGON ((0 0, 7 0, 6 -1, 6 -0.1, 6 0.1, 3 5.9, 3 6.1, 3.1 6, 2.9 6, 0 0))",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((6 0), (3 6))");
        }

        [Test]
        public void TestMultiPointAll()
        {
            CheckIsSimpleAll("MULTIPOINT((1 1), (1 2), (1 2), (1 3), (1 4), (1 4), (1 5), (1 5))",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((1 2), (1 4), (1 5))");
        }

        [Test]
        public void TestGeometryCollectionAll()
        {
            CheckIsSimpleAll("GEOMETRYCOLLECTION(MULTILINESTRING ((10 20, 90 20), (10 30, 90 30), (50 40, 50 10)), " +
                             "MULTIPOINT((1 1), (1 2), (1 2), (1 3), (1 4), (1 4), (1 5), (1 5)), "+
                             "POLYGON ((0 0, 7 0, 6 -1, 6 -0.1, 6 0.1, 3 5.9, 3 6.1, 3.1 6, 2.9 6, 0 0))"+
                             ")",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((50 20), (50 30), (1 2), (1 4), (1 5), (6 0), (3 6))");
        }



        private void CheckIsSimple(string wkt, IBoundaryNodeRule bnRule, bool expectedResult)
        {
            CheckIsSimple(wkt, bnRule, expectedResult, null);
        }

        private void CheckIsSimple(string wkt, IBoundaryNodeRule bnRule, bool expectedResult, Coordinate expectedLocation)
        {
            var g = Read(wkt);
            var op = new NetTopologySuite.Operation.Valid.IsSimpleOp(g, bnRule);
            bool isSimple = op.IsSimple();
            var nonSimpleLoc = op.NonSimpleLocation;

            // if geom is not simple, should have a valid location
            Assert.That(isSimple || nonSimpleLoc != null);

            Assert.That(expectedResult == isSimple);

            if (!isSimple && expectedLocation != null)
            {
                Assert.That(expectedLocation.Distance(nonSimpleLoc) < Tolerance);
            }
        }

        private void CheckIsSimpleAll(string wkt, IBoundaryNodeRule bnRule,
            string wktExpectedPts)
        {
            var g = Read(wkt);
            var op = new NetTopologySuite.Operation.Valid.IsSimpleOp(g, bnRule);
            op.FindAllLocations = true;
            op.IsSimple();
            var nonSimpleCoords = op.NonSimpleLocations;

            var nsPts = g.Factory.CreateMultiPointFromCoords(CoordinateArrays.ToCoordinateArray(nonSimpleCoords));
            var expectedPts = Read(wktExpectedPts);
            CheckEqual(expectedPts, nsPts);
        }
    }
}
