namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Feature type enumeration
    /// </summary>
    public enum ShapeGeometryType
    {
        /// <summary>
        /// Null Shape
        /// </summary>
        NullShape = 0,

        /// <summary>
        /// Point
        /// </summary>
        Point = 1,

        /// <summary>
        /// LineString
        /// </summary>
        LineString = 3,

        /// <summary>
        /// Polygon
        /// </summary>
        Polygon = 5,

        /// <summary>
        /// MultiPoint
        /// </summary>
        MultiPoint = 8,

        /// <summary>
        /// PointMZ
        /// </summary>
        PointZM = 11,

        /// <summary>
        /// PolyLineMZ
        /// </summary>
        LineStringZM = 13,

        /// <summary>
        /// PolygonMZ
        /// </summary>
        PolygonZM = 15,

        /// <summary>
        /// MultiPointMZ
        /// </summary>
        MultiPointZM = 18,

        /// <summary>
        /// PointM
        /// </summary>
        PointM = 21,

        /// <summary>
        /// LineStringM
        /// </summary>
        LineStringM = 23,

        /// <summary>
        /// PolygonM
        /// </summary>
        PolygonM = 25,

        /// <summary>
        /// MultiPointM
        /// </summary>
        MultiPointM = 28,

        /// <summary>
        /// MultiPatch
        /// </summary>
        MultiPatch = 31,

        /// <summary>
        /// PointZ
        /// </summary>
        PointZ = 9,

        /// <summary>
        /// LineStringZ
        /// </summary>
        LineStringZ = 10,

        /// <summary>
        /// PolygonZ
        /// </summary>
        PolygonZ = 19,

        /// <summary>
        /// MultiPointZ
        /// </summary>
        MultiPointZ = 20,
    }        
}
