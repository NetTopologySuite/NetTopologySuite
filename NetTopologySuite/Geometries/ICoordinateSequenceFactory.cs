namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An object that knows how to build a particular implementation of
    /// <c>ICoordinateSequence</c> from an array of Coordinates.
    /// </summary>
    /// <seealso cref="ICoordinateSequence" />
    public interface ICoordinateSequenceFactory
    {
        /// <summary>
        /// Returns a <see cref="ICoordinateSequence" /> based on the given array; 
        /// whether or not the array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">A coordinates array, which may not be null nor contain null elements</param>
        /// <returns>A coordinate sequence.</returns>
        ICoordinateSequence Create(Coordinate[] coordinates);

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" />  which is a copy
        /// of the given <see cref="ICoordinateSequence" />.
        /// This method must handle null arguments by creating an empty sequence.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns>A coordinate sequence</returns>
        ICoordinateSequence Create(ICoordinateSequence coordSeq);

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and dimension.
        /// For this to be useful, the <see cref="ICoordinateSequence" /> implementation must be mutable.
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
        ICoordinateSequence Create(int size, int dimension);

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and dimension
        /// with measure support. For this to be useful, the <see cref="ICoordinateSequence" />
        /// implementation must be mutable.
        /// </summary>
        /// <remarks>
        /// If the requested dimension or measures are larger than the CoordinateSequence implementation
        /// can provide, then a sequence of maximum possible dimension should be created.
        /// An error should not be thrown.
        /// <para/>
        /// A default implementation of this method could look like this:
        /// <code>
        /// public ICoordinateSequence Create(int size, int dimension, int measures)
        /// {
        ///     return create(size, dimension);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="size">The number of coordinates in the sequence</param>
        /// <param name="dimension">The dimension of the coordinates in the sequence (if user-specifiable,
        /// otherwise ignored)</param>
        /// <param name="measures">The number of measures of the coordinates in the sequence (if user-specifiable,
        /// otherwise ignored)</param>
        /// 
        ICoordinateSequence Create(int size, int dimension, int measures);

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and ordinates.
        /// For this to be useful, the <see cref="ICoordinateSequence" /> implementation must be mutable.
        /// </summary>        
        /// <param name="size">The number of coordinates.</param>
        /// <param name="ordinates">
        /// The ordinates each coordinate has. <see cref="Geometries.Ordinates.XY"/> is fix, <see cref="Geometries.Ordinates.Z"/> and <see cref="Geometries.Ordinates.M"/> can be set.
        /// </param>
        /// <returns>A coordinate sequence.</returns>
        ICoordinateSequence Create(int size, Ordinates ordinates);

        /// <summary>
        /// Gets the Ordinate flags that sequences created by this factory can maximal cope with.
        /// </summary>
        Ordinates Ordinates { get; }
    }
}
