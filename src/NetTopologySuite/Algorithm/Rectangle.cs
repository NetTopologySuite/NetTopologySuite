using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Triangulate;
using System;
using System.Drawing;
using System.Security.Cryptography;

namespace NetTopologySuite.Algorithm
{
    internal static class Rectangle
    {
        /// <summary>
        /// Creates a rectangular <see cref="Polygon"/> from a base segment
        /// defining the position and orientation of one side of the rectangle, and
        /// three points defining the locations of the line segments
        /// forming the opposite, left and right sides of the rectangle.
        /// The base segment and side points must be presented so that the
        /// rectangle has CW orientation.
        /// <para/>
        /// The rectangle corners are computed as intersections of
        /// lines, which generally cannot produce exact values.
        /// If a rectangle corner is determined to coincide with a side point
        /// the side point value is used to avoid numerical inaccuracy.
        /// <para/>
        /// The first side of the constructed rectangle contains the base segment.
        /// </summary>
        /// <param name="baseRightPt">The right point of the base segment</param>
        /// <param name="baseLeftPt">The left point of the base segment</param>
        /// <param name="oppositePt">The point defining the opposite side</param>
        /// <param name="rightSidePt">The point defining the right side</param>
        /// <param name="leftSidePt">The point defining the left side</param>
        /// <param name="factory">The geometry factory</param>
        /// <returns>The rectangular polygon</returns>
        public static Polygon CreateFromSidePts(Coordinate baseRightPt, Coordinate baseLeftPt,
            Coordinate oppositePt,
            Coordinate leftSidePt, Coordinate rightSidePt,
            GeometryFactory factory)
        {
            //-- deltas for the base segment provide slope
            double dx = baseLeftPt.X - baseRightPt.X;
            double dy = baseLeftPt.Y - baseRightPt.Y;
            // Assert: dx and dy are not both zero

            double baseC = ComputeLineEquationC(dx, dy, baseRightPt);
            double oppC = ComputeLineEquationC(dx, dy, oppositePt);
            double leftC = ComputeLineEquationC(-dy, dx, leftSidePt);
            double rightC = ComputeLineEquationC(-dy, dx, rightSidePt);

            //-- compute lines along edges of rectangle
            var baseLine = CreateLineForStandardEquation(-dy, dx, baseC);
            var oppLine = CreateLineForStandardEquation(-dy, dx, oppC);
            var leftLine = CreateLineForStandardEquation(-dx, -dy, leftC);
            var rightLine = CreateLineForStandardEquation(-dx, -dy, rightC);

            /*
             * Corners of rectangle are the intersections of the 
             * base and opposite, and left and right lines.
             * The rectangle is constructed with CW orientation.
             * The first side of the constructed rectangle contains the base segment.
             * 
             * If a corner coincides with a input point
             * the exact value is used to avoid numerical inaccuracy.
             */
            var p0 = rightSidePt.Equals2D(baseRightPt) ? baseRightPt.Copy()
                : baseLine.LineIntersection(rightLine);
            var p1 = leftSidePt.Equals2D(baseLeftPt) ? baseLeftPt.Copy()
                : baseLine.LineIntersection(leftLine);
            var p2 = leftSidePt.Equals2D(oppositePt) ? oppositePt.Copy()
                : oppLine.LineIntersection(leftLine);
            var p3 = rightSidePt.Equals2D(oppositePt) ? oppositePt.Copy()
                : oppLine.LineIntersection(rightLine);

            var shell = factory.CreateLinearRing(
                new Coordinate[] { p0, p1, p2, p3, p0.Copy() });
            return factory.CreatePolygon(shell);
        }

        /// <summary>
        /// Computes the constant C in the standard line equation Ax + By = C
        /// from A and B and a point on the line.
        /// </summary>
        /// <param name="a">The X coefficient</param>
        /// <param name="b">The Y coefficient</param>
        /// <param name="p">A point on the line</param>
        /// <returns>The constant C</returns>
        private static double ComputeLineEquationC(double a, double b, Coordinate p)
        {
            return a * p.Y - b * p.X;
        }

        private static LineSegment CreateLineForStandardEquation(double a, double b, double c)
        {
            Coordinate p0;
            Coordinate p1;
            /*
             * Line equation is ax + by = c
             * Slope m = -a/b.
             * Y-intercept = c/b
             * X-intercept = c/a
             * 
             * If slope is low, use constant X values; if high use Y values.
             * This handles lines that are vertical (b = 0, m = Inf ) 
             * and horizontal (a = 0, m = 0).
             */
            if (Math.Abs(b) > Math.Abs(a))
            {
                //-- abs(m) < 1
                p0 = new Coordinate(0.0, c / b);
                p1 = new Coordinate(1.0, c / b - a / b);
            }
            else
            {
                //-- abs(m) >= 1
                p0 = new Coordinate(c / a, 0.0);
                p1 = new Coordinate(c / a - b / a, 1.0);
            }
            return new LineSegment(p0, p1);
        }
    }
}
