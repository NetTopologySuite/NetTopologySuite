using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An object that knows how to build a particular implementation of
    /// <c>CoordinateSequence</c> from an array of Coordinates.
    /// </summary>
    /// <seealso cref="CoordinateSequence" />
    [Serializable]
    public abstract class CoordinateSequenceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSequenceFactory"/> class.`
        /// </summary>
        protected CoordinateSequenceFactory()
            : this(Ordinates.AllOrdinates) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="ordinates">
        /// The maximum set of <see cref="Geometries.Ordinates"/> flags that this instance will be
        /// able to create sequences for.
        /// </param>
        protected CoordinateSequenceFactory(Ordinates ordinates) => Ordinates = Ordinates.XY | ordinates;

        /// <summary>
        /// Gets the Ordinate flags that sequences created by this factory can maximal cope with.
        /// </summary>
        public Ordinates Ordinates { get; }

        /// <summary>
        /// Returns a <see cref="CoordinateSequence" /> based on the given array; 
        /// whether or not the array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">A coordinates array, which may not be null nor contain null elements</param>
        /// <returns>A coordinate sequence.</returns>
        public virtual CoordinateSequence Create(Coordinate[]? coordinates)
        {
            var result = Create(coordinates?.Length ?? 0, CoordinateArrays.Dimension(coordinates), CoordinateArrays.Measures(coordinates));
            if (coordinates != null)
            {
                for (int i = 0; i < coordinates.Length; i++)
                {
                    for (int dim = 0; dim < result.Dimension; dim++)
                    {
                        result.SetOrdinate(i, dim, coordinates[i][dim]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" />  which is a copy
        /// of the given <see cref="CoordinateSequence" />.
        /// This method must handle null arguments by creating an empty sequence.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns>A coordinate sequence</returns>
        public virtual CoordinateSequence Create(CoordinateSequence? coordSeq)
        {
            var result = Create(coordSeq?.Count ?? 0, coordSeq?.Dimension ?? 2, coordSeq?.Measures ?? 0);
            if (coordSeq != null)
            {
                for (int i = 0; i < coordSeq.Count; i++)
                {
                    for (int dim = 0; dim < result.Dimension; dim++)
                    {
                        result.SetOrdinate(i, dim, coordSeq.GetOrdinate(i, dim));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and dimension.
        /// For this to be useful, the <see cref="CoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <remarks>
        /// If the requested dimension is larger than the CoordinateSequence implementation
        /// can provide, then a sequence of maximum possible dimension should be created.
        /// An error should not be thrown.
        /// </remarks>
        /// <param name="size"></param>
        /// <param name="dimension">the dimension of the coordinates in the sequence 
        /// (if user-specifiable, otherwise ignored)</param>
        /// <returns>A coordinate sequence</returns>
        public CoordinateSequence Create(int size, int dimension) => Create(size, dimension, 0);

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and dimension
        /// with measure support. For this to be useful, the <see cref="CoordinateSequence" />
        /// implementation must be mutable.
        /// </summary>
        /// <remarks>
        /// If the requested dimension or measures are larger than the CoordinateSequence implementation
        /// can provide, then a sequence of maximum possible dimension should be created.
        /// An error should not be thrown.
        /// </remarks>
        /// <param name="size">The number of coordinates in the sequence</param>
        /// <param name="dimension">The dimension of the coordinates in the sequence (if user-specifiable,
        /// otherwise ignored)</param>
        /// <param name="measures">The number of measures of the coordinates in the sequence (if user-specifiable,
        /// otherwise ignored)</param>
        public abstract CoordinateSequence Create(int size, int dimension, int measures);

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and ordinates.
        /// For this to be useful, the <see cref="CoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <param name="size">The number of coordinates.</param>
        /// <param name="ordinates">
        /// The ordinates each coordinate has. <see cref="Geometries.Ordinates.XY"/> is fix, <see cref="Geometries.Ordinates.Z"/> and <see cref="Geometries.Ordinates.M"/> can be set.
        /// </param>
        /// <returns>A coordinate sequence.</returns>
        public virtual CoordinateSequence Create(int size, Ordinates ordinates) => Create(size, OrdinatesUtility.OrdinatesToDimension(ordinates & Ordinates), OrdinatesUtility.OrdinatesToMeasures(ordinates & Ordinates));
    }
}
