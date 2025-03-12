using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions to compute intersection points between lines and line segments.
    /// <para/>
    /// In general it is not possible to compute
    /// the intersection point of two lines exactly, due to numerical roundoff.
    /// This is particularly true when the lines are nearly parallel.
    /// These routines uses numerical conditioning on the input values
    /// to ensure that the computed value is very close to the correct value.
    /// <para/>
    /// The Z-ordinate is ignored, and not populated.
    /// </summary>
    /// <remarks>
    /// NOTE: In JTS this function is called Intersection.
    /// </remarks>
    /// <author>mdavis</author>
    public class IntersectionComputer
    {
        /// <summary>
        /// Computes the intersection point of two lines.
        /// If the lines are parallel or collinear this case is detected
        /// and <c>null</c> is returned.
        /// </summary>
        /// <param name="p1">An endpoint of line 1</param>
        /// <param name="p2">An endpoint of line 1</param>
        /// <param name="q1">An endpoint of line 2</param>
        /// <param name="q2">An endpoint of line 2</param>
        /// <returns>
        /// The intersection point between the lines, if there is one,
        /// or null if the lines are parallel or collinear</returns>
        /// <seealso cref="CGAlgorithmsDD.Intersection(Coordinate, Coordinate, Coordinate, Coordinate)"/>
        public static Coordinate Intersection(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            return CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
            //-- this is less robust
            //return IntersectionFP(p1, p2, q1, q2);
        }

        /// <summary>
        /// Compute intersection of two lines, using a floating-point algorithm.
        /// This is less accurate than {@link CGAlgorithmsDD#intersection(Coordinate, Coordinate, Coordinate, Coordinate)}.
        /// It has caused spatial predicate failures in some cases.
        /// This is kept for testing purposes.
        /// </summary>
        /// <param name="p1">An endpoint of line 1</param>
        /// <param name="p2">An endpoint of line 1</param>
        /// <param name="q1">An endpoint of line 2</param>
        /// <param name="q2">An endpoint of line 2</param>
        /// <returns>
        /// The intersection point between the lines, if there is one,
        /// or null if the lines are parallel or collinear</returns>
        private static Coordinate IntersectionFP(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            // compute midpoint of "kernel envelope"
            double minX0 = p1.X < p2.X ? p1.X : p2.X;
            double minY0 = p1.Y < p2.Y ? p1.Y : p2.Y;
            double maxX0 = p1.X > p2.X ? p1.X : p2.X;
            double maxY0 = p1.Y > p2.Y ? p1.Y : p2.Y;

            double minX1 = q1.X < q2.X ? q1.X : q2.X;
            double minY1 = q1.Y < q2.Y ? q1.Y : q2.Y;
            double maxX1 = q1.X > q2.X ? q1.X : q2.X;
            double maxY1 = q1.Y > q2.Y ? q1.Y : q2.Y;

            double intMinX = minX0 > minX1 ? minX0 : minX1;
            double intMaxX = maxX0 < maxX1 ? maxX0 : maxX1;
            double intMinY = minY0 > minY1 ? minY0 : minY1;
            double intMaxY = maxY0 < maxY1 ? maxY0 : maxY1;

            double midx = (intMinX + intMaxX) / 2.0;
            double midy = (intMinY + intMaxY) / 2.0;

            // condition ordinate values by subtracting midpoint
            double p1x = p1.X - midx;
            double p1y = p1.Y - midy;
            double p2x = p2.X - midx;
            double p2y = p2.Y - midy;
            double q1x = q1.X - midx;
            double q1y = q1.Y - midy;
            double q2x = q2.X - midx;
            double q2y = q2.Y - midy;

            // unrolled computation using homogeneous coordinates eqn
            double px = p1y - p2y;
            double py = p2x - p1x;
            double pw = p1x * p2y - p2x * p1y;

            double qx = q1y - q2y;
            double qy = q2x - q1x;
            double qw = q1x * q2y - q2x * q1y;

            double x = py * qw - qy * pw;
            double y = qx * pw - px * qw;
            double w = px * qy - qx * py;

            double xInt = x / w;
            double yInt = y / w;

            // check for parallel lines
            if ((double.IsNaN(xInt)) || (double.IsInfinity(xInt)
                || double.IsNaN(yInt)) || (double.IsInfinity(yInt)))
            {
                return null;
            }

            // de-condition intersection point
            return p1.Create(xInt + midx, yInt + midy);
            //return new Coordinate(xInt + midx, yInt + midy);
        }

        /// <summary>
        /// Computes the intersection point of a line and a line segment (if any).
        /// There will be no intersection point if:
        /// <list type=">bullet">
        /// <item><description>the segment does not intersect the line</description></item>
        /// <item><description>the line or the segment are degenerate (have zero length)</description></item>
        /// </list>
        /// If the segment is collinear with the line the first segment endpoint is returned.
        /// </summary>
        /// <returns>The intersection point, or <c>null</c> if it is not possible to find an intersection</returns>
        public static Coordinate LineSegment(Coordinate line1, Coordinate line2, Coordinate seg1, Coordinate seg2)
        {
            var orientS1 = Orientation.Index(line1, line2, seg1);
            if (orientS1 == OrientationIndex.None) return seg1.Copy();

            var orientS2 = Orientation.Index(line1, line2, seg2);
            if (orientS2 == OrientationIndex.None) return seg2.Copy();

            /*
             * If segment lies completely on one side of the line, it does not intersect
             */
            if ((orientS1 > 0 && orientS2 > 0) || (orientS1 < 0 && orientS2 < 0))
            {
                return null;
            }

            /*
             * The segment intersects the line.
             * The full line-line intersection is used to compute the intersection point.
             */
            var intPt = Intersection(line1, line2, seg1, seg2);
            if (intPt != null) return intPt;

            /*
             * Due to robustness failure it is possible the intersection computation will return null.
             * In this case choose the closest point
             */
            double dist1 = DistanceComputer.PointToLinePerpendicular(seg1, line1, line2);
            double dist2 = DistanceComputer.PointToLinePerpendicular(seg2, line1, line2);
            if (dist1 < dist2)
                return seg1.Copy();
            return seg2;
        }

    }

}
