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
            FeatureCollection fc = new FeatureCollection();
            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new ArgumentException("Expected a property name.");                    
                string val = (string)reader.Value;
                if (val == "features")
                {
                    reader.Read();
                    if (reader.TokenType != JsonToken.StartArray)
                        throw new ArgumentException("Expected token '[' not found.");

                    reader.Read();
                    while (reader.TokenType != JsonToken.EndArray)
                        fc.Add(serializer.Deserialize<Feature>(reader));
                    reader.Read();
                    continue;
                }
                if (val == "type")
                {
                    reader.Read();
                    if (reader.TokenType != JsonToken.String && (string) reader.Value != "FeatureCollection")
                        throw new ArgumentException("Expected value 'FeatureCollection' not found.");
                    reader.Read();
                    continue;
                }
                if (val == "crs")
                {
                    reader.Read();
                    fc.CRS = serializer.Deserialize<ICRSObject>(reader);                    
                    continue;    
                }

                // additional members are ignored: see https://code.google.com/p/nettopologysuite/issues/detail?id=186
                reader.Read(); // read property value
                reader.Read(); // move next                
            }
            return fc;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(FeatureCollection).IsAssignableFrom(objectType);
        }
    }
}