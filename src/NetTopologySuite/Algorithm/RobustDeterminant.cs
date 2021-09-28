using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Implements an algorithm to compute the
    /// sign of a 2x2 determinant for double precision values robustly.
    /// It is a direct translation of code developed by Olivier Devillers.
    ///
    /// The original code carries the following copyright notice:
    /// ************************************************************************
    /// Author : Olivier Devillers
    /// Olivier.Devillers@sophia.inria.fr
    /// http:/www.inria.fr:/prisme/personnel/devillers/anglais/determinant.html
    ///
    /// Olivier Devillers has allowed the code to be distributed under
    /// the LGPL (2012-02-16) saying "It is ok for LGPL distribution."
    ///
    /// *************************************************************************
    /// *************************************************************************
    /// Copyright (c) 1995  by  INRIA Prisme Project
    /// BP 93 06902 Sophia Antipolis Cedex, France.
    /// All rights reserved
    /// *************************************************************************
    /// </summary>
    public class RobustDeterminant
    {
        /*

        // test point to allow injecting test code
        public static int SignOfDet2x2(double x1, double y1, double x2, double y2)
        {
          int d1 = OriginalSignOfDet2x2(x1, y1, x2, y2);
          int d2 = -OriginalSignOfDet2x2(y1, x1, x2, y2);
          assert d1 == -d2;
          return d1;
        }
         */

        /*
         * Test code to force a standard ordering of input ordinates.
         * A possible fix for a rare problem where evaluation is order-dependent.
         */
        /*
        public static int SignOfDet2x2(double x1, double y1, double x2, double y2)
        {
          if (x1 > x2) {
            return -SignOfDet2x2ordX(x2, y2, x1, y1);
          }
          return SignOfDet2x2ordX(x1, y1, x2, y2);
        }

        private static int SignOfDet2x2ordX(double x1, double y1, double x2, double y2)
        {
          if (y1 > y2) {
            return -OriginalSignOfDet2x2(y1, x1, y2, x2);
          }
          return OriginalSignOfDet2x2(x1, y1, x2, y2);
        }
         */

        /// <summary>
        /// Computes the sign of the determinant of the 2x2 matrix with the given entries, in a robust way.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>-1 if the determinant is negative,</description></item>
        /// <item><description>1 if the determinant is positive,</description></item>
        /// <item><description>0 if the determinant is null.</description></item>
        /// </list>
        /// </returns>
        //private static int OriginalSignOfDet2x2(double x1, double y1, double x2, double y2) {
        public static int SignOfDet2x2(double x1, double y1, double x2, double y2)
        {
            // returns -1 if the determinant is negative,
            // returns  1 if the determinant is positive,
            // returns  0 if the determinant is null.

            int sign;
            double swap;
            double k;
            long count = 0;

            sign = 1;

            /*
             *  testing null entries
             */
            if ((x1 == 0.0) || (y2 == 0.0))
            {
                if ((y1 == 0.0) || (x2 == 0.0))
                {
                    return 0;
                }
                else if (y1 > 0)
                {
                    if (x2 > 0)
                    {
                        return -sign;
                    }
                    else
                    {
                        return sign;
                    }
                }
                else
                {
                    if (x2 > 0)
                    {
                        return sign;
                    }
                    else
                    {
                        return -sign;
                    }
                }
            }
            if ((y1 == 0.0) || (x2 == 0.0))
            {
                if (y2 > 0)
                {
                    if (x1 > 0)
                    {
                        return sign;
                    }
                    else
                    {
                        return -sign;
                    }
                }
                else
                {
                    if (x1 > 0)
                    {
                        return -sign;
                    }
                    else
                    {
                        return sign;
                    }
                }
            }

            /*
             *  making y coordinates positive and permuting the entries
             */
            /*
             *  so that y2 is the biggest one
             */
            if (0.0 < y1)
            {
                if (0.0 < y2)
                {
                    if (y1 <= y2)
                    {
                        ;
                    }
                    else
                    {
                        sign = -sign;
                        swap = x1;
                        x1 = x2;
                        x2 = swap;
                        swap = y1;
                        y1 = y2;
                        y2 = swap;
                    }
                }
                else
                {
                    if (y1 <= -y2)
                    {
                        sign = -sign;
                        x2 = -x2;
                        y2 = -y2;
                    }
                    else
                    {
                        swap = x1;
                        x1 = -x2;
                        x2 = swap;
                        swap = y1;
                        y1 = -y2;
                        y2 = swap;
                    }
                }
            }
            else
            {
                if (0.0 < y2)
                {
                    if (-y1 <= y2)
                    {
                        sign = -sign;
                        x1 = -x1;
                        y1 = -y1;
                    }
                    else
                    {
                        swap = -x1;
                        x1 = x2;
                        x2 = swap;
                        swap = -y1;
                        y1 = y2;
                        y2 = swap;
                    }
                }
                else
                {
                    if (y1 >= y2)
                    {
                        x1 = -x1;
                        y1 = -y1;
                        x2 = -x2;
                        y2 = -y2;
                        ;
                    }
                    else
                    {
                        sign = -sign;
                        swap = -x1;
                        x1 = -x2;
                        x2 = swap;
                        swap = -y1;
                        y1 = -y2;
                        y2 = swap;
                    }
                }
            }

            /*
             *  making x coordinates positive
             */
            /*
             *  if |x2| < |x1| one can conclude
             */
            if (0.0 < x1)
            {
                if (0.0 < x2)
                {
                    if (x1 <= x2)
                    {
                        ;
                    }
                    else
                    {
                        return sign;
                    }
                }
                else
                {
                    return sign;
                }
            }
            else
            {
                if (0.0 < x2)
                {
                    return -sign;
                }
                else
                {
                    if (x1 >= x2)
                    {
                        sign = -sign;
                        x1 = -x1;
                        x2 = -x2;
                        ;
                    }
                    else
                    {
                        return -sign;
                    }
                }
            }

            /*
             *  all entries strictly positive   x1 <= x2 and y1 <= y2
             */
            while (true)
            {
                count = count + 1;
                k = Math.Floor(x2 / x1);
                x2 = x2 - k * x1;
                y2 = y2 - k * y1;

                /*
                 *  testing if R (new U2) is in U1 rectangle
                 */
                if (y2 < 0.0)
                {
                    return -sign;
                }
                if (y2 > y1)
                {
                    return sign;
                }

                /*
                 *  finding R'
                 */
                if (x1 > x2 + x2)
                {
                    if (y1 < y2 + y2)
                    {
                        return sign;
                    }
                }
                else
                {
                    if (y1 > y2 + y2)
                    {
                        return -sign;
                    }
                    else
                    {
                        x2 = x1 - x2;
                        y2 = y1 - y2;
                        sign = -sign;
                    }
                }
                if (y2 == 0.0)
                {
                    if (x2 == 0.0)
                    {
                        return 0;
                    }
                    else
                    {
                        return -sign;
                    }
                }
                if (x2 == 0.0)
                {
                    return sign;
                }

                /*
                 *  exchange 1 and 2 role.
                 */
                k = Math.Floor(x1 / x2);
                x1 = x1 - k * x2;
                y1 = y1 - k * y2;

                /*
                 *  testing if R (new U1) is in U2 rectangle
                 */
                if (y1 < 0.0)
                {
                    return sign;
                }
                if (y1 > y2)
                {
                    return -sign;
                }

                /*
                 *  finding R'
                 */
                if (x2 > x1 + x1)
                {
                    if (y2 < y1 + y1)
                    {
                        return -sign;
                    }
                }
                else
                {
                    if (y2 > y1 + y1)
                    {
                        return sign;
                    }
                    else
                    {
                        x1 = x2 - x1;
                        y1 = y2 - y1;
                        sign = -sign;
                    }
                }
                if (y1 == 0.0)
                {
                    if (x1 == 0.0)
                    {
                        return 0;
                    }
                    else
                    {
                        return sign;
                    }
                }
                if (x1 == 0.0)
                {
                    return -sign;
                }
            }

        }

        /// <summary>
        /// Returns the index of the direction of the point <c>q</c> relative to
        /// a vector specified by <c>p1-p2</c>.
        /// </summary>
        /// <param name="p1">The origin point of the vector</param>
        /// <param name="p2">The final point of the vector</param>
        /// <param name="q">the point to compute the direction to</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>1 if q is counter-clockwise (left) from p1-p2</description></item>
        /// <item><description>-1 if q is clockwise (right) from p1-p2</description></item>
        /// <item><description>0 if q is collinear with p1-p2</description></item></list>
        /// </returns>
        public static int OrientationIndex(Coordinate p1, Coordinate p2, Coordinate q)
        {
            /*
             * MD - 9 Aug 2010 It seems that the basic algorithm is slightly orientation
             * dependent, when computing the orientation of a point very close to a
             * line. This is possibly due to the arithmetic in the translation to the
             * origin.
             *
             * For instance, the following situation produces identical results in spite
             * of the inverse orientation of the line segment:
             *
             * Coordinate p0 = new Coordinate(219.3649559090992, 140.84159161824724);
             * Coordinate p1 = new Coordinate(168.9018919682399, -5.713787599646864);
             *
             * Coordinate p = new Coordinate(186.80814046338352, 46.28973405831556); int
             * orient = orientationIndex(p0, p1, p); int orientInv =
             * orientationIndex(p1, p0, p);
             *
             *
             */

            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = q.X - p2.X;
            double dy2 = q.Y - p2.Y;
            return SignOfDet2x2(dx1, dy1, dx2, dy2);
        }

    }
}
