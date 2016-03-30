using System.Globalization;

namespace NetTopologySuite.IO.Converters
{
    using System;
    using System.Diagnostics;

    using GeoAPI.Geometries;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class EnvelopeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Envelope envelope = value as Envelope;
            if (envelope == null) return;

            writer.WritePropertyName("bbox");
            writer.WriteStartArray();
            writer.WriteValue(envelope.MinX.ToString(NumberFormatInfo.InvariantInfo));
            writer.WriteValue(envelope.MinY.ToString(NumberFormatInfo.InvariantInfo));
            writer.WriteValue(envelope.MaxX.ToString(NumberFormatInfo.InvariantInfo));
            writer.WriteValue(envelope.MaxY.ToString(NumberFormatInfo.InvariantInfo));
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            Debug.Assert((string)reader.Value == "bbox");
            reader.Read(); // move to array start

            JArray envelope = serializer.Deserialize<JArray>(reader);
            Debug.Assert(envelope.Count == 4);

            double minX = Double.Parse((string) envelope[0], NumberFormatInfo.InvariantInfo);
            double minY = Double.Parse((string) envelope[1], NumberFormatInfo.InvariantInfo);
            double maxX = Double.Parse((string) envelope[2], NumberFormatInfo.InvariantInfo);
            double maxY = Double.Parse((string) envelope[3], NumberFormatInfo.InvariantInfo);

            Debug.Assert(minX <= maxX);
            Debug.Assert(minY <= maxY);

            reader.Read(); // move away from array end
            return new Envelope(minX, maxX, minY, maxY);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Envelope);
        }
    }
}