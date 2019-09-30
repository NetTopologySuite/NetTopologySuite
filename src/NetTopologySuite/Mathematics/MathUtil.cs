namespace NetTopologySuite.Mathematics
{
    /// <summary>
    /// Various utility functions for mathematical and numerical operations.
    /// </summary>
    public class MathUtil
    {
        /// <summary>
        /// Clamps a <c>double</c> value to a given range.
        /// </summary>
        /// <param name="x">The value to clamp</param>
        /// <param name="min">The minimum value of the range</param>
        /// <param name="max">The maximum value of the range</param>
        /// <returns>The clamped value</returns>
        public static double Clamp(double x, double min, double max)
        {
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }

        /// <summary>
        /// Clamps a <c>int</c> value to a given range.
        /// </summary>
        /// <param name="x">The value to clamp</param>
        /// <param name="min">The minimum value of the range</param>
        /// <param name="max">The maximum value of the range</param>
        /// <returns>The clamped value</returns>
        public static int Clamp(int x, int min, int max)
        {
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }

// ReSharper disable InconsistentNaming
        private static readonly double LOG10 = System.Math.Log(10);
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Computes the base-10 logarithm of a <c>double</c> value.
        /// <para>
        /// <list type="bullet">
        /// <item><description>If the argument is NaN or less than zero, then the result is NaN.</description></item>
        /// <item><description>If the argument is positive infinity, then the result is positive infinity.</description></item>
        /// <item><description>If the argument is positive zero or negative zero, then the result is negative infinity.</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="x">A positive number</param>
        /// <returns>The value log a, the base-10 logarithm of the input value</returns>
        public static double Log10(double x)
        {
            double ln = System.Math.Log(x);
            if (double.IsInfinity(ln)) return ln;
            if (double.IsNaN(ln)) return ln;
            return ln / LOG10;
        }

        /// <summary>
        /// Computes an index which wraps around a given maximum value.
        /// For values &gt;= 0, this is equals to <c>val % max</c>.
        /// For values &lt; 0, this is equal to <c>max - (-val) % max</c>
        /// </summary>
        /// <param name="index">The index to wrap</param>
        /// <param name="max">The maximum value (or modulus)</param>
        /// <returns>The wrapped index</returns>
        public static int Wrap(int index, int max)
        {
            if (index < 0)
            {
                return max - ((-index) % max);
            }
            return index % max;
        }

        /// <summary>
        /// Computes the average of two numbers.
        /// </summary>
        /// <param name="x1">A number</param>
        /// <param name="x2">A number</param>
        /// <returns>The average of the inputs</returns>
        public static double Average(double x1, double x2)
        {
            return (x1 + x2) / 2.0;
        }

        /// <summary>
        /// Computes the maximum fo three values
        /// </summary>
        /// <param name="v1">A number</param>
        /// <param name="v2">A number</param>
        /// <param name="v3">A number</param>
        /// <returns>The maximum value of <paramref name="v1"/>, <paramref name="v2"/> and <paramref name="v3"/></returns>
        public static double Max(double v1, double v2, double v3)
        {
            double max = v1;
            if (v2 > v1) max = v2;
            if (v2 > v3) max = v3;
            return max;
        }

        /// <summary>
        /// Computes the maximum of four values
        /// </summary>
        /// <param name="v1">A number</param>
        /// <param name="v2">A number</param>
        /// <param name="v3">A number</param>
        /// <param name="v4">A number</param>
        /// <returns>The maximum value of <paramref name="v1"/>, <paramref name="v2"/>, <paramref name="v3"/> and <paramref name="v4"/></returns>
        public static double Max(double v1, double v2, double v3, double v4)
        {
            double max = v1;
            if (v2 > max) max = v2;
            if (v3 > max) max = v3;
            if (v4 > max) max = v4;
            return max;
        }

        /// <summary>
        /// Computes the minimum of four values
        /// </summary>
        /// <param name="v1">A number</param>
        /// <param name="v2">A number</param>
        /// <param name="v3">A number</param>
        /// <returns>The minimum value of <paramref name="v1"/>, <paramref name="v2"/> and <paramref name="v3"/></returns>
        public static double Min(double v1, double v2, double v3)
        {
            double min = v1;
            if (v2 < min) min = v2;
            if (v3 < min) min = v3;
            return min;
        }

        /// <summary>
        /// Computes the minimum of four values
        /// </summary>
        /// <param name="v1">A number</param>
        /// <param name="v2">A number</param>
        /// <param name="v3">A number</param>
        /// <param name="v4">A number</param>
        /// <returns>The minimum value of <paramref name="v1"/>, <paramref name="v2"/>, <paramref name="v3"/> and <paramref name="v4"/></returns>
        public static double Min(double v1, double v2, double v3, double v4)
        {
            double min = v1;
            if (v2 < min) min = v2;
            if (v3 < min) min = v3;
            if (v4 < min) min = v4;
            return min;
        }

    }
}
