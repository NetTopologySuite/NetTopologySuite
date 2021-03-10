using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Constants used in the WKT (Well-Known Text) format.
    /// </summary>
    /// <author>Martin Davis</author>
    public class WKTConstants
    {
        /// <summary>
        /// Token text for <see cref="GeometryCollection"/> geometries
        /// </summary>
        public const string GEOMETRYCOLLECTION = "GEOMETRYCOLLECTION";
        /// <summary>
        /// Token text for <see cref="LinearRing"/> geometries
        /// </summary>
        public const string LINEARRING = "LINEARRING";
        /// <summary>
        /// Token text for <see cref="LineString"/> geometries
        /// </summary>
        public const string LINESTRING = "LINESTRING";
        /// <summary>
        /// Token text for <see cref="MultiPolygon"/> geometries
        /// </summary>
        public const string MULTIPOLYGON = "MULTIPOLYGON";
        /// <summary>
        /// Token text for <see cref="MultiLineString"/> geometries
        /// </summary>
        public const string MULTILINESTRING = "MULTILINESTRING";
        /// <summary>
        /// Token text for <see cref="MultiPoint"/> geometries
        /// </summary>
        public const string MULTIPOINT = "MULTIPOINT";
        /// <summary>
        /// Token text for <see cref="Point"/> geometries
        /// </summary>
        public const string POINT = "POINT";
        /// <summary>
        /// Token text for <see cref="Polygon"/> geometries
        /// </summary>
        public const string POLYGON = "POLYGON";

        /// <summary>
        /// Token text for empty geometries
        /// </summary>
        public const string EMPTY = "EMPTY";

        /// <summary>
        /// Token text indicating that geometries have measure-ordinate values
        /// </summary>
        public const string M = "M";
        /// <summary>
        /// Token text indicating that geometries have z-ordinate values
        /// </summary>
        public const string Z = "Z";
        /// <summary>
        /// Token text indicating that geometries have both z- and measure-ordinate values
        /// </summary>
        public const string ZM = "ZM";

    }
}
