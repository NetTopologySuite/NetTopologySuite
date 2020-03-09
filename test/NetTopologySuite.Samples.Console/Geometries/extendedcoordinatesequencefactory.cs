#nullable disable
using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Samples.Geometries
{
    /// <summary>
    /// Creates ExtendedCoordinateSequenceFactory internally represented
    /// as an array of <c>ExtendedCoordinate</c>s.
    /// </summary>
    public class ExtendedCoordinateSequenceFactory : CoordinateSequenceFactory
    {
        private ExtendedCoordinateSequenceFactory() : base(Ordinates.XYZM) { }

        /// <summary> Returns the singleton instance of ExtendedCoordinateSequenceFactory
        /// </summary>
        public static ExtendedCoordinateSequenceFactory Instance { get; } = new ExtendedCoordinateSequenceFactory();

        /// <summary> Returns an ExtendedCoordinateSequence based on the given array -- the array is used
        /// directly if it is an instance of ExtendedCoordinate[]; otherwise it is
        /// copied.
        /// </summary>
        public override CoordinateSequence Create(Coordinate[] coordinates)
        {
            return coordinates is ExtendedCoordinate[]?
                new ExtendedCoordinateSequence((ExtendedCoordinate[])coordinates) :
                new ExtendedCoordinateSequence(coordinates);
        }

        public override CoordinateSequence Create(int size, int dimension, int measures)
        {
            throw new NotImplementedException();
        }
    }
}
