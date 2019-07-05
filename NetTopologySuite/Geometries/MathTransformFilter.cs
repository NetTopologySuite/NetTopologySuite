using System;

using ProjNet.CoordinateSystems.Transformations;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An <see cref="IEntireCoordinateSequenceFilter"/> implementation that runs
    /// <see cref="CoordinateSequence.Apply(MathTransform)"/>.
    /// </summary>
    public sealed class MathTransformFilter : IEntireCoordinateSequenceFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MathTransformFilter"/> class.
        /// </summary>
        /// <param name="transform">
        /// The value for <see cref="Transform"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transform"/> is <see langword="null"/>.
        /// </exception>
        [CLSCompliant(false)]
        public MathTransformFilter(MathTransform transform)
        {
            Transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        /// <summary>
        /// Gets the <see cref="MathTransform"/> to apply to each <see cref="CoordinateSequence"/>.
        /// </summary>
        [CLSCompliant(false)]
        public MathTransform Transform { get; }

        /// <inheritdoc />
        public bool Done => false;

        /// <inheritdoc />
        public bool GeometryChanged => true;

        /// <inheritdoc />
        public void Filter(CoordinateSequence seq)
        {
            if (seq is null)
            {
                throw new ArgumentNullException(nameof(seq));
            }

            seq.Apply(Transform);
        }
    }
}
