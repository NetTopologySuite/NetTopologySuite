namespace GisSharpBlog.NetTopologySuite.IO
{
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
    };
}
