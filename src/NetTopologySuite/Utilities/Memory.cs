using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// Utility functions to report memory usage.
    /// </summary>
    /// <author>mbdavis</author>
    public class Memory
    {
        /// <summary>
        /// Gets a value indicating the total memory used.
        /// </summary>
        public static long Total => GC.GetTotalMemory(true);

        /// <summary>
        /// Gets a string describing the total memory used
        /// </summary>
        public static string TotalString => Format(Total);

        /// <summary>
        /// Number of bytes in a kilo-byte
        /// </summary>
        public const double KB = 1024;

        /// <summary>
        /// Number of bytes in mega-byte
        /// </summary>
        public const double MB = 1048576;

        /// <summary>
        /// Number of bytes in a giga-byte
        /// </summary>
        public const double GB = 1073741824;

        /// <summary>
        /// Formats a number of bytes
        /// </summary>
        /// <param name="mem">The number of bytes</param>
        /// <returns>A string describing a number of bytes</returns>
        public static string Format(long mem)
        {
            if (mem < 2 * KB)
                return mem + " bytes";
            if (mem < 2 * MB)
                return Round(mem / KB) + " KB";
            if (mem < 2 * GB)
                return Round(mem / MB) + " MB";
            return Round(mem / GB) + " GB";
        }

        /// <summary>
        /// Rounds a double to 2 decimal places
        /// </summary>
        /// <param name="d">The number to round</param>
        /// <returns>The rounded number</returns>
        public static double Round(double d)
        {
            return Math.Ceiling(d * 100) / 100;
        }
    }
}
