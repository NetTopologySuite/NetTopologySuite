using System;
using GeoAPI.Operations.Buffer;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Contains the parameters which describe how a buffer should be constructed.
    /// </summary>
    /// <author>Martin Davis</author>
    public class BufferParameters : IBufferParameters
    {

        /**
         * 
         */
        [Obsolete]
        public const int CAP_ROUND = 1;
        /**
         * 
         */
        [Obsolete]
        public const int CAP_FLAT = 2;
        /**
         * 
         */
        [Obsolete]
        public const int CAP_SQUARE = 3;

        /**
         * 
         */
        [Obsolete]
        public const int JOIN_ROUND = 1;
        /**
         * 
         */
        [Obsolete]
        public const int JOIN_MITRE = 2;
        /**
         * 
         */
        [Obsolete]
        public const int JOIN_BEVEL = 3;

        /**
         * The default number of facets into which to divide a fillet of 90 degrees.
         * A value of 8 gives less than 2% max error in the buffer distance.
         * For a max error of < 1%, use QS = 12
         */
        public const int DefaultQuadrantSegments = 8;

        /**
         * The default mitre limit
         * Allows fairly pointy mitres.
         */
        public const double DefaultMitreLimit = 5.0;


        private int _quadrantSegments = DefaultQuadrantSegments;
        private EndCapStyle _endCapStyle = EndCapStyle.Round;
        private JoinStyle _joinStyle = JoinStyle.Round;
        private double _mitreLimit = DefaultMitreLimit;

        ///<summary>
        /// Creates a default set of parameters
        ///</summary>
        public BufferParameters()
        {
        }

        ///<summary>
        /// Creates a set of parameters with the given quadrantSegments value.
        /// </summary>
        /// <param name="quadrantSegments">The number of quadrant segments to use</param>
        public BufferParameters(int quadrantSegments)
        {
            QuadrantSegments = quadrantSegments;
        }

        /**
         * Creates a set of parameters with the
         * given quadrantSegments and endCapStyle values.
         * 
         * @param quadrantSegments the number of quadrant segments to use
         * @param endCapStyle the end cap style to use
         */
        public BufferParameters(int quadrantSegments,
            EndCapStyle endCapStyle) : this(quadrantSegments)
        {
            EndCapStyle = endCapStyle;
        }

        /**
         * Creates a set of parameters with the
         * given parameter values.
         * 
         * @param quadrantSegments the number of quadrant segments to use
         * @param endCapStyle the end cap style to use
         * @param joinStyle the join style to use
         * @param mitreLimit the mitre limit to use
         */
        public BufferParameters(int quadrantSegments,
            EndCapStyle endCapStyle,
            JoinStyle joinStyle,
            double mitreLimit)
            :this(quadrantSegments, endCapStyle)
        {
            JoinStyle = joinStyle;
            MitreLimit = mitreLimit;
        }

        ///<summary>
        /// Gets/sets the number of quadrant segments which will be used
        ///</summary>
        /// <remarks>
        /// QuadrantSegments is the number of line segments used to approximate an angle fillet.
        /// <list type="Table">
        /// <item>qs &gt;>= 1</item><description>joins are round, and qs indicates the number of segments to use to approximate a quarter-circle.</description>
        /// <item>qs = 0</item><description>joins are beveled</description>
        /// <item>qs &lt; 0</item><description>joins are mitred, and the value of qs indicates the mitre ration limit as <c>mitreLimit = |qs|</c></description>
        /// </list>
        /// </remarks>
        public int QuadrantSegments
        {
            get { return _quadrantSegments; }
            set
            {
                _quadrantSegments = value;
                /** 
                 * Indicates how to construct fillets.
                 * If qs &gt;= 1, fillet is round, and qs indicates number of 
                 * segments to use to approximate a quarter-circle.
                 * If qs = 0, fillet is butt (i.e. no filleting is performed)
                 * If qs &lt; 0, fillet is mitred, and absolute value of qs
                 * indicates maximum length of mitre according to
                 * 
                 * mitreLimit = |qs|
                 */
                if (_quadrantSegments == 0)
                    _joinStyle = JoinStyle.Bevel;
                if (_quadrantSegments < 0)
                {
                    _joinStyle = JoinStyle.Mitre;
                    _mitreLimit = Math.Abs(_quadrantSegments);
                }

                if (value <= 0)
                {
                    _quadrantSegments = 1;
                }

                /**
                 * If join style was set by the value,
                 * use the default for the actual quadrantSegments value.
                 */
                if (_joinStyle != JoinStyle.Round)
                {
                    _quadrantSegments = DefaultQuadrantSegments;
                }
            }
        }

        ///<summary>
        /// Gets/Sets the end cap style of the generated buffer.
        ///</summary>
        /// <remarks>
        /// <para>
        /// The styles supported are <see cref="EndCapStyle.Round"/>, <see cref="EndCapStyle.Flat"/>, and <see cref="EndCapStyle.Square"/>.
        /// </para>
        /// <para>The default is <see cref="EndCapStyle.Round"/>.</para>
        /// </remarks>
        public EndCapStyle EndCapStyle
        {
            get { return _endCapStyle; }
            set { _endCapStyle = value; }
        }


        ///<summary>
        /// Gets/Sets the join style for outside (reflex) corners between line segments.
        ///</summary>
        /// <remarks>
        /// <para>Allowable values are <see cref="JoinStyle.Round"/> (which is the default), <see cref="JoinStyle.Mitre"/> and <see cref="JoinStyle.Bevel"/></para>
        /// </remarks>
        public JoinStyle JoinStyle
        {
            get { return _joinStyle; }
            set { _joinStyle = value; }
        }

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
        public double MitreLimit
        {
            get { return _mitreLimit; }
            set { _mitreLimit = value; }
        }


    }
}
