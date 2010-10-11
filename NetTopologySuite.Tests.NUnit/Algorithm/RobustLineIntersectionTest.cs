using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NetTopologySuite.Tests.NUnit;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /**
     * Tests robustness and correctness of RobustLineIntersector.
     * Failure modes can include exceptions thrown, or incorrect
     * results returned.
     * 
     * @author Owner
     *
     */
    [TestFixture]
    public class RobustLineIntersectionTest
    {
        /**
         * Test from strk which is bad in GEOS (2009-04-14).
         * 
         * @throws ParseException
         */
        [Test]
        public void TestLeduc1()
        {
            ComputeIntersection(
                    "LINESTRING (305690.0434123494 254176.46578338774, 305601.9999843455 254243.19999846347)",
                    "LINESTRING (305689.6153764265 254177.33102743194, 305692.4999844298 254171.4999983967)",
                    (LineIntersectionDegrees)1,
                    "POINT (305690.0434123494 254176.46578338774)",
                    0);
        }

        /**
         * Test from strk which is bad in GEOS (2009-04-14).
         * 
         * @throws ParseException
         */
        [Test]
        public void TestGeos1()
        {
            ComputeIntersection(
                    "LINESTRING (588750.7429703881 4518950.493668233, 588748.2060409798 4518933.9452804085)",
                    "LINESTRING (588745.824857241 4518940.742239175, 588748.2060437313 4518933.9452791475)",
                    (LineIntersectionDegrees)1,
                    "POINT (588748.2060416829 4518933.945284994)",
                    0);
        }

        /**
         * Test from strk which is bad in GEOS (2009-04-14).
         * 
         * @throws ParseException
         */
        [Test]
        public void TestGeos2()
        {
            ComputeIntersection(
                    "LINESTRING (588743.626135934 4518924.610969561, 588732.2822865889 4518925.4314047815)",
                    "LINESTRING (588739.1191384895 4518927.235700594, 588731.7854614238 4518924.578370095)",
                    (LineIntersectionDegrees)1,
                    "POINT (588733.8306132929 4518925.319423238)",
                    0);
        }

        /**
         * This used to be a failure case (exception), but apparently works now.
         * Possibly normalization has fixed this?
         * 
         * @throws ParseException
         */
        [Test]
        public void TestDaveSkeaCase()
        {
            ComputeIntersection(
                    "LINESTRING ( 2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649 )",
                    "LINESTRING ( 1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034 )",
                    (LineIntersectionDegrees)2,
                    new Coordinate[] {
						GeometryUtils.CoordFac.Create(2089426.5233462777, 1180182.3877339689),
						GeometryUtils.CoordFac.Create(2085646.6891757075, 1195618.7333999649)
				}, 0);
        }

        /**
         * Outside envelope using HCoordinate method.
         * 
         * @throws ParseException
         */
        [Test]
        public void TestCmp5CaseWKT()
        {
            ComputeIntersection(
                    "LINESTRING (4348433.262114629 5552595.478385733, 4348440.849387404 5552599.272022122 )",
                    "LINESTRING (4348433.26211463  5552595.47838573,  4348440.8493874   5552599.27202212  )",
                    (LineIntersectionDegrees)1,
                    new Coordinate[] {
						GeometryUtils.CoordFac.Create(4348437.0557510145, 5552597.375203926),
				},
                    0);
        }

        /**
         * Result of this test should be the same as the WKT one!
         * @throws ParseException
         */
        [Test]
        public void TestCmp5CaseRaw()
        {
            ComputeIntersection(
                    new Coordinate[] { 
						GeometryUtils.CoordFac.Create(4348433.262114629, 5552595.478385733),
						GeometryUtils.CoordFac.Create(4348440.849387404, 5552599.272022122),
						 						
						GeometryUtils.CoordFac.Create(4348433.26211463,  5552595.47838573),
						GeometryUtils.CoordFac.Create(4348440.8493874,   5552599.27202212)
				}, (LineIntersectionDegrees)1,
                    new Coordinate[] {
						GeometryUtils.CoordFac.Create(4348437.0557510145, 5552597.375203926),
				},
                    0);
        }

        static void ComputeIntersection(String wkt1, String wkt2,
                LineIntersectionDegrees expectedIntersectionNum,
                Coordinate[] intPt,
                double distanceTolerance)
        {
            ILineString<Coordinate> l1 = (ILineString<Coordinate>)GeometryUtils.ReadWKT(wkt1);
            ILineString<Coordinate> l2 = (ILineString<Coordinate>)GeometryUtils.ReadWKT(wkt2);
            Coordinate[] pt = new Coordinate[] {
				l1.Coordinates[0], l1.Coordinates[1],
				l2.Coordinates[0], l2.Coordinates[1]
		};
            ComputeIntersection(pt, expectedIntersectionNum, intPt, distanceTolerance);
        }

        static void ComputeIntersection(String wkt1, String wkt2,
                LineIntersectionDegrees expectedIntersectionNum,
                String expectedWKT,
                double distanceTolerance)
        {
            ILineString<Coordinate> l1 = (ILineString<Coordinate>)GeometryUtils.ReadWKT(wkt1);
            ILineString<Coordinate> l2 = (ILineString<Coordinate>)GeometryUtils.ReadWKT(wkt2);
            Coordinate[] pt = new Coordinate[] {
				l1.Coordinates[0], l1.Coordinates[1],
				l2.Coordinates[0], l2.Coordinates[1]
		};
            IGeometry<Coordinate> g = GeometryUtils.ReadWKT(expectedWKT);
            Coordinate[] intPt = g.Coordinates.ToArray();
            ComputeIntersection(pt, expectedIntersectionNum, intPt, distanceTolerance);
        }

        /**
         * 
         * @param pt
         * @param expectedIntersectionNum
         * @param intPt the expected intersection points (maybe null if not tested)
         */

        static void ComputeIntersection(Coordinate[] pt,
                LineIntersectionDegrees expectedIntersectionNum,
                Coordinate[] intPt,
                double distanceTolerance)
        {
            LineIntersector<Coordinate> li = new RobustLineIntersector<Coordinate>(GeometryUtils.GeometryFactory);
            Intersection<Coordinate> intersection = li.ComputeIntersection(pt[0], pt[1], pt[2], pt[3]);

            LineIntersectionDegrees intNum = intersection.IntersectionDegree;
            Assert.AreEqual(expectedIntersectionNum, intNum, "Number of intersections not as expected");

            if (intPt != null)
            {
                Assert.AreEqual((int)intNum, intPt.Length, "Wrong number of expected int pts provided");
                // test that both points are represented here
                Boolean isIntPointsCorrect = true;
                if (intNum == LineIntersectionDegrees.Intersects)
                {
                    TestIntPoints(intPt[0], intersection.GetIntersectionPoint(0), distanceTolerance);
                }
                else if (intNum == LineIntersectionDegrees.Collinear)
                {
                    TestIntPoints(intPt[1], intersection.GetIntersectionPoint(0), distanceTolerance);
                    TestIntPoints(intPt[1], intersection.GetIntersectionPoint(0), distanceTolerance);

                    if (!(Equals(intPt[0], intersection.GetIntersectionPoint(0), distanceTolerance)
                            || Equals(intPt[0], intersection.GetIntersectionPoint(1), distanceTolerance)))
                    {
                        TestIntPoints(intPt[0], intersection.GetIntersectionPoint(0), distanceTolerance);
                        TestIntPoints(intPt[0], intersection.GetIntersectionPoint(1), distanceTolerance);
                    }
                    else if (!(Equals(intPt[1], intersection.GetIntersectionPoint(0), distanceTolerance)
                            || Equals(intPt[1], intersection.GetIntersectionPoint(1), distanceTolerance)))
                    {
                        TestIntPoints(intPt[1], intersection.GetIntersectionPoint(0), distanceTolerance);
                        TestIntPoints(intPt[1], intersection.GetIntersectionPoint(1), distanceTolerance);
                    }
                }
                //assertTrue("Int Pts not equal", isIntPointsCorrect);
            }
        }

        static void TestIntPoints(Coordinate p, Coordinate q, double distanceTolerance)
        {
            Boolean isEqual = Equals(p, q, distanceTolerance);
            Assert.IsTrue(isEqual, "Int Pts not equal - "
                    + p.ToString() + " vs "
                    + q.ToString());
        }

        public static Boolean Equals(Coordinate p0, Coordinate p1, double distanceTolerance)
        {
            return p0.Distance(p1) <= distanceTolerance;
        }

    }
}