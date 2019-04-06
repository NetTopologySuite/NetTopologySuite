using GeoAPI.Operation.Buffer;

namespace GeoAPI.Operation.Buffer
{
    /// <summary>
    /// An interface for classes that control the parameters for the buffer building process
    /// <para>
    /// The parameters allow control over:
    /// <list type="Bullet">
    /// <item>Quadrant segments (accuracy of approximation for circular arcs)</item>
    /// <item>End Cap style</item>
    /// <item>Join style</item>
    /// <item>Mitre limit</item>
    /// <item>whether the buffer is single-sided</item>
    /// </list>
    /// </para>
    /// </summary>
    public interface IBufferParameters
    {
        ///<summary>
        /// Gets/Sets the number of quadrant segments which will be used
        ///</summary>
        /// <remarks>
        /// QuadrantSegments is the number of line segments used to approximate an angle fillet.
        /// <list type="Table">
        /// <item>qs &gt;>= 1</item><description>joins are round, and qs indicates the number of segments to use to approximate a quarter-circle.</description>
        /// <item>qs = 0</item><description>joins are beveled</description>
        /// <item>qs &lt; 0</item><description>joins are mitred, and the value of qs indicates the mitre ration limit as <c>mitreLimit = |qs|</c></description>
        /// </list>
        /// </remarks>
        int QuadrantSegments { get; set; }

        ///<summary>
        /// Gets/Sets the end cap style of the generated buffer.
        ///</summary>
        /// <remarks>
        /// <para>
        /// The styles supported are <see cref="GeoAPI.Operation.Buffer.EndCapStyle.Round"/>, <see cref="GeoAPI.Operation.Buffer.EndCapStyle.Flat"/>, and <see cref="GeoAPI.Operation.Buffer.EndCapStyle.Square"/>.
        /// </para>
        /// <para>The default is <see cref="GeoAPI.Operation.Buffer.EndCapStyle.Round"/>.</para>
        /// </remarks>
        EndCapStyle EndCapStyle { get; set; }

        ///<summary>
        /// Gets/Sets the join style for outside (reflex) corners between line segments.
        ///</summary>
        /// <remarks>
        /// <para>Allowable values are <see cref="GeoAPI.Operation.Buffer.JoinStyle.Round"/> (which is the default), <see cref="GeoAPI.Operation.Buffer.JoinStyle.Mitre"/> and <see cref="GeoAPI.Operation.Buffer.JoinStyle.Bevel"/></para>
        /// </remarks>
        JoinStyle JoinStyle { get; set; }

        ///<summary>
        /// Sets the limit on the mitre ratio used for very sharp corners.
        ///</summary>
        /// <remarks>
        /// <para>
        /// The mitre ratio is the ratio of the distance from the corner
        /// to the end of the mitred offset corner.
        /// When two line segments meet at a sharp angle,
        /// a miter join will extend far beyond the original geometry.
        /// (and in the extreme case will be infinitely far.)
        /// To prevent unreasonable geometry, the mitre limit
        /// allows controlling the maximum length of the join corner.
        /// Corners with a ratio which exceed the limit will be beveled.
        /// </para>
        /// </remarks>
        double MitreLimit { get; set; }

        /// <summary>
        /// Gets or sets whether the computed buffer should be single-sided.
        /// A single-sided buffer is constructed on only one side of each input line.
        /// <para>
        /// The side used is determined by the sign of the buffer distance:
        /// <list type="Bullet">
        /// <item>a positive distance indicates the left-hand side</item>
        /// <item>a negative distance indicates the right-hand side</item>
        /// </list>
        /// The single-sided buffer of point geometries is  the same as the regular buffer.
        /// </para><para>
        /// The End Cap Style for single-sided buffers is always ignored,
        /// and forced to the equivalent of <see cref="GeoAPI.Operation.Buffer.EndCapStyle.Flat"/>.
        /// </para>
        /// </summary>
        bool IsSingleSided { get; set; }

        /// <summary>
        /// Gets or sets the factor used to determine the simplify distance tolerance
        /// for input simplification.
        /// Simplifying can increase the performance of computing buffers.
        /// Generally the simplify factor should be greater than 0.
        /// Values between 0.01 and .1 produce relatively good accuracy for the generate buffer.
        /// Larger values sacrifice accuracy in return for performance.
        /// </summary>
        double SimplifyFactor { get; set; }
    }
}