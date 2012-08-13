using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NetTopologySuite.CoordinateSystems
{
    using System;

    /// <summary>
    /// Defines the GeoJSON Coordinate Reference System Objects (CRS) types as defined in the <see cref="http://geojson.org/geojson-spec.html#coordinate-reference-system-objects">geojson.org v1.0 spec</see>.
    /// </summary>
    [Flags]
    public enum CRSType
    {
        /// <summary>
        /// Defines the <see cref="http://geojson.org/geojson-spec.html#named-crs">Named</see> CRS type.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        Name,

        /// <summary>
        /// Defines the <see cref="http://geojson.org/geojson-spec.html#linked-crs">Linked</see> CRS type.
        /// </summary>
        [JsonProperty(PropertyName = "link", Required = Required.Always)]
        Link
    }
}