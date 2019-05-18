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
        private static ExtendedCoordinateSequenceFactory instance;

        private ExtendedCoordinateSequenceFactory() { }

        /// <summary> Returns the singleton instance of ExtendedCoordinateSequenceFactory
        /// </summary>
        public static ExtendedCoordinateSequenceFactory Instance()
        {
            return instance;
        }

        /// <summary> Returns an ExtendedCoordinateSequence based on the given array -- the array is used
        /// directly if it is an instance of ExtendedCoordinate[]; otherwise it is
        /// copied.
        /// </summary>
        public virtual CoordinateSequence Create(Coordinate[] coordinates)
        {
            return coordinates is ExtendedCoordinate[] ?
                new ExtendedCoordinateSequence((ExtendedCoordinate[]) coordinates) :
                new ExtendedCoordinateSequence(coordinates);
        }

        public CoordinateSequence Create(CoordinateSequence coordSeq)
        {
            throw new NotImplementedException();
        }

        public CoordinateSequence Create(int size, int dimension)
        {
            throw new NotImplementedException();
        }

        public CoordinateSequence Create(int size, int dimension, int measures)
        {
            throw new NotImplementedException();
        }

        public CoordinateSequence Create(int size, Ordinates ordinates)
        {
            throw new NotImplementedException();
        }

        public Ordinates Ordinates => Ordinates.XYZM;

        static ExtendedCoordinateSequenceFactory()
        {
            instance = new ExtendedCoordinateSequenceFactory();
        }
    }
}
