namespace NetTopologySuite.IO
{
    /// <summary>
    /// Shapefile specific constants
    /// </summary>
    internal class ShapeFileConstants
    {
        /// <summary>
        /// Every value less that this is considered as not set.
        /// </summary>
        internal const double NoDataBorder = -10e38d;
        
        /// <summary>
        /// A value that represents an unset value
        /// </summary>
        internal const double NoDataValue = -10e38d - 1;
    }
}