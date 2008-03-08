using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    public static class Radians
    {
        /// <summary>
        /// Converts degress to radians.
        /// </summary>
        /// <param name="degrees">Angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        public static Double ToRadians(Double degrees)
        {
            return degrees * 0.0174532925199432958;
        }
    }
}