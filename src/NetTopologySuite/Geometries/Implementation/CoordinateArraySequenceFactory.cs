using System;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
    [Serializable]
    public sealed class CoordinateArraySequenceFactory : CoordinateSequenceFactory
    {
        private CoordinateArraySequenceFactory() : base(Ordinates.XYZM) { }

        /// <summary>
        /// Returns the singleton instance of CoordinateArraySequenceFactory.
        /// </summary>
        public static CoordinateArraySequenceFactory Instance { get; } = new CoordinateArraySequenceFactory();

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public override CoordinateSequence Create(Coordinate[] coordinates)
        {
            return new CoordinateArraySequence(coordinates);
        }

        /// <inheritdoc cref="CoordinateSequenceFactory.Create(CoordinateSequence)"/>
        public override CoordinateSequence Create(CoordinateSequence coordSeq)
        {
            return new CoordinateArraySequence(coordSeq);
        }

        /// <inheritdoc cref="CoordinateSequenceFactory.Create(int, int, int)"/>
        public override CoordinateSequence Create(int size, int dimension, int measures)
        {
            int spatial = dimension - measures;

            // DEVIATION: JTS can't create Coordinate instances other than XY, XYZ, XYM, or XYZM.
            // we can, so no need to clip spatial / measures.
            if (spatial < 2)
            {
                spatial = 2; // handle bogus dimension
            }

            return new CoordinateArraySequence(size, spatial + measures, measures);
        }
    }
}
