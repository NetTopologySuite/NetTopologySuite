namespace NetTopologySuite.Geometries.Implementation
{
    using System;
    using GeoAPI.Geometries;

    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
    [Serializable]
    public sealed class CoordinateArraySequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly CoordinateArraySequenceFactory instance = new CoordinateArraySequenceFactory();

        private CoordinateArraySequenceFactory() { }

        /// <summary>
        /// Returns the singleton instance of CoordinateArraySequenceFactory.
        /// </summary>
        public static CoordinateArraySequenceFactory Instance => instance;

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public ICoordinateSequence Create(Coordinate[] coordinates)
        {
            return new CoordinateArraySequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            return new CoordinateArraySequence(coordSeq);
        }

        public ICoordinateSequence Create(int size, int dimension)
        {
            if (dimension > 3)
                dimension = 3;
            // throw new ArgumentOutOfRangeException("dimension must <= 3");
            // handle bogus dimension
            if (dimension < 2)
                // TODO: change to dimension = 2  ???
                return new CoordinateArraySequence(size);
            return new CoordinateArraySequence(size, dimension);
        }

        public ICoordinateSequence Create(int size, int dimension, int measures)
        {
            if (dimension > 4)
            {
                dimension = 4;
                //throw new IllegalArgumentException("dimension must be <= 4");
            }
            if (measures > 1)
            {
                measures = 1;
                //throw new IllegalArgumentException("measures must be <= 1");
            }
            if (dimension < 2)
                dimension = 2; // handle bogus dimension
            if (dimension - measures < 2)
            {
                throw new ArgumentException($"Minimum spatial dimension of 2 required. Input was dimension {dimension}and number of measures {measures}.");
            }
            return new CoordinateArraySequence(size, dimension, measures);
        }

        public ICoordinateSequence Create(int size, Ordinates ordinates)
        {
            return new CoordinateArraySequence(size, OrdinatesUtility.OrdinatesToDimension(ordinates));
        }

        public Ordinates Ordinates => Ordinates.XYZ;
    }
}
