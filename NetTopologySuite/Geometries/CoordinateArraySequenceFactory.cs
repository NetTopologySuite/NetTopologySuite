using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
    [Serializable]
    public sealed class CoordinateArraySequenceFactory<TCoordinate> : ICoordinateSequenceFactory<TCoordinate>
    {
        private static readonly CoordinateArraySequenceFactory<TCoordinate> instance
            = new CoordinateArraySequenceFactory<TCoordinate>();

        private CoordinateArraySequenceFactory() {}

        /// <summary>
        /// Returns the singleton instance of CoordinateArraySequenceFactory.
        /// </summary>
        public static CoordinateArraySequenceFactory<TCoordinate> Instance
        {
            get { return instance; }
        }

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        public ICoordinateSequence<TCoordinate> Create(IEnumerable<TCoordinate> coordinates)
        {
            return new CoordinateArraySequence<TCoordinate>(coordinates);
        }

        public ICoordinateSequence<TCoordinate> Create(ICoordinateSequence<TCoordinate> coordSeq)
        {
            return new CoordinateArraySequence<TCoordinate>(coordSeq);
        }

        public ICoordinateSequence<TCoordinate> Create(Int32 size, Int32 dimension)
        {
            return new CoordinateArraySequence<TCoordinate>(size);
        }
    }
}