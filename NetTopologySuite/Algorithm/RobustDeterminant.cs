using System;

namespace GisSharpBlog.NetTopologySuite.Algorithm
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
    /// *************************************************************************
    /// *************************************************************************
    /// Copyright (c) 1995  by  INRIA Prisme Project
    /// BP 93 06902 Sophia Antipolis Cedex, France.
    /// All rights reserved
    /// *************************************************************************
    /// </summary>
    public class RobustDeterminant
    {     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns>
        /// returns -1 if the determinant is negative,
        /// returns  1 if the determinant is positive,
        /// retunrs  0 if the determinant is null.
        /// </returns>
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
    }
}
