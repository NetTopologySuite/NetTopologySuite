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

        /// <summary>
        /// Clamps an integer to a given maximum limit.
        /// </summary>
        /// <param name="x">The value to clamp</param>
        /// <param name="max">The maximum value of the range</param>
        /// <returns>The clamped value</returns>
        public static int ClampMax(int x, int max)
        {
            if (x > max) return max;
            return x;
        }

        /// <summary>
        /// Computes the ceiling function of the dividend of two integers.
        /// </summary>
        /// <param name="num">The numerator</param>
        /// <param name="denom">The denominator</param>
        /// <returns>The ceiling of <c>num / denom</c></returns>
        public static int Ceiling(int num, int denom)
        {
            int div = num / denom;
            return div * denom >= num ? div : div + 1;
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

        /// <summary>The inverse of the Golden Ratio phi.</summary>
        public static readonly double PhiInv = (System.Math.Sqrt(5) - 1.0) / 2.0;

        /// <summary>
        /// Generates a quasi-random sequence of numbers in the range [0,1].
        /// They are produced by an additive recurrence with 1/&#966; as the constant.
        /// This produces a low-discrepancy sequence which is more evenly
        /// distribute than random numbers.
        /// <para/>
        /// See <a href='https://en.wikipedia.org/wiki/Low-discrepancy_sequence#Additive_recurrence'>Wikipedia: Low-discrepancy Sequences - Additive Recurrence</a>.
        /// <para/>
        /// The sequence is initialized by calling it
        /// with any positive fractional number; 0 works well for most uses.
        /// </summary>
        /// <param name="curr">The current number in the sequence</param>
        /// <returns>The next value in the sequence</returns>
        public static double QuasiRandom(double curr)
        {
            return QuasiRandom(curr, PhiInv);
        }

        /// <summary>
        /// Generates a quasi-random sequence of numbers in the range [0,1].
        /// They are produced by an additive recurrence with constant &#945;.
        /// <code>
        /// R(&#945;) :  t<sub>n</sub> = { t<sub>0</sub> + n&#945; },  n = 1,2,3,...
        /// </code>
        /// When &#945; is irrational this produces a
        /// <a href='https://en.wikipedia.org/wiki/Low-discrepancy_sequence#Additive_recurrence'>Low discrepancy sequence</a>
        /// which is more evenly distributed than random numbers.
        /// <para/>
        /// The sequence is initialized by calling it
        /// with any positive fractional number. 0 works well for most uses.
        /// </summary>
        /// <param name="curr">The current number in the sequence</param>
        /// <param name="alpha">the sequence's additive constant</param>
        /// <returns>The next value in the sequence</returns>
        public static double QuasiRandom(double curr, double alpha)
        {
            double next = curr + alpha;
            if (next < 1) return next;
            return next - System.Math.Floor(next);
        }

        /// <summary>
        /// Generates a randomly-shuffled list of the integers from [0..n-1].
        /// <para/>
        /// One use is to randomize points inserted into a <see cref="Index.KdTree.KdTree{T}"/>.
        /// </summary>
        /// <param name="n">The number of integers to shuffle</param>
        /// <returns>The shuffled array</returns>
        public static int[] Shuffle(int n)
        {
            var rnd = new System.Random(13);
            int[] ints = new int[n];
            for (int i = 0; i < n; i++)
            {
                ints[i] = i;
            }
            for (int i = n - 1; i >= 1; i--)
            {
                int j = rnd.Next(i + 1);
                int last = ints[i];
                ints[i] = ints[j];
                ints[j] = last;
            }
            return ints;
        }

    }
}
