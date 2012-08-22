namespace NetTopologySuite.IO.Converters
{
    using System;

    using NetTopologySuite.CoordinateSystems;

    using Newtonsoft.Json;

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
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ICRSObject).IsAssignableFrom(objectType);
        }
    }
}