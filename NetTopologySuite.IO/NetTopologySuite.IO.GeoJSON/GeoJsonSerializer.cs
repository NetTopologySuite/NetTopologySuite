using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Json Serializer with support for GeoJson object structure.
    /// </summary>
    public class GeoJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        public GeoJsonSerializer() :this(GeometryFactory.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        public GeoJsonSerializer(IGeometryFactory geometryFactory)
        {
            base.Converters.Add(new ICRSObjectConverter());
            base.Converters.Add(new FeatureCollectionConverter());
            base.Converters.Add(new FeatureConverter());
            base.Converters.Add(new AttributesTableConverter());
            base.Converters.Add(new GeometryConverter(geometryFactory));
            base.Converters.Add(new GeometryArrayConverter());
            base.Converters.Add(new CoordinateConverter());
            base.Converters.Add(new EnvelopeConverter());
        }
    }

    /// <summary>
    /// Represents a GeoJSON Writer allowing for serialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonWriter
    {
        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public string Write(IGeometry geometry)
        {
            if (geometry == null) 
                throw new ArgumentNullException("geometry");

            GeoJsonSerializer g = new GeoJsonSerializer(geometry.Factory);
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
            GeoJsonSerializer g = new GeoJsonSerializer();
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
            GeoJsonSerializer g = new GeoJsonSerializer();
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
            GeoJsonSerializer g = new GeoJsonSerializer();
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, value);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a GeoJSON Reader allowing for deserialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonReader
    {
        /// <summary>
        /// Reads the specified json.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public TObject Read<TObject>(string json)
            where TObject : class
        {
            GeoJsonSerializer g = new GeoJsonSerializer();
            using (StringReader sr = new StringReader(json))
            {
                return g.Deserialize<TObject>(new JsonTextReader(sr));
            }
        }
    }
}