using System;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Models an OGC SFS <c>LinearRing</c>.
    /// </summary>
    /// <remarks>
    /// A <c>LinearRing</c> is a <see cref="LineString"/> which is both closed and simple.
    /// In other words,
    /// the first and last coordinate in the ring must be equal,
    /// and the ring must not self-intersect.
    /// Either orientation of the ring is allowed.
    /// <para/>
    /// A ring must have either 0 or 3 or more points.
    /// The first and last points must be equal (in 2D).
    /// If these conditions are not met, the constructors throw
    /// an <see cref="ArgumentException"/><br/>
    /// A ring with 3 points is invalid, because it is collapsed
    /// and thus has a self-intersection. It is allowed to be constructed
    /// so that it can be represented, and repaired if needed.
    /// </remarks>
    [Serializable]
    public class LinearRing : LineString
    {
        /// <summary>
        /// The minimum number of vertices allowed in a valid non-empty ring.
        /// Empty rings with 0 vertices are also valid.
        /// </summary>
        public const int MinimumValidSize = 3;

        /// <summary>
        /// Constructs a <c>LinearRing</c> with the vertices specified
        /// by the given <see cref="CoordinateSequence"/>.
        /// </summary>
        /// <param name="points">A sequence points forming a closed and simple linestring,
        /// or <c>null</c> to create the empty geometry.</param>
        /// <param name="factory">The factory that creates this <c>LinearRing</c></param>
        /// <exception cref="ArgumentException">If the ring is not closed, or has too few points</exception>
        public LinearRing(CoordinateSequence points, GeometryFactory factory)
            : base(points, factory)
        {
            ValidateConstruction();
        }

        /// <summary>
        ///
        /// </summary>
        private void ValidateConstruction()
        {
            if (!IsEmpty && !base.IsClosed)
                throw new ArgumentException("points must form a closed linestring");
            if (CoordinateSequence.Count >= 1 && CoordinateSequence.Count < MinimumValidSize)
                throw new ArgumentException($"Number of points must be 0 or >={MinimumValidSize}");
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.LinearRing;

        /// <summary>
        /// Returns <c>Dimensions.False</c>, since by definition LinearRings do not have a boundary.
        /// </summary>
        public override Dimension BoundaryDimension => Dimension.False;

        /// <summary>
        ///
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                if (IsEmpty)
                {
                    // empty LinearRings are closed by definition
                    return true;
                }
                return base.IsClosed;
            }
        }

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"LinearRing"</returns>
        public override string GeometryType => Geometry.TypeNameLinearRing;

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()
        {
            return new LinearRing(CoordinateSequence.Copy(), Factory);
        }

        /// <inheritdoc cref="Geometry.Reverse"/>
        [Obsolete("Call Geometry.Reverse()")]
#pragma warning disable 809
        public override Geometry Reverse()
        {
            return base.Reverse();
        }
#pragma warning restore 809

        /// <summary>
        /// The actual implementation of the <see cref="Geometry.Reverse"/> function for <c>LINEARRING</c>s.
        /// </summary>
        /// <returns>A reversed geometry</returns>
        protected override Geometry ReverseInternal()
        {
            var sequence = CoordinateSequence.Copy();
            CoordinateSequences.Reverse(sequence);
            return Factory.CreateLinearRing(sequence);
        }

        /// <summary>
        /// Gets a value indicating if this <c>LINEARRING</c> is oriented <see cref="OrientationIndex.CounterClockwise"/>
        /// </summary>
        public bool IsCCW => Orientation.IsCCW(CoordinateSequence);

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearRing"/> class.
        /// </summary>
        /// <param name="points">The points used for create this instance.</param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">If the ring is not closed, or has too few points</exception>
        public LinearRing(Coordinate[] points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) { }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
