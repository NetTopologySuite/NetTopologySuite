namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A supoort class: the purpose is to integrate System.BitConverter methods not presents in .NET Compact Framework.
    /// </summary>
    public class BitConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
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
        public static double Int64BitsToDouble(long x)
        {
            byte[] bytes = System.BitConverter.GetBytes(x);
            double value = System.BitConverter.ToDouble(bytes, 0);
            return value;
        }
    }
}
