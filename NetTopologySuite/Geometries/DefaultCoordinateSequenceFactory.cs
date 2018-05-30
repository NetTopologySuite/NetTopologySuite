using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Creates CoordinateSequences internally represented
    /// as an array of x's and an array of y's.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    [Obsolete("No longer used")]
    public class DefaultCoordinateSequenceFactory : ICoordinateSequenceFactory
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Readonly added
        private static readonly DefaultCoordinateSequenceFactory instance =
            new DefaultCoordinateSequenceFactory();

        /// <summary>
        ///
        /// </summary>
        private DefaultCoordinateSequenceFactory() { }

        // see http://www.javaworld.com/javaworld/javatips/jw-javatip122.html
        private object ReadResolve()
        {
            return Instance;
        }

        /// <summary>
        /// Returns the singleton instance of DefaultCoordinateSequenceFactory.
        /// </summary>
        /// <returns>Singleton instance of DefaultCoordinateSequenceFactory.</returns>
        public static DefaultCoordinateSequenceFactory Instance => instance;

        /// <summary>
        /// Returns a DefaultCoordinateSequence based on the given array
        /// (the array is not copied).
        /// </summary>
        /// <param name="coordinates">Coordinates array, which may not be null
        /// nor contain null elements</param>
        /// <returns>Singleton instance of DefaultCoordinateSequenceFactory.</returns>
        public ICoordinateSequence Create(Coordinate[] coordinates)
        {
            return new DefaultCoordinateSequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence Create(int size, int dimension)
        {
            throw new NotImplementedException();
        }

        public ICoordinateSequence Create(int size, Ordinates ordinates)
        {
            throw new NotImplementedException();
        }

        public Ordinates Ordinates => Ordinates.XYZ;
    }
}
