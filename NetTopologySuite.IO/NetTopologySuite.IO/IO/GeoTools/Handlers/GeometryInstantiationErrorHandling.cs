namespace NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Possible ways of handling geometry creation issues.
    /// </summary>
    public enum GeometryInstantiationErrorHandlingOption
    {
        /// <summary>
        /// Let the code run into exception
        /// </summary>
        ThrowException,

        /// <summary>
        /// Create an empty geometry instead
        /// </summary>
        Empty,

        /// <summary>
        /// Try to fix the geometry
        /// </summary>
        /// <remarks>
        /// Possible fixes are:
        /// <list type="bullet">
        /// <item>For LineStrings with only one point provided, duplicate that point to have at least two points.</item>
        /// <item>For LinearRings with unclosed coordinate sequence, close the sequence by adding a clone of the first coordinate</item>
        /// <item>...</item>
        /// <item>For Holes in polygon, use </item>
        /// </list></remarks>
        TryFix,

        /// <summary>
        /// Ignore this geometry/feature altogether
        /// </summary>
        Null,
    }
}