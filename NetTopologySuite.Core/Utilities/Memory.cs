﻿using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    ///     Utility functions to report memory usage.
    /// </summary>
    /// <author>mbdavis</author>
    public class Memory
    {
        public const double KB = 1024;
        public const double MB = 1048576;
        public const double GB = 1073741824;

        public static long Total => GC.GetTotalMemory(true);

        public static string TotalString => Format(Total);

        public static string Format(long mem)
        {
            if (mem < 2*KB)
                return mem + " bytes";
            if (mem < 2*MB)
                return Round(mem/KB) + " KB";
            if (mem < 2*GB)
                return Round(mem/MB) + " MB";
            return Round(mem/GB) + " GB";
        }

        public static double Round(double d)
        {
            return Math.Ceiling(d*100)/100;
        }
    }
}