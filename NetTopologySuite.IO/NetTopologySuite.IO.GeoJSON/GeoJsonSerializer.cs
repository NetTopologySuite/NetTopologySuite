using GeoAPI.Geometries;
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
        public new static JsonSerializer CreateDefault()
        {
            var s = JsonSerializer.CreateDefault();
            AddGeoJsonConverters(s, GeometryFactory.Default);
            return s;
        }

        public static JsonSerializer Create(IGeometryFactory factory)
        {
            return Create(new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}, factory);
        }

        public static JsonSerializer Create(JsonSerializerSettings settings, IGeometryFactory factory)
        {
            var s = JsonSerializer.Create(settings);
            AddGeoJsonConverters(s, factory);
            return s;
        }

        private static void AddGeoJsonConverters(JsonSerializer s, IGeometryFactory factory)
        {
            var c = s.Converters;
            c.Add(new ICRSObjectConverter());
            c.Add(new FeatureCollectionConverter());
            c.Add(new FeatureConverter());
            c.Add(new AttributesTableConverter());
            c.Add(new GeometryConverter(factory));
            c.Add(new GeometryArrayConverter());
            c.Add(new CoordinateConverter());
            c.Add(new EnvelopeConverter());

        }

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
}