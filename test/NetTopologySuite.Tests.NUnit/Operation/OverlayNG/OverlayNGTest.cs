using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGTest : GeometryTestCase
    {
        [Test]
        public void TestEmptyAPolygonIntersection()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyBIntersection()
        {
            var a = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var b = Read("POLYGON EMPTY");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyABIntersection()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON EMPTY");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyADifference()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON EMPTY");
            var actual = Difference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyAUnion()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyASymDifference()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var actual = SymDifference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyLinePolygonIntersection()
        {
            var a = Read("LINESTRING EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("LINESTRING EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyLinePolygonDifference()
        {
            var a = Read("LINESTRING EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("LINESTRING EMPTY");
            var actual = Difference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyPointPolygonIntersection()
        {
            var a = Read("POINT EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POINT EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestDisjointIntersection()
        {
            var a = Read("POLYGON ((60 90, 90 90, 90 60, 60 60, 60 90))");
            var b = Read("POLYGON ((200 300, 300 300, 300 200, 200 200, 200 300))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestDisjointIntersectionNoOpt()
        {
            var a = Read("POLYGON ((60 90, 90 90, 90 60, 60 60, 60 90))");
            var b = Read("POLYGON ((200 300, 300 300, 300 200, 200 200, 200 300))");
            var expected = Read("POLYGON EMPTY");
            var actual = IntersectionNoOpt(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestPolygonPolygonWithLineTouchIntersection()
        {
            var a = Read("POLYGON ((360 200, 220 200, 220 180, 300 180, 300 160, 300 140, 360 200))");
            var b = Read("MULTIPOLYGON (((280 180, 280 160, 300 160, 300 180, 280 180)), ((220 230, 240 230, 240 180, 220 180, 220 230)))");
            var expected = Read("POLYGON ((220 200, 240 200, 240 180, 220 180, 220 200))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestBoxTriIntersection()
        {
            var a = Read("POLYGON ((0 6, 4 6, 4 2, 0 2, 0 6))");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON ((3 2, 1 2, 2 5, 3 2))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestBoxTriUnion()
        {
            var a = Read("POLYGON ((0 6, 4 6, 4 2, 0 2, 0 6))");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON ((0 6, 4 6, 4 2, 3 2, 3 0, 1 0, 1 2, 0 2, 0 6))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void Test2spikesIntersection()
        {
            var a = Read("POLYGON ((0 100, 40 100, 40 0, 0 0, 0 100))");
            var b = Read("POLYGON ((70 80, 10 80, 60 50, 11 20, 69 11, 70 80))");
            var expected = Read("MULTIPOLYGON (((40 80, 40 62, 10 80, 40 80)), ((40 38, 40 16, 11 20, 40 38)))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void Test2spikesUnion()
        {
            var a = Read("POLYGON ((0 100, 40 100, 40 0, 0 0, 0 100))");
            var b = Read("POLYGON ((70 80, 10 80, 60 50, 11 20, 69 11, 70 80))");
            var expected = Read("POLYGON ((0 100, 40 100, 40 80, 70 80, 69 11, 40 16, 40 0, 0 0, 0 100), (40 62, 40 38, 60 50, 40 62))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestTriBoxIntersection()
        {
            var a = Read("POLYGON ((68 35, 35 42, 40 9, 68 35))");
            var b = Read("POLYGON ((20 60, 50 60, 50 30, 20 30, 20 60))");
            var expected = Read("POLYGON ((37 30, 35 42, 50 39, 50 30, 37 30))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestNestedShellsIntersection()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("POLYGON ((120 180, 180 180, 180 120, 120 120, 120 180))");
            var expected = Read("POLYGON ((120 180, 180 180, 180 120, 120 120, 120 180))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestNestedShellsUnion()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("POLYGON ((120 180, 180 180, 180 120, 120 120, 120 180))");
            var expected = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestATouchingNestedPolyUnion()
        {
            var a = Read("MULTIPOLYGON (((0 200, 200 200, 200 0, 0 0, 0 200), (50 50, 190 50, 50 200, 50 50)), ((60 100, 100 60, 50 50, 60 100)))");
            var b = Read("POLYGON ((135 176, 180 176, 180 130, 135 130, 135 176))");
            var expected = Read("MULTIPOLYGON (((0 0, 0 200, 50 200, 200 200, 200 0, 0 0), (50 50, 190 50, 50 200, 50 50)), ((50 50, 60 100, 100 60, 50 50)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestTouchingPolyDifference()
        {
            var a = Read("POLYGON ((200 200, 200 0, 0 0, 0 200, 200 200), (100 100, 50 100, 50 200, 100 100))");
            var b = Read("POLYGON ((150 100, 100 100, 150 200, 150 100))");
            var expected = Read("MULTIPOLYGON (((0 0, 0 200, 50 200, 50 100, 100 100, 150 100, 150 200, 200 200, 200 0, 0 0)), ((50 200, 150 200, 100 100, 50 200)))");
            var actual = Difference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestTouchingHoleUnion()
        {
            var a = Read("POLYGON ((100 300, 300 300, 300 100, 100 100, 100 300), (200 200, 150 200, 200 300, 200 200))");
            var b = Read("POLYGON ((130 160, 260 160, 260 120, 130 120, 130 160))");
            var expected = Read("POLYGON ((100 100, 100 300, 200 300, 300 300, 300 100, 100 100), (150 200, 200 200, 200 300, 150 200))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestTouchingMultiHoleUnion()
        {
            var a = Read("POLYGON ((100 300, 300 300, 300 100, 100 100, 100 300), (200 200, 150 200, 200 300, 200 200), (250 230, 216 236, 250 300, 250 230), (235 198, 300 200, 237 175, 235 198))");
            var b = Read("POLYGON ((130 160, 260 160, 260 120, 130 120, 130 160))");
            var expected = Read("POLYGON ((100 300, 200 300, 250 300, 300 300, 300 200, 300 100, 100 100, 100 300), (200 300, 150 200, 200 200, 200 300), (250 300, 216 236, 250 230, 250 300), (300 200, 235 198, 237 175, 300 200))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestBoxLineIntersection()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("LINESTRING (50 150, 150 150)");
            var expected = Read("LINESTRING (100 150, 150 150)");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestBoxLineUnion()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("LINESTRING (50 150, 150 150)");
            var expected = Read("GEOMETRYCOLLECTION (LINESTRING (50 150, 100 150), POLYGON ((100 200, 200 200, 200 100, 100 100, 100 150, 100 200)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestAdjacentBoxesIntersection()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("POLYGON ((300 200, 300 100, 200 100, 200 200, 300 200))");
            var expected = Read("LINESTRING (200 100, 200 200)");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestAdjacentBoxesUnion()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("POLYGON ((300 200, 300 100, 200 100, 200 200, 300 200))");
            var expected = Read("POLYGON ((100 100, 100 200, 200 200, 300 200, 300 100, 200 100, 100 100))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestCollapseBoxGoreIntersection()
        {
            var a = Read("MULTIPOLYGON (((1 1, 5 1, 5 0, 1 0, 1 1)), ((1 1, 5 2, 5 4, 1 4, 1 1)))");
            var b = Read("POLYGON ((1 0, 1 2, 2 2, 2 0, 1 0))");
            var expected = Read("POLYGON ((2 0, 1 0, 1 1, 1 2, 2 2, 2 1, 2 0))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestCollapseBoxGoreUnion()
        {
            var a = Read("MULTIPOLYGON (((1 1, 5 1, 5 0, 1 0, 1 1)), ((1 1, 5 2, 5 4, 1 4, 1 1)))");
            var b = Read("POLYGON ((1 0, 1 2, 2 2, 2 0, 1 0))");
            var expected = Read("POLYGON ((2 0, 1 0, 1 1, 1 2, 1 4, 5 4, 5 2, 2 1, 5 1, 5 0, 2 0))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }
        [Test]
        public void TestSnapBoxGoreIntersection()
        {
            var a = Read("MULTIPOLYGON (((1 1, 5 1, 5 0, 1 0, 1 1)), ((1 1, 5 2, 5 4, 1 4, 1 1)))");
            var b = Read("POLYGON ((4 3, 5 3, 5 0, 4 0, 4 3))");
            var expected = Read("MULTIPOLYGON (((4 3, 5 3, 5 2, 4 2, 4 3)), ((4 0, 4 1, 5 1, 5 0, 4 0)))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestSnapBoxGoreUnion()
        {
            var a = Read("MULTIPOLYGON (((1 1, 5 1, 5 0, 1 0, 1 1)), ((1 1, 5 2, 5 4, 1 4, 1 1)))");
            var b = Read("POLYGON ((4 3, 5 3, 5 0, 4 0, 4 3))");
            var expected = Read("POLYGON ((1 1, 1 4, 5 4, 5 3, 5 2, 5 1, 5 0, 4 0, 1 0, 1 1), (1 1, 4 1, 4 2, 1 1))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestCollapseTriBoxIntersection()
        {
            var a = Read("POLYGON ((1 2, 1 1, 9 1, 1 2))");
            var b = Read("POLYGON ((9 2, 9 1, 8 1, 8 2, 9 2))");
            var expected = Read("POINT (8 1)");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestCollapseTriBoxUnion()
        {
            var a = Read("POLYGON ((1 2, 1 1, 9 1, 1 2))");
            var b = Read("POLYGON ((9 2, 9 1, 8 1, 8 2, 9 2))");
            var expected = Read("MULTIPOLYGON (((1 1, 1 2, 8 1, 1 1)), ((8 1, 8 2, 9 2, 9 1, 8 1)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        /**
         * Fails because polygon A collapses totally, but one
         * L edge is still labelled with location A:iL due to being located
         * inside original A polygon by PiP test for incomplete edges.
         * That edge is then marked as in-result-area, but 
         * it is the only edge marked in-result, so result ring can't
         * be formed because ring is incomplete
         */
        [Test]
        public void TestCollapseAIncompleteRingUnion()
        {
            var a = Read("POLYGON ((0.9 1.7, 1.3 1.4, 2.1 1.4, 2.1 0.9, 1.3 0.9, 0.9 0, 0.9 1.7))");
            var b = Read("POLYGON ((1 3, 3 3, 3 1, 1.3 0.9, 1 0.4, 1 3))");
            var expected = Read("POLYGON ((1 1, 1 2, 1 3, 3 3, 3 1, 2 1, 1 1))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        /**
         * Fails because edge of B is computed as Interior to A because it
         * is checked against full precision input, rather than collapsed linework.
         * 
         * Probably need to determine location against output rings
         */
        [Test]
        public void TestCollapseResultShouldHavePolygonUnion()
        {
            var a = Read("POLYGON ((1 3.3, 1.3 1.4, 3.1 1.4, 3.1 0.9, 1.3 0.9, 1 -0.2, 0.8 1.3, 1 3.3))");
            var b = Read("POLYGON ((1 2.9, 2.9 2.9, 2.9 1.3, 1.7 1, 1.3 0.9, 1 0.4, 1 2.9))");
            var expected = Read("POLYGON ((1 1, 1 3, 3 3, 3 1, 2 1, 1 1))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        /**
         * Fails because current isResultAreaEdge does not accept L edges as result area boundary
         */
        [Test]
        public void TestCollapseHoleAlongEdgeOfBIntersection()
        {
            var a = Read("POLYGON ((0 3, 3 3, 3 0, 0 0, 0 3), (1 1.2, 1 1.1, 2.3 1.1, 1 1.2))");
            var b = Read("POLYGON ((1 1, 2 1, 2 0, 1 0, 1 1))");
            var expected = Read("POLYGON ((1 1, 2 1, 2 0, 1 0, 1 1))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        /**
         * Fails because A holes collapse to L edges, and are not computed as in Int of A,
         * so are not included as result area edges.
         */
        [Test]
        public void TestCollapseHolesAlongAllEdgesOfBIntersection()
        {
            var a = Read("POLYGON ((0 3, 3 3, 3 0, 0 0, 0 3), (1 2.2, 1 2.1, 2 2.1, 1 2.2), (2.1 2, 2.2 2, 2.1 1, 2.1 2), (2 0.9, 2 0.8, 1 0.9, 2 0.9), (0.9 1, 0.8 1, 0.9 2, 0.9 1))");
            var b = Read("POLYGON ((1 2, 2 2, 2 1, 1 1, 1 2))");
            var expected = Read("POLYGON ((1 2, 2 2, 2 1, 1 1, 1 2))");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestVerySmallBIntersection()
        {
            var a = Read("POLYGON ((2.526855443750341 48.82324221874807, 2.5258255 48.8235855, 2.5251389 48.8242722, 2.5241089 48.8246155, 2.5254822 48.8246155, 2.5265121 48.8242722, 2.526855443750341 48.82324221874807))");
            var b = Read("POLYGON ((2.526512100000002 48.824272199999996, 2.5265120999999953 48.8242722, 2.5265121 48.8242722, 2.526512100000002 48.824272199999996))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 100000000);
            CheckEqual(expected, actual);
        }

        /**
         * Currently noding is incorrect, producing one 2pt edge which is coincident
         * with a 3-pt edge.  The EdgeMerger doesn't check that merged edges are identical,
         * so merges the 3pt edge into the 2-pt edge.
         * FIXED by better noding.
         */
        [Test]
        public void TestEdgeDisappears()
        {
            var a = Read("LINESTRING (2.1279144 48.8445282, 2.126884443750796 48.84555818124935, 2.1268845 48.8455582, 2.1268845 48.8462448)");
            var b = Read("LINESTRING EMPTY");
            var expected = Read("LINESTRING EMPTY");
            var actual = Intersection(a, b, 1000000);
            CheckEqual(expected, actual);
        }

        /**
         * Probably due to B collapsing completely and disconnected edges being located incorrectly in B interior.
         * Have seen other cases of this as well.
         * Also - a B edge is marked as a Hole, which is incorrect.
         * 
         * FIXED - copy-paste error in Edge.mergedRingRole
         */
        [Test]
        public void TestBcollapseLocateIssue()
        {
            var a = Read("POLYGON ((2.3442078 48.9331054, 2.3435211 48.9337921, 2.3428345 48.9358521, 2.3428345 48.9372253, 2.3433495 48.9370537, 2.3440361 48.936367, 2.3442078 48.9358521, 2.3442078 48.9331054))");
            var b = Read("POLYGON ((2.3442078 48.9331054, 2.3435211 48.9337921, 2.3433494499999985 48.934307100000005, 2.3438644 48.9341354, 2.3442078 48.9331055, 2.3442078 48.9331054))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1000);
            CheckEqual(expected, actual);
        }

        /**
         * Fails because a component of B collapses completely and labelling is wrong.
         * Labelling marks a single collapsed edge as B:i.
         * Edge is only connected to two other edges both marked B:e.
         * B:i edge is included in area result edges, and faild because it does not form a ring.
         * 
         * Perhaps a fix is to ignore connected single Bi edges which do not form a ring?
         * This may be dangerous since it may hide other labelling problems?
         * 
         * FIXED by computing location of both edge endpoints.
         */
        [Test]
        public void TestBcollapseEdgeLabeledInterior()
        {
            var a = Read("POLYGON ((2.384376506250038 48.91765596875102, 2.3840332 48.916626, 2.3840332 48.9138794, 2.3833466 48.9118195, 2.3812866 48.9111328, 2.37854 48.9111328, 2.3764801 48.9118195, 2.3723602 48.9159393, 2.3703003 48.916626, 2.3723602 48.9173126, 2.3737335 48.9186859, 2.3757935 48.9193726, 2.3812866 48.9193726, 2.3833466 48.9186859, 2.384376506250038 48.91765596875102))");
            var b = Read("MULTIPOLYGON (((2.3751067666731345 48.919143677778855, 2.3757935 48.9193726, 2.3812866 48.9193726, 2.3812866 48.9179993, 2.3809433 48.9169693, 2.3799133 48.916626, 2.3771667 48.916626, 2.3761368 48.9169693, 2.3754501 48.9190292, 2.3751067666731345 48.919143677778855)), ((2.3826108673454116 48.91893115612326, 2.3833466 48.9186859, 2.3840331750033394 48.91799930833141, 2.3830032 48.9183426, 2.3826108673454116 48.91893115612326)))");
            var expected = Read("POLYGON ((2.375 48.91833333333334, 2.375 48.92, 2.381666666666667 48.92, 2.381666666666667 48.91833333333334, 2.381666666666667 48.916666666666664, 2.38 48.916666666666664, 2.3766666666666665 48.916666666666664, 2.375 48.91833333333334))");
            var actual = Intersection(a, b, 600);
            CheckEqual(expected, actual);
        }

        /**
         * This failure is due to B inverting due to an snapped intersection being added 
         * to a segment by a nearby vertex, and the snap vertex "jumped" across another segment.
         * This is because the nearby snap intersection tolerance in SnapIntersectionAdder was too large (FACTOR = 10).
         * 
         * FIXED by reducing the tolerance factor to 100.
         * 
         * However, it may be that there is no safe tolerance level?  
         * Perhaps there can always be situations where a snap intersection will jump across a segment?
         */
        [Test]
        public void TestBNearVertexSnappingCausesInversion()
        {
            var a = Read("POLYGON ((2.2494507 48.8864136, 2.2484207 48.8867569, 2.2477341 48.8874435, 2.2470474 48.8874435, 2.2463608 48.8853836, 2.2453308 48.8850403, 2.2439575 48.8850403, 2.2429276 48.8853836, 2.2422409 48.8860703, 2.2360611 48.8970566, 2.2504807 48.8956833, 2.2494507 48.8864136))");
            var b = Read("POLYGON ((2.247734099999997 48.8874435, 2.2467041 48.8877869, 2.2453308 48.8877869, 2.2443008 48.8881302, 2.243957512499544 48.888473487500455, 2.2443008 48.8888168, 2.2453308 48.8891602, 2.2463608 48.8888168, 2.247734099999997 48.8874435))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 200);
            CheckEqual(expected, actual);
        }

        /**
         * Failure due to B hole collapsing and edges being labeled Exterior.
         * They are coincident with an A hole edge, but because labeled E are not
         * included in Intersection result.
         * This occurred because of a very subtle instance field update sequence bug 
         * in Edge.mergeEdge.
         */
        [Test]
        public void TestBCollapsedHoleEdgeLabelledExterior()
        {
            var a = Read("POLYGON ((309500 3477900, 309900 3477900, 309900 3477600, 309500 3477600, 309500 3477900), (309741.87561330193 3477680.6737848604, 309745.53718649445 3477677.607851833, 309779.0333599192 3477653.585555199, 309796.8051681937 3477642.143583868, 309741.87561330193 3477680.6737848604))");
            var b = Read("POLYGON ((309500 3477900, 309900 3477900, 309900 3477600, 309500 3477600, 309500 3477900), (309636.40806633036 3477777.2910157656, 309692.56085444096 3477721.966349552, 309745.53718649445 3477677.607851833, 309779.0333599192 3477653.585555199, 309792.0991800499 3477645.1734264474, 309779.03383125085 3477653.5853248164, 309745.53756275156 3477677.6076231804, 309692.5613257677 3477721.966119165, 309636.40806633036 3477777.2910157656))");
            var expected = Read("POLYGON ((309500 3477600, 309500 3477900, 309900 3477900, 309900 3477600, 309500 3477600), (309741.88 3477680.67, 309745.54 3477677.61, 309779.03 3477653.59, 309792.1 3477645.17, 309796.81 3477642.14, 309741.88 3477680.67))");
            var actual = Intersection(a, b, 100);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLineUnion()
        {
            var a = Read("LINESTRING (0 0, 1 1)");
            var b = Read("LINESTRING (1 1, 2 2)");
            var expected = Read("LINESTRING (0 0, 1 1, 2 2)");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLine2Union()
        {
            var a = Read("LINESTRING (0 0, 1 1, 0 1)");
            var b = Read("LINESTRING (1 1, 2 2, 3 3)");
            var expected = Read("MULTILINESTRING ((0 0, 1 1), (0 1, 1 1), (1 1, 2 2, 3 3))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLine3Union()
        {
            var a = Read("MULTILINESTRING ((0 1, 1 1), (2 2, 2 0))");
            var b = Read("LINESTRING (0 0, 1 1, 2 2, 3 3)");
            var expected = Read("MULTILINESTRING ((0 0, 1 1), (0 1, 1 1), (1 1, 2 2), (2 0, 2 2), (2 2, 3 3))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLine4Union()
        {
            var a = Read("LINESTRING (100 300, 200 300, 200 100, 100 100)");
            var b = Read("LINESTRING (300 300, 200 300, 200 300, 200 100, 300 100)");
            var expected = Read("MULTILINESTRING ((200 100, 100 100), (300 300, 200 300), (200 300, 200 100), (200 100, 300 100), (100 300, 200 300))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLineFigure8Union()
        {
            var a = Read("LINESTRING (5 1, 2 2, 5 3, 2 4, 5 5)");
            var b = Read("LINESTRING (5 1, 8 2, 5 3, 8 4, 5 5)");
            var expected = Read("MULTILINESTRING ((5 3, 2 2, 5 1, 8 2, 5 3), (5 3, 2 4, 5 5, 8 4, 5 3))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLineRingUnion()
        {
            var a = Read("LINESTRING (1 1, 5 5, 9 1)");
            var b = Read("LINESTRING (1 1, 9 1)");
            var expected = Read("LINESTRING (1 1, 5 5, 9 1, 1 1)");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestDisjointLinesRoundedIntersection()
        {
            var a = Read("LINESTRING (3 2, 3 4)");
            var b = Read("LINESTRING (1.1 1.6, 3.8 1.9)");
            var expected = Read("POINT (3 2)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestPolygonMultiLineUnion()
        {
            var a = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var b = Read("MULTILINESTRING ((150 250, 150 50), (250 250, 250 50))");
            var expected = Read("GEOMETRYCOLLECTION (LINESTRING (150 50, 150 100), LINESTRING (150 200, 150 250), LINESTRING (250 50, 250 250), POLYGON ((100 100, 100 200, 150 200, 200 200, 200 100, 150 100, 100 100)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLinePolygonUnion()
        {
            var a = Read("LINESTRING (50 150, 150 150)");
            var b = Read("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            var expected = Read("GEOMETRYCOLLECTION (LINESTRING (50 150, 100 150), POLYGON ((100 200, 200 200, 200 100, 100 100, 100 150, 100 200)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLinePolygonUnionAlongPolyBoundary()
        {
            var a = Read("LINESTRING (150 300, 250 300)");
            var b = Read("POLYGON ((100 400, 200 400, 200 300, 100 300, 100 400))");
            var expected = Read("GEOMETRYCOLLECTION (LINESTRING (200 300, 250 300), POLYGON ((200 300, 150 300, 100 300, 100 400, 200 400, 200 300)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLinePolygonIntersectionAlongPolyBoundary()
        {
            var a = Read("LINESTRING (150 300, 250 300)");
            var b = Read("POLYGON ((100 400, 200 400, 200 300, 100 300, 100 400))");
            var expected = Read("LINESTRING (200 300, 150 300)");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        //============================================================


        public static Geometry Difference(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Difference, pm);
        }

        public static Geometry SymDifference(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.SymDifference, pm);
        }

        public static Geometry Intersection(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Intersection, pm);
        }

        public static Geometry Union(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Union, pm);
        }

        public static Geometry Difference(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Difference, pm);
        }

        public static Geometry SymDifference(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.SymDifference, pm);
        }

        public static Geometry Intersection(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Intersection, pm);
        }

        public static Geometry Union(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Union, pm);
        }

        public static Geometry IntersectionNoOpt(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            var ov = new NetTopologySuite.Operation.OverlayNg.OverlayNG(a, b, pm, SpatialFunction.Intersection);
            ov.Optimized = false;
            return ov.GetResult();
        }
    }
}
