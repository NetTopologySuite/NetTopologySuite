using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a GeoJSON Writer allowing for serialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonWriter
    {
        public GeoJsonWriter()
        {
            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public string Write(IGeometry geometry)
        {
            if (geometry == null) 
                throw new ArgumentNullException("geometry");

            JsonSerializer g = GeoJsonSerializer.Create(SerializerSettings, geometry.Factory);

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, geometry);
            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns></returns>
        public string Write(IFeature feature)
        {
            JsonSerializer g = GeoJsonSerializer.Create(SerializerSettings, feature.Geometry.Factory); ;
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, feature);
            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        /// <returns></returns>
        public string Write(FeatureCollection featureCollection)
        {
            JsonSerializer g = GeoJsonSerializer.Create(SerializerSettings, featureCollection.Features[0].Geometry.Factory); ;
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, featureCollection);
            return sb.ToString();
        }

        /// <summary>
        /// Writes any specified object.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns></returns>
        public string Write(object value)
        {
            JsonSerializer g = GeoJsonSerializer.Create(SerializerSettings, GeometryFactory.Default); ;
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, value);
            return sb.ToString();
        }
    }
}