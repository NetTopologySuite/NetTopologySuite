// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoJSONObjectType.cs" company="Jörg Battermann">
//   Copyright © Jörg Battermann 2011
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace NetTopologySuite.IO
{
    /// <summary>
    /// Defines the GeoJSON Objects types as defined in the <a href="http://geojson.org/geojson-spec.html#geojson-objects">geojson.org v1.0 spec</a>.
    /// </summary>
    public enum GeoJsonObjectType
    {
        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#point">Point</a> type.
        /// </summary>
        Point,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#multipoint">MultiPoint</a> type.
        /// </summary>
        MultiPoint,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#linestring">LineString</a> type.
        /// </summary>
        LineString,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#multilinestring">MultiLineString</a> type.
        /// </summary>
        MultiLineString,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#polygon">Polygon</a> type.
        /// </summary>
        Polygon,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#multipolygon">MultiPolygon</a> type.
        /// </summary>
        MultiPolygon,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#geometry-collection">GeometryCollection</a> type.
        /// </summary>
        GeometryCollection,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#feature-objects">Feature</a> type.
        /// </summary>
        Feature,

        /// <summary>
        /// Defines the <a href="http://geojson.org/geojson-spec.html#feature-collection-objects">FeatureCollection</a> type.
        /// </summary>
        FeatureCollection
    }
}
