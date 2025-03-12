using NetTopologySuite.Algorithm;
using System;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A value class containing the parameters which
    /// specify how a buffer should be constructed.
    /// <para/>
    /// The parameters allow control over:
    /// <list type="bullet">
    /// <item><description>Quadrant segments (accuracy of approximation for circular arcs)</description></item>
    /// <item><description>End Cap style</description></item>
    /// <item><description>Join style</description></item>
    /// <item><description>Mitre limit</description></item>
    /// <item><description>whether the buffer is single-sided</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    public class BufferParameters
    {
        /// <summary>
        /// The default number of facets into which to divide a fillet of 90 degrees.<br/>
        /// A value of 8 gives less than 2% max error in the buffer distance.<para/>
        /// For a max error of &lt; 1%, use QS = 12.<para/>
        /// For a max error of &lt; 0.1%, use QS = 18.
        /// </summary>
        public const int DefaultQuadrantSegments = 8;

        /// <summary>
        /// The default number of facets into which to divide a fillet of 90 degrees.<br/>
        /// A value of 8 gives less than 2% max error in the buffer distance.<para/>
        /// For a max error of &lt; 1%, use QS = 12.<para/>
        /// For a max error of &lt; 0.1%, use QS = 18.
        /// </summary>
        public const JoinStyle DefaultJoinStyle = JoinStyle.Round;

        /// <summary>
        /// The default mitre limit
        /// Allows fairly pointy mitres.
        /// </summary>
        public const double DefaultMitreLimit = 5.0;

        /// <summary>
        /// The default simplify factor.
        /// Provides an accuracy of about 1%, which matches
        /// the accuracy of the <see cref="DefaultQuadrantSegments"/> parameter.
        /// </summary>
        public const double DefaultSimplifyFactor = 0.01;

        private int _quadrantSegments = DefaultQuadrantSegments;
        private EndCapStyle _endCapStyle = EndCapStyle.Round;
        private JoinStyle _joinStyle = JoinStyle.Round;
        private double _mitreLimit = DefaultMitreLimit;
        private double _simplifyFactor = DefaultSimplifyFactor;

        /// <summary>
        /// Creates a default set of parameters
        /// </summary>
        public BufferParameters()
        {
        }

        /// <summary>
        /// Creates a set of parameters with the given quadrantSegments value.
        /// </summary>
        /// <param name="quadrantSegments">The number of quadrant segments to use</param>
        public BufferParameters(int quadrantSegments)
        {
            QuadrantSegments = quadrantSegments;
        }

        /// <summary>
        /// Creates a set of parameters with the
        /// given quadrantSegments and endCapStyle values.
        /// </summary>
        /// <param name="quadrantSegments"> the number of quadrant segments to use</param>
        /// <param name="endCapStyle"> the end cap style to use</param>
        public BufferParameters(int quadrantSegments,
            EndCapStyle endCapStyle)
            : this(quadrantSegments)
        {
            EndCapStyle = endCapStyle;
        }

        /// <summary>
        /// Creates a set of parameters with the
        /// given parameter values.
        /// </summary>
        /// <param name="quadrantSegments"> the number of quadrant segments to use</param>
        /// <param name="endCapStyle"> the end cap style to use</param>
        /// <param name="joinStyle"> the join style to use</param>
        /// <param name="mitreLimit"> the mitre limit to use</param>
        public BufferParameters(int quadrantSegments,
            EndCapStyle endCapStyle,
            JoinStyle joinStyle,
            double mitreLimit)
            : this(quadrantSegments, endCapStyle)
        {
            JoinStyle = joinStyle;
            MitreLimit = mitreLimit;
        }

        /// <summary>
        /// Gets or sets the number of line segments in a quarter-circle
        /// used to approximate angle fillets in round endcaps and joins.
        /// The value should be at least 1.
        /// <para/>
        /// This determines the
        /// error in the approximation to the true buffer curve.<br/>
        /// The default value of 8 gives less than 2% error in the buffer distance.<para/>
        /// For an error of &lt; 1%, use QS = 12.<para/>
        /// For an error of &lt; 0.1%, use QS = 18.<para/>
        /// The error is always less than the buffer distance
        /// (in other words, the computed buffer curve is always inside the true
        /// curve).
        /// </summary>
        public int QuadrantSegments
        {
            get => _quadrantSegments;
            set => _quadrantSegments = value;
        }

        /// <summary>
        /// Computes the maximum distance error due to a given level of approximation to a true arc.
        /// </summary>
        /// <param name="quadSegs">The number of segments used to approximate a quarter-circle</param>
        /// <returns>The error of approximation</returns>
        public static double BufferDistanceError(int quadSegs)
        {
            double alpha = AngleUtility.PiOver2 / quadSegs;
            return 1 - Math.Cos(alpha / 2.0);
        }

        /// <summary>
        /// Gets or sets the end cap style of the generated buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The styles supported are <see cref="EndCapStyle.Round"/>,
        /// <see cref="Buffer.EndCapStyle.Flat"/>, and
        /// <see cref="Buffer.EndCapStyle.Square"/>.
        /// </para>
        /// <para>The default is <see cref="Buffer.EndCapStyle.Round"/>.</para>
        /// </remarks>
        public EndCapStyle EndCapStyle
        {
            get => _endCapStyle;
            set => _endCapStyle = value;
        }

        /// <summary>
        /// Gets/Sets the join style for outside (reflex) corners between line segments.
        /// </summary>
        /// <remarks>
        /// <para>The styles supported are <see cref="Buffer.JoinStyle.Round"/>,
        /// <see cref="Buffer.JoinStyle.Mitre"/> and <see cref="Buffer.JoinStyle.Bevel"/></para>
        /// The default is <see cref="Buffer.JoinStyle.Round"/>
        /// </remarks>
        public JoinStyle JoinStyle
        {
            get => _joinStyle;
            set => _joinStyle = value;
        }

        /// <summary>
        /// Sets the limit on the mitre ratio used for very sharp corners.
        /// </summary>
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
        public double MitreLimit
        {
            get => _mitreLimit;
            set => _mitreLimit = value;
        }

        /// <summary>
        /// Gets or sets whether the computed buffer should be single-sided.
        /// A single-sided buffer is constructed on only one side of each input line.
        /// <para>
        /// The side used is determined by the sign of the buffer distance:
        /// <list type="bullet">
        /// <item><description>a positive distance indicates the left-hand side</description></item>
        /// <item><description>a negative distance indicates the right-hand side</description></item>
        /// </list>
        /// The single-sided buffer of point geometries is  the same as the regular buffer.
        /// </para><para>
        /// The End Cap Style for single-sided buffers is always ignored,
        /// and forced to the equivalent of <see cref="Buffer.EndCapStyle.Flat"/>.
        /// </para>
        /// </summary>
        public bool IsSingleSided { get; set; }

        /// <summary>
        /// Factor used to determine the simplify distance tolerance
        /// for input simplification.
        /// Simplifying can increase the performance of computing buffers.
        /// Generally the simplify factor should be greater than 0.
        /// Values between 0.01 and .1 produce relatively good accuracy for the generate buffer.
        /// Larger values sacrifice accuracy in return for performance.
        /// </summary>
        public double SimplifyFactor
        {
            get => _simplifyFactor;
            set => _simplifyFactor = value < 0 ? 0 : value;
        }

        /// <summary>
        /// Creates a copy 
        /// </summary>
        /// <returns></returns>
        public BufferParameters Copy()
        {
            var bp = new BufferParameters
            {
                _quadrantSegments = _quadrantSegments,
                _endCapStyle = _endCapStyle,
                _joinStyle = _joinStyle,
                _mitreLimit = _mitreLimit,
                IsSingleSided = IsSingleSided,
                _simplifyFactor = _simplifyFactor
            };
            return bp;
        }
    }
}
