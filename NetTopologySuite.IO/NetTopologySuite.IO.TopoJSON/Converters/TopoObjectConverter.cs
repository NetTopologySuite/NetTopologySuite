using System;
using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Builders;
using NetTopologySuite.IO.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    public class TopoObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected StartObject but was " + reader.TokenType);

            long id = 0;
            string type = null;
            IAttributesTable properties = null;
            double[][] coordinates = null;
            int[][][] arcs = null;
            TopoObject[] geometries = null;

            reader.Read();
            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new ArgumentException("Expected PropertyName but was " + reader.TokenType);
                string propertyName = (string)reader.Value;
                switch (propertyName)
                {
                    case "id":
                        id = ReadId(reader);
                        break;
                    case "type":
                        type = ReadType(reader);
                        break;
                    case "properties":
                        properties = serializer.Deserialize<IAttributesTable>(reader);
                        break;
                    case "coordinates":
                        coordinates = ReadPoints(reader, serializer);
                        break;
                    case "arcs":
                        arcs = ReadArcs(reader, serializer);
                        break;
                    case "geometries":
                        geometries = ReadGeometries(reader, serializer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("unhandled property: " + propertyName);
                }
            }

            reader.Read();
            TopoBuilder builder = new TopoBuilder(type,
                id,
                properties,
                coordinates,
                arcs,
                geometries);
            return builder.Build();
        }

        private static long ReadId(JsonReader reader)
        {
            reader.Read();
            long type = (long)reader.Value;
            reader.Read();
            return type;
        }

        private static string ReadType(JsonReader reader)
        {
            reader.Read();
            string type = (string)reader.Value;
            reader.Read();
            return type;
        }

        /// <summary>
        /// actually can be:
        /// * a double[] => Point
        /// * a double[][] => MultiPoint
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>      
        private double[][] ReadPoints(JsonReader reader, JsonSerializer serializer)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Expected StartArray but was " + reader.TokenType);

            try
            {
                JArray s_points = serializer.Deserialize<JArray>(reader);
                if (s_points.Count == 0)
                    throw new ArgumentException("at least a point expected!");
                JToken first = s_points[0];
                double[][] d_points;
                if (first is JValue)
                {
                    // Point
                    d_points = new double[1][];
                    d_points[0] = new double[s_points.Count];
                    for (int i = 0; i < s_points.Count; i++)
                    {
                        double val = s_points[i].Value<double>();
                        d_points[0][i] = val;
                    }
                    return d_points;
                }

                // MultiPoint
                d_points = new double[s_points.Count][];
                for (int i = 0; i < s_points.Count; i++)
                {
                    JArray s_coord = (JArray)s_points[i];
                    double[] d_coord = new double[s_coord.Count];
                    for (int j = 0; j < s_coord.Count; j++)
                    {
                        double val = s_coord[j].Value<double>();
                        d_coord[j] = val;
                    }
                    d_points[i] = d_coord;
                }
                return d_points;
            }
            finally
            {
                if (reader.TokenType != JsonToken.EndArray)
                    throw new ArgumentException("Expected EndArray but was " + reader.TokenType);
                reader.Read();
            }
        }

        /// <summary>
        /// actually can be:
        /// * a int[] => LineString
        /// * a int[][] => MultiLineString/Polygon
        /// * a int[][][] => MultiPolygon
        /// </summary>
        private int[][][] ReadArcs(JsonReader reader, JsonSerializer serializer)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Expected StartArray but was " + reader.TokenType);

            try
            {
                JArray s_arcs = serializer.Deserialize<JArray>(reader);
                int[][][] d_arcs;
                if (s_arcs[0] is JValue)
                {
                    // LineString
                    d_arcs = new int[1][][];
                    int[][] d_arc = new int[1][];
                    d_arc[0] = ConvertArr(s_arcs);
                    d_arcs[0] = d_arc;
                    return d_arcs;
                }

                d_arcs = new int[s_arcs.Count][][];
                for (int i = 0; i < s_arcs.Count; i++)
                {
                    JArray s_arc = (JArray)s_arcs[i];
                    if (s_arc[0] is JArray)
                    {
                        // MultiPolygon                                              
                        int[][] d_arc = new int[s_arc.Count][];
                        for (int j = 0; j < s_arc.Count; j++)
                        {
                            JArray s_inner = (JArray)s_arc[j];
                            int[] d_inner = ConvertArr(s_inner);
                            d_arc[i] = d_inner;
                        }
                        d_arcs[i] = d_arc;
                    }
                    else
                    {
                        // LineString/Polygon                        
                        int[][] d_arc = new int[1][];
                        d_arc[0] = ConvertArr(s_arc); ;
                        d_arcs[i] = d_arc;
                    }
                }
                return d_arcs;
            }
            finally
            {
                if (reader.TokenType != JsonToken.EndArray)
                    throw new ArgumentException("Expected EndArray but was " + reader.TokenType);
                reader.Read();
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TopoObject).IsAssignableFrom(objectType);
        }

        private static TopoObject[] ReadGeometries(JsonReader reader, JsonSerializer serializer)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                throw new ArgumentException("Expected StartArray but was " + reader.TokenType);
            reader.Read();
            List<TopoObject> list = new List<TopoObject>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                TopoObject item = serializer.Deserialize<TopoObject>(reader);
                list.Add(item);
            }
            reader.Read();
            return list.ToArray();
        }

        private static int[] ConvertArr(JArray arr)
        {
            int[] res = new int[arr.Count];
            for (int i = 0; i < arr.Count; i++)
                res[i] = arr[i].Value<int>();
            return res;
        }
    }
}