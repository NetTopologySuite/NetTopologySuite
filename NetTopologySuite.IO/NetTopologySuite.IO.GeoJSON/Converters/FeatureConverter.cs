﻿using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts Feature object to its JSON representation.
    /// </summary>
    public class FeatureConverter : JsonConverter
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
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            IFeature feature = value as Feature;
            if (feature == null)
                return;

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("Feature");
            writer.WritePropertyName("geometry");
            serializer.Serialize(writer, feature.Geometry);
            serializer.Serialize(writer, feature.Attributes);
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
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "type"))
                throw new ArgumentException("Expected token 'type' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.String && (string)reader.Value != "Feature")
                throw new ArgumentException("Expected value 'Feature' not found.");
            reader.Read();

            object featureId = null;
            Feature feature = new Feature();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string prop = (string)reader.Value;
                switch (prop)
                {
                    case "id":                        
                        reader.Read(); 
                        featureId = reader.Value;
                        reader.Read(); 
                        break;                        
                    case "bbox":
                        // Read, but can't do anything with it, assigning Envelopes is impossible without reflection
                        var bbox = serializer.Deserialize<Envelope>(reader);
                        //Debug.WriteLine("BBOX: {0}", bbox.ToString());
                        break;
                    case "geometry":
                        reader.Read();
                        if (reader.TokenType == JsonToken.Null)
                        {
                            reader.Read();
                            break;
                        }
                            
                        if (reader.TokenType != JsonToken.StartObject)
                            throw new ArgumentException("Expected token '{' not found.");
                        var geometry = serializer.Deserialize<IGeometry>(reader);
                        feature.Geometry = geometry;
                        if (reader.TokenType != JsonToken.EndObject)
                            throw new ArgumentException("Expected token '}' not found.");
                        reader.Read();
                        break;
                    case "properties":
                        feature.Attributes = serializer.Deserialize<AttributesTable>(reader);
                        break;
                    default:
                    {                        
                        reader.Read(); // move next                        
                        // jump to next property
                        while (reader.TokenType != JsonToken.PropertyName)
                            reader.Read();                         
                        break;
                        //string err = String.Format("token unhandled: {0}.", prop);
                        //throw new ArgumentException(err);
                    }
                }
            }

            if (reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");
            reader.Read(); // move next

            IAttributesTable attributes = feature.Attributes;
            if (attributes != null)
            {
                if (featureId != null && !attributes.Exists("id"))
                    attributes.AddAttribute("id", featureId);
            }
            return feature;
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
            return typeof(Feature).IsAssignableFrom(objectType);
        }
    }
}