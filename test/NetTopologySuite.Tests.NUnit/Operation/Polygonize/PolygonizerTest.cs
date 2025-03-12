using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Polygonize
{
    [TestFixture]
    public class PolygonizerTest : GeometryTestCase
    {
        [Test]
        public void TestEmptyInput()
        {
            CheckPolygonize(new string[] { "LINESTRING EMPTY", "LINESTRING EMPTY" },
              new string[] { });
        }

        [Test]
        public void TestPolygonWithTouchingHole()
        {
            CheckPolygonize(new string[]{
                "LINESTRING (100 180, 20 20, 160 20, 100 180)",
                "LINESTRING (100 180, 80 60, 120 60, 100 180)",
            },
            new string[]{
                "POLYGON ((100 180, 120 60, 80 60, 100 180))",
                "POLYGON ((100 180, 160 20, 20 20, 100 180), (100 180, 80 60, 120 60, 100 180))"
            });
        }

        [Test]
        public void TestPolygonWithTouchingHoleAndNotch()
        {
            CheckPolygonize(new string[]{
        "LINESTRING (0 0, 4 0)",
        "LINESTRING (4 0, 5 3)",
"LINESTRING (5 3, 4 6, 6 6, 5 3)",
"LINESTRING (5 3, 6 0)",
"LINESTRING (6 0, 10 0, 5 10, 0 0)",
"LINESTRING (4 0, 6 0)"
    },
            new string[]{
"POLYGON ((5 3, 4 0, 0 0, 5 10, 10 0, 6 0, 5 3), (5 3, 6 6, 4 6, 5 3))",
"POLYGON ((5 3, 4 6, 6 6, 5 3))",
"POLYGON ((4 0, 5 3, 6 0, 4 0))"
    });
        }
        [Test]
        public void TestPolygonal1()
        {
            CheckPolygonize(true, new string[]{
        "LINESTRING (100 100, 100 300, 300 300, 300 100, 100 100)",
        "LINESTRING (150 150, 150 250, 250 250, 250 150, 150 150)"
    },
            new string[]{
"POLYGON ((100 100, 100 300, 300 300, 300 100, 100 100), (150 150, 150 250, 250 250, 250 150, 150 150))"
    });
        }
        [Test]
        public void TestPolygonal2()
        {
            CheckPolygonize(true, new string[]{
        "LINESTRING (100 100, 100 0, 0 0, 0 100, 100 100)"
            ,"LINESTRING (10 10, 10 30, 20 30)"
            ,"LINESTRING (20 30, 30 30, 30 20)"
            ,"LINESTRING (30 20, 30 10, 10 10)"
            ,"LINESTRING (40 40, 40 20, 30 20)"
            ,"LINESTRING (30 20, 20 20, 20 30)"
            ,"LINESTRING (20 30, 20 40, 40 40))"
    },
            new string[]{
"POLYGON ((0 0, 0 100, 100 100, 100 0, 0 0), (10 10, 30 10, 30 20, 40 20, 40 40, 20 40, 20 30, 10 30, 10 10))",
"POLYGON ((20 20, 20 30, 30 30, 30 20, 20 20))"
    });
        }
        [Test]
        public void TestPolygonalOuterOnly1()
        {
            CheckPolygonize(true, new string[] {
        "LINESTRING (10 10, 10 20, 20 20)"
            ,"LINESTRING (20 20, 20 10)"
            ,"LINESTRING (20 10, 10 10)"
            ,"LINESTRING (20 20, 30 20, 30 10, 20 10)"
    },
            new string[]{
"POLYGON ((20 20, 20 10, 10 10, 10 20, 20 20))"
    });
        }
        [Test]
        public void TestPolygonalOuterOnly2()
        {
            CheckPolygonize(true, new string[] {
        "LINESTRING (100 400, 200 400, 200 300)"
            ,"LINESTRING (200 300, 150 300)"
            ,"LINESTRING (150 300, 100 300, 100 400)"
            ,"LINESTRING (200 300, 250 300, 250 200)"
            ,"LINESTRING (250 200, 200 200)"
            ,"LINESTRING (200 200, 150 200, 150 300)"
            ,"LINESTRING (250 200, 300 200, 300 100, 200 100, 200 200)"
    },
            new string[]{
        "POLYGON ((150 300, 100 300, 100 400, 200 400, 200 300, 150 300))"
       ,"POLYGON ((200 200, 250 200, 300 200, 300 100, 200 100, 200 200))"
    });
        }

        readonly string[] LINES_CHECKERBOARD = new string[] {
      "LINESTRING (10 20, 20 20)",
      "LINESTRING (10 20, 10 30)",
      "LINESTRING (20 10, 10 10, 10 20)",
      "LINESTRING (10 30, 20 30)",
      "LINESTRING (10 30, 10 40, 20 40)",
      "LINESTRING (30 10, 20 10)",
      "LINESTRING (20 20, 20 10)",
      "LINESTRING (20 20, 30 20)",
      "LINESTRING (20 30, 20 20)",
      "LINESTRING (20 30, 30 30)",
      "LINESTRING (20 40, 20 30)",
      "LINESTRING (20 40, 30 40)",
      "LINESTRING (40 20, 40 10, 30 10)",
      "LINESTRING (30 20, 30 10)",
      "LINESTRING (30 20, 40 20)",
      "LINESTRING (30 30, 30 20)",
      "LINESTRING (30 30, 40 30)",
      "LINESTRING (30 40, 30 30)",
      "LINESTRING (30 40, 40 40, 40 30)",
      "LINESTRING (40 30, 40 20)"
  };

        [Test]
        public void TestPolygonalOuterOnlyCheckerboard()
        {
            CheckPolygonize(true, LINES_CHECKERBOARD,
            new string[]{
        "POLYGON ((10 20, 20 20, 20 10, 10 10, 10 20))"
        ,"POLYGON ((20 30, 10 30, 10 40, 20 40, 20 30))"
        ,"POLYGON ((30 20, 20 20, 20 30, 30 30, 30 20))"
        ,"POLYGON ((30 10, 30 20, 40 20, 40 10, 30 10))"
        ,"POLYGON ((30 40, 40 40, 40 30, 30 30, 30 40))"
    });
        }

        /**
         * Input is invalid (non-noded), but Polygonizer should still not fail.
         * Output is undefined, however.
         */
        [Test]
        public void TestNonNodedWithHoleNotAssignable()
        {
            CheckPolygonizeNoError(
                new []{
                    "MULTILINESTRING ((10 90, 30 90, 30 30, 70 30, 70 90, 90 90, 90 10, 10 10, 10 90), (30 90, 70 90, 70 30, 30 30, 30 90))"
                });
        }

        [Test]
        public void TestUniqueInvalidRings()
        {
            CheckPolygonizeInvalidRings(
                "MULTILINESTRING ((0 0, 2 0, 2 2, 0 2, 0 0), (0 0, 1 1), (2 2, 4 4, 2 4, 4 2, 2 2))",
                "LINESTRING (2 2, 4 2, 2 4, 4 4, 2 2)"
                );
        }

        [Test]
        public void TestUniqueInvalidRings2()
        {
            CheckPolygonizeInvalidRings(
                "MULTILINESTRING ((3 8, 7 8), (7 8, 7 3), (7 3, 3 3), (3 3, 3 8), (7 3, 9 6, 9 5, 7 8), (3 8, 1 5), (1 5, 1 6), (1 6, 3 3))",
                "MULTILINESTRING ((3 8, 3 3, 1 6, 1 5, 3 8), (7 3, 7 8, 9 5, 9 6, 7 3))"
                );
        }



        private void CheckPolygonize(string[] inputWKT, string[] expectedOutputWKT)
        {
            CheckPolygonize(false, inputWKT, expectedOutputWKT);
        }

        private void CheckPolygonize(bool extractOnlyPolygonal, string[] inputWKT, string[] expectedWKT)
        {
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(ReadList(inputWKT));
            var expected = ReadList(expectedWKT);
            var actual = polygonizer.GetPolygons();
            CheckEqual(expected, actual);
        }

        private void CheckPolygonizeNoError(string[] inputWKT)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(ReadList(inputWKT));
            try
            {
                var actual = polygonizer.GetPolygons();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine(ex.Message);
                TestContext.WriteLine(ex.StackTrace);
                Assert.Fail("Polygonizer threw an unexpected error");
            }
        }
        private void CheckPolygonizeInvalidRings(string inputWKT, string expectedWKT)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(Read(inputWKT));
            var actualList = polygonizer.GetInvalidRingLines();
            var expected = Read(expectedWKT);
            var actual = expected.Factory.BuildGeometry(actualList);
            /*
             * Use topological equality to handle differences in ring orientation and order
             */
            bool isSameLinework = expected.EqualsTopologically(actual);
            Assert.That(isSameLinework, Is.True);
        }
    }
}
