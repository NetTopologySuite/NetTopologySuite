using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    public class ArcsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {      
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Expected StartArray but was " + reader.TokenType);

            JArray s_arcs = serializer.Deserialize<JArray>(reader);
            double[][][] d_arcs = new double[s_arcs.Count][][];
            for (int i = 0; i < s_arcs.Count; i++)
            {
                JArray s_arc = (JArray)s_arcs[i];
                double[][] d_arc = new double[s_arc.Count][];
                for (int j = 0; j < s_arc.Count; j++)
                {
                    JArray s_coord = (JArray) s_arc[j];
                    double[] d_coord = new double[s_coord.Count];
                    for (int k = 0; k < s_coord.Count; k++)
                    {
                        double d = s_coord[k].Value<double>();
                        d_coord[k] = d;
                    }
                    d_arc[j] = d_coord;
                }
                d_arcs[i] = d_arc;
            }

            if (reader.TokenType != JsonToken.EndArray)
                throw new ArgumentException("Expected EndArray but was " + reader.TokenType);
            return d_arcs;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(double[][][]).IsAssignableFrom(objectType);
        }
    }
}
