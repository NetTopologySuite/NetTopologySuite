﻿using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.Polygon;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Triangulate.Polygon
{
    public class PolygonTriangulatorTest : GeometryTestCase
    {
        [Test]
        public void TestQuad()
        {
            CheckTri("POLYGON ((10 10, 20 40, 90 90, 90 10, 10 10))"
                , "GEOMETRYCOLLECTION (POLYGON ((10 10, 20 40, 90 90, 10 10)), POLYGON ((90 90, 90 10, 10 10, 90 90)))");
        }

        [Test]
        public void TestPent()
        {
            CheckTri("POLYGON ((10 10, 20 40, 90 90, 100 50, 90 10, 10 10))", 
                     "GEOMETRYCOLLECTION (POLYGON ((10 10, 20 40, 90 90, 10 10)), POLYGON ((90 90, 100 50, 90 10, 90 90)), " +
                        "POLYGON ((90 10, 10 10, 90 90, 90 10)))");
        }

        [Test]
        public void TestHoleCW()
        {
            CheckTri("POLYGON ((10 90, 90 90, 90 20, 10 10, 10 90), (30 70, 80 70, 50 30, 30 70))",
                     "GEOMETRYCOLLECTION ("+
                        "POLYGON ((10 10, 10 90, 50 30, 10 10)), POLYGON ((10 10, 50 30, 90 20, 10 10)), " +
                        "POLYGON ((10 90, 30 70, 50 30, 10 90)), POLYGON ((10 90, 80 70, 30 70, 10 90)), " +
                        "POLYGON ((10 90, 90 90, 80 70, 10 90)), POLYGON ((50 30, 80 70, 90 20, 50 30)), " +
                        "POLYGON ((80 70, 90 90, 90 20, 80 70)))");
        }

        [Test]
        public void TestTouchingHoles()
        {
            CheckTri("POLYGON ((10 10, 10 90, 90 90, 90 10, 10 10), (20 80, 30 30, 50 70, 20 80), (50 70, 70 20, 80 80, 50 70))",
                     "GEOMETRYCOLLECTION (POLYGON ((10 10, 10 90, 20 80, 10 10)), POLYGON ((30 30, 50 70, 70 20, 30 30)), " +
                            "POLYGON ((80 80, 50 70, 20 80, 80 80)), POLYGON ((20 80, 10 90, 90 90, 20 80)), " +
                            "POLYGON ((10 10, 20 80, 30 30, 10 10)), POLYGON ((80 80, 20 80, 90 90, 80 80)), " +
                            "POLYGON ((90 10, 10 10, 30 30, 90 10)), POLYGON ((70 20, 80 80, 90 90, 70 20)), " +
                            "POLYGON ((90 10, 30 30, 70 20, 90 10)), POLYGON ((70 20, 90 90, 90 10, 70 20)))");
        }

        [Test]
        public void TestRepeatedPoints()
        {
            CheckTri("POLYGON ((71 195, 178 335, 178 335, 239 185, 380 210, 290 60, 110 70, 71 195))",
                     "GEOMETRYCOLLECTION (POLYGON ((71 195, 178 335, 239 185, 71 195)), POLYGON ((71 195, 239 185, 290 60, 71 195)), " +
                        "POLYGON ((71 195, 290 60, 110 70, 71 195)), POLYGON ((239 185, 380 210, 290 60, 239 185)))");
        }

        [Test]
        public void TestEmpty()
        {
            CheckTri("POLYGON EMPTY"
                , "GEOMETRYCOLLECTION EMPTY");
        }

        [Test]
        public void TestMultiPolygon()
        {
            CheckTri("MULTIPOLYGON(((10 10, 20 50, 50 50, 40 20, 10 10)), ((10 70, 10 90, 50 90, 40 70, 10 70)), ((20 60, 90 90, 90 20, 60 60, 20 60)))",
                     "GEOMETRYCOLLECTION (POLYGON ((10 10, 20 50, 50 50, 10 10)), POLYGON ((50 50, 40 20, 10 10, 50 50)), " +
                        "POLYGON ((90 90, 90 20, 60 60, 90 90)), POLYGON ((60 60, 20 60, 90 90, 60 60)), " +
                        "POLYGON ((10 70, 10 90, 50 90, 10 70)), POLYGON ((50 90, 40 70, 10 70, 50 90)))");
        }

        [Test]
        public void TestCeeShape()
        {
            CheckTri("POLYGON ((110 170, 138 272, 145 286, 152 296, 160 307, 303 307, 314 301, 332 287, 343 278, 352 270, 385 99, 374 89, 359 79, 178 89, 167 91, 153 99, 146 107, 173 157, 182 163, 191 170, 199 176, 208 184, 218 194, 226 203, 198 252, 188 247, 182 239, 175 231, 167 223, 161 213, 156 203, 155 198, 110 170))");
        }

        [Test, Description("Ear clipping creates a collapsed corner (A-B-A), which was not detected by flat corner removal")]
        public void TestCollapsedCorner()
        {
            CheckTri("POLYGON ((186 90, 71 17, 74 10, 65 0, 0 121, 186 90), (73 34, 67 41, 71 17, 73 34))");
        }
        /**
         * A failing case revealing that joining holes by a zero-length cut
         * was introducing duplicate vertices.
         */
        [Test]
        public void TestBadHoleJoinZeroLenCutDuplicateVertices()
        {
            CheckTri("POLYGON ((71 12, 0 0, 7 47, 16 94, 71 52, 71 12), (7 38, 25 48, 7 47, 7 38), (13 59, 13 54, 26 53, 13 59))");
        }

        /**
         * A failing case for hole joining with two touching holes.
         * Fails due to PolygonHoleJoiner not handling holes which have same leftmost vertex.
         * Note that input is normalized.
         */
        [Test]
        public void TestBadHoleJoinTouchingHoles()
        {
            CheckTri("POLYGON ((0 0, 0 9, 9 9, 9 0, 0 0), (1 4, 5 1, 5 4, 1 4), (1 4, 5 5, 6 8, 1 4))");
        }

        [Test]
        public void TestBadHoleJoinHolesTouchVertical()
        {
            CheckTri("POLYGON ((1 9, 9 9, 9 0, 1 0, 1 9), (1 4, 5 1, 5 4, 1 4), (1 5, 5 5, 6 8, 1 5))");
        }

        [Test]
        public void TestBadHoleJoinHoleTouchesShellVertical()
        {
            CheckTri("POLYGON ((1 9, 9 9, 9 0, 1 0, 1 9), (1 5, 5 5, 6 8, 1 5))",
                     "GEOMETRYCOLLECTION (POLYGON ((1 0, 1 5, 5 5, 1 0)), POLYGON ((6 8, 1 5, 1 9, 6 8)), " +
                        "POLYGON ((9 9, 9 0, 1 0, 9 9)), POLYGON ((6 8, 1 9, 9 9, 6 8)), " +
                        "POLYGON ((9 9, 1 0, 5 5, 9 9)), POLYGON ((5 5, 6 8, 9 9, 5 5)))");
        }

        public void testBadHoleJoinHoleTouchesShell()
        {
            CheckTri("POLYGON ((5 5, 9 5, 9 0, 0 0, 5 5), (3 3, 6 1, 5 3, 3 3))",
                     "GEOMETRYCOLLECTION (POLYGON ((0 0, 3 3, 6 1, 0 0)), POLYGON ((5 3, 3 3, 5 5, 5 3)), " +
                        "POLYGON ((5 5, 9 5, 9 0, 5 5)), POLYGON ((9 0, 0 0, 6 1, 9 0)), " + 
                        "POLYGON ((6 1, 5 3, 5 5, 6 1)), POLYGON ((5 5, 9 0, 6 1, 5 5)))"
                );
        }

        private void CheckTri(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = PolygonTriangulator.Triangulate(geom);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        /**
         * Check union of result equals original geom
         * @param wkt
         */
        private void CheckTri(string wkt)
        {
            var geom = Read(wkt);
            var actual = PolygonTriangulator.Triangulate(geom);
            var actualUnion = actual.Union();

            // compare to fully noded verstion of input polygon
            var nodedGeom = geom.Union(geom);


            CheckEqual(nodedGeom, actualUnion);
        }
    }

}
