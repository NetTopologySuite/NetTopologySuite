using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    public class GeometryFixerTest : GeometryTestCase
    {

        [Test]
        public void TestPoint()
        {
            CheckFix("POINT (0 0)", "POINT (0 0)");
        }

        [Test]
        public void TestPointNaN()
        {
            CheckFix("POINT (0 Nan)", "POINT EMPTY");
        }

        [Test]
        public void TestPointEmpty()
        {
            CheckFix("POINT EMPTY", "POINT EMPTY");
        }

        [Test]
        public void TestPointPosInf()
        {
            CheckFix(CreatePoint(0, double.PositiveInfinity), "POINT EMPTY");
        }

        [Test]
        public void TestPointNegInf()
        {
            CheckFix(CreatePoint(0, double.PositiveInfinity), "POINT EMPTY");
        }

        private Point CreatePoint(double x, double y)
        {
            var p = new Coordinate(x, y);
            var pt = GeometryFactory.CreatePoint(p);
            return pt;
        }

        //----------------------------------------

        [Test]
        public void TestMultiPointNaN()
        {
            CheckFix("MULTIPOINT ((0 Nan))",
                "MULTIPOINT EMPTY");
        }

        [Test]
        public void TestMultiPoint()
        {
            CheckFix("MULTIPOINT ((0 0), (1 1))",
                "MULTIPOINT ((0 0), (1 1))");
        }

        [Test]
        public void TestMultiPointWithEmpty()
        {
            CheckFix("MULTIPOINT ((0 0), EMPTY)",
                "MULTIPOINT ((0 0))");
        }

        [Test]
        public void TestMultiPointWithMultiEmpty()
        {
            CheckFix("MULTIPOINT (EMPTY, EMPTY)",
                "MULTIPOINT EMPTY");
        }

        //----------------------------------------

        [Test]
        public void TestLineStringEmpty()
        {
            CheckFix("LINESTRING EMPTY",
                "LINESTRING EMPTY");
        }

        [Test]
        public void TestLineStringCollapse()
        {
            CheckFix("LINESTRING (0 0, 1 NaN, 0 0)",
                "LINESTRING EMPTY");
        }

        [Test]
        public void TestLineStringCollapseMultipleRepeated()
        {
            CheckFix("LINESTRING (0 0, 0 0, 0 0)",
                "LINESTRING EMPTY");
        }

        [Test]
        public void TestLineStringKeepCollapse()
        {
            CheckFixKeepCollapse("LINESTRING (0 0, 0 0, 0 0)",
                "POINT (0 0)");
        }

        [Test]
        public void TestLineStringRepeated()
        {
            CheckFix("LINESTRING (0 0, 0 0, 0 0, 0 0, 0 0, 1 1)",
                "LINESTRING (0 0, 1 1)");
        }

        /**
         * Checks that self-crossing are valid, and that entire geometry is copied
         */
        [Test]
        public void TestLineStringSelfCross()
        {
            CheckFix("LINESTRING (0 0, 9 9, 9 5, 0 5)",
                "LINESTRING (0 0, 9 9, 9 5, 0 5)");
        }

        //----------------------------------------

        [Test]
        public void TestLinearRingEmpty()
        {
            CheckFix("LINEARRING EMPTY",
                "LINEARRING EMPTY");
        }

        [Test]
        public void TestLinearRingCollapsePoint()
        {
            CheckFix("LINEARRING (0 0, 1 NaN, 0 0)",
                "LINEARRING EMPTY");
        }

        [Test]
        public void TestLinearRingCollapseLine()
        {
            CheckFix("LINEARRING (0 0, 1 NaN, 1 0, 0 0)",
                "LINEARRING EMPTY");
        }

        [Test]
        public void TestLinearRingKeepCollapsePoint()
        {
            CheckFixKeepCollapse("LINEARRING (0 0, 1 NaN, 0 0)",
                "POINT (0 0)");
        }

        [Test]
        public void TestLinearRingKeepCollapseLine()
        {
            CheckFixKeepCollapse("LINEARRING (0 0, 1 NaN, 1 0, 0 0)",
                "LINESTRING (0 0, 1 0, 0 0)");
        }

        [Test]
        public void TestLinearRingValid()
        {
            CheckFix("LINEARRING (10 10, 10 90, 90 90, 90 10, 10 10)",
                "LINEARRING (10 10, 10 90, 90 90, 90 10, 10 10)");
        }

        [Test]
        public void TestLinearRingFlat()
        {
            CheckFix("LINEARRING (10 10, 10 90, 90 90, 10 90, 10 10)",
                "LINESTRING (10 10, 10 90, 90 90, 10 90, 10 10)");
        }

        /**
         * Checks that invalid self-crossing ring is returned as a LineString
         */
        [Test]
        public void TestLinearRingSelfCross()
        {
            CheckFix("LINEARRING (10 10, 10 90, 90 10, 90 90, 10 10)",
                "LINESTRING (10 10, 10 90, 90 10, 90 90, 10 10)");
        }

        //----------------------------------------

        /**
         * Self-crossing LineStrings are valid, so are unchanged
         */
        [Test]
        public void TestMultiLineStringSelfCross()
        {
            CheckFix("MULTILINESTRING ((10 90, 90 10, 90 90), (90 50, 10 50))",
                "MULTILINESTRING ((10 90, 90 10, 90 90), (90 50, 10 50))");
        }

        [Test]
        public void TestMultiLineStringWithCollapse()
        {
            CheckFix("MULTILINESTRING ((10 10, 90 90), (10 10, 10 10, 10 10))",
                "LINESTRING (10 10, 90 90))");
        }

        [Test]
        public void TestMultiLineStringKeepCollapse()
        {
            CheckFixKeepCollapse("MULTILINESTRING ((10 10, 90 90), (10 10, 10 10, 10 10))",
                "GEOMETRYCOLLECTION (POINT (10 10), LINESTRING (10 10, 90 90))");
        }

        [Test]
        public void TestMultiLineStringWithEmpty()
        {
            CheckFix("MULTILINESTRING ((10 10, 90 90), EMPTY)",
                "LINESTRING (10 10, 90 90))");
        }

        [Test]
        public void TestMultiLineStringWithMultiEmpty()
        {
            CheckFix("MULTILINESTRING (EMPTY, EMPTY)",
                "MULTILINESTRING EMPTY");
        }

        //----------------------------------------

        [Test]
        public void TestPolygonEmpty()
        {
            CheckFix("POLYGON EMPTY",
                "POLYGON EMPTY");
        }

        [Test]
        public void TestPolygonBowtie()
        {
            CheckFix("POLYGON ((10 90, 90 10, 90 90, 10 10, 10 90))",
                "MULTIPOLYGON (((10 90, 50 50, 10 10, 10 90)), ((50 50, 90 90, 90 10, 50 50)))");
        }

        [Test]
        public void TestPolygonHolesZeroAreaOverlapping()
        {
            CheckFix(
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (80 70, 30 70, 30 20, 30 70, 80 70), (70 80, 70 30, 20 30, 70 30, 70 80))",
                "POLYGON ((90 90, 90 10, 10 10, 10 90, 90 90))");
        }

        [Test]
        public void TestPolygonPosAndNegOverlap()
        {
            CheckFix("POLYGON ((10 90, 50 90, 50 30, 70 30, 70 50, 30 50, 30 70, 90 70, 90 10, 10 10, 10 90))",
                "POLYGON ((10 90, 50 90, 50 70, 90 70, 90 10, 10 10, 10 90), (50 50, 50 30, 70 30, 70 50, 50 50))");
        }

        [Test]
        public void TestHolesTouching()
        {
            CheckFix(
                "POLYGON ((0 0, 0 5, 6 5, 6 0, 0 0), (3 1, 4 1, 4 2, 3 2, 3 1), (3 2, 1 4, 5 4, 4 2, 4 3, 3 2, 2 3, 3 2))",
                "MULTIPOLYGON (((0 0, 0 5, 6 5, 6 0, 0 0), (1 4, 2 3, 3 2, 3 1, 4 1, 4 2, 5 4, 1 4)), ((3 2, 4 3, 4 2, 3 2)))");
        }

        [Test]
        public void TestPolygonNaN()
        {
            CheckFix("POLYGON ((10 90, 90 NaN, 90 10, 10 10, 10 90))",
                "POLYGON ((10 10, 10 90, 90 10, 10 10))");
        }

        [Test]
        public void TestPolygonRepeated()
        {
            CheckFix("POLYGON ((10 90, 90 10, 90 10, 90 10, 90 10, 90 10, 10 10, 10 90))",
                "POLYGON ((10 10, 10 90, 90 10, 10 10))");
        }

        [Test]
        public void TestPolygonShellCollapse()
        {
            CheckFix("POLYGON ((10 10, 10 90, 90 90, 10 90, 10 10), (20 80, 60 80, 60 40, 20 40, 20 80))",
                "POLYGON EMPTY");
        }

        [Test]
        public void TestPolygonShellCollapseNaN()
        {
            CheckFix("POLYGON ((10 10, 10 NaN, 90 NaN, 10 NaN, 10 10))",
                "POLYGON EMPTY");
        }

        [Test]
        public void TestPolygonShellKeepCollapseNaN()
        {
            CheckFixKeepCollapse("POLYGON ((10 10, 10 NaN, 90 NaN, 10 NaN, 10 10))",
                "POINT (10 10)");
        }

        [Test]
        public void TestPolygonShellKeepCollapse()
        {
            CheckFixKeepCollapse("POLYGON ((10 10, 10 90, 90 90, 10 90, 10 10), (20 80, 60 80, 60 40, 20 40, 20 80))",
                "LINESTRING (10 10, 10 90, 90 90, 10 90, 10 10)");
        }

        [Test]
        public void TestPolygonHoleCollapse()
        {
            CheckFix("POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (80 80, 20 80, 20 20, 20 80, 80 80))",
                "POLYGON ((10 10, 10 90, 90 90, 90 10, 10 10))");
        }

        [Test]
        public void TestPolygonHoleKeepCollapse()
        {
            CheckFixKeepCollapse("POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (80 80, 20 80, 20 20, 20 80, 80 80))",
                "POLYGON ((10 10, 10 90, 90 90, 90 10, 10 10))");
        }

        //----------------------------------------

        [Test]
        public void TestMultiPolygonEmpty()
        {
            CheckFix("MULTIPOLYGON EMPTY",
                "MULTIPOLYGON EMPTY");
        }

        [Test]
        public void TestMultiPolygonMultiEmpty()
        {
            CheckFix("MULTIPOLYGON (EMPTY, EMPTY)",
                "MULTIPOLYGON EMPTY");
        }

        [Test]
        public void TestMultiPolygonWithEmpty()
        {
            CheckFix(
                "MULTIPOLYGON (((10 40, 40 40, 40 10, 10 10, 10 40)), EMPTY, ((50 40, 80 40, 80 10, 50 10, 50 40)))",
                "MULTIPOLYGON (((10 40, 40 40, 40 10, 10 10, 10 40)), ((50 40, 80 40, 80 10, 50 10, 50 40)))");
        }

        [Test]
        public void TestMultiPolygonWithCollapse()
        {
            CheckFix("MULTIPOLYGON (((10 40, 40 40, 40 10, 10 10, 10 40)), ((50 40, 50 40, 50 40, 50 40, 50 40)))",
                "POLYGON ((10 10, 10 40, 40 40, 40 10, 10 10))");
        }

        [Test]
        public void TestMultiPolygonKeepCollapse()
        {
            CheckFixKeepCollapse(
                "MULTIPOLYGON (((10 40, 40 40, 40 10, 10 10, 10 40)), ((50 40, 50 40, 50 40, 50 40, 50 40)))",
                "GEOMETRYCOLLECTION (POINT (50 40), POLYGON ((10 10, 10 40, 40 40, 40 10, 10 10)))");
        }

        //----------------------------------------

        [Test]
        public void TestGCEmpty()
        {
            CheckFix("GEOMETRYCOLLECTION EMPTY",
                "GEOMETRYCOLLECTION EMPTY");
        }

        [Test]
        public void TestGCWithAllEmpty()
        {
            CheckFix("GEOMETRYCOLLECTION (POINT EMPTY, LINESTRING EMPTY, POLYGON EMPTY)",
                "GEOMETRYCOLLECTION (POINT EMPTY, LINESTRING EMPTY, POLYGON EMPTY)");
        }

        //----------------------------------------

        [Test]
        public void TestPolygonZBowtie()
        {
            CheckFixZ("POLYGON Z ((10 90 1, 90 10 9, 90 90 9, 10 10 1, 10 90 1))",
                "MULTIPOLYGON Z(((10 10 1, 10 90 1, 50 50 5, 10 10 1)), ((50 50 5, 90 90 9, 90 10 9, 50 50 5)))");
        }

        [Test]
        public void TestPolygonZHoleOverlap()
        {
            CheckFixZ("POLYGON Z ((10 90 1, 60 90 6, 60 10 6, 10 10 1, 10 90 1), (20 80 2, 90 80 9, 90 20 9, 20 20 2, 20 80 2))",
                "POLYGON Z((10 10 1, 10 90 1, 60 90 6, 60 80 6, 20 80 2, 20 20 2, 60 20 6, 60 10 6, 10 10 1))");
        }

        [Test]
        public void TestMultiLineStringZKeepCollapse()
        {
            CheckFixZKeepCollapse("MULTILINESTRING Z ((10 10 1, 90 90 9), (10 10 1, 10 10 2, 10 10 3))",
                "GEOMETRYCOLLECTION Z (POINT (10 10 1), LINESTRING (10 10 1, 90 90 9))");
        }


        //================================================


        private void CheckFix(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            CheckFix(geom, false, wktExpected);
        }

        private void CheckFixKeepCollapse(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            CheckFix(geom, true, wktExpected);
        }

        private void CheckFix(Geometry input, string wktExpected)
        {
            CheckFix(input, false, wktExpected);
        }

        private void CheckFixKeepCollapse(Geometry input, string wktExpected)
        {
            CheckFix(input, true, wktExpected);
        }

        private void CheckFix(Geometry input, bool keepCollapse, string wktExpected)
        {
            Geometry actual;
            if (keepCollapse)
            {
                var fixer = new GeometryFixer(input);
                fixer.KeepCollapsed = true;
                actual = fixer.GetResult();
            }
            else
            {
                actual = GeometryFixer.Fix(input);
            }

            Assert.That(actual.IsValid, Is.True, "Result is invalid");
            Assert.That(ReferenceEquals(input, actual), Is.False, "Input geometry was not copied");
            Assert.That(CheckDeepCopy(input, actual), Is.True, "Result has aliased coordinates");

            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        private static bool CheckDeepCopy(Geometry geom1, Geometry geom2)
        {
            var pts1 = geom1.Coordinates;
            var pts2 = geom2.Coordinates;
            foreach (var p2 in pts2)
            {
                if (IsIn(p2, pts1))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIn(Coordinate p, Coordinate[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
                if (ReferenceEquals(p, pts[i])) return true;

            return false;
        }

        private void CheckFixZ(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            CheckFixZ(geom, false, wktExpected);
        }

        private void CheckFixZKeepCollapse(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            CheckFixZ(geom, true, wktExpected);
        }

        private void CheckFixZ(Geometry input, bool keepCollapse, string wktExpected)
        {
            Geometry actual;
            if (keepCollapse)
            {
                var fixer = new GeometryFixer(input);
                fixer.KeepCollapsed = true;
                actual = fixer.GetResult();
            }
            else
            {
                actual = GeometryFixer.Fix(input);
            }

            Assert.That(actual.IsValid, Is.True, "Result is invalid");

            var expected = Read(wktExpected);
            CheckEqualXYZ(expected, actual);
        }

    }
}
