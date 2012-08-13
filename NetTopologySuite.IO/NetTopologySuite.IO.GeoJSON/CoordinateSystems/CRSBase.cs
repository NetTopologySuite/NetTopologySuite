using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Base class for all ICRSObject implementing types
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class CRSBase : ICRSObject
    {
        /// <summary>
        /// Gets the type of the CRSBase object.
        /// </summary>
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CRSType Type { get; internal set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        [JsonProperty(PropertyName = "properties", Required = Required.Always)]
        public Dictionary<string, object> Properties { get; internal set; }
    }
}