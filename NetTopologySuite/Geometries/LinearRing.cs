using System;
using GeoAPI.Geometries;
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
    /// and the interior of the ring must not self-intersect.
    /// Either orientation of the ring is allowed.
    /// <para>
    /// A ring must have either 0 or 4 or more points.
    /// The first and last points must be equal (in 2D).
    /// If these conditions are not met, the constructors throw
    /// an <see cref="ArgumentException"/></para>
    /// </remarks>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class LinearRing : LineString, ILinearRing
    {
        /// <summary>
        /// The minimum number of vertices allowed in a valid non-empty ring (= 4).
        /// Empty rings with 0 vertices are also valid.
        /// </summary>
        public const int MinimumValidSize = 4;

        /// <summary>
        /// Constructs a <c>LinearRing</c> with the vertices specified
        /// by the given <see cref="ICoordinateSequence"/>.
        /// </summary>
        /// <param name="points">A sequence points forming a closed and simple linestring,
        /// or <c>null</c> to create the empty geometry.</param>
        /// <param name="factory">The factory that creates this <c>LinearRing</c></param>
        /// <exception cref="ArgumentException">If the ring is not closed, or has too few points</exception>
        public LinearRing(ICoordinateSequence points, IGeometryFactory factory)
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
                throw new ArgumentException("Number of points must be 0 or >3");
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        protected override SortIndexValue SortIndex => SortIndexValue.LinearRing;

        /// <summary>
        /// Returns <c>Dimensions.False</c>, since by definition LinearRings do not have a boundary.
        /// </summary>
        public override Dimension BoundaryDimension => Dimension.False;

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
        public override string GeometryType => "LinearRing";

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override IGeometry CopyInternal()
        {
            return new LinearRing(CoordinateSequence.Copy(), Factory);
        }

        public override IGeometry Reverse()
        {
            var sequence = CoordinateSequence.Reversed();
            return Factory.CreateLinearRing(sequence);
        }

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
        //[Obsolete("Use GeometryFactory instead")]
        public LinearRing(Coordinate[] points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) { }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
