using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class IntersectionTest
    {
        private const double MAX_ABS_ERROR = 1e-5;

        [Test]
        public void TestSimple()
        {
            CheckIntersection(
                0, 0, 10, 10,
                0, 10, 10, 0,
                5, 5);
        }

        [Test]
        public void TestCollinear()
        {
            CheckIntersectionNull(
                0, 0, 10, 10,
                20, 20, 30, 30);
        }

        [Test]
        public void TestParallel()
        {
            CheckIntersectionNull(
                0, 0, 10, 10,
                10, 0, 20, 10);
        }

        // See JTS GitHub issue #464
        [Test]
        public void TestAlmostCollinear()
        {
            CheckIntersection(
                35613471.6165017, 4257145.306132293, 35613477.7705378, 4257160.528222711,
                35613477.77505724, 4257160.539653536, 35613479.85607389, 4257165.92369170,
                35613477.772841461, 4257160.5339209242);
        }

        // same as above but conditioned manually
        [Test]
        public void TestAlmostCollinearCond()
        {
            CheckIntersection(
                1.6165017, 45.306132293, 7.7705378, 60.528222711,
                7.77505724, 60.539653536, 9.85607389, 65.92369170,
                7.772841461, 60.5339209242);
        }


        //------------------------------------------------------------

        [Test]
        public void TestLineSegCross()
        {
            checkIntersectionLineSegment(0, 0, 0, 1, -1, 9, 1, 9, 0, 9);
            checkIntersectionLineSegment(0, 0, 0, 1, -1, 2, 1, 4, 0, 3);
        }

        [Test]
        public void TestLineSegTouch()
        {
            checkIntersectionLineSegment(0, 0, 0, 1, -1, 9, 0, 9, 0, 9);
            checkIntersectionLineSegment(0, 0, 0, 1, 0, 2, 1, 4, 0, 2);
        }

        [Test]
        public void TestLineSegCollinear()
        {
            checkIntersectionLineSegment(0, 0, 0, 1, 0, 9, 0, 8, 0, 9);
        }

        [Test]
        public void TestLineSegNone()
        {
            checkIntersectionLineSegmentNull(0, 0, 0, 1, 2, 9, 1, 9);
            checkIntersectionLineSegmentNull(0, 0, 0, 1, -2, 9, -1, 9);
            checkIntersectionLineSegmentNull(0, 0, 0, 1, 2, 9, 1, 9);
        }

        //==================================================


        private void CheckIntersection(
            double p1x, double p1y, double p2x, double p2y,
            double q1x, double q1y, double q2x, double q2y,
            double expectedx, double expectedy)
        {
            var p1 = new Coordinate(p1x, p1y);
            var p2 = new Coordinate(p2x, p2y);
            var q1 = new Coordinate(q1x, q1y);
            var q2 = new Coordinate(q2x, q2y);
            //Coordinate actual = CGAlgorithmsDD.intersection(p1, p2, q1, q2);
            var actual = IntersectionComputer.Intersection(p1, p2, q1, q2);
            var expected = new Coordinate(expectedx, expectedy);
            double dist = actual.Distance(expected);
            //System.out.println("Expected: " + expected + "  Actual: " + actual + "  Dist = " + dist);
            Assert.That(dist <= MAX_ABS_ERROR);
        }

        private void CheckIntersectionNull(
            double p1x, double p1y, double p2x, double p2y,
            double q1x, double q1y, double q2x, double q2y)
        {
            var p1 = new Coordinate(p1x, p1y);
            var p2 = new Coordinate(p2x, p2y);
            var q1 = new Coordinate(q1x, q1y);
            var q2 = new Coordinate(q2x, q2y);
            var actual = IntersectionComputer.Intersection(p1, p2, q1, q2);
            Assert.That(actual == null);
        }


        private void checkIntersectionLineSegment(double p1x, double p1y, double p2x, double p2y,
            double q1x, double q1y, double q2x, double q2y,
            double expectedx, double expectedy)
        {
            var p1 = new Coordinate(p1x, p1y);
            var p2 = new Coordinate(p2x, p2y);
            var q1 = new Coordinate(q1x, q1y);
            var q2 = new Coordinate(q2x, q2y);
            //Coordinate actual = CGAlgorithmsDD.intersection(p1, p2, q1, q2);
            var actual = IntersectionComputer.LineSegment(p1, p2, q1, q2);
            var expected = new Coordinate(expectedx, expectedy);
            double dist = actual.Distance(expected);
            //System.out.println("Expected: " + expected + "  Actual: " + actual + "  Dist = " + dist);
            Assert.That(dist, Is.LessThanOrEqualTo(MAX_ABS_ERROR));
        }

        private void checkIntersectionLineSegmentNull(double p1x, double p1y, double p2x, double p2y,
            double q1x, double q1y, double q2x, double q2y)
        {
            var p1 = new Coordinate(p1x, p1y);
            var p2 = new Coordinate(p2x, p2y);
            var q1 = new Coordinate(q1x, q1y);
            var q2 = new Coordinate(q2x, q2y);
            var actual = IntersectionComputer.LineSegment(p1, p2, q1, q2);
            Assert.That(actual, Is.Null);
        }
    }
}
