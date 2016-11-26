namespace NetTopologySuite.CoordinateSystems
{
    using System;

    /// <summary>
    /// Defines the GeoJSON Coordinate Reference System Objects (CRS) types as defined in the <see href="http://geojson.org/geojson-spec.html#coordinate-reference-system-objects">geojson.org v1.0 spec</see>.
    /// </summary>
    [Flags]
    public enum CRSTypes
    {
        /// <summary>
        /// Defines the <see href="http://geojson.org/geojson-spec.html#named-crs">Named</see> CRS type.
        /// </summary>
        Name,

        /// <summary>
        /// Defines the <see href="http://geojson.org/geojson-spec.html#linked-crs">Linked</see> CRS type.
        /// </summary>
        Link
    }
}