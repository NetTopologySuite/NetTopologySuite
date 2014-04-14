using System;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts IAttributesTable object to its JSON representation.
    /// </summary>
    public class AttributesTableConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            IAttributesTable attributes = value as IAttributesTable;
            if (attributes == null)
                return;

            writer.WritePropertyName("properties");

            writer.WriteStartObject();
            string[] names = attributes.GetNames();
            foreach (string name in names)
            {
                writer.WritePropertyName(name);
                object val = attributes[name];
                serializer.Serialize(writer, val);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "properties"))
                throw new ArgumentException("Expected token 'properties' not found.");
            reader.Read();
            object attributesTable = InternalReadJson(reader, serializer);
            reader.Read();
            return attributesTable;
        }

        private static object InternalReadJson(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected token '{' not found.");
            reader.Read();
            AttributesTable attributesTable = new AttributesTable();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string attributeName = (string) reader.Value;
                reader.Read();
                object attributeValue;
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // inner object
                    attributeValue = InternalReadJson(reader, serializer);
                }
                else
                {
                    attributeValue = reader.Value;
                }
                reader.Read();
                attributesTable.AddAttribute(attributeName, attributeValue);
            }            
            return attributesTable;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IAttributesTable).IsAssignableFrom(objectType);
        }
    }
}