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