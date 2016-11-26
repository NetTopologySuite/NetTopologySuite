namespace NetTopologySuite.CoordinateSystems
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see href="http://geojson.org/geojson-spec.html#linked-crs">Linked CRS type</see>.
    /// </summary>
    public class LinkedCRS : CRSBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedCRS"/> class.
        /// </summary>
        /// <param name="href">The mandatory <see href="http://geojson.org/geojson-spec.html#linked-crs">href</see> member must be a dereferenceable URI.</param>
        /// <param name="type">The optional type member will be put in the properties Dictionary as specified in the <see href="http://geojson.org/geojson-spec.html#linked-crs">GeoJSON spec</see>.</param>
        public LinkedCRS(string href, string type = "") : this (new Uri(href), type) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedCRS"/> class.
        /// </summary>
        /// <param name="href">The mandatory <see href="http://geojson.org/geojson-spec.html#linked-crs">href</see> member must be a dereferenceable URI.</param>
        /// <param name="type">The optional type member will be put in the properties Dictionary as specified in the <see href="http://geojson.org/geojson-spec.html#linked-crs">GeoJSON spec</see>.</param>
        public LinkedCRS(Uri href, string type = "")            
        {
            if (href == null)
                throw new ArgumentNullException("href");

            this.Properties = new Dictionary<string, object> { { "href", href.ToString() } };

            if (!string.IsNullOrEmpty(type))
                this.Properties.Add("type", type);

            this.Type = CRSTypes.Link;
        }        
    }
}