using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class RobustLineIntersectionTest
    {
        private readonly WKTReader _reader = new WKTReader();

        /// <summary>
        /// Following cases were failures when using the CentralEndpointIntersector heuristic.
        /// This is because one segment lies at a significant angle to the other,
        /// with only one endpoint is close to the other segment.
        /// The CE heuristic chose the wrong endpoint to return.
        /// The fix is to use a new heuristic which out of the 4 endpoints
        /// chooses the one which is closest to the other segment.
        /// This works in all known failure cases.
        /// </summary>                  
        [TestAttribute]
        public void TestCentralEndpointHeuristicFailure()
        {
            CheckIntersection(
                "LINESTRING (163.81867067 -211.31840378, 165.9174252 -214.1665075)",
                "LINESTRING (2.84139601 -57.95412726, 469.59990601 -502.63851732)",
                1,
                "POINT (163.81867067 -211.31840378)",
                0);
        }

        [TestAttribute]
        public void TestCentralEndpointHeuristicFailure2()
        {
            CheckIntersection(
                "LINESTRING (-58.00593335955 -1.43739086465, -513.86101637525 -457.29247388035)",
                "LINESTRING (-215.22279674875 -158.65425425385, -218.1208801283 -160.68343590235)",
                1,
                "POINT ( -215.22279674875 -158.65425425385 )",
                0);
        }

        /// <summary>
        /// Tests a case where intersection point is rounded, 
        /// and it is computed as a nearest endpoint.
        /// Exposed a bug due to aliasing of endpoint. 
        ///  
        /// MD 8 Mar 2013
        /// </summary>
        [TestAttribute]
        public void TestRoundedPointsNotAltered()
        {
            CheckInputNotAltered(
                "LINESTRING (-58.00593335955 -1.43739086465, -513.86101637525 -457.29247388035)",
                "LINESTRING (-215.22279674875 -158.65425425385, -218.1208801283 -160.68343590235)",
                100000);
        }

        /// <summary>
        /// Test from Tomas Fa - JTS list 6/13/2012
        /// </summary>
        /// <remarks>
        /// Fails using original JTS DeVillers determine orientation test.
        /// Succeeds using DD and Shewchuk orientation
        /// </remarks>
        [TestAttribute]
        public void TestTomasFa_1()
        {
            CheckIntersectionNone(
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
        [TestAttribute]
        public void TestTomasFa_2()
        {
            CheckIntersectionNone(
                "LINESTRING (-5.9 163.1, 76.1 250.7)",
                "LINESTRING (14.6 185.0, 96.6 272.6)");
        }

        /// <summary>
        /// Test involving two non-almost-parallel lines.
        /// Does not seem to cause problems with basic line inersection algorithm.
        /// </summary>
        [TestAttribute]
        public void TestLeduc_1()
        {
            CheckIntersection(
                "LINESTRING (305690.0434123494 254176.46578338774, 305601.9999843455 254243.19999846347)",
                "LINESTRING (305689.6153764265 254177.33102743194, 305692.4999844298 254171.4999983967)",
                1,
                "POINT (305690.0434123494 254176.46578338774)",
                0);
        }

        /*
        * Test from strk which is bad in GEOS (2009-04-14).
        */

        [TestAttribute]
        public void TestGEOS_1()
        {
            CheckIntersection(
                "LINESTRING (588750.7429703881 4518950.493668233, 588748.2060409798 4518933.9452804085)",
                "LINESTRING (588745.824857241 4518940.742239175, 588748.2060437313 4518933.9452791475)",
                1,
                "POINT (588748.2060416829 4518933.945284994)",
                0);
        }

        /*
        * Test from strk which is bad in GEOS (2009-04-14).
        */

        [TestAttribute]
        public void TestGEOS_2()
        {
            CheckIntersection(
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
            CheckIntersection(
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
            CheckIntersection(
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
            CheckIntersection(
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

        private void CheckIntersectionNone(String wkt1, String wkt2)
        {
            var l1 = (LineString)_reader.Read(wkt1);
            var l2 = (LineString)_reader.Read(wkt2);
            var pt = new[]
                     {
                         l1.GetCoordinateN(0), l1.GetCoordinateN(1),
                         l2.GetCoordinateN(0), l2.GetCoordinateN(1)
                     };
            CheckIntersection(pt, 0, null, 0);
        }


        private void CheckIntersection(String wkt1, String wkt2,
            int expectedIntersectionNum,
            Coordinate[] intPt,
            double distanceTolerance)
        {
            var l1 = (LineString)_reader.Read(wkt1);
            var l2 = (LineString)_reader.Read(wkt2);
            var pt = new[]
                     {
                         new Coordinate(l1.Coordinates[0]), new Coordinate(l1.Coordinates[1]),
                         new Coordinate(l2.Coordinates[0]), new Coordinate(l2.Coordinates[1])
                     };
            CheckIntersection(pt, expectedIntersectionNum, intPt, distanceTolerance);
        }

        private void CheckIntersection(String wkt1, String wkt2,
            int expectedIntersectionNum,
            String expectedWKT,
            double distanceTolerance)
        {
            var l1 = (LineString)_reader.Read(wkt1);
            var l2 = (LineString)_reader.Read(wkt2);
            var pt = new[]
                     {
                         new Coordinate(l1.Coordinates[0]), new Coordinate(l1.Coordinates[1]),
                         new Coordinate(l2.Coordinates[0]), new Coordinate(l2.Coordinates[1])
                     };
            var g = _reader.Read(expectedWKT);
            var intPt = g.Coordinates;
            CheckIntersection(pt, expectedIntersectionNum, intPt, distanceTolerance);
        }

        /// <summary>
        /// Check that intersection of segment defined by points in pt array
        /// is equal to the expectedIntPt value (up to the given distanceTolerance)
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="expectedIntersectionNum"></param>
        /// <param name="expectedIntPt">the expected intersection points (maybe null if not tested)</param>
        /// <param name="distanceTolerance">tolerance to use for equality test</param>
        private void CheckIntersection(Coordinate[] pt,
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
                if (intNum == 1)
                {
                    CheckIntPoints(expectedIntPt[0], li.GetIntersection(0), distanceTolerance);
                }
                else if (intNum == 2)
                {
                    CheckIntPoints(expectedIntPt[1], li.GetIntersection(0), distanceTolerance);
                    CheckIntPoints(expectedIntPt[1], li.GetIntersection(0), distanceTolerance);

                    if (!(Equals(expectedIntPt[0], li.GetIntersection(0), distanceTolerance)
                          || Equals(expectedIntPt[0], li.GetIntersection(1), distanceTolerance)))
                    {
                        CheckIntPoints(expectedIntPt[0], li.GetIntersection(0), distanceTolerance);
                        CheckIntPoints(expectedIntPt[0], li.GetIntersection(1), distanceTolerance);
                    }
                    else if (!(Equals(expectedIntPt[1], li.GetIntersection(0), distanceTolerance)
                               || Equals(expectedIntPt[1], li.GetIntersection(1), distanceTolerance)))
                    {
                        CheckIntPoints(expectedIntPt[1], li.GetIntersection(0), distanceTolerance);
                        CheckIntPoints(expectedIntPt[1], li.GetIntersection(1), distanceTolerance);
                    }
                }
            }
        }

        private void CheckIntPoints(Coordinate p, Coordinate q, double distanceTolerance)
        {
            var isEqual = Equals(p, q, distanceTolerance);
            Assert.IsTrue(isEqual, "Int Pts not equal - "
                                   + WKTWriter.ToPoint(p) + " vs "
                                   + WKTWriter.ToPoint(q));
        }

        public bool Equals(Coordinate p0, Coordinate p1, double distanceTolerance)
        {
            return p0.Distance(p1) <= distanceTolerance;
        }

        private void CheckInputNotAltered(String wkt1, String wkt2, int scaleFactor)
        {
            LineString l1 = (LineString)_reader.Read(wkt1);
            LineString l2 = (LineString)_reader.Read(wkt2);
            Coordinate[] pt =
            {
                l1.GetCoordinateN(0),
                l1.GetCoordinateN(1), 
                l2.GetCoordinateN(0), 
                l2.GetCoordinateN(1)
            };
            CheckInputNotAltered(pt, scaleFactor);
        }

        public void CheckInputNotAltered(Coordinate[] pt, int scaleFactor)
        {
            // save input points
            Coordinate[] savePt = new Coordinate[4];
            for (int i = 0; i < 4; i++)
            {
                savePt[i] = new Coordinate(pt[i]);
            }

            LineIntersector li = new RobustLineIntersector();
            li.PrecisionModel = new PrecisionModel(scaleFactor);
            li.ComputeIntersection(pt[0], pt[1], pt[2], pt[3]);

            // check that input points are unchanged
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(savePt[i], pt[i], "Input point " + i + " was altered - ");
            }
        }
    }
}