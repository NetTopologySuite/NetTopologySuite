using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    internal class RelateNGGCTest : RelateNGTestCase
    {
        [Test]
        public void TestDimensionWithEmpty()
        {
            const string a = "LINESTRING(0 0, 1 1)";
            const string b = "GEOMETRYCOLLECTION(POLYGON EMPTY,LINESTRING(0 0, 1 1))";
            CheckCoversCoveredBy(a, b, true);
            CheckEquals(a, b, true);
        }

        // see https://github.com/libgeos/geos/issues/1027
        [Test]
        public void TestMP_GLP_GEOS1027()
        {
            const string a = "MULTIPOLYGON (((0 0, 3 0, 3 3, 0 3, 0 0)))";
            const string b = "GEOMETRYCOLLECTION ( LINESTRING (1 2, 1 1), POINT (0 0))";
            CheckRelate(a, b, "1020F1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, true);
            CheckCrosses(a, b, false);
            CheckEquals(a, b, false);
        }

        // see https://github.com/libgeos/geos/issues/1022
        [Test]
        public void TestGPL_A()
        {
            const string a = "GEOMETRYCOLLECTION (POINT (7 1), LINESTRING (6 5, 6 4))";
            const string b = "POLYGON ((7 1, 1 3, 3 9, 7 1))";
            CheckRelate(a, b, "F01FF0212");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCrosses(a, b, false);
            CheckTouches(a, b, true);
            CheckEquals(a, b, false);
        }

        // see https://github.com/libgeos/geos/issues/982
        [Test]
        public void TestP_GPL()
        {
            const string a = "POINT(0 0)";
            const string b = "GEOMETRYCOLLECTION(POINT(0 0), LINESTRING(0 0, 1 0))";
            CheckRelate(a, b, "F0FFFF102");
            CheckIntersectsDisjoint(a, b, true);
            CheckContainsWithin(a, b, false);
            CheckCrosses(a, b, false);
            CheckTouches(a, b, true);
            CheckEquals(a, b, false);
        }

        [Test]
        public void TestLineInOverlappingPolygonsTouchingInteriorEdge()
        {
            const string a = "LINESTRING (3 7, 7 3)";
            const string b = "GEOMETRYCOLLECTION (POLYGON ((1 9, 7 9, 7 3, 1 3, 1 9)), POLYGON ((9 1, 3 1, 3 7, 9 7, 9 1)))";
            CheckRelate(a, b, "1FF0FF212");
            CheckContainsWithin(b, a, true);
        }

        [Test]
        public void TestLineInOverlappingPolygonsCrossingInteriorEdgeAtVertex()
        {
            const string a = "LINESTRING (2 2, 8 8)";
            const string b = "GEOMETRYCOLLECTION (POLYGON ((1 1, 1 7, 7 7, 7 1, 1 1)), POLYGON ((9 9, 9 3, 3 3, 3 9, 9 9)))";
            CheckRelate(a, b, "1FF0FF212");
            CheckContainsWithin(b, a, true);
        }

        [Test]
        public void TestLineInOverlappingPolygonsCrossingInteriorEdgeProper()
        {
            const string a = "LINESTRING (2 4, 6 8)";
            const string b = "GEOMETRYCOLLECTION (POLYGON ((1 1, 1 7, 7 7, 7 1, 1 1)), POLYGON ((9 9, 9 3, 3 3, 3 9, 9 9)))";
            CheckRelate(a, b, "1FF0FF212");
            CheckContainsWithin(b, a, true);
        }

        [Test]
        public void TestPolygonInOverlappingPolygonsTouchingBoundaries()
        {
            const string a = "GEOMETRYCOLLECTION (POLYGON ((1 9, 6 9, 6 4, 1 4, 1 9)), POLYGON ((9 1, 4 1, 4 6, 9 6, 9 1)) )";
            const string b = "POLYGON ((2 6, 6 2, 8 4, 4 8, 2 6))";
            CheckRelate(a, b, "212F01FF2");
            CheckContainsWithin(a, b, true);
        }

        [Test]
        public void TestLineInOverlappingPolygonsBoundaries()
        {
            const string a = "LINESTRING (1 6, 9 6, 9 1, 1 1, 1 6)";
            const string b = "GEOMETRYCOLLECTION (POLYGON ((1 1, 1 6, 6 6, 6 1, 1 1)), POLYGON ((9 1, 4 1, 4 6, 9 6, 9 1)))";
            CheckRelate(a, b, "F1FFFF2F2");
            CheckContainsWithin(a, b, false);
            CheckCoversCoveredBy(a, b, false);
            CheckCoversCoveredBy(b, a, true);
        }

        [Test]
        public void TestLineCoversOverlappingPolygonsBoundaries()
        {
            const string a = "LINESTRING (1 6, 9 6, 9 1, 1 1, 1 6)";
            const string b = "GEOMETRYCOLLECTION (POLYGON ((1 1, 1 6, 6 6, 6 1, 1 1)), POLYGON ((9 1, 4 1, 4 6, 9 6, 9 1)))";
            CheckRelate(a, b, "F1FFFF2F2");
            CheckContainsWithin(b, a, false);
            CheckCoversCoveredBy(b, a, true);
        }

        [Test]
        public void TestAdjacentPolygonsContainedInAdjacentPolygons()
        {
            const string a = "GEOMETRYCOLLECTION (POLYGON ((2 2, 2 5, 4 5, 4 2, 2 2)), POLYGON ((8 2, 4 3, 4 4, 8 5, 8 2)))";
            const string b = "GEOMETRYCOLLECTION (POLYGON ((1 1, 1 6, 4 6, 4 1, 1 1)), POLYGON ((9 1, 4 1, 4 6, 9 6, 9 1)))";
            CheckRelate(a, b, "2FF1FF212");
            CheckContainsWithin(b, a, true);
            CheckCoversCoveredBy(b, a, true);
        }

        [Test]
        public void TestGCMultiPolygonIntersectsPolygon()
        {
            const string a = "POLYGON ((2 5, 3 5, 3 3, 2 3, 2 5))";
            const string b = "GEOMETRYCOLLECTION (MULTIPOLYGON (((1 4, 4 4, 4 1, 1 1, 1 4)), ((5 4, 8 4, 8 1, 5 1, 5 4))))";
            CheckRelate(a, b, "212101212");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(b, a, false);
        }

        [Test]
        public void TestPolygonContainsGCMultiPolygonElement()
        {
            const string a = "POLYGON ((0 5, 4 5, 4 1, 0 1, 0 5))";
            const string b = "GEOMETRYCOLLECTION (MULTIPOLYGON (((1 4, 3 4, 3 2, 1 2, 1 4)), ((6 4, 8 4, 8 2, 6 2, 6 4))))";
            CheckRelate(a, b, "212FF1212");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(b, a, false);
        }

        /**
         * Demonstrates the need for assigning computed nodes to their rings,
         * so that subsequent PIP testing can report node as being on ring boundary.
         */
        [Test]
        public void TestPolygonOverlappingGCPolygon()
        {
            const string a = "GEOMETRYCOLLECTION (POLYGON ((18.6 40.8, 16.8825 39.618567, 16.9319 39.5461, 17.10985 39.485133, 16.6143 38.4302, 16.43145 38.313267, 16.2 37.5, 14.8 37.8, 14.96475 40.474933, 18.6 40.8)))";
            const string b = "POLYGON ((16.3649953125 38.37219358064516, 16.3649953125 39.545924774193544, 17.949465625000002 39.545924774193544, 17.949465625000002 38.37219358064516, 16.3649953125 38.37219358064516))";
            CheckRelate(b, a, "212101212");
            CheckRelate(a, b, "212101212");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, false);
        }

        const string wktAdjacentPolys = "GEOMETRYCOLLECTION (POLYGON ((5 5, 2 9, 9 9, 9 5, 5 5)), POLYGON ((3 1, 5 5, 9 5, 9 1, 3 1)), POLYGON ((1 9, 2 9, 5 5, 3 1, 1 1, 1 9)))";

        [Test]
        public void TestAdjPolygonsCoverPolygonWithEndpointInside()
        {
            const string a = wktAdjacentPolys;
            const string b = "POLYGON ((3 7, 7 7, 7 3, 3 3, 3 7))";
            CheckRelate(b, a, "2FF1FF212");
            CheckRelate(a, b, "212FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestAdjPolygonsCoverPointAtNode()
        {
            const string a = wktAdjacentPolys;
            const string b = "POINT (5 5)";
            CheckRelate(b, a, "0FFFFF212");
            CheckRelate(a, b, "0F2FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestAdjPolygonsCoverPointOnEdge()
        {
            const string a = wktAdjacentPolys;
            const string b = "POINT (7 5)";
            CheckRelate(b, a, "0FFFFF212");
            CheckRelate(a, b, "0F2FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestAdjPolygonsContainingPolygonTouchingInteriorEndpoint()
        {
            const string a = wktAdjacentPolys;
            const string b = "POLYGON ((5 5, 7 5, 7 3, 5 3, 5 5))";
            CheckRelate(a, b, "212FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestAdjPolygonsOverlappedByPolygonWithHole()
        {
            const string a = wktAdjacentPolys;
            const string b = "POLYGON ((0 10, 10 10, 10 0, 0 0, 0 10), (2 8, 8 8, 8 2, 2 2, 2 8))";
            CheckRelate(a, b, "2121FF212");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, false);
        }

        [Test]
        public void TestAdjPolygonsContainingLine()
        {
            const string a = wktAdjacentPolys;
            const string b = "LINESTRING (5 5, 7 7)";
            CheckRelate(a, b, "102FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestAdjPolygonsContainingLineAndPoint()
        {
            const string a = wktAdjacentPolys;
            const string b = "GEOMETRYCOLLECTION (POINT (5 5), LINESTRING (5 7, 7 7))";
            CheckRelate(a, b, "102FF1FF2");
            CheckIntersectsDisjoint(a, b, true);
            CheckCoversCoveredBy(a, b, true);
        }

        [Test]
        public void TestEmptyMultiPointElements()
        {
            const string a = "POLYGON ((3 7, 7 7, 7 3, 3 3, 3 7))";
            const string b = "GEOMETRYCOLLECTION (MULTIPOINT (EMPTY, (5 5)), LINESTRING (1 9, 4 9))";
            CheckIntersectsDisjoint(a, b, true);
        }
    }
}
