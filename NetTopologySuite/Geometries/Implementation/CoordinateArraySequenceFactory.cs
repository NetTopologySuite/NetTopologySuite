namespace NetTopologySuite.Geometries.Implementation
{
    using System;
    using NetTopologySuite.Geometries;

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
                dimension = 2;
            return new CoordinateArraySequence(size, dimension);
        }

        public ICoordinateSequence Create(int size, int dimension, int measures)
        {
            int spatial = dimension - measures;

            if (measures > 1)
            {
                measures = 1; // clip measures
            }

            if (spatial > 3)
            {
                spatial = 3; // clip spatial dimension
                // throw new ArgumentException("spatial dimension must be <= 3");
            }

            if (spatial < 2)
            {
                spatial = 2; // handle bogus dimension
            }

            return new CoordinateArraySequence(size, spatial + measures, measures);
        }

        public ICoordinateSequence Create(int size, Ordinates ordinates)
        {
            return new CoordinateArraySequence(size, OrdinatesUtility.OrdinatesToDimension(ordinates));
        }

        public Ordinates Ordinates => Ordinates.XYZ;
    }
}
