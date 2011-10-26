using System;
using GeoAPI.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    public class EnvelopeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var envelope = value as Envelope;
            if (envelope == null) return;

            writer.WritePropertyName("bbox");
            writer.WriteStartArray();
            writer.WriteValue(envelope.MinX);
            writer.WriteValue(envelope.MinY);
            writer.WriteValue(envelope.MaxX);
            writer.WriteValue(envelope.MaxY);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            System.Diagnostics.Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            System.Diagnostics.Debug.Assert((string)reader.Value == "bbox");

            var envelope = serializer.Deserialize<JArray>(reader);
            System.Diagnostics.Debug.Assert(envelope.Count == 4);

            var minX = Double.Parse((string)envelope[0]);
            var minY = Double.Parse((string)envelope[1]);
            var maxX = Double.Parse((string)envelope[2]);
            var maxY = Double.Parse((string)envelope[3]);

            System.Diagnostics.Debug.Assert(minX <= maxX);
            System.Diagnostics.Debug.Assert(minY <= maxY);

            return new Envelope(minX, minY, maxX, maxY);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Envelope);
        }
    }
}