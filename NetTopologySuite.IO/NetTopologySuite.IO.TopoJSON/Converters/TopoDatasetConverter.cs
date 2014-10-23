using System;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TopoDatasetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null) 
                throw new ArgumentNullException("value");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (!(value is  FeatureCollection))
            {
                string s = String.Format("FeatureCollection expected but was {0}", value.GetType().Name);
                throw new ArgumentException(s);
            }

            FeatureCollection coll = (FeatureCollection) value;            
            serializer.Serialize(writer, new
            {
                type = "Topology",
                objects = new
                {
                    data = coll.Features
                }
            });
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("used only in serialization");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(FeatureCollection).IsAssignableFrom(objectType);
        }
    }
}