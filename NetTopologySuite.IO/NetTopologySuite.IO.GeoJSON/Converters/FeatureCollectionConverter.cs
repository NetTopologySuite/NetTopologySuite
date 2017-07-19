using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
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
            {
                if (serializer.NullValueHandling == NullValueHandling.Ignore)
                {
                    writer.WriteToken(null);
                }
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(coll.Type);
            writer.WritePropertyName("features");
            serializer.Serialize(writer, coll.Features);
            if (serializer.NullValueHandling == NullValueHandling.Include || coll.CRS != null)
            {
                writer.WritePropertyName("crs");
                serializer.Serialize(writer, coll.CRS);
            }
            var bbox = coll.BoundingBox;
            if (serializer.NullValueHandling == NullValueHandling.Include || bbox != null)
            {
                writer.WritePropertyName("bbox");
                serializer.Serialize(writer, bbox, typeof(Envelope));
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            bool read = reader.Read();
            FeatureCollection fc = new FeatureCollection();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string val = (string)reader.Value;
                switch (val)
                {
                    case "features":
                        // move to begin of array
                        read = reader.Read();
                        if (reader.TokenType != JsonToken.StartArray)
                            throw new ArgumentException("Expected token '[' not found.");

                        // move to first feature
                        read = reader.Read();
                        while (read && reader.TokenType != JsonToken.EndArray)
                        {
                            fc.Add(serializer.Deserialize<Feature>(reader));
                            read = reader.Read();
                        }
                        read = reader.Read();
                        break;
                    case "type":
                        read = reader.Read();
                        if (reader.TokenType != JsonToken.String && (string)reader.Value != "FeatureCollection")
                            throw new ArgumentException("Expected value 'FeatureCollection' not found.");
                        read = reader.Read();
                        break;
                    case "bbox":
                        fc.BoundingBox = serializer.Deserialize<Envelope>(reader);
                        /*
                        read = reader.Read();
                        if (reader.TokenType != JsonToken.StartArray)
                            throw new ArgumentException("Expected token '{' not found.");

                        var env = serializer.Deserialize<double[]>(reader);
                        fc.BoundingBox = new Envelope(env[0], env[2], env[1], env[3]);

                        if (reader.TokenType != JsonToken.EndArray)
                            throw new ArgumentException("Expected token '}' not found.");

                        read = reader.Read();
                        */
                        break;
                    case "crs":
                        read = reader.Read();
                        fc.CRS = serializer.Deserialize<ICRSObject>(reader);
                        break;
                    default:
                        // additional members are ignored: see https://code.google.com/p/nettopologysuite/issues/detail?id=186
                        /*
                         * see also: http://gis.stackexchange.com/a/25309/463
                         * "you can have a properties element at the top level of a feature collection, 
                         * but don't expect any tools to know its there"
                         */
                        read = reader.Read(); // move next                        
                        // jump to next property
                        while (read && reader.TokenType != JsonToken.PropertyName)
                            read = reader.Read();
                        break;
                }
            }

            if (read && reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");

            return fc;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(FeatureCollection).IsAssignableFrom(objectType);
        }
    }
}