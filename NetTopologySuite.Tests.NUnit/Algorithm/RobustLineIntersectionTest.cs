using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class RobustLineIntersectionTest
    {
        private readonly WKTReader _reader = new WKTReader();

        /// <summary>
        /// Test from Tomas Fa - JTS list 6/13/2012
        /// </summary>
        /// <remarks>
        /// Fails using original JTS DeVillers determine orientation test.
        /// Succeeds using DD and Shewchuk orientation
        /// </remarks>
        [Test]
        [Category("KnownToFail")]
        public void TestTomasFa_1()
        {
            ComputeIntersectionNone(
                "LINESTRING (-42.0 163.2, 21.2 265.2)",
                "LINESTRING (-26.2 188.7, 37.0 290.7)");
        }

        /// <summary>
        /// Test from Tomas Fa - JTS list 6/13/2012
        /// </summary>
        /// <remarks>
        /// Fails using original JTS DeVillers determine orientation test.
        /// Succeeds using DD and Shewchuk orientation
        /// </remarks>
        [Test]
        [Category("KnownToFail")]
        public void TestTomasFa_2()
        {
            ComputeIntersectionNone(
                "LINESTRING (-5.9 163.1, 76.1 250.7)",
                "LINESTRING (14.6 185.0, 96.6 272.6)");
        }

        /*
        * Test from strk which is bad in GEOS (2009-04-14).
        */

        [Test]
        public void TestLeduc_1()
        {
            ComputeIntersection(
                "LINESTRING (305690.0434123494 254176.46578338774, 305601.9999843455 254243.19999846347)",
                "LINESTRING (305689.6153764265 254177.33102743194, 305692.4999844298 254171.4999983967)",
                1,
                "POINT (305690.0434123494 254176.46578338774)",
                0);
        }

        /*
        * Test from strk which is bad in GEOS (2009-04-14).
        */

        [Test]
        public void TestGEOS_1()
        {
            ComputeIntersection(
                "LINESTRING (588750.7429703881 4518950.493668233, 588748.2060409798 4518933.9452804085)",
                "LINESTRING (588745.824857241 4518940.742239175, 588748.2060437313 4518933.9452791475)",
                1,
                "POINT (588748.2060416829 4518933.945284994)",
                0);
        }

        /*
        * Test from strk which is bad in GEOS (2009-04-14).
        */

        [Test]
        public void TestGEOS_2()
        {
            ComputeIntersection(
                "LINESTRING (588743.626135934 4518924.610969561, 588732.2822865889 4518925.4314047815)",
                "LINESTRING (588739.1191384895 4518927.235700594, 588731.7854614238 4518924.578370095)",
                1,
                "POINT (588733.8306132929 4518925.319423238)",
                0);
        }

        /*
        * This used to be a failure case (exception), but apparently works now.
        * Possibly normalization has fixed this?
        */

        [Ignore(
            "The result of this test is currently failing.  The lines are very close to colinear along a partial segment, so I think there is rounding problems occuring.  There are changes in versions of JTS beyond version 1.9 which calculate HCoordinate intersections slightly differently, so those changes may resolve this test failure in the future."
            )]
        public void TestDaveSkeaCase()
        {
            ComputeIntersection(
                "LINESTRING ( 2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649 )",
                "LINESTRING ( 1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034 )",
                2,
                new Coordinate[]
                    {
                        new Coordinate(2087536.6062609926, 1187900.560566967)
                    },
                0);
        }

        /*
        * Outside envelope using HCoordinate method.
        */

        [Ignore(
            "The result of this test is currently failing.  The lines are very close to colinear, so I think there is rounding problems occuring.  There are changes in versions of JTS beyond version 1.9 which calculate HCoordinate intersections slightly differently, so those changes may resolve this test failure in the future."
            )]
        public void TestCmp5CaseWKT()
        {
            ComputeIntersection(
                "LINESTRING (4348433.262114629 5552595.478385733, 4348440.849387404 5552599.272022122 )",
                "LINESTRING (4348433.26211463  5552595.47838573,  4348440.8493874   5552599.27202212  )",
                1,
                new[]
                    {
                        new Coordinate(4348437.0557510145, 5552597.375203926),
                    },
                0);
        }

        /*
        * Result of this test should be the same as the WKT one!
        */

        [Ignore(
            "The result of this test is currently failing.  The lines are very close to colinear, so I think there is rounding problems occuring.  There are changes in versions of JTS beyond version 1.9 which calculate HCoordinate intersections slightly differently, so those changes may resolve this test failure in the future."
            )]
        public void TestCmp5CaseRaw()
        {
            ComputeIntersection(
                new[]
                    {
                        new Coordinate(4348433.262114629, 5552595.478385733),
                        new Coordinate(4348440.849387404, 5552599.272022122),

                        new Coordinate(4348433.26211463, 5552595.47838573),
                        new Coordinate(4348440.8493874, 5552599.27202212)
                    },
                1,
                new[]
                    {
                        new Coordinate(4348440.8493874, 5552599.27202212),
                    },
                0);
        }

        private void ComputeIntersectionNone(String wkt1, String wkt2)
        {
            var l1 = (LineString) _reader.Read(wkt1);
            var l2 = (LineString) _reader.Read(wkt2);
            var pt = new[]
                                  {
                                      l1.GetCoordinateN(0), l1.GetCoordinateN(1),
                                      l2.GetCoordinateN(0), l2.GetCoordinateN(1)
                                  };
            ComputeIntersection(pt, 0, null, 0);
        }


        private void ComputeIntersection(String wkt1, String wkt2,
                                         int expectedIntersectionNum,
                                         Coordinate[] intPt,
                                         double distanceTolerance)
        {
            var l1 = (LineString) _reader.Read(wkt1);
            var l2 = (LineString) _reader.Read(wkt2);
            var pt = new[]
                                  {
                                      new Coordinate(l1.Coordinates[0]), new Coordinate(l1.Coordinates[1]),
                                      new Coordinate(l2.Coordinates[0]), new Coordinate(l2.Coordinates[1])
                                  };
            ComputeIntersection(pt, expectedIntersectionNum, intPt, distanceTolerance);
        }

        private void ComputeIntersection(String wkt1, String wkt2,
                                         int expectedIntersectionNum,
                                         String expectedWKT,
                                         double distanceTolerance)
        {
            var l1 = (LineString) _reader.Read(wkt1);
            var l2 = (LineString) _reader.Read(wkt2);
            var pt = new[]
                                  {
                                      new Coordinate(l1.Coordinates[0]), new Coordinate(l1.Coordinates[1]),
                                      new Coordinate(l2.Coordinates[0]), new Coordinate(l2.Coordinates[1])
                                  };
            var g = _reader.Read(expectedWKT);
            var intPt = g.Coordinates;
            ComputeIntersection(pt, expectedIntersectionNum, intPt, distanceTolerance);
        }

        /*
        * @param pt
        * @param expectedIntersectionNum
        * @param expectedintPt the expected intersection points (maybe null if not tested)
        */


        private static void ComputeIntersection(Coordinate[] pt,
                                         int expectedIntersectionNum,
                                         Coordinate[] expectedIntPt,
                                         double distanceTolerance)
        {
            LineIntersector li = new RobustLineIntersector();
            li.ComputeIntersection(pt[0], pt[1], pt[2], pt[3]);

            int intNum = li.IntersectionNum;
            Assert.AreEqual(expectedIntersectionNum, intNum, "Number of intersections not as expected");

            if (expectedIntPt != null)
            {
                Assert.AreEqual(intNum, expectedIntPt.Length, "Wrong number of expected int pts provided");
                // test that both points are represented here
                var isIntPointsCorrect = true;
                if (intNum == 1)
                {
                    TestIntPoints(expectedIntPt[0], li.GetIntersection(0), distanceTolerance);
                }
                else if (intNum == 2)
                {
                    TestIntPoints(expectedIntPt[1], li.GetIntersection(0), distanceTolerance);
                    TestIntPoints(expectedIntPt[1], li.GetIntersection(0), distanceTolerance);

                    if (!(Equals(expectedIntPt[0], li.GetIntersection(0), distanceTolerance)
                          || Equals(expectedIntPt[0], li.GetIntersection(1), distanceTolerance)))
                    {
                        TestIntPoints(expectedIntPt[0], li.GetIntersection(0), distanceTolerance);
                        TestIntPoints(expectedIntPt[0], li.GetIntersection(1), distanceTolerance);
                    }
                    else if (!(Equals(expectedIntPt[1], li.GetIntersection(0), distanceTolerance)
                               || Equals(expectedIntPt[1], li.GetIntersection(1), distanceTolerance)))
                    {
                        TestIntPoints(expectedIntPt[1], li.GetIntersection(0), distanceTolerance);
                        TestIntPoints(expectedIntPt[1], li.GetIntersection(1), distanceTolerance);
                    }
                }
                //assertTrue("Int Pts not equal", isIntPointsCorrect);
            }
        }

        private static void TestIntPoints(Coordinate p, Coordinate q, double distanceTolerance)
        {
            var isEqual = Equals(p, q, distanceTolerance);
            Assert.IsTrue(isEqual, "Int Pts not equal - "
                                   + WKTWriter.ToPoint(p) + " vs "
                                   + WKTWriter.ToPoint(q));
        }

        public static bool Equals(Coordinate p0, Coordinate p1, double distanceTolerance)
        {
            return p0.Distance(p1) <= distanceTolerance;
        }
    }
}