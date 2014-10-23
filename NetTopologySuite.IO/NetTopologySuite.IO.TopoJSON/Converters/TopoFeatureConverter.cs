using System;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TopoFeatureConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null)
                throw new ArgumentNullException("value");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (!(value is IFeature))
            {
                string s = String.Format("IFeature expected but was {0}", value.GetType().Name);
                throw new ArgumentException(s);
            }

            IFeature feature = (IFeature)value;

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            IGeometry geometry = feature.Geometry;
            writer.WriteValue(geometry.GeometryType);
            serializer.Serialize(writer, geometry);
            IAttributesTable attributes = feature.Attributes;
            serializer.Serialize(writer, attributes);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("used only in serialization");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IFeature).IsAssignableFrom(objectType);
        }
    }
}