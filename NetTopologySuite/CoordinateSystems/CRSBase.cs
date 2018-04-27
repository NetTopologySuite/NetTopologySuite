namespace NetTopologySuite.CoordinateSystems
{
    using System.Collections.Generic;

    /// <summary>
    /// Base class for all ICRSObject implementing types
    /// </summary>
    //[JsonObject(MemberSerialization.OptIn)]
    public abstract class CRSBase : ICRSObject
    {
        /// <summary>
        /// Gets the type of the CRSBase object.
        /// </summary>
        //[JsonProperty(PropertyName = "type", Required = Required.Always)]
        //[JsonConverter(typeof(CamelCaseStringEnumConverter))]
        public CRSTypes Type { get; internal set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        //[JsonProperty(PropertyName = "properties", Required = Required.Always)]
        public Dictionary<string, object> Properties { get; internal set; }
    }
}