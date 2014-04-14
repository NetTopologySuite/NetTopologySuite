using System;
using NetTopologySuite.IO.Helpers;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TransformConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected StartObject but was " + reader.TokenType);

            double[] scale = null;
            double[] translate = null;

            // read scale OR translate property
            reader.Read();
            if (reader.TokenType != JsonToken.PropertyName)
                throw new ArgumentException("Expected PropertyName but was " + reader.TokenType);            
            string propertyName = (string)reader.Value;
            reader.Read();            
            double[] arr = serializer.Deserialize<double[]>(reader);
            if (String.Equals(propertyName, "scale"))
                scale = arr;
            else if (String.Equals(propertyName, "translate"))
                translate = arr;
            else throw new ArgumentException("unhandled property: " + propertyName);

            // read scale OR translate property, again
            reader.Read();
            if (reader.TokenType != JsonToken.PropertyName)
                throw new ArgumentException("Expected PropertyName but was " + reader.TokenType);
            propertyName = (string)reader.Value;
            reader.Read();
            arr = serializer.Deserialize<double[]>(reader);
            if (String.Equals(propertyName, "scale"))
                scale = arr;
            else if (String.Equals(propertyName, "translate"))
                translate = arr;
            else throw new ArgumentException("unhandled property: " + propertyName);

            reader.Read();
            return new Transform(scale, translate);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ITransform).IsAssignableFrom(objectType);
        }
    }
}
