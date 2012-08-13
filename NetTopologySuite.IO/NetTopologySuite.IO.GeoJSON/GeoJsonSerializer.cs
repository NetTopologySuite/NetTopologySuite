using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using NetTopologySuite.Features;

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
        public GeoJsonSerializer()
            :this(GeometryFactory.Default)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GeoJsonSerializer(IGeometryFactory geometryFactory)
        {
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

            var g = new GeoJsonSerializer(geometry.Factory);
            var sb = new StringBuilder();
            
            using (var sw = new StringWriter(sb))
                g.Serialize(sw, geometry);
            
            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns></returns>
        public string Write(Feature feature)
        {
            var g = new GeoJsonSerializer();
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
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
            var g = new GeoJsonSerializer();
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
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
            var g = new GeoJsonSerializer();
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
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
        /// <typeparam name="TGeometry">The type of the geometry.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public TGeometry Read<TGeometry> (string json)
            where TGeometry : class, IGeometry
        {
            var g = new GeoJsonSerializer();
            using (var sr = new StringReader(json))
            {
                return g.Deserialize<TGeometry>(new JsonTextReader(sr));
            }
        }

        /// <summary>
        /// Reads the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TGeometry")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "json")]
        public IGeometry Read(string json)
        {
            throw new NotSupportedException("You must call Read<TGeometry>(string json)");
        }
    }
}