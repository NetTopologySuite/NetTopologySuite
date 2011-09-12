namespace NetTopologySuite.Geometries.Implementation
{
    using System;

    #region geoapi vs nts
#if useFullGeoAPI
    using GeoAPI.Geometries;
#else
    using ICoordinate = NetTopologySuite.Geometries.Coordinate;
    using IGeometry = NetTopologySuite.Geometries.Geometry;
    using IPoint = NetTopologySuite.Geometries.Point;
    using ILineString = NetTopologySuite.Geometries.LineString;
    using ILinearRing = NetTopologySuite.Geometries.LinearRing;
    using IPolygon = NetTopologySuite.Geometries.Polygon;
    using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
    using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
    using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
    using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
#endif
    #endregion

    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
//#if !SILVERLIGHT
    [Serializable]
//#endif
    public sealed class CoordinateArraySequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly CoordinateArraySequenceFactory instance = new CoordinateArraySequenceFactory();
        
        private CoordinateArraySequenceFactory() { }

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
        /// <returns></returns>
        public ICoordinateSequence Create(ICoordinate[] coordinates) 
        {
            return new CoordinateArraySequence(coordinates);
        }

        public ICoordinateSequence Create(ICoordinateSequence coordSeq) 
        {
            return new CoordinateArraySequence(coordSeq);
        }

        public ICoordinateSequence Create(int size, int dimension)
        {
            return new CoordinateArraySequence(size);
        }
    }
}
