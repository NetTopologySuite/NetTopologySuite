using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Contains functions to compute intersections between lines.
    /// </summary>
    /// <author>mdavis</author>
    public  class IntersectionComputer
    {
        /// <summary>
        /// Computes the intersection point of two lines.
        /// If the lines are parallel or collinear this case is detected
        /// and <c>null</c> is returned.
        /// <para/>
        /// In general it is not possible to accurately compute
        /// the intersection point of two lines, due to
        /// numerical round off.
        /// This is particularly true when the input lines are nearly parallel.
        /// This routine uses numerical conditioning on the input values
        /// to ensure that the computed value should be very close to the correct value.
        /// </summary>
        /// <param name="p1">An endpoint of line 1</param>
        /// <param name="p2">An endpoint of line 1</param>
        /// <param name="q1">An endpoint of line 2</param>
        /// <param name="q2">An endpoint of line 2</param>
        /// <returns>
        /// <remarks>
        /// NOTE: In JTS this function is called Intersection.
        /// </remarks>
        /// The intersection point between the lines, if there is one,
        /// or null if the lines are parallel or collinear</returns>
        /// <seealso cref="CGAlgorithmsDD.Intersection(Coordinate, Coordinate, Coordinate, Coordinate)"/>

        public static Coordinate Intersection(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
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
            return new Coordinate(xInt + midx, yInt + midy);
        }

    }

}
