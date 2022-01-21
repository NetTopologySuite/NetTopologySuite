namespace NetTopologySuite.Samples.Technique
{
    /// <summary>
    /// Method to use to fix invalid geometry.
    /// </summary>
    public enum InvalidGeomFixMethod : int
    {
        /// <summary>
        /// Do not attempt to fix invalid geometry.
        /// </summary>
        FixNone = 0,

        /// <summary>
        /// Attempt to fix invalid geometry with the buffer method.
        /// </summary>
        FixViaBuffer = 1,

        /// <summary>
        /// Attempt to fix invalid geometry with the precision method.
        /// </summary>
        FixViaPrecision = 2,
    }
}
