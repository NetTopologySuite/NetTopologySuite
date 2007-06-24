using System;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// An object that knows how to build a particular implementation of
    /// <c>ICoordinateSequence</c> from an array of Coordinates.
    /// </summary>
    /// <seealso cref="ICoordinateSequence" />
    public interface ICoordinateSequenceFactory
    {
        /// <summary>
        /// Returns a CoordinateSequence based on the given array; whether or not the
        /// array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">Coordinates array, which may not be null nor contain null elements</param>
        ICoordinateSequence Create(ICoordinate[] coordinates);

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" />  which is a copy
        /// of the given <see cref="ICoordinateSequence" />.
        /// This method must handle null arguments by creating an empty sequence.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns></returns>
        ICoordinateSequence Create(ICoordinateSequence coordSeq);

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and dimension.
        /// For this to be useful, the <see cref="ICoordinateSequence" /> implementation must
        /// be mutable.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension">the dimension of the coordinates in the sequence 
        /// (if user-specifiable, otherwise ignored)</param>
        /// <returns></returns>
        ICoordinateSequence Create(int size, int dimension);
    }
}
