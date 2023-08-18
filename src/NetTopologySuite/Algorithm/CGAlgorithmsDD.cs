using System;
using System.Net.Http.Headers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Implements basic computational geometry algorithms using <seealso cref="DD"/> arithmetic.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class CGAlgorithmsDD
    {
        /// <summary>
        /// Returns the index of the direction of the point <c>q</c> relative to
        /// a vector specified by <c>p1-p2</c>.
        /// </summary>
        /// <param name="p1">The origin point of the vector</param>
        /// <param name="p2">The final point of the vector</param>
        /// <param name="q">the point to compute the direction to</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>1</c> if q is counter-clockwise (left) from p1-p2</description></item>
        /// <item><description><c>-1</c> if q is clockwise (right) from p1-p2</description></item>
        /// <item><description><c>0</c> if q is collinear with p1-p2</description></item></list>
        /// </returns>
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
        {
            return OrientationIndex(p1.X, p1.Y, p2.X, p2.Y, q.X, q.Y);
        }

        /// <summary>
        /// Returns the index of the direction of the point <c>q</c> relative to
        /// a vector specified by <c>p1-p2</c>.
        /// </summary>
        /// <param name="p1x">The x-ordinate of the origin point of the vector</param>
        /// <param name="p1y">The y-ordinate of the origin point of the vector</param>
        /// <param name="p2x">The x-ordinate of the final point of the vector</param>
        /// <param name="p2y">The y-ordinate of the final point of the vector</param>
        /// <param name="qx">The x-ordinate of the point to compute the direction to</param>
        /// <param name="qy">The y-ordinate of the point to compute the direction to</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>1</c> if q is counter-clockwise (left) from p1-p2</description></item>
        /// <item><description><c>-1</c> if q is clockwise (right) from p1-p2</description></item>
        /// <item><description><c>0</c> if q is collinear with p1-p2</description></item></list>
        /// </returns>
        public static int OrientationIndex(double p1x, double p1y, double p2x, double p2y, double qx, double qy)
        {
            // fast filter for orientation index
            // avoids use of slow extended-precision arithmetic in many cases
            int index = OrientationIndexFilter(p1x, p1y, p2x, p2y, qx, qy);
            if (index <= 1) return index;

            // normalize coordinates
            var dx1 = DD.ValueOf(p2x) - p1x;
            var dy1 = DD.ValueOf(p2y) - p1y;
            var dx2 = DD.ValueOf(qx) - p2x;
            var dy2 = DD.ValueOf(qy) - p2y;

            return ((dx1 * dy2) - (dy1 * dx2)).Signum();
            //return SignOfDet2x2(dx1, dy1, dx2, dy2);
        }

        /// <summary>
        /// Computes the sign of the determinant of the 2x2 matrix
        /// with the given entries.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>-1 if the determinant is negative,</description></item>
        /// <item><description>1 if the determinant is positive,</description></item>
        /// <item><description>0 if the determinant is 0.</description></item>
        /// </list>
        /// </returns>
        public static int SignOfDet2x2(DD x1, DD y1, DD x2, DD y2)
        {
            return (x1 * y2 - y1 * x2).Signum();
        }

        /// <summary>
        /// A value which is safely greater than the
        /// relative round-off error in double-precision numbers
        /// </summary>
        private const double DoublePrecisionSafeEpsilon = 1e-15;

        /// <summary>
        /// A filter for computing the orientation index of three coordinates.
        /// <para/>
        /// If the orientation can be computed safely using standard DP
        /// arithmetic, this routine returns the orientation index.
        /// Otherwise, a value i > 1 is returned.
        /// In this case the orientation index must
        /// be computed using some other more robust method.
        /// The filter is fast to compute, so can be used to
        /// avoid the use of slower robust methods except when they are really needed,
        /// thus providing better average performance.
        /// <para/>
        /// Uses an approach due to Jonathan Shewchuk, which is in the public domain.
        /// </summary>
        /// <param name="pax">The x-ordinate of point A</param>
        /// <param name="pay">The y-ordinate of point A</param>
        /// <param name="pbx">The x-ordinate of point B</param>
        /// <param name="pby">The y-ordinate of point B</param>
        /// <param name="pcx">The x-ordinate of point C</param>
        /// <param name="pcy">The y-ordinate of point C</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>The orientation index if it can be computed safely</description></item>
        /// <item><description>&gt; 1 if the orientation index cannot be computed safely</description></item>>
        /// </list>
        /// </returns>
        private static int OrientationIndexFilter(double pax, double pay, double pbx, double pby, double pcx, double pcy)
        {
            double detsum;

            double detleft = (pax - pcx)*(pby - pcy);
            double detright = (pay - pcy)*(pbx - pcx);
            double det = detleft - detright;

            if (detleft > 0.0)
            {
                if (detright <= 0.0)
                {
                    return Signum(det);
                }
                detsum = detleft + detright;
            }
            else if (detleft < 0.0)
            {
                if (detright >= 0.0)
                {
                    return Signum(det);
                }
                detsum = -detleft - detright;
            }
            else
            {
                return Signum(det);
            }

            double errbound = DoublePrecisionSafeEpsilon*detsum;
            if ((det >= errbound) || (-det >= errbound))
            {
                return Signum(det);
            }

            return 2;
        }

        private static int Signum(double x)
        {
            if (x > 0) return 1;
            if (x < 0) return -1;
            return 0;
        }

        /// <summary>
        /// Computes an intersection point between two lines
        /// using DD arithmetic.
        /// If the lines are parallel (either identical
        /// or separate) a null value is returned.
        /// </summary>
        /// <param name="p1">An endpoint of line segment 1</param>
        /// <param name="p2">An endpoint of line segment 1</param>
        /// <param name="q1">An endpoint of line segment 2</param>
        /// <param name="q2">An endpoint of line segment 2</param>
        /// <returns>An intersection point if one exists, or <c>null</c> if lines are parallel.</returns>
        public static Coordinate Intersection(
            Coordinate p1, Coordinate p2,
            Coordinate q1, Coordinate q2)
        {
            var px = (DD)p1.Y - (DD)p2.Y;
            var py = (DD)p2.X - (DD)p1.X;
            var pw = (DD)p1.X * (DD)p2.Y - (DD)p2.X * (DD)p1.Y;

            var qx = (DD)q1.Y - (DD)q2.Y;
            var qy = (DD)q2.X - (DD)q1.X;
            var qw = (DD)q1.X * (DD)q2.Y - (DD)q2.X * (DD)q1.Y;

            var x = py * qw - qy * pw;
            var y = qx * pw - px * qw;
            var w = px * qy - qx * py;

            double xInt = (x / w).ToDoubleValue();
            double yInt = (y / w).ToDoubleValue();

            if (double.IsNaN(xInt) || double.IsInfinity(xInt) || double.IsNaN(yInt) || double.IsInfinity(yInt))
            {
                return null;
            }

            return new Coordinate(xInt, yInt);
        }
    }
}
