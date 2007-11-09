namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// The types of Precision Model which NTS supports.
    /// </summary>
    public enum PrecisionModels
    {
        /// <summary> 
        /// Floating precision corresponds to the standard 
        /// Double-precision floating-point representation, which is
        /// based on the IEEE-754 standard
        /// </summary>
        Floating = 0,

        /// <summary>
        /// Floating single precision corresponds to the standard
        /// single-precision floating-point representation, which is
        /// based on the IEEE-754 standard
        /// </summary>
        FloatingSingle = 1,

        /// <summary> 
        /// Fixed Precision indicates that coordinates have a fixed number of decimal places.
        /// The number of decimal places is determined by the log10 of the scale factor.
        /// </summary>
        Fixed = 2,
    }
}
