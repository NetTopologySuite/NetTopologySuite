namespace NetTopologySuite.IO
{
    /*
    /// <summary>
    /// Byte order
    /// </summary>
    public enum ByteOrder
    {
        /// <summary>
        /// BigEndian
        /// </summary>
        BigEndian = 0x00,

        /// <summary>
        /// LittleEndian
        /// </summary>
        LittleEndian = 0x01,
    }
    */
    /// <summary>
    /// WKB Geometry Types
    /// </summary>
    public enum WKBGeometryTypes
    {
        /// <summary>
        /// Point.
        /// </summary>
        WKBPoint = 1,

        /// <summary>
        /// LineString.
        /// </summary>
        WKBLineString = 2,

        /// <summary>
        /// Polygon.
        /// </summary>
        WKBPolygon = 3,

        /// <summary>
        /// MultiPoint.
        /// </summary>
        WKBMultiPoint = 4,

        /// <summary>
        /// MultiLineString.
        /// </summary>
        WKBMultiLineString = 5,

        /// <summary>
        /// MultiPolygon.
        /// </summary>
        WKBMultiPolygon = 6,

        /// <summary>
        /// GeometryCollection.
        /// </summary>
        WKBGeometryCollection = 7,

        /// <summary>
        /// Point with Z coordinate.
        /// </summary>
        WKBPointZ = 1001,

        /// <summary>
        /// LineString with Z coordinate.
        /// </summary>
        WKBLineStringZ = 1002,

        /// <summary>
        /// Polygon with Z coordinate.
        /// </summary>
        WKBPolygonZ = 1003,

        /// <summary>
        /// MultiPoint with Z coordinate.
        /// </summary>
        WKBMultiPointZ = 1004,

        /// <summary>
        /// MultiLineString with Z coordinate.
        /// </summary>
        WKBMultiLineStringZ = 1005,

        /// <summary>
        /// MultiPolygon with Z coordinate.
        /// </summary>
        WKBMultiPolygonZ = 1006,

        /// <summary>
        /// GeometryCollection with Z coordinate.
        /// </summary>
        WKBGeometryCollectionZ = 1007,

        /// <summary>
        /// Point with M ordinate value.
        /// </summary>
        WKBPointM = 2001,

        /// <summary>
        /// LineString with M ordinate value.
        /// </summary>
        WKBLineStringM = 2002,

        /// <summary>
        /// Polygon with M ordinate value.
        /// </summary>
        WKBPolygonM = 2003,

        /// <summary>
        /// MultiPoint with M ordinate value.
        /// </summary>
        WKBMultiPointM = 2004,

        /// <summary>
        /// MultiLineString with M ordinate value.
        /// </summary>
        WKBMultiLineStringM = 2005,

        /// <summary>
        /// MultiPolygon with M ordinate value.
        /// </summary>
        WKBMultiPolygonM = 2006,

        /// <summary>
        /// GeometryCollection with M ordinate value.
        /// </summary>
        WKBGeometryCollectionM = 2007,

        /// <summary>
        /// Point with Z coordinate and M ordinate value.
        /// </summary>
        WKBPointZM = 3001,

        /// <summary>
        /// LineString with Z coordinate and M ordinate value.
        /// </summary>
        WKBLineStringZM = 3002,

        /// <summary>
        /// Polygon with Z coordinate and M ordinate value.
        /// </summary>
        WKBPolygonZM = 3003,

        /// <summary>
        /// MultiPoint with Z coordinate and M ordinate value.
        /// </summary>
        WKBMultiPointZM = 3004,

        /// <summary>
        /// MultiLineString with Z coordinate and M ordinate value.
        /// </summary>
        WKBMultiLineStringZM = 3005,

        /// <summary>
        /// MultiPolygon with Z coordinate and M ordinate value.
        /// </summary>
        WKBMultiPolygonZM = 3006,

        /// <summary>
        /// GeometryCollection with Z coordinate and M ordinate value.
        /// </summary>
        WKBGeometryCollectionZM = 3007
    };
}