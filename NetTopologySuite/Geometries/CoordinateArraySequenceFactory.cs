using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
    [Serializable]
    public sealed class CoordinateArraySequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly CoordinateArraySequenceFactory instance = new CoordinateArraySequenceFactory();

        private CoordinateArraySequenceFactory() {}

        /// <summary>
        /// Returns the singleton instance of CoordinateArraySequenceFactory.
        /// </summary>
        public static CoordinateArraySequenceFactory Instance
        {
            get { return instance; }
        }

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        public ICoordinateSequence Create(ICoordinate[] coordinates)
        {
            return new CoordinateArraySequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            return new CoordinateArraySequence(coordSeq);
        }

        public ICoordinateSequence Create(Int32 size, Int32 dimension)
        {
            return new CoordinateArraySequence(size);
        }
    }
}