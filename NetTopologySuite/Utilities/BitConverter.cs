using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A supoort class: the purpose is to integrate System.BitConverter methods not presents in .NET Compact Framework.
    /// </summary>
    [Obsolete("All supported platforms would be better off using the more efficient System.BitConverter methods instead.", error: true)]
    public class BitConverter
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [Obsolete("Use System.BitConverter.DoubleToInt64Bits instead.", error: true)]
        public static long DoubleToInt64Bits(double x)
        {
            byte[] bytes = System.BitConverter.GetBytes(x);
            long value = System.BitConverter.ToInt64(bytes, 0);
            return value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [Obsolete("Use System.BitConverter.Int64BitsToDouble instead.", error: true)]
        public static double Int64BitsToDouble(long x)
        {
            byte[] bytes = System.BitConverter.GetBytes(x);
            double value = System.BitConverter.ToDouble(bytes, 0);
            return value;
        }
    }
}
