using System;
using System.Diagnostics;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /**
     * Tests for Z computation for intersections.
     * 
     * @author mdavis
     *
     */
    public class RobustLineIntersectorZTest : GeometryTestCase
    {

        [Test]
        public void TestInterior()
        {
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(1, 3, 10, 3, 1, 30),
                pt(2, 2, 11));
        }

        [Test]
        public void TestInterior2D()
        {
            CheckIntersection(Line(1, 1, 3, 3), Line(1, 3, 3, 1),
                pt(2, 2, double.NaN));
        }

        [Test]
        public void TestInterior3D2D()
        {
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(1, 3, 3, 1),
                pt(2, 2, 2));
        }

        [Test]
        public void TestInterior2D3D()
        {
            CheckIntersection(Line(1, 1, 3, 3), Line(1, 3, 10, 3, 1, 30),
                pt(2, 2, 20));
        }

        [Test]
        public void TestInterior2D3DPart()
        {
            // result is average of line1 interpolated and line2 p0 Z
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(1, 3, 10, 3, 1, double.NaN),
                pt(2, 2, 6));
        }

        [Test]
        public void TestEndpoint()
        {
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(3, 3, 3, 3, 1, 30),
                pt(3, 3, 3));
        }

        [Test]
        public void TestEndpoint2D()
        {
            CheckIntersection(Line(1, 1, 3, 3), Line(3, 3, 3, 1),
                pt(3, 3, double.NaN));
        }

        [Test]
        public void TestEndpoint2D3D()
        {
            // result Z is from 3D point
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(3, 3, 3, 1),
                pt(3, 3, 3));
        }

        [Test]
        public void TestInteriorEndpoint()
        {
            // result Z is from 3D point
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(2, 2, 10, 3, 1, 30),
                pt(2, 2, 10));
        }

        [Test]
        public void TestInteriorEndpoint3D2D()
        {
            // result Z is interpolated
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(2, 2, 3, 1),
                pt(2, 2, 2));
        }

        [Test]
        public void TestInteriorEndpoint2D3D()
        {
            // result Z is from 3D point
            CheckIntersection(Line(1, 1, 3, 3), Line(2, 2, 10, 3, 1, 20),
                pt(2, 2, 10));
        }

        [Test]
        public void TestCollinearEqual()
        {
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(1, 1, 1, 3, 3, 3),
                pt(1, 1, 1), pt(3, 3, 3));
        }

        [Test]
        public void TestCollinearEqual3D2D()
        {
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(1, 1, 3, 3),
                pt(1, 1, 1), pt(3, 3, 3));
        }

        [Test]
        public void TestCollinearEndpoint()
        {
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(3, 3, 3, 5, 5, 5),
                pt(3, 3, 3));
        }

        [Test]
        public void TestCollinearEndpoint3D2D()
        {
            // result Z is from 3D point
            CheckIntersection(Line(1, 1, 1, 3, 3, 3), Line(3, 3, 5, 5),
                pt(3, 3, 3));
        }

        [Test]
        public void TestCollinearContained()
        {
            CheckIntersection(Line(1, 1, 1, 5, 5, 5), Line(3, 3, 3, 4, 4, 4),
                pt(3, 3, 3), pt(4, 4, 4));
        }

        [Test]
        public void TestCollinearContained3D2D()
        {
            // result Z is interpolated
            CheckIntersection(Line(1, 1, 1, 5, 5, 5), Line(3, 3, 4, 4),
                pt(3, 3, 3), pt(4, 4, 4));
        }

        //----------------------------------

        [Test]
        public void TestInteriorXY()
        {
            CheckIntersection(
                new LineSegment(new Coordinate(1, 1), new Coordinate(3, 3)),
                new LineSegment(new Coordinate(1, 3), new Coordinate(3, 1)),
                pt(2, 2));
        }

        [Test]
        public void TestCollinearContainedXY()
        {
            CheckIntersection(
                new LineSegment(new Coordinate(1, 1), new Coordinate(5, 5)),
                new LineSegment(new Coordinate(3, 3), new Coordinate(4, 4)),
                pt(3, 3), pt(4, 4));
        }

        //======================================================================================================

        private void CheckIntersection(LineSegment line1, LineSegment line2,
            Coordinate p1, Coordinate p2)
        {
            CheckIntersectionDir(line1, line2, p1, p2);
            CheckIntersectionDir(line2, line1, p1, p2);
            var line1Rev = new LineSegment(line1.P1, line1.P0);
            var line2Rev = new LineSegment(line2.P1, line2.P0);
            CheckIntersectionDir(line1Rev, line2Rev, p1, p2);
            CheckIntersectionDir(line2Rev, line1Rev, p1, p2);
        }

        private void CheckIntersectionDir(LineSegment line1, LineSegment line2, Coordinate p1, Coordinate p2)
        {
            LineIntersector li = new RobustLineIntersector();
            li.ComputeIntersection(
                line1.P0, line1.P1,
                line2.P0, line2.P1);

            Assert.That(li.IntersectionNum, Is.EqualTo(2));

            var actual1 = li.GetIntersection(0);
            var actual2 = li.GetIntersection(1);
            // normalize actual results
            if (actual1.CompareTo(actual2) > 0)
            {
                actual1 = li.GetIntersection(1);
                actual2 = li.GetIntersection(0);
            }

            CheckEqualXYZ(p1, actual1);
            CheckEqualXYZ(p2, actual2);
        }

        private void CheckIntersection(LineSegment line1, LineSegment line2, Coordinate pt)
        {
            CheckIntersectionDir(line1, line2, pt);
            CheckIntersectionDir(line2, line1, pt);
            var line1Rev = new LineSegment(line1.P1, line1.P0);
            var line2Rev = new LineSegment(line2.P1, line2.P0);
            CheckIntersectionDir(line1Rev, line2Rev, pt);
            CheckIntersectionDir(line2Rev, line1Rev, pt);
        }

        private void CheckIntersectionDir(LineSegment line1, LineSegment line2, Coordinate pt)
        {
            LineIntersector li = new RobustLineIntersector();
            li.ComputeIntersection(
                line1.P0, line1.P1,
                line2.P0, line2.P1);
            Assert.That(li.IntersectionNum, Is.EqualTo(1));
            var actual = li.GetIntersection(0);
            CheckEqualXYZ(pt, actual);
        }

        [DebuggerStepperBoundary]
        private static Coordinate pt(double x, double y, double z)
        {
            return new CoordinateZ(x, y, z);
        }

        [DebuggerStepperBoundary]
        private static Coordinate pt(double x, double y)
        {
            return pt(x, y, double.NaN);
        }

        [DebuggerStepperBoundary]
        private static LineSegment Line(double x1, double y1, double z1,
            double x2, double y2, double z2)
        {
            return new LineSegment(new CoordinateZ(x1, y1, z1),
                new CoordinateZ(x2, y2, z2));
        }

        [DebuggerStepperBoundary]
        private static LineSegment Line(double x1, double y1,
            double x2, double y2)
        {
            return new LineSegment(new Coordinate(x1, y1),
                new Coordinate(x2, y2));
        }
    }
}
