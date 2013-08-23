using System;
using System.Collections.Generic;
using NetTopologySuite.CoordinateSystems;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts ICRSObject object to its JSON representation.
    /// </summary>
    public class ICRSObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            ICRSObject crs = value as ICRSObject;
            if (crs == null)
                return;
                
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            string type = Enum.GetName(typeof(CRSTypes), crs.Type);        
            writer.WriteValue(type.ToLowerInvariant());
            CRSBase crsb = value as CRSBase;
            if (crsb != null)
            {
                writer.WritePropertyName("properties");
                serializer.Serialize(writer, crsb.Properties);
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected token '{' not found.");
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string) reader.Value == "type"))
                throw new ArgumentException("Expected token 'type' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.String)
                throw new ArgumentException("Expected string value not found.");
            string crsType = (string)reader.Value;
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string) reader.Value == "properties"))
                throw new ArgumentException("Expected token 'properties' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected token '{' not found.");
            Dictionary<string, object> dictionary = serializer.Deserialize<Dictionary<string, object>>(reader);
            CRSBase result = null;
            switch (crsType)
            {
                case "link":
                    object href = dictionary["href"];
                    object type = dictionary["type"];
                    result = new LinkedCRS((string)href, type != null ? (string)type : "");
                    break;
                case "name":
                    object name = dictionary["name"];
                    result = new NamedCRS((string)name);
                    break;
            }
            if (reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");
            reader.Read();
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ICRSObject).IsAssignableFrom(objectType);
        }
    }
}