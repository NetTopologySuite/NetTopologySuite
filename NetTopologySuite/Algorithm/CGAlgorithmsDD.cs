using GeoAPI.Geometries;
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
        /// <list type="Bullet">
        /// <item>1 if q is counter-clockwise (left) from p1-p2</item>
        /// <item>-1 if q is clockwise (right) from p1-p2</item>
        /// <item>0 if q is collinear with p1-p2</item></list>
        /// </returns>
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
        {
            // fast filter for orientation index
            // avoids use of slow extended-precision arithmetic in many cases
            int index = OrientationIndexFilter(p1, p2, q);
            if (index <= 1) return index;

            // normalize coordinates
            var dx1 = DD.ValueOf(p2.X) - p1.X;
            var dy1 = DD.ValueOf(p2.Y) - p1.Y;
            var dx2 = DD.ValueOf(q.X) - p2.X;
            var dy2 = DD.ValueOf(q.Y) - p2.Y;

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
        /// <list type="Bullet">
        /// <item>-1 if the determinant is negative,</item>
        /// <item>1 if the determinant is positive,</item>
        /// <item>0 if the determinant is 0.</item>
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
        /// <returns>
        /// <list type="Bullet">
        /// <item>The orientation index if it can be computed safely</item>
        /// <item>&gt; 1 if the orientation index cannot be computed safely</item>>
        /// </list>
        /// </returns>
        private static int OrientationIndexFilter(Coordinate pa, Coordinate pb, Coordinate pc)
        {
            double detsum;

            double detleft = (pa.X - pc.X)*(pb.Y - pc.Y);
            double detright = (pa.Y - pc.Y)*(pb.X - pc.X);
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
        /// Currently does not handle case of parallel lines.
        /// </summary>
        /// <param name="p1">A point of 1st segment</param>
        /// <param name="p2">Another point of 1st segment</param>
        /// <param name="q1">A point of 2nd segment</param>
        /// <param name="q2">Another point of 2nd segment</param>
        /// <returns></returns>
        public static Coordinate Intersection(
            Coordinate p1, Coordinate p2,
            Coordinate q1, Coordinate q2)
        {
            var denom1 = (DD.ValueOf(q2.Y) - DD.ValueOf(q1.Y))*(DD.ValueOf(p2.X) - DD.ValueOf(p1.X));
            var denom2 = (DD.ValueOf(q2.X) - DD.ValueOf(q1.X))*(DD.ValueOf(p2.Y) - DD.ValueOf(p1.Y));
            var denom = denom1 - denom2;

            /**
             * Cases:
             * - denom is 0 if lines are parallel
             * - intersection point lies within line segment p if fracP is between 0 and 1
             * - intersection point lies within line segment q if fracQ is between 0 and 1
             */

            var numx1 = (DD.ValueOf(q2.X) - DD.ValueOf(q1.X)) * (DD.ValueOf(p1.Y) - DD.ValueOf(q1.Y));
            var numx2 = (DD.ValueOf(q2.Y) - DD.ValueOf(q1.Y)) * (DD.ValueOf(p1.X) - DD.ValueOf(q1.X));
            var numx = numx1 - numx2;
            var fracP = (numx / denom)/*.ToDoubleValue()*/;

            double x = (DD.ValueOf(p1.X) + (DD.ValueOf(p2.X) - DD.ValueOf(p1.X)) * fracP).ToDoubleValue();

            var numy1 = (DD.ValueOf(p2.X) - DD.ValueOf(p1.X)) * (DD.ValueOf(p1.Y) - DD.ValueOf(q1.Y));
            var numy2 = (DD.ValueOf(p2.Y) - DD.ValueOf(p1.Y)) * (DD.ValueOf(p1.X) - DD.ValueOf(q1.X));
            var numy = numy1 - numy2;
            var fracQ = numy / denom;

            double y = (DD.ValueOf(q1.Y) + (DD.ValueOf(q2.Y) - DD.ValueOf(q1.Y)) * fracQ).ToDoubleValue();

            return new Coordinate(x, y);
        }
    }
}
