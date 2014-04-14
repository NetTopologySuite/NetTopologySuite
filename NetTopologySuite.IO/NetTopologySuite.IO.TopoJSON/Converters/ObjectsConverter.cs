using System;
using System.Collections.Generic;
using NetTopologySuite.IO.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class ObjectsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected StartObject but was " + reader.TokenType);

            IDictionary<string, TopoObject> dict = new Dictionary<string, TopoObject>();
            do
            {
                reader.Read();
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new ArgumentException("Expected PropertyName but was " + reader.TokenType);
                string key = (string) reader.Value;
                reader.Read();
                TopoObject val = serializer.Deserialize<TopoObject>(reader);
                dict.Add(key, val);

            } 
            while (reader.TokenType != JsonToken.EndObject);
            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, TopoObject>).IsAssignableFrom(objectType);
        }
    }
}
