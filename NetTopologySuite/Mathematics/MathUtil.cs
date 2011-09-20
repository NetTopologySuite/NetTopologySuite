using System;

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
        /// Computes the base-10 logarithm of a <tt>double</tt> value.
        /// <para>
        /// <list type="Bullet">
        /// <item>If the argument is NaN or less than zero, then the result is NaN.</item>
        /// <item>If the argument is positive infinity, then the result is positive infinity.</item>
        /// <item>If the argument is positive zero or negative zero, then the result is negative infinity.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="x">A positive number</param>
        /// <returns>The value log a, the base-10 logarithm of the input value</returns>
        public static double Log10(double x)
        {
            double ln = System.Math.Log(x);
            if (Double.IsInfinity(ln)) return ln;
            if (Double.IsNaN(ln)) return ln;
            return ln / LOG10;
        }
    }
}