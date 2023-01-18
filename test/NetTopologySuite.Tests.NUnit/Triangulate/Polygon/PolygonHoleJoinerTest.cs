using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Triangulate.Polygon;
using NetTopologySuite.Triangulate.Polygon;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Tests.NUnit.Triangulate.Polygon
{
    public class PolygonHoleJoinerTest : GeometryTestCase
    {

        /**
         * A failing case for hole joining with two holes that touch at leftmost vertex
         * Fails due to PolygonHoleJoiner joining holes with same leftmost vertex
         * so that the result linework self-crosses.
         * Note that input is normalized.
         */
        [Test]
        public void TestTouchingHoles()
        {
            CheckJoin(
          "POLYGON ((0 0, 0 9, 9 9, 9 0, 0 0), (1 4, 5 1, 5 4, 1 4), (1 4, 5 5, 6 8, 1 4))",
          "POLYGON ((0 0, 0 9, 1 4, 5 1, 5 4, 1 4, 5 5, 6 8, 1 4, 0 9, 9 9, 9 0, 0 0))"
                );
        }

        [Test]
        public void TestHolesTouchEdgeVertical()
        {
            CheckJoin(
          "POLYGON ((1 9, 9 9, 9 0, 1 0, 1 9), (1 4, 5 1, 5 4, 1 4), (1 5, 5 5, 6 8, 1 5))",
          "POLYGON ((1 9, 9 9, 9 0, 1 0, 1 4, 5 1, 5 4, 1 4, 1 5, 5 5, 6 8, 1 5, 1 9))"
                );
        }

        [Test]
        public void TestHoleTouchesEdgeVertical()
        {
            CheckJoin(
          "POLYGON ((1 9, 9 9, 9 0, 1 0, 1 9), (1 5, 5 5, 6 8, 1 5))",
          "POLYGON ((1 0, 1 5, 5 5, 6 8, 1 5, 1 9, 9 9, 9 0, 1 0))"
                );
        }

        /**
         * Hole ring is CW, to test ring orientation handling.
         */
        [Test]
        public void TestHoleTouchesEdgeWithCloserVertex()
        {
            CheckJoin(
          "POLYGON ((1 9, 9 9, 5 6, 9 6, 19 1, 9 1, 1 9), (8 5, 9 3, 5 5, 8 5))",
          "POLYGON ((1 9, 9 9, 5 6, 9 6, 19 1, 9 1, 5 5, 9 3, 8 5, 5 5, 1 9))"
                );
        }

        [Test]
        public void TestHoleTouchesEdge()
        {
            CheckJoin(
          "POLYGON ((5 5, 9 5, 9 0, 0 0, 5 5), (3 3, 6 1, 5 3, 3 3))",
          "POLYGON ((5 5, 9 5, 9 0, 0 0, 3 3, 6 1, 5 3, 3 3, 5 5))"
                );
        }

        [Test]
        public void TestHoleTouchesVertex()
        {
            CheckJoin(
          "POLYGON ((70 10, 0 0, 7 47, 20 90, 70 60, 70 10), (10 40, 30 50, 7 47, 10 40))",
          "POLYGON ((70 10, 0 0, 7 47, 10 40, 30 50, 7 47, 20 90, 70 60, 70 10))"
                );
        }

        [Test]
        public void TestHolesManyVertical()
        {
            CheckJoin(
          "POLYGON ((10 90, 90 90, 90 10, 10 10, 50 20, 10 30, 50 50, 10 70, 53 80, 10 90), (60 70, 60 60, 50 60, 50 70, 60 70), (55 55, 55 50, 50 50, 50 55, 55 55), (61 45, 61 40, 50 40, 50 45, 61 45), (60 30, 50 30, 50 35, 60 35, 60 30), (50 15, 50 18, 60 18, 60 15, 50 15), (60 85, 50 85, 55 80, 50 75, 60 75, 60 85))",
          "POLYGON ((10 10, 50 15, 60 15, 60 18, 50 18, 50 15, 10 10, 50 20, 50 30, 60 30, 60 35, 50 35, 50 40, 61 40, 61 45, 50 45, 50 40, 50 35, 50 30, 50 20, 10 30, 50 50, 55 50, 55 55, 50 55, 50 60, 60 60, 60 70, 50 70, 50 75, 60 75, 60 85, 50 85, 55 80, 50 75, 50 70, 50 60, 50 55, 50 50, 10 70, 53 80, 10 90, 90 90, 90 10, 10 10))"
                );
        }

        /**
         * A failing case revealing that joining a hole by a zero-length cut
         * was introducing duplicate vertices.
         */
        [Test]
        public void TestZeroLenCutDuplicateVertices()
        {
            CheckJoin(
                "POLYGON ((70 10, 0 0, 7 47, 16 94, 70 60, 70 10), (10 40, 30 49, 7 47, 10 40), (13 59, 13 54, 26 53, 13 59))",
                "POLYGON ((0 0, 7 47, 10 40, 30 49, 7 47, 13 54, 26 53, 13 59, 13 54, 7 47, 16 94, 70 60, 70 10, 0 0))"
                );
        }

        /**
         * Revealed a bug due to original use of tolerance for coordinate comparison (now removed)
         */
        [Test]
        public void TestBugHoleJoinCrosses()
        {
            CheckJoin(
          "POLYGON ((27.182 58.2714, 27.182 58.27, 27.1804 58.27, 27.1804 58.2714, 27.182 58.2714), (27.18097 58.2701, 27.18091 58.27009, 27.18086 58.27007, 27.18086 58.27004, 27.18097 58.2701), (27.18079 58.27099, 27.18074 58.27029, 27.18075 58.27024, 27.18079 58.27019, 27.18151 58.27021, 27.18085 58.27104, 27.18079 58.27099))",
          "POLYGON ((27.1804 58.27, 27.1804 58.2714, 27.18074 58.27029, 27.18075 58.27024, 27.18079 58.27019, 27.18086 58.27004, 27.18097 58.2701, 27.18091 58.27009, 27.18086 58.27007, 27.18086 58.27004, 27.18079 58.27019, 27.18151 58.27021, 27.18085 58.27104, 27.18079 58.27099, 27.18074 58.27029, 27.1804 58.2714, 27.182 58.2714, 27.182 58.27, 27.1804 58.27))"
                );
        }

        private void CheckJoin(string wkt, string wktExpected)
        {
            var geom = (NetTopologySuite.Geometries.Polygon)Read(wkt);
            Geometry actual = PolygonHoleJoiner.JoinAsPolygon(geom);
            if (wktExpected == null)
            {
                TestContext.WriteLine("Result: {0}", actual);
                return;
            }
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }
    }
}
