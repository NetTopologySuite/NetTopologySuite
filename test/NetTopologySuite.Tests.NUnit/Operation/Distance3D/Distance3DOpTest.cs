using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance3D;
using NUnit.Framework;
// using ParseException = NetTopologySuite.IO.ParseException;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance3d
{
public class Distance3DOpTest
{
    static readonly WKTReader Rdr = new WKTReader();

    /*
    public void testTest()
    {
        CheckDistance("LINESTRING (250 250 0, 260 260 0)",
                "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                70.71067811865476);

        testLinePolygonFlat();
    }
    */

        [Test]
        public void TestEmpty()
        {
            checkDistance("POINT EMPTY", "POINT EMPTY", 0);
            checkDistance("LINESTRING EMPTY", "POINT (0 0 0)", 0);
            checkDistance("MULTILINESTRING EMPTY", "POLYGON EMPTY", 0);
            checkDistance("MULTIPOLYGON EMPTY", "POINT (0 0 0)", 0);
        }

        [Test]
        public void TestPartiallyEmpty()
        {
            checkDistance("GEOMETRYCOLLECTION( MULTIPOINT (0 0 0), POLYGON EMPTY)", "POINT (0 1 0)", 1,
                    Coord(0, 0, 0), Coord(0, 1, 0));
            checkDistance("GEOMETRYCOLLECTION( MULTIPOINT (11 11 0), POLYGON EMPTY)",
                    "GEOMETRYCOLLECTION( MULTIPOINT EMPTY, LINESTRING (10 10 0, 10 20 0 ))",
                    1,
                    Coord(11, 11, 0), Coord(10, 11, double.NaN));
        }

        [Test]
        public void TestPointPointFlat()
        {
            checkDistance("POINT (10 10 0 )", "POINT (20 20 0 )", 14.1421356,
                    Coord(10, 10, 0), Coord(20, 20, 0));
            checkDistance("POINT (5 10 0 )", "POINT (15 20 0 )", 14.1421356,
                    Coord(5, 10, 0), Coord(15, 20, 0));
        }

        [Test]
        public void TestPointPoint()
        {
            checkDistance("POINT (0 0 0 )",
                    "POINT (0 0 1 )",
                    1,
                    Coord(0, 0, 0),
                    Coord(0, 0, 1));
            checkDistance("POINT (10 10 1 )",
                    "POINT (11 11 2 )",
                    1.7320508075688772,
                    Coord(10, 10, 1),
                    Coord(11, 11, 2));
            checkDistance("POINT (10 10 0 )",
                    "POINT (10 20 10 )",
                    14.142135623730951,
                    Coord(10, 10, 0),
                    Coord(10, 20, 10));
        }

        [Test]
        public void TestPointSegFlat()
        {
            checkDistance("LINESTRING (10 10 0, 10 20 0 )",
                    "POINT (20 15 0 )",
                    10,
                    Coord(10, 15, double.NaN),
                    Coord(20, 15, 0));
        }

        [Test]
        public void TestPointSeg()
        {
            checkDistance("LINESTRING (0 0 0, 10 10 10 )",
                    "POINT (5 5 5 )",
                    0,
                    Coord(5, 5, double.NaN),
                    Coord(5, 5, 5));
            checkDistance("LINESTRING (10 10 10, 20 20 20 )",
                    "POINT (11 11 10 )",
                    0.816496580927726,
                    Coord(11, 11, double.NaN),
                    Coord(11, 11, 10));
        }

        [Test]
        public void TestPointSegRobust()
        {
            checkDistance("LINESTRING (0 0 0, 10000000 10000000 1 )",
                    "POINT (9999999 9999999 .9999999 )",
                    0);
            checkDistance("LINESTRING (0 0 0, 10000000 10000000 1 )",
                    "POINT (5000000 5000000 .5 )",
                    0);
        }

        [Test]
        public void TestCrossSegmentsFlat()
        {
            checkDistance("LINESTRING (0 0 0, 10 10 0 )",
                    "LINESTRING (10 0 0, 0 10 0 )",
            0);
            checkDistance("LINESTRING (0 0 10, 30 10 10 )",
                    "LINESTRING (10 0 10, 0 10 10 )",
            0);
        }

        [Test]
        public void TestCrossSegments()
        {
            checkDistance("LINESTRING (0 0 0, 10 10 0 )",
                    "LINESTRING (10 0 1, 0 10 1 )",
            1);
            checkDistance("LINESTRING (0 0 0, 20 20 0 )",
                    "LINESTRING (10 0 1, 0 10 1 )",
            1);
            checkDistance("LINESTRING (20 10 20, 10 20 10 )",
                    "LINESTRING (10 10 20, 20 20 10 )",
            0);
        }

        /**
         * Many of these tests exhibit robustness errors 
         * due to numerical roundoff in the distance algorithm mathematics.
         * This happens when computing nearly-coincident lines 
         * with very large ordinate values
         */
        [Test]
        public void TestCrossSegmentsRobust()
        {
            checkDistance("LINESTRING (0 0 0, 10000000 10000000 1 )",
                    "LINESTRING (0 0 1, 10000000 10000000 0 )",
                    0, 0.001);  // expected is 0, but actual is larger

            checkDistance("LINESTRING (-10000 -10000 0, 10000 10000 1 )",
                    "LINESTRING (-10000 -10000 1, 10000 10000 0 )",
                    0);

            // previous case with X,Y scaled by 1000 - exposes robustness issue
            checkDistance("LINESTRING (-10000000 -10000000 0, 10000000 10000000 1 )",
                    "LINESTRING (-10000000 -10000000 1, 10000000 10000000 0 )",
                    0, 0.02);  // expected is 0, but actual is larger

            // works because lines are orthogonal, so doesn't hit roundoff problems
            checkDistance("LINESTRING (20000000 10000000 20, 10000000 20000000 10 )",
                    "LINESTRING (10000000 10000000 20, 20000000 20000000 10 )",
                    0);
        }

        [Test]
        public void TestTSegmentsFlat()
        {
            checkDistance("LINESTRING (10 10 0, 10 20 0 )",
                    "LINESTRING (20 15 0, 25 15 0 )",
                    10,
                    Coord(10, 15, double.NaN),
                    Coord(20, 15, 0));
        }

        [Test]
        public void TestParallelSegmentsFlat()
        {
            checkDistance("LINESTRING (10 10 0, 20 20 0 )",
                            "LINESTRING (10 20 0, 20 30 0 )",
                            7.0710678118654755);
        }

        [Test]
        public void TestParallelSegments()
        {
            checkDistance("LINESTRING (0 0 0, 1 0 0 )",
                            "LINESTRING (0 0 1, 1 0 1 )",
                            1);
            checkDistance("LINESTRING (10 10 0, 20 10 0 )",
                    "LINESTRING (10 20 10, 20 20 10 )",
                    14.142135623730951);
            checkDistance("LINESTRING (10 10 0, 20 20 0 )",
                    "LINESTRING (10 20 10, 20 30 10 )",
                    12.24744871391589);
            // = distance from LINESTRING (10 10 0, 20 20 0 ) to POINT(10 20 10)
            // = hypotenuse(7.0710678118654755, 10)
        }

        [Test]
        public void TestLineLine()
        {
            checkDistance("LINESTRING (0 1 2, 1 1 1, 1 0 2 )",
                    "LINESTRING (0 0 0.1, .5 .5 0, 1 1 0, 1.5 1.5 0, 2 2 0 )",
                    1);
            checkDistance("LINESTRING (10 10 20, 20 20 30, 20 20 1, 30 30 5 )",
                    "LINESTRING (1 80 10, 0 39 5, 39 0 5, 80 1 20)",
                    0.7071067811865476,
                    Coord(20, 20, 30),
                    Coord(19.5, 19.5, double.NaN));
        }

        [Test]
        public void TestPointPolygon()
        {
            // point above poly
            checkDistance("POINT (150 150 10)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    10);
            // point below poly
            checkDistance("POINT (150 150 -10)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    10);
            // point right of poly in YZ plane
            checkDistance("POINT (10 150 150)",
                    "POLYGON ((0 100 200, 0 200 200, 0 200 100, 0 100 100, 0 100 200))",
                    10);
        }

        [Test]
        public void TestPointPolygonFlat()
        {
            // inside
            checkDistance("POINT (150 150 0)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    0,
                    Coord(150, 150, 0),
                    Coord(150, 150, 0));
            // outside
            checkDistance("POINT (250 250 0)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    70.71067811865476,
                    Coord(250, 250, 0),
                    Coord(200, 200, 0));
            // on
            checkDistance("POINT (200 200 0)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    0);
        }

        [Test]
        public void TestLinePolygonFlat()
        {
            // line inside
            checkDistance("LINESTRING (150 150 0, 160 160 0)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    0);
            // line outside
            checkDistance("LINESTRING (200 250 0, 260 260 0)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    50,
                    Coord(200, 250, 0),
                    Coord(200, 200, 0));
            // line touching
            checkDistance("LINESTRING (200 200 0, 260 260 0)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    0);
        }

        [Test]
        public void TestLinePolygonSimple()
        {
            // line crossing inside
            checkDistance("LINESTRING (150 150 10, 150 150 -10)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    0);
            // vertical line above
            checkDistance("LINESTRING (200 200 10, 260 260 100)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    10);
            // vertical line touching
            checkDistance("LINESTRING (200 200 0, 260 260 100)",
                    "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
                    0);
        }

        const string polyHoleFlat = "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0), (120 180 0, 180 180 0, 180 120 0, 120 120 0, 120 180 0))";

        [Test]
        public void TestLinePolygonHoleFlat()
        {
            // line crossing hole
            checkDistance("LINESTRING (150 150 10, 150 150 -10)", polyHoleFlat, 30,
                    Coord(150, 150, 10), Coord(150, 180, double.NaN));
            // line crossing interior
            checkDistance("LINESTRING (110 110 10, 110 110 -10)", polyHoleFlat, 0,
                    Coord(110, 110, -10), Coord(110, 110, -10));
            // vertical line above hole
            checkDistance("LINESTRING (130 130 10, 150 150 100)", polyHoleFlat, 14.14213562373095,
                    Coord(130, 130, 10), Coord(130, 120, double.NaN));
            // vertical line touching hole
            checkDistance("LINESTRING (120 180 0, 120 180 100)", polyHoleFlat, 0,
                    Coord(120, 180, 0), Coord(120, 180, 0));
        }

        [Test]
        public void TestPointPolygonHoleFlat()
        {
            // point above poly hole
            checkDistance("POINT (130 130 10)", polyHoleFlat, 14.14213562373095,
                    Coord(130, 130, 10), Coord(130, 120, double.NaN));
            // point below poly hole
            checkDistance("POINT (130 130 -10)", polyHoleFlat, 14.14213562373095,
                    Coord(130, 130, -10), Coord(130, 120, double.NaN));
            // point above poly
            checkDistance("POINT (110 110 100)", polyHoleFlat, 100,
                    Coord(110, 110, 100), Coord(110, 110, 100));
        }

        const string poly2HoleFlat = "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0), (110 110 0, 110 130 0, 130 130 0, 130 110 0, 110 110 0), (190 110 0, 170 110 0, 170 130 0, 190 130 0, 190 110 0))";

        /**
         * A case proving that polygon/polygon distance requires
         * computing distance between all rings, not just the shells.
         */
        [Test]
        public void TestPolygonPolygonLinkedThruHoles()
        {
            // note distance is zero!
            checkDistance(
                    // polygon with two holes
                    poly2HoleFlat,
                    // polygon parallel to XZ plane with shell passing through holes in other polygon
                    "POLYGON ((120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10))",
                    0);

            // confirm that distance of simple poly boundary is non-zero
            checkDistance(
                    // polygon with two holes
                    poly2HoleFlat,
                    // boundary of polygon parallel to XZ plane with shell passing through holes
                    "LINESTRING (120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10)",
                    10);
        }


        [Test]
        public void TestMultiPoint()
        {
            checkDistance(
                    "MULTIPOINT ((0 0 0), (0 0 100), (100 100 100))",
                    "MULTIPOINT ((100 100 99), (50 50 50), (25 100 33))",
                    1,
                    Coord(100, 100, 100),
                    Coord(100, 100, 99));
        }

        [Test]
        public void TestMultiLineString()
        {
            checkDistance(
                    "MULTILINESTRING ((0 0 0, 10 10 10), (0 0 100, 25 25 25, 40 40 50), (100 100 100, 100 101 102))",
                    "MULTILINESTRING ((100 100 99, 100 100 99), (100 102 102, 200 200 20), (25 100 33, 25 100 35))",
                    1
                    );
        }

        [Test]
        public void TestMultiPolygon()
        {
            checkDistance(
                    // Polygons parallel to XZ plane
                    "MULTIPOLYGON ( ((120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10)), ((120 200 -10, 120 200 190, 180 200 190, 180 200 -10, 120 200 -10)) )",
                    // Polygons parallel to XY plane
                    "MULTIPOLYGON ( ((100 200 200, 200 200 200, 200 100 200, 100 100 200, 100 200 200)), ((100 200 210, 200 200 210, 200 100 210, 100 100 210, 100 200 210)) )",
                    10
                    );
        }

        [Test]
        public void TestMultiMixed()
        {
            checkDistance(
                    "MULTILINESTRING ((0 0 0, 10 10 10), (0 0 100, 25 25 25, 40 40 50), (100 100 100, 100 101 101))",
                    "MULTIPOINT ((100 100 99), (50 50 50), (25 100 33))",
                    1,
                    Coord(100, 100, 100), Coord(100, 100, 99));
        }


        //==========================================================
        // Convenience methods
        //==========================================================

        private const double DIST_TOLERANCE = 0.00001;

        private void checkDistance(string wkt1, string wkt2, double expectedDistance)
        {
            checkDistance(wkt1, wkt2, expectedDistance, DIST_TOLERANCE);
        }

        private void checkDistance(string wkt1, string wkt2, double expectedDistance, params CoordinateZ[] expectedCoordinates)
        {
            checkDistance(wkt1, wkt2, expectedDistance, expectedCoordinates, DIST_TOLERANCE);
        }

        private void checkDistance(string wkt1, string wkt2, double expectedDistance, double tolerance)
        {
            checkDistance(wkt1, wkt2, expectedDistance, new CoordinateZ[0], tolerance);
        }

        private void checkDistance(string wkt1, string wkt2, double expectedDistance, CoordinateZ[] expectedCoords, double tolerance)
        {
            Geometry g1;
            Geometry g2;
            try
            {
                g1 = Rdr.Read(wkt1);
            }
            catch (ParseException)
            {
                throw;
            }
            try
            {
                g2 = Rdr.Read(wkt2);
            }
            catch (ParseException)
            {
                throw;
            }
            // check both orders for arguments
            checkDistance(g1, g2, expectedDistance, expectedCoords, tolerance);
            checkDistance(g2, g1, expectedDistance, Reversed(expectedCoords), tolerance);
        }

        private void checkDistance(Geometry g1, Geometry g2, double expectedDistance, CoordinateZ[] expectedCoords, double tolerance)
        {
            var distOp = new Distance3DOp(g1, g2);
            double dist = distOp.Distance();
            Assert.That(dist, Is.EqualTo(expectedDistance).Within(tolerance));

            if (expectedCoords.Length == 2)
            {
                var nearestCoords = distOp.NearestPoints();
                Assert.IsTrue(nearestCoords[0].Equals2D(expectedCoords[0], tolerance));
                Assert.IsTrue(nearestCoords[1].Equals2D(expectedCoords[1], tolerance));
                if (!double.IsNaN(expectedCoords[0].Z))
                {
                    Assert.IsTrue(((CoordinateZ)nearestCoords[0]).EqualInZ(expectedCoords[0], tolerance));
                }
                if (!double.IsNaN(expectedCoords[1].Z))
                {
                    Assert.IsTrue(((CoordinateZ)nearestCoords[1]).EqualInZ((CoordinateZ)expectedCoords[1], tolerance));
                }
            }
        }

        private static CoordinateZ Coord(double x, double y, double z)
        {
            return new CoordinateZ(x, y, z);
        }

        private static CoordinateZ[] Reversed(CoordinateZ[] coordinates)
        {
            var reversed = new CoordinateZ[coordinates.Length];
            int maxIndex = coordinates.Length - 1;
            for (int i = 0; i < coordinates.Length; i++)
            {
                reversed[i] = coordinates[maxIndex - i];
            }
            return reversed;
        }
    }
}
