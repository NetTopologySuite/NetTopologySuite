using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    internal class RelateNGTest : RelateNGTestCase
    {
        [Test]
        public void TestPointsDisjoint()
        {
            const string a = "POINT (0 0)";
            const string b = "POINT (1 1)";
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
            CheckEquals(a, b, false);
            CheckRelate(a, b, "FF0FFF0F2");
        }

        //======= P/P  =============

        [Test]
        public void TestPointsContained()
        {
            const string a = "MULTIPOINT (0 0, 1 1, 2 2)";
            const string b = "MULTIPOINT (1 1, 2 2)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckEquals(a, b, false);
            CheckRelate(a, b, "0F0FFFFF2");
        }

        [Test]
        public void TestPointsEqual()
        {
            const string a = "MULTIPOINT (0 0, 1 1, 2 2)";
            const string b = "MULTIPOINT (0 0, 1 1, 2 2)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckEquals(a, b, true);
        }

        [Test]
        public void TestValidateRelatePP_13()
        {
            const string a = "MULTIPOINT ((80 70), (140 120), (20 20), (200 170))";
            const string b = "MULTIPOINT ((80 70), (140 120), (80 170), (200 80))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckContainsWithin(b, a, false);
            CheckCoversCoveredBy(a, b, false);
            CheckOverlaps(a, b, true);
            CheckTouches(a, b, false);
        }

        //======= L/P  =============

        [Test]
        public void TestLinePointContains()
        {
            const string a = "LINESTRING (0 0, 1 1, 2 2)";
            const string b = "MULTIPOINT (0 0, 1 1, 2 2)";
            CheckRelate(a, b, "0F10FFFF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckContainsWithin(b, a, false);
            CheckCoversCoveredBy(a, b, true);
            CheckCoversCoveredBy(b, a, false);
        }

        [Test]
        public void TestLinePointOverlaps()
        {
            const string a = "LINESTRING (0 0, 1 1)";
            const string b = "MULTIPOINT (0 0, 1 1, 2 2)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckContainsWithin(b, a, false);
            CheckCoversCoveredBy(a, b, false);
            CheckCoversCoveredBy(b, a, false);
        }

        [Test]
        public void TestZeroLengthLinePoint()
        {
            const string a = "LINESTRING (0 0, 0 0)";
            const string b = "POINT (0 0)";
            CheckRelate(a, b, "0FFFFFFF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckContainsWithin(b, a, true);
            CheckCoversCoveredBy(a, b, true);
            CheckCoversCoveredBy(b, a, true);
            CheckEquals(a, b, true);
        }

        [Test]
        public void TestZeroLengthLineLine()
        {
            const string a = "LINESTRING (10 10, 10 10, 10 10)";
            const string b = "LINESTRING (10 10, 10 10)";
            CheckRelate(a, b, "0FFFFFFF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckContainsWithin(b, a, true);
            CheckCoversCoveredBy(a, b, true);
            CheckCoversCoveredBy(b, a, true);
            CheckEquals(a, b, true);
        }

        // tests bug involving checking for non-zero-length lines
        [Test]
        public void TestNonZeroLengthLinePoint()
        {
            const string a = "LINESTRING (0 0, 0 0, 9 9)";
            const string b = "POINT (1 1)";
            CheckRelate(a, b, "0F1FF0FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckContainsWithin(b, a, false);
            CheckCoversCoveredBy(a, b, true);
            CheckCoversCoveredBy(b, a, false);
            CheckEquals(a, b, false);
        }

        [Test]
        public void TestLinePointIntAndExt()
        {
            const string a = "MULTIPOINT((60 60), (100 100))";
            const string b = "LINESTRING(40 40, 80 80)";
            CheckRelate(a, b, "0F0FFF102");
        }

        //======= L/L  =============

        [Test]
        public void TestLinesCrossProper()
        {
            const string a = "LINESTRING (0 0, 9 9)";
            const string b = "LINESTRING(0 9, 9 0)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
        }

        [Test]
        public void TestLinesOverlap()
        {
            const string a = "LINESTRING (0 0, 5 5)";
            const string b = "LINESTRING(3 3, 9 9)";
            CheckIntersectsDisjoint(a, b, true);
            CheckTouches(a, b, false);
            CheckOverlaps(a, b, true);
        }

        [Test]
        public void TestLinesCrossVertex()
        {
            const string a = "LINESTRING (0 0, 8 8)";
            const string b = "LINESTRING(0 8, 4 4, 8 0)";
            CheckIntersectsDisjoint(a, b, true);
        }

        [Test]
        public void TestLinesTouchVertex()
        {
            const string a = "LINESTRING (0 0, 8 0)";
            const string b = "LINESTRING(0 8, 4 0, 8 8)";
            CheckIntersectsDisjoint(a, b, true);
        }

        [Test]
        public void TestLinesDisjointByEnvelope()
        {
            const string a = "LINESTRING (0 0, 9 9)";
            const string b = "LINESTRING(10 19, 19 10)";
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
        }

        [Test]
        public void TestLinesDisjoint()
        {
            const string a = "LINESTRING (0 0, 9 9)";
            const string b = "LINESTRING (4 2, 8 6)";
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
        }

        [Test]
        public void TestLinesClosedEmpty()
        {
            const string a = "MULTILINESTRING ((0 0, 0 1), (0 1, 1 1, 1 0, 0 0))";
            const string b = "LINESTRING EMPTY";
            CheckRelate(a, b, "FF1FFFFF2");
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
        }

        [Test]
        public void TestLinesRingTouchAtNode()
        {
            const string a = "LINESTRING (5 5, 1 8, 1 1, 5 5)";
            const string b = "LINESTRING (5 5, 9 5)";
            CheckRelate(a, b, "F01FFF102");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckTouches(a, b, true);
        }

        [Test]
        public void TestLinesTouchAtBdy()
        {
            const string a = "LINESTRING (5 5, 1 8)";
            const string b = "LINESTRING (5 5, 9 5)";
            CheckRelate(a, b, "FF1F00102");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckTouches(a, b, true);
        }

        [Test]
        public void TestLinesOverlapWithDisjointLine()
        {
            const string a = "LINESTRING (1 1, 9 9)";
            const string b = "MULTILINESTRING ((2 2, 8 8), (6 2, 8 4))";
            CheckRelate(a, b, "101FF0102");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckOverlaps(a, b, true);
        }

        [Test]
        public void TestLinesDisjointOverlappingEnvelopes()
        {
            const string a = "LINESTRING (60 0, 20 80, 100 80, 80 120, 40 140)";
            const string b = "LINESTRING (60 40, 140 40, 140 160, 0 160)";
            CheckRelate(a, b, "FF1FF0102");
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
            CheckTouches(a, b, false);
        }

        /**
         * Case from https://github.com/locationtech/jts/issues/270
         * Strictly, the lines cross, since their interiors intersect
         * according to the Orientation predicate.
         * However, the computation of the intersection point is 
         * non-robust, and reports it as being equal to the endpoint 
         * POINT (-10 0.0000000000000012)
         * For consistency the relate algorithm uses the intersection node topology.
         */
        [Test]
        public void TestLinesCross_JTS270()
        {
            const string a = "LINESTRING (0 0, -10 0.0000000000000012)";
            const string b = "LINESTRING (-9.999143275740073 -0.1308959557133398, -10 0.0000000000001054)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckCrosses(a, b, false);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, true);
        }

        [Test]
        public void TestLinesContained_JTS396()
        {
            const string a = "LINESTRING (1 0, 0 2, 0 0, 2 2)";
            const string b = "LINESTRING (0 0, 2 2)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCoversCoveredBy(a, b, true);
            CheckCrosses(a, b, false);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, false);
        }


        /**
         * This case shows that lines must be self-noded, 
         * so that node topology is constructed correctly
         * (at least for some predicates).
         */
        [Test]
        public void TestLinesContainedWithSelfIntersection()
        {
            const string a = "LINESTRING (2 0, 0 2, 0 0, 2 2)";
            const string b = "LINESTRING (0 0, 2 2)";
            //CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCoversCoveredBy(a, b, true);
            CheckCrosses(a, b, false);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestLineContainedInRing()
        {
            const string a = "LINESTRING(60 60, 100 100, 140 60)";
            const string b = "LINESTRING(100 100, 180 20, 20 20, 100 100)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(b, a, true);
            CheckCoversCoveredBy(b, a, true);
            CheckCrosses(a, b, false);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, false);
        }

        // see https://github.com/libgeos/geos/issues/933
        [Test]
        public void TestLineLineProperIntersection()
        {
            const string a = "MULTILINESTRING ((0 0, 1 1), (0.5 0.5, 1 0.1, -1 0.1))";
            const string b = "LINESTRING (0 0, 1 1)";
            //CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCoversCoveredBy(a, b, true);
            CheckCrosses(a, b, false);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestLineSelfIntersectionCollinear()
        {
            const string a = "LINESTRING (9 6, 1 6, 1 0, 5 6, 9 6)";
            const string b = "LINESTRING (9 9, 3 1)";
            CheckRelate(a, b, "0F1FFF102");
        }

        //======= A/P  =============

        [Test]
        public void TestPolygonPointInside()
        {
            const string a = "POLYGON ((0 10, 10 10, 10 0, 0 0, 0 10))";
            const string b = "POINT (1 1)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
        }

        [Test]
        public void TestPolygonPointOutside()
        {
            const string a = "POLYGON ((10 0, 0 0, 0 10, 10 0))";
            const string b = "POINT (8 8)";
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
        }

        [Test]
        public void TestPolygonPointInBoundary()
        {
            const string a = "POLYGON ((10 0, 0 0, 0 10, 10 0))";
            const string b = "POINT (1 0)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestAreaPointInExterior()
        {
            const string a = "POLYGON ((1 5, 5 5, 5 1, 1 1, 1 5))";
            const string b = "POINT (7 7)";
            CheckRelate(a, b, "FF2FF10F2");
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckTouches(a, b, false);
            CheckOverlaps(a, b, false);
        }

        //======= A/L  =============


        [Test]
        public void TestAreaLineContainedAtLineVertex()
        {
            const string a = "POLYGON ((1 5, 5 5, 5 1, 1 1, 1 5))";
            const string b = "LINESTRING (2 3, 3 5, 4 3)";
            CheckIntersectsDisjoint(a, b, true);
            //CheckContainsWithin(a, b, true);
            //CheckCoversCoveredBy(a, b, true);
            CheckTouches(a, b, false);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestAreaLineTouchAtLineVertex()
        {
            const string a = "POLYGON ((1 5, 5 5, 5 1, 1 1, 1 5))";
            const string b = "LINESTRING (1 8, 3 5, 5 8)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckTouches(a, b, true);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestPolygonLineInside()
        {
            const string a = "POLYGON ((0 10, 10 10, 10 0, 0 0, 0 10))";
            const string b = "LINESTRING (1 8, 3 5, 5 8)";
            CheckRelate(a, b, "102FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
        }

        [Test]
        public void TestPolygonLineOutside()
        {
            const string a = "POLYGON ((10 0, 0 0, 0 10, 10 0))";
            const string b = "LINESTRING (4 8, 9 3)";
            CheckIntersectsDisjoint(a, b, false);
            CheckContainsWithin(a, b, false);
        }

        [Test]
        public void TestPolygonLineInBoundary()
        {
            const string a = "POLYGON ((10 0, 0 0, 0 10, 10 0))";
            const string b = "LINESTRING (1 0, 9 0)";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, true);
            CheckTouches(a, b, true);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestPolygonLineCrossingContained()
        {
            const string a = "MULTIPOLYGON (((20 80, 180 80, 100 0, 20 80)), ((20 160, 180 160, 100 80, 20 160)))";
            const string b = "LINESTRING (100 140, 100 40)";
            CheckRelate(a, b, "1020F1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCoversCoveredBy(a, b, true);
            CheckTouches(a, b, false);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestValidateRelateLA_220()
        {
            const string a = "LINESTRING (90 210, 210 90)";
            const string b = "POLYGON ((150 150, 410 150, 280 20, 20 20, 150 150))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckTouches(a, b, false);
            CheckOverlaps(a, b, false);
        }

        /**
         * See RelateLA.xml (line 585)
         */
        [Test]
        public void TestLineCrossingPolygonAtShellHolePoint()
        {
            const string a = "LINESTRING (60 160, 150 70)";
            const string b = "POLYGON ((190 190, 360 20, 20 20, 190 190), (110 110, 250 100, 140 30, 110 110))";
            CheckRelate(a, b, "F01FF0212");
            CheckTouches(a, b, true);
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckTouches(a, b, true);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestLineCrossingPolygonAtNonVertex()
        {
            const string a = "LINESTRING (20 60, 150 60)";
            const string b = "POLYGON ((150 150, 410 150, 280 20, 20 20, 150 150))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckTouches(a, b, false);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestPolygonLinesContainedCollinearEdge()
        {
            const string a = "POLYGON ((110 110, 200 20, 20 20, 110 110))";
            const string b = "MULTILINESTRING ((110 110, 60 40, 70 20, 150 20, 170 40), (180 30, 40 30, 110 80))";
            CheckRelate(a, b, "102101FF2");
        }

        //======= A/A  =============


        [Test]
        public void TestPolygonsEdgeAdjacent()
        {
            const string a = "POLYGON ((1 3, 3 3, 3 1, 1 1, 1 3))";
            const string b = "POLYGON ((5 3, 5 1, 3 1, 3 3, 5 3))";
            //CheckIntersectsDisjoint(a, b, true);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, true);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestPolygonsEdgeAdjacent2()
        {
            const string a = "POLYGON ((1 3, 4 3, 3 0, 1 1, 1 3))";
            const string b = "POLYGON ((5 3, 5 1, 3 0, 4 3, 5 3))";
            //CheckIntersectsDisjoint(a, b, true);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, true);
            CheckOverlaps(a, b, false);
        }

        [Test]
        public void TestPolygonsNested()
        {
            const string a = "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))";
            const string b = "POLYGON ((2 8, 8 8, 8 2, 2 2, 2 8))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCoversCoveredBy(a, b, true);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestPolygonsOverlapProper()
        {
            const string a = "POLYGON ((1 1, 1 7, 7 7, 7 1, 1 1))";
            const string b = "POLYGON ((2 8, 8 8, 8 2, 2 2, 2 8))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckOverlaps(a, b, true);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestPolygonsOverlapAtNodes()
        {
            const string a = "POLYGON ((1 5, 5 5, 5 1, 1 1, 1 5))";
            const string b = "POLYGON ((7 3, 5 1, 3 3, 5 5, 7 3))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckOverlaps(a, b, true);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestPolygonsContainedAtNodes()
        {
            const string a = "POLYGON ((1 5, 5 5, 6 2, 1 1, 1 5))";
            const string b = "POLYGON ((1 1, 5 5, 6 2, 1 1))";
            //CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCoversCoveredBy(a, b, true);
            CheckOverlaps(a, b, false);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestPolygonsNestedWithHole()
        {
            const string a = "POLYGON ((40 60, 420 60, 420 320, 40 320, 40 60), (200 140, 160 220, 260 200, 200 140))";
            const string b = "POLYGON ((80 100, 360 100, 360 280, 80 280, 80 100))";
            //CheckIntersectsDisjoint(true, a, b);
            CheckContainsWithin(a, b, false);
            CheckContainsWithin(b, a, false);
            //CheckCoversCoveredBy(false, a, b);
            //CheckOverlaps(true, a, b);
            CheckPredicate(RelatePredicate.Contains(), a, b, false);
            //CheckTouches(false, a, b);
        }

        [Test]
        public void TestPolygonsOverlappingWithBoundaryInside()
        {
            const string a = "POLYGON ((100 60, 140 100, 100 140, 60 100, 100 60))";
            const string b = "MULTIPOLYGON (((80 40, 120 40, 120 80, 80 80, 80 40)), ((120 80, 160 80, 160 120, 120 120, 120 80)), ((80 120, 120 120, 120 160, 80 160, 80 120)), ((40 80, 80 80, 80 120, 40 120, 40 80)))";
            CheckRelate(a, b, "21210F212");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckContainsWithin(b, a, false);
            CheckCoversCoveredBy(a, b, false);
            CheckOverlaps(a, b, true);
            CheckTouches(a, b, false);
        }

        [Test]
        public void TestPolygonsOverlapVeryNarrow()
        {
            const string a = "POLYGON ((120 100, 120 200, 200 200, 200 100, 120 100))";
            const string b = "POLYGON ((100 100, 100000 110, 100000 100, 100 100))";
            CheckRelate(a, b, "212111212");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckContainsWithin(b, a, false);
            //CheckCoversCoveredBy(false, a, b);
            //CheckOverlaps(true, a, b);
            //CheckTouches(false, a, b);
        }

        [Test]
        public void TestValidateRelateAA_86()
        {
            const string a = "POLYGON ((170 120, 300 120, 250 70, 120 70, 170 120))";
            const string b = "POLYGON ((150 150, 410 150, 280 20, 20 20, 150 150), (170 120, 330 120, 260 50, 100 50, 170 120))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckOverlaps(a, b, false);
            CheckPredicate(RelatePredicate.Within(), a, b, false);
            CheckTouches(a, b, true);
        }

        [Test]
        public void TestValidateRelateAA_97()
        {
            const string a = "POLYGON ((330 150, 200 110, 150 150, 280 190, 330 150))";
            const string b = "MULTIPOLYGON (((140 110, 260 110, 170 20, 50 20, 140 110)), ((300 270, 420 270, 340 190, 220 190, 300 270)))";
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckOverlaps(a, b, false);
            CheckPredicate(RelatePredicate.Within(), a, b, false);
            CheckTouches(a, b, true);
        }

        [Test]
        public void TestAdjacentPolygons()
        {
            const string a = "POLYGON ((1 9, 6 9, 6 1, 1 1, 1 9))";
            const string b = "POLYGON ((9 9, 9 4, 6 4, 6 9, 9 9))";
            CheckRelateMatches(a, b, IntersectionMatrixPattern.Adjacent, true);
        }

        [Test]
        public void TestAdjacentPolygonsTouchingAtPoint()
        {
            const string a = "POLYGON ((1 9, 6 9, 6 1, 1 1, 1 9))";
            const string b = "POLYGON ((9 9, 9 4, 6 4, 7 9, 9 9))";
            CheckRelateMatches(a, b, IntersectionMatrixPattern.Adjacent, false);
        }

        [Test]
        public void TestAdjacentPolygonsOverlappping()
        {
            const string a = "POLYGON ((1 9, 6 9, 6 1, 1 1, 1 9))";
            const string b = "POLYGON ((9 9, 9 4, 6 4, 5 9, 9 9))";
            CheckRelateMatches(a, b, IntersectionMatrixPattern.Adjacent, false);
        }

        [Test]
        public void TestContainsProperlyPolygonContained()
        {
            const string a = "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))";
            const string b = "POLYGON ((2 8, 5 8, 5 5, 2 5, 2 8))";
            CheckRelateMatches(a, b, IntersectionMatrixPattern.ContainsProperly, true);
        }

        [Test]
        public void TestContainsProperlyPolygonTouching()
        {
            const string a = "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))";
            const string b = "POLYGON ((9 1, 5 1, 5 5, 9 5, 9 1))";
            CheckRelateMatches(a, b, IntersectionMatrixPattern.ContainsProperly, false);
        }

        [Test]
        public void TestContainsProperlyPolygonsOverlapping()
        {
            const string a = "GEOMETRYCOLLECTION (POLYGON ((1 9, 6 9, 6 4, 1 4, 1 9)), POLYGON ((2 4, 6 7, 9 1, 2 4)))";
            const string b = "POLYGON ((5 5, 6 5, 6 4, 5 4, 5 5))";
            CheckRelateMatches(a, b, IntersectionMatrixPattern.ContainsProperly, true);
        }

        //================  Repeated Points  ==============

        [Test]
        public void TestRepeatedPointLL()
        {
            const string a = "LINESTRING(0 0, 5 5, 5 5, 5 5, 9 9)";
            const string b = "LINESTRING(0 9, 5 5, 5 5, 5 5, 9 0)";
            CheckRelate(a, b, "0F1FF0102");
            CheckIntersectsDisjoint(a, b, true);
        }

        [Test]
        public void TestRepeatedPointAA()
        {
            const string a = "POLYGON ((1 9, 9 7, 9 1, 1 3, 1 9))";
            const string b = "POLYGON ((1 3, 1 3, 1 3, 3 7, 9 7, 9 7, 1 3))";
            CheckRelate(a, b, "212F01FF2");
        }

    }
}
