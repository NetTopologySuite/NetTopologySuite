using System;
using System.Collections;
using System.Text;

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
        WKBGeometryCollection = 7
    };
}
