namespace NetTopologySuite.CoordinateSystems
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see href="http://geojson.org/geojson-spec.html#named-crs">Named CRS type</see>.
    /// </summary>
    public class NamedCRS : CRSBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedCRS"/> class.
        /// </summary>
        /// <param name="name">
        /// The mandatory <see href="http://geojson.org/geojson-spec.html#named-crs">name</see>
        /// member must be a string identifying a coordinate reference system. OGC CRS URNs such as
        /// 'urn:ogc:def:crs:OGC:1.3:CRS84' shall be preferred over legacy identifiers such as 'EPSG:4326'.
        /// </param>
        public NamedCRS(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentOutOfRangeException("name", "May not be empty");

            this.Properties = new Dictionary<string, object> { { "name", name } };
            this.Type = CRSTypes.Name;
        }
    }
}