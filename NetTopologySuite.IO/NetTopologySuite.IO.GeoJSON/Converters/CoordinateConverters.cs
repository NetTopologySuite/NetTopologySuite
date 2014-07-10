namespace NetTopologySuite.IO.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using GeoAPI.Geometries;

    using Newtonsoft.Json;

    public class CoordinateConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WritePropertyName("coordinates");

            List<List<Coordinate[]>> coordinatesss = value as List<List<Coordinate[]>>;
            if (coordinatesss != null)
            {
                WriteJsonCoordinatesEnumerable2(writer, coordinatesss, serializer);
                return;
            }

            List<Coordinate[]> coordinatess = value as List<Coordinate[]>;
            if (coordinatess != null)
            {
                WriteJsonCoordinatesEnumerable(writer, coordinatess, serializer);
                return;
            }

            IEnumerable<Coordinate> coordinates = value as IEnumerable<Coordinate>;
            if (coordinates != null)
            {
                WriteJsonCoordinates(writer, coordinates, serializer);
                return;
            }

            Coordinate coordinate = value as Coordinate;
            if (coordinate != null)
            {
                WriteJsonCoordinate(writer, coordinate, serializer);
                return;
            }

        }

        private static void WriteJsonCoordinate(JsonWriter writer, Coordinate coordinate, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            writer.WriteValue(coordinate.X);
            writer.WriteValue(coordinate.Y);
            if (!Double.IsNaN(coordinate.Z))
                writer.WriteValue(coordinate.Z);
            
            writer.WriteEndArray();
        }

        private static void WriteJsonCoordinates(JsonWriter writer, IEnumerable<Coordinate> coordinates, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (Coordinate coordinate in coordinates)
                WriteJsonCoordinate(writer, coordinate, serializer);
            writer.WriteEndArray();
        }

        private static void WriteJsonCoordinatesEnumerable(JsonWriter writer, IEnumerable<Coordinate[]> coordinates, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (Coordinate[] coordinate in coordinates)
                WriteJsonCoordinates(writer, coordinate, serializer);
            writer.WriteEndArray();
        }
        private static void WriteJsonCoordinatesEnumerable2(JsonWriter writer, List<List<Coordinate[]>> coordinates, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (List<Coordinate[]> coordinate in coordinates)
                WriteJsonCoordinatesEnumerable(writer, coordinate, serializer);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            Debug.Assert((string)reader.Value == "coordinates");

            object result;
            if (objectType == typeof (Coordinate))
                result = ReadJsonCoordinate(reader);
            else if (typeof (IEnumerable<Coordinate>).IsAssignableFrom(objectType))
                result = ReadJsonCoordinates(reader);
            else if (typeof (List<Coordinate[]>).IsAssignableFrom(objectType))
                result = ReadJsonCoordinatesEnumerable(reader);
            else if (typeof (List<List<Coordinate[]>>).IsAssignableFrom(objectType))
                result = ReadJsonCoordinatesEnumerable2(reader);
            else throw new ArgumentException("unmanaged type: " + objectType);
            reader.Read();
            return result;

        }

        private static Coordinate ReadJsonCoordinate(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;

            Coordinate c = new Coordinate();

            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer);
            c.X = Convert.ToDouble(reader.Value);

            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer);
            c.Y = Convert.ToDouble(reader.Value);
            
            reader.Read();
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                c.Z = Convert.ToDouble(reader.Value);
                reader.Read();
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);

            return c;
        }

        private static Coordinate[] ReadJsonCoordinates(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;

            List<Coordinate> coordinates = new List<Coordinate>();
            while (true)
            {
                Coordinate c = ReadJsonCoordinate(reader);
                if (c == null) break;
                coordinates.Add(c);
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return coordinates.ToArray();
        }

        private static List<Coordinate[]> ReadJsonCoordinatesEnumerable(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;

            List<Coordinate[]> coordinates = new List<Coordinate[]>();
            while (true)
            {
                Coordinate[] res = ReadJsonCoordinates(reader);
                if (res == null)break;
                coordinates.Add(res);
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return coordinates;
        }

        private static List<List<Coordinate[]>> ReadJsonCoordinatesEnumerable2(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;
            List<List<Coordinate[]>> coordinates = new List<List<Coordinate[]>>();

            while (true)
            {
                List<Coordinate[]> res = ReadJsonCoordinatesEnumerable(reader);
                if (res == null) break;
                coordinates.Add(res);
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return coordinates;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Coordinate) || 
                   objectType == typeof(Coordinate[]) ||
                   objectType == typeof(List<Coordinate[]>) ||
                   objectType == typeof(List<List<Coordinate[]>>) ||
                   typeof(IEnumerable<Coordinate>).IsAssignableFrom(objectType) ||
                   typeof(IEnumerable<IEnumerable<Coordinate>>).IsAssignableFrom(objectType) ||
                   typeof(IEnumerable<IEnumerable<IEnumerable<Coordinate>>>).IsAssignableFrom(objectType);
        }
    }
}