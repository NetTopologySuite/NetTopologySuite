using System;
using System.Collections.Generic;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TopoFeaturesCollConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null)
                throw new ArgumentNullException("value");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (!(value is IEnumerable<IFeature>))
            {
                string s = String.Format("IEnumerable<IFeature> expected but was {0}", value.GetType().Name);
                throw new ArgumentException(s);
            }

            IEnumerable<IFeature> features = (IEnumerable<IFeature>)value;
            foreach (IFeature feature in features)
                serializer.Serialize(writer, feature);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("used only in serialization");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<IFeature>).IsAssignableFrom(objectType);
        }
    }
}