namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Specifies the degrees of intersection between two lines or line
    /// segments.
    /// </summary>
    /// <remarks>
    /// The integer values of the members of this enum are also the number
    /// of intersections between them.
    /// </remarks>
    public enum LineIntersectionDegrees
    {
        // These numbers indicate the number of intersections
        // which are present in the intersection between two lines
        // or segments. Do not reorder.
        /// <summary>
        /// Lines or line segments do not intersect.
        /// </summary>
        /// <value>0</value>
        DoesNotIntersect = 0,

        /// <summary>
        /// Lines or line segments have at most one intersection.
        /// </summary>
        /// <value>1</value>
        Intersects = 1,

        /// <summary>
        /// Lines or line segments have at least two intersections,
        /// and are therefore collinear.
        /// </summary>
        /// <value>2</value>
        Collinear = 2
    }
}