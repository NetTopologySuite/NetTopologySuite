using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Geometries;
using NetTopologySuite.IO.Helpers;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class DataConverter : JsonConverter
    {
        private readonly IGeometryFactory _factory;

        public DataConverter(IGeometryFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _factory = factory;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IGeometry geom = value as IGeometry;
            if (geom == null)
                return;

            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.PropertyName || (string)reader.Value != "type")
                throw new ArgumentException("Expected property 'type' not found");
            reader.Read();
            if (reader.TokenType != JsonToken.String && (string)reader.Value != "Topology")
                throw new ArgumentException("Expected value 'Topology' not found");

            IDictionary<string, TopoObject> dict = null;
            ITransform transform = null;
            double[][][] arcs = null;
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new ArgumentException("Expected a property but found " + reader.TokenType);

                string propertyName = (string)reader.Value;
                switch (propertyName)
                {
                    case "objects":
                        reader.Read(); // start object
                        dict = serializer.Deserialize<IDictionary<string, TopoObject>>(reader);
                        break;

                    case "transform":
                        reader.Read(); // start object
                        transform = serializer.Deserialize<ITransform>(reader);
                        break;

                    case "arcs":
                        reader.Read(); // start array
                        arcs = serializer.Deserialize<double[][][]>(reader);
                        reader.Read(); // end array
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("unhandled property: " + propertyName);
                }
            }

            // nothing to do
            if (dict == null)
                return null;

            if (arcs == null)
                throw new ArgumentException("arcs should be not null");
            transform = transform ?? new Transform(); // use a "null" transform as default
            ITransformer transformer = new Transformer(transform, arcs, _factory);

            IDictionary<string, FeatureCollection> result = new Dictionary<string, FeatureCollection>();
            foreach (string key in dict.Keys)
            {
                FeatureCollection coll = transformer.Create(dict[key]);
                result.Add(key, coll);
            }
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, FeatureCollection>).IsAssignableFrom(objectType);
        }
    }
}
