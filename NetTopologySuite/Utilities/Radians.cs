namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// Converts radians to degress.
    /// </summary>
    public class Radians
    {
        /// <summary>
        /// Converts radians to degress.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        public static double ToDegrees(double radians)
        {
            return radians * 57.29577951308232;
        }
    }
}
