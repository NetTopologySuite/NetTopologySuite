using System;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts FeatureCollection object to its JSON representation.
    /// </summary>
    public class FeatureCollectionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            FeatureCollection coll = value as FeatureCollection;
            if (coll == null)
                return;

            writer.WriteStartObject();
            writer.WritePropertyName("features");
            serializer.Serialize(writer, coll.Features);
            writer.WritePropertyName("type");
            writer.WriteValue(coll.Type);
            if (coll.CRS != null)
            {
                writer.WritePropertyName("crs");
                serializer.Serialize(writer, coll.CRS);
            }            
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string) reader.Value == "features"))
                throw new ArgumentException("Expected token 'features' not found.");            
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Expected token '[' not found.");
            FeatureCollection featureCollection = new FeatureCollection();
            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
                featureCollection.Add(serializer.Deserialize<Feature>(reader));
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string) reader.Value == "type"))
                throw new ArgumentException("Expected token 'type' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.String && (string) reader.Value != "FeatureCollection")
                throw new ArgumentException("Expected value 'FeatureCollection' not found.");
            reader.Read();
            if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "crs")
            {
                reader.Read();
                featureCollection.CRS = serializer.Deserialize<ICRSObject>(reader);
            }
            if (reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");
            reader.Read();
            return featureCollection;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(FeatureCollection).IsAssignableFrom(objectType);
        }
    }
}