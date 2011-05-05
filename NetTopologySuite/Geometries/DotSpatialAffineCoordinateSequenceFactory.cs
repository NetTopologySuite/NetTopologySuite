namespace GisSharpBlog.NetTopologySuite.Geometries
{
    using System;
    using GeoAPI.Geometries;

#if !SILVERLIGHT
    [Serializable]
#endif
    public class DotSpatialAffineCoordinateSequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly DotSpatialAffineCoordinateSequenceFactory instance = new DotSpatialAffineCoordinateSequenceFactory();

        private DotSpatialAffineCoordinateSequenceFactory() { }

        /// <summary>
        /// Returns the singleton instance of DotSpatialAffineCoordinateSequenceFactory.
        /// </summary>
        /// <returns></returns>
        public static DotSpatialAffineCoordinateSequenceFactory Instance
        {
            get { return instance; }
        }

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public ICoordinateSequence Create(ICoordinate[] coordinates)
        {
            return new DotSpatialAffineCoordinateSequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            return new DotSpatialAffineCoordinateSequence(coordSeq);
        }

        public ICoordinateSequence Create(int size, int dimension)
        {
            return new DotSpatialAffineCoordinateSequence(size, dimension);
        }
    }
}
