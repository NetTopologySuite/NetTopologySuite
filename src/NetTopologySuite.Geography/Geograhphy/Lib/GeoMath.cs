using System;
using System.Globalization;
using System.Runtime.ExceptionServices;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geography.Lib
{
    /**
     * Mathematical functions needed by GeographicLib.
     * <p>
     * Define mathematical functions and constants so that any version of Java
     * can be used.
     **********************************************************************/
    public static class GeoMath
    {
        /**
         * The number of binary digits in the fraction of a double precision
         * number (equivalent to C++'s {@code numeric_limits<double>::digits}).
         **********************************************************************/
        public const int digits = 53;

        /**
         * Square a number.
         * <p>
         * @param x the argument.
         * @return <i>x</i><sup>2</sup>.
         **********************************************************************/
        public static double sq(double x)
        {
            return x * x;
        }

        /**
         * The inverse hyperbolic tangent function.  This is defined in terms of
         * Math.log1p(<i>x</i>) in order to maintain accuracy near <i>x</i> = 0.
         * In addition, the odd parity of the function is enforced.
         * <p>
         * @param x the argument.
         * @return atanh(<i>x</i>).
         **********************************************************************/
        public static double atanh(double x)
        {
            double y = Math.Abs(x); // Enforce odd parity
            y = /*Math.*/Log1p(2 * y / (1 - y)) / 2;
            return x > 0 ? y : (x < 0 ? -y : x);
        }

        /**
         * Normalize a sine cosine pair.
         * <p>
         * @param p return parameter for normalized quantities with sinx<sup>2</sup>
         *   + cosx<sup>2</sup> = 1.
         * @param sinx the sine.
         * @param cosx the cosine.
         **********************************************************************/
        public static void norm(ref (double first, double second) p, double sinx, double cosx)
        {
            double r = GeoMath.Hypot(sinx, cosx);
            p.first = sinx / r;
            p.second = cosx / r;
        }

        /**
         * The error-free sum of two numbers.
         * <p>
         * @param u the first number in the sum.
         * @param v the second number in the sum.
         * @param p output Pair(<i>s</i>, <i>t</i>) with <i>s</i> = round(<i>u</i> +
         *   <i>v</i>) and <i>t</i> = <i>u</i> + <i>v</i> - <i>s</i>.
         * <p>
         * See D. E. Knuth, TAOCP, Vol 2, 4.2.2, Theorem B.
         **********************************************************************/
        public static void sum(ref (double first, double second) p, double u, double v)
        {
            double s = u + v;
            double up = s - v;
            double vpp = s - up;
            up -= u;
            vpp -= v;
            double t = -(up + vpp);
            // u + v =       s      + t
            //       = round(u + v) + t
            p.first = s;
            p.second = t;
        }

        /**
         * Evaluate a polynomial.
         * <p>
         * @param N the order of the polynomial.
         * @param p the coefficient array (of size <i>N</i> + <i>s</i> + 1 or more).
         * @param s starting index for the array.
         * @param x the variable.
         * @return the value of the polynomial.
         * <p>
         * Evaluate <i>y</i> = &sum;<sub><i>n</i>=0..<i>N</i></sub>
         * <i>p</i><sub><i>s</i>+<i>n</i></sub>
         * <i>x</i><sup><i>N</i>&minus;<i>n</i></sup>.  Return 0 if <i>N</i> &lt; 0.
         * Return <i>p</i><sub><i>s</i></sub>, if <i>N</i> = 0 (even if <i>x</i> is
         * infinite or a nan).  The evaluation uses Horner's method.
         **********************************************************************/
        public static double polyval(int N, double[] p, int s, double x)
        {
            double y = N < 0 ? 0 : p[s++];
            while (--N >= 0) y = y * x + p[s++];
            return y;
        }

        /**
         * Coarsen a value close to zero.
         * <p>
         * @param x the argument
         * @return the coarsened value.
         * <p>
         * This makes the smallest gap in <i>x</i> = 1/16 &minus; nextafter(1/16, 0)
         * = 1/2<sup>57</sup> for reals = 0.7 pm on the earth if <i>x</i> is an angle
         * in degrees.  (This is about 1000 times more resolution than we get with
         * angles around 90 degrees.)  We use this to avoid having to deal with near
         * singular cases when <i>x</i> is non-zero but tiny (e.g.,
         * 10<sup>&minus;200</sup>).  This converts &minus;0 to +0; however tiny
         * negative numbers get converted to &minus;0.
         **********************************************************************/
        public static double AngRound(double x)
        {
            const double z = 1 / 16.0;
            if (x == 0) return 0;
            double y = Math.Abs(x);
            // The compiler mustn't "simplify" z - (z - y) to y
            y = y < z ? z - (z - y) : y;
            return x < 0 ? -y : y;
        }

        /**
         * The remainder function.
         * <p>
         * @param x the numerator of the division
         * @param y the denominator of the division
         * @return the remainder in the range [&minus;<i>y</i>/2, <i>y</i>/2].
         * <p>
         * The range of <i>x</i> is unrestricted; <i>y</i> must be positive.
         **********************************************************************/
        public static double remainder(double x, double y)
        {
            x = x % y;
            return x < -y / 2 ? x + y : (x < y / 2 ? x : x - y);
        }

        /**
         * Normalize an angle.
         * <p>
         * @param x the angle in degrees.
         * @return the angle reduced to the range [&minus;180&deg;, 180&deg;).
         * <p>
         * The range of <i>x</i> is unrestricted.
         **********************************************************************/
        public static double AngNormalize(double x)
        {
            x = remainder(x, 360.0);
            return x == -180 ? 180 : x;
        }

        /**
         * Normalize a latitude.
         * <p>
         * @param x the angle in degrees.
         * @return x if it is in the range [&minus;90&deg;, 90&deg;], otherwise
         *   return NaN.
         **********************************************************************/
        public static double LatFix(double x)
        {
            return Math.Abs(x) > 90 ? double.NaN : x;
        }

        /**
         * The exact difference of two angles reduced to (&minus;180&deg;, 180&deg;].
         * <p>
         * @param x the first angle in degrees.
         * @param y the second angle in degrees.
         * @param p output Pair(<i>d</i>, <i>e</i>) with <i>d</i> being the rounded
         *   difference and <i>e</i> being the error.
         * <p>
         * The computes <i>z</i> = <i>y</i> &minus; <i>x</i> exactly, reduced to
         * (&minus;180&deg;, 180&deg;]; and then sets <i>z</i> = <i>d</i> + <i>e</i>
         * where <i>d</i> is the nearest representable number to <i>z</i> and
         * <i>e</i> is the truncation error.  If <i>d</i> = &minus;180, then <i>e</i>
         * &gt; 0; If <i>d</i> = 180, then <i>e</i> &le; 0.
         **********************************************************************/
        public static void AngDiff(ref (double first, double second) p, double x, double y)
        {
            sum(ref p, AngNormalize(-x), AngNormalize(y));
            double d = AngNormalize(p.first), t = p.second;
            sum(ref p, d == 180 && t > 0 ? -180 : d, t);
        }

        /**
         * Evaluate the sine and cosine function with the argument in degrees
         *
         * @param p return Pair(<i>s</i>, <i>t</i>) with <i>s</i> = sin(<i>x</i>) and
         *   <i>c</i> = cos(<i>x</i>).
         * @param x in degrees.
         * <p>
         * The results obey exactly the elementary properties of the trigonometric
         * functions, e.g., sin 9&deg; = cos 81&deg; = &minus; sin 123456789&deg;.
         **********************************************************************/
        public static void sincosd(ref (double first, double second) p, double x)
        {
            // In order to minimize round-off errors, this function exactly reduces
            // the argument to the range [-45, 45] before converting it to radians.
            double r;
            int q;
            r = x % 360.0;
            q = (int) Math.Round(r / 90); // If r is NaN this returns 0
            r -= 90 * q;
            // now abs(r) <= 45
            r = AngleUtility.ToRadians(r);
            // Possibly could call the gnu extension sincos
            double s = Math.Sin(r), c = Math.Cos(r);
            double sinx, cosx;
            switch (q & 3)
            {
                case 0:
                    sinx = s;
                    cosx = c;
                    break;
                case 1:
                    sinx = c;
                    cosx = -s;
                    break;
                case 2:
                    sinx = -s;
                    cosx = -c;
                    break;
                default:
                    sinx = -c;
                    cosx = s;
                    break; // case 3
            }

            if (x != 0)
            {
                sinx += 0.0;
                cosx += 0.0;
            }

            p.first = sinx;
            p.second = cosx;
        }

        /**
         * Evaluate the atan2 function with the result in degrees
         *
         * @param y the sine of the angle
         * @param x the cosine of the angle
         * @return atan2(<i>y</i>, <i>x</i>) in degrees.
         * <p>
         * The result is in the range (&minus;180&deg; 180&deg;].  N.B.,
         * atan2d(&plusmn;0, &minus;1) = +180&deg;; atan2d(&minus;&epsilon;,
         * &minus;1) = &minus;180&deg;, for &epsilon; positive and tiny;
         * atan2d(&plusmn;0, 1) = &plusmn;0&deg;.
         **********************************************************************/
        public static double atan2d(double y, double x)
        {
            // In order to minimize round-off errors, this function rearranges the
            // arguments so that result of atan2 is in the range [-pi/4, pi/4] before
            // converting it to degrees and mapping the result to the correct
            // quadrant.
            int q = 0;
            if (Math.Abs(y) > Math.Abs(x))
            {
                double t;
                t = x;
                x = y;
                y = t;
                q = 2;
            }

            if (x < 0)
            {
                x = -x;
                ++q;
            }

            // here x >= 0 and x >= abs(y), so angle is in [-pi/4, pi/4]
            double ang = AngleUtility.ToDegrees(Math.Atan2(y, x));
            switch (q)
            {
                // Note that atan2d(-0.0, 1.0) will return -0.  However, we expect that
                // atan2d will not be called with y = -0.  If need be, include
                //
                //   case 0: ang = 0 + ang; break;
                //
                // and handle mpfr as in AngRound.
                case 1:
                    ang = (y >= 0 ? 180 : -180) - ang;
                    break;
                case 2:
                    ang = 90 - ang;
                    break;
                case 3:
                    ang = -90 + ang;
                    break;
            }

            return ang;
        }

        /**
         * Test for finiteness.
         * <p>
         * @param x the argument.
         * @return true if number is finite, false if NaN or infinite.
         **********************************************************************/
        public static bool isfinite(double x)
        {
            return Math.Abs(x) <= double.MaxValue;
        }

        /**
         * Normalize a sine cosine pair.
         * <p>
         * @param sinx the sine.
         * @param cosx the cosine.
         * @return a Pair of normalized quantities with sinx<sup>2</sup> +
         *   cosx<sup>2</sup> = 1.
         *
         * @deprecated Use {@link #sincosd(Pair, double)} instead.
         **********************************************************************/
        // @Deprecated
        public static (double first, double second) norm(double sinx, double cosx)
        {
            var p = (0d, 0d);
            norm(ref p, sinx, cosx);
            return p;
        }

        /**
         * The error-free sum of two numbers.
         * <p>
         * @param u the first number in the sum.
         * @param v the second number in the sum.
         * @return Pair(<i>s</i>, <i>t</i>) with <i>s</i> = round(<i>u</i> +
         *   <i>v</i>) and <i>t</i> = <i>u</i> + <i>v</i> - <i>s</i>.
         * <p>
         * See D. E. Knuth, TAOCP, Vol 2, 4.2.2, Theorem B.
         *
         * @deprecated Use {@link #sincosd(Pair, double)} instead.
         **********************************************************************/
        // @Deprecated
        public static (double first, double second) sum(double u, double v)
        {
            var p = (0d, 0d);
            sum(ref p, u, v);
            return p;
        }

        /**
         * The exact difference of two angles reduced to (&minus;180&deg;, 180&deg;].
         * <p>
         * @param x the first angle in degrees.
         * @param y the second angle in degrees.
         * @return Pair(<i>d</i>, <i>e</i>) with <i>d</i> being the rounded
         *   difference and <i>e</i> being the error.
         * <p>
         * The computes <i>z</i> = <i>y</i> &minus; <i>x</i> exactly, reduced to
         * (&minus;180&deg;, 180&deg;]; and then sets <i>z</i> = <i>d</i> + <i>e</i>
         * where <i>d</i> is the nearest representable number to <i>z</i> and
         * <i>e</i> is the truncation error.  If <i>d</i> = &minus;180, then <i>e</i>
         * &gt; 0; If <i>d</i> = 180, then <i>e</i> &le; 0.
         *
         * @deprecated Use {@link #sincosd(Pair, double)} instead.
         **********************************************************************/
        // @Deprecated
        public static (double first, double second) AngDiff(double x, double y)
        {
            var p = (0d, 0d);
            AngDiff(ref p, x, y);
            return p;
        }

        /**
         * Evaluate the sine and cosine function with the argument in degrees
         *
         * @param x in degrees.
         * @return Pair(<i>s</i>, <i>t</i>) with <i>s</i> = sin(<i>x</i>) and
         *   <i>c</i> = cos(<i>x</i>).
         * <p>
         * The results obey exactly the elementary properties of the trigonometric
         * functions, e.g., sin 9&deg; = cos 81&deg; = &minus; sin 123456789&deg;.
         *
         * @deprecated Use {@link #sincosd(Pair, double)} instead.
         **********************************************************************/
        // @Deprecated
        public static (double first, double second) sincosd(double x)
        {
            var p = (0d, 0d);
            sincosd(ref p, x);
            return p;
        }

        /// <summary>
        /// Compute Math.Log( 1 + x ) without loosing precision for small values of <paramref name="x"/>
        /// </summary>
        /// <param name="x">The argument</param>
        /// <returns>Value of Math.Log( x + 1 )</returns>
        private static double Log1p(double x)
        {
            if (x <= 1)
                throw new ArgumentOutOfRangeException(nameof(x),
                    $"Invalid input argument: {x.ToString(NumberFormatInfo.InvariantInfo)}");

            // Is x large enough for obvious solution?
            if (Math.Abs(x) > 1E-4) return Math.Log(1d + x);

            // Use Taylor approx.log(1 + x) = x - x ^ 2 / 2 with error roughly x^3 / 3
            // Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8
            return (-0.5 * x + 1) * x;

        }

        internal static double Hypot(params double[] args)
        {
            var acc = new Accumulator(args[0] * args[0]);
            for (int i = 1; i < args.Length; i++)
                acc.Add(args[i] * args[i]);

            return Math.Sqrt(acc.Sum());

        }

        /// <summary>
        /// Computes the cubic root of <paramref name="x"/>
        /// </summary>
        /// <param name="x">A value</param>
        /// <returns></returns>
        public static double Cbrt(double x)
        {
            const double oneThird = 1d / 3d;
            return Math.Pow(x, oneThird);
        }

        /// <summary>
        /// Computes <paramref name="a"/> with the sign of <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The value provider</param>
        /// <param name="b">The sign provider</param>
        /// <returns>|a| * Sign(b)</returns>
        public static double CopySign(double a, double b)
        {
            return Math.Abs(a) * Math.Sign(b);
        }

        public static double Ulp(double value)
        {
            // This is actually a constant in the same static class as this method, but 
            // we put it here for brevity of this example.
            const double MaxULP = 1.9958403095347198116563727130368E+292;

            if (double.IsNaN(value))
            {
                return double.NaN;
            }
            if (double.IsPositiveInfinity(value) || double.IsNegativeInfinity(value))
            {
                return double.PositiveInfinity;
            }
            if (value == 0.0)
            {
                return double.Epsilon;    // Equivalent of Double.MIN_VALUE in Java; Double.MinValue in C# is the actual minimum value a double can hold.
            }
            if (Math.Abs(value) == double.MaxValue)
            {
                return MaxULP;
            }

            long bits = BitConverter.DoubleToInt64Bits(value);

            // This is safe because we already checked for value == Double.MaxValue.
            return Math.Abs(BitConverter.Int64BitsToDouble(bits + 1) - value);
        }

        /// <summary>
        /// Transforms a cartesian angle to a heading
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double ToHeading(double angle)
        {
            return -angle + 90;
        }
    }
}
