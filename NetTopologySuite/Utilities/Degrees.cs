using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    public class Degrees
    {
        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        public static Double ToDegrees(Double radians)
        {
            return radians * 57.29577951308232;
        }
    }
}