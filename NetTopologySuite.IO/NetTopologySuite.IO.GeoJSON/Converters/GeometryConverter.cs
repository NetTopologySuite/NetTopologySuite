using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    public class GeometryConverter : JsonConverter
    {
        private IGeometryFactory _factory;
        
        public GeometryConverter()
            :this(GeometryFactory.Default)
        {
            
        }

        public GeometryConverter(IGeometryFactory geometryFactory)
        {
            _factory = geometryFactory;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geom = value as IGeometry;
            if (geom == null)
                return;

            writer.WriteStartObject();

            var geomType = ToGeoJsonObject(geom);
            writer.WritePropertyName("type");
            writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), geomType));
            
            switch (geomType)
            {
                case GeoJsonObjectType.Point:
                    serializer.Serialize(writer, geom.Coordinate);
                    break;
                case GeoJsonObjectType.LineString:
                case GeoJsonObjectType.MultiPoint:
                    serializer.Serialize(writer, geom.Coordinates);
                    break;
                case GeoJsonObjectType.Polygon:
                    var poly = geom as IPolygon;
                    Debug.Assert(poly != null);
                    serializer.Serialize(writer, PolygonCoordiantes(poly));
                    break;

                case GeoJsonObjectType.MultiPolygon:
                    var mpoly = geom as IMultiPolygon;
                    Debug.Assert(mpoly != null);
                    var list = new List<List<Coordinate[]>>();
                    foreach (IPolygon mempoly in mpoly.Geometries)
                        list.Add(PolygonCoordiantes(mempoly));
                    serializer.Serialize(writer, list);
                    break;

                case GeoJsonObjectType.GeometryCollection:
                    var gc = geom as IGeometryCollection;
                    Debug.Assert(gc != null);
                    serializer.Serialize(writer, gc.Geometries);
                    break;
                default:
                    var coordinates = new List<Coordinate[]>();
                    foreach (var geometry in ((IGeometryCollection)geom).Geometries)
                        coordinates.Add(geometry.Coordinates);
                    serializer.Serialize(writer, coordinates);
                    break;
            }

            writer.WriteEndObject();
        }



        private GeoJsonObjectType ToGeoJsonObject(IGeometry geom)
        {
            if (geom is IPoint)
                return GeoJsonObjectType.Point;
            if (geom is ILineString)
                return GeoJsonObjectType.LineString;
            if (geom is IPolygon)
                return GeoJsonObjectType.Polygon;
            if (geom is IMultiPoint)
                return GeoJsonObjectType.MultiPoint;
            if (geom is IMultiLineString)
                return GeoJsonObjectType.MultiLineString;
            if (geom is IMultiPolygon)
                return GeoJsonObjectType.MultiPolygon;
            if (geom is IGeometryCollection)
                return GeoJsonObjectType.GeometryCollection;

            throw new ArgumentException("geom");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "type"))
            {
                throw new Exception();
            }
            reader.Read();
            if (reader.TokenType != JsonToken.String)
            {
                throw new Exception();
            }

            var geometryType = (GeoJsonObjectType)Enum.Parse(typeof (GeoJsonObjectType), (string) reader.Value);
            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                    var coordinate = serializer.Deserialize<Coordinate>(reader);
                    return _factory.CreatePoint(coordinate);
                
                case GeoJsonObjectType.LineString:
                    var coordinates = serializer.Deserialize<Coordinate[]>(reader);
                    return _factory.CreateLineString(coordinates);
                
                case GeoJsonObjectType.Polygon:
                    var coordinatess = serializer.Deserialize<List<Coordinate[]>>(reader);
                    return CreatePolygon(coordinatess);

                case GeoJsonObjectType.MultiPoint:
                    coordinates = serializer.Deserialize<Coordinate[]>(reader);
                    return _factory.CreateMultiPoint(coordinates);

                case GeoJsonObjectType.MultiLineString:
                    coordinatess = serializer.Deserialize<List<Coordinate[]>>(reader);
                    var strings = new List<ILineString>();
                    for (var i = 0; i < coordinatess.Count; i++)
                        strings.Add(_factory.CreateLineString(coordinatess[i]));
                    return _factory.CreateMultiLineString(strings.ToArray());
                
                case GeoJsonObjectType.MultiPolygon:
                    var coordinatesss = serializer.Deserialize<List<List<Coordinate[]>>>(reader);
                    var polygons = new List<IPolygon>();
                    foreach (var coordinateses in coordinatesss)
                        polygons.Add(CreatePolygon(coordinateses));
                    return _factory.CreateMultiPolygon(polygons.ToArray());

                case GeoJsonObjectType.GeometryCollection:
                    var geoms = serializer.Deserialize<List<IGeometry>>(reader);
                    return _factory.CreateGeometryCollection(geoms.ToArray());
                    //ReadJson(reader,)
            }

            return null;
        }

        private static List<Coordinate[]> PolygonCoordiantes(IPolygon polygon)
        {
            var res = new List<Coordinate[]>();
            res.Add(polygon.Shell.Coordinates);
            foreach (ILineString interiorRing in polygon.InteriorRings)
                res.Add(interiorRing.Coordinates);
            return res;
        }

        private IPolygon CreatePolygon(IList<Coordinate[]> coordinatess)
        {
            var shell = _factory.CreateLinearRing(coordinatess[0]);
            var rings = new List<ILinearRing>();
            for (var i = 1; i < coordinatess.Count; i++)
                rings.Add(_factory.CreateLinearRing(coordinatess[i]));
            return _factory.CreatePolygon(shell, rings.ToArray());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType) && !objectType.IsAbstract;
        }
    }

    public class GeometryArrayConverter : JsonConverter
    {
        private readonly IGeometryFactory _factory;

        public GeometryArrayConverter()
            :this(GeometryFactory.Default)
        {
        }
        public GeometryArrayConverter(IGeometryFactory factory)
        {
            _factory = factory;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WritePropertyName("geometries");
            WriteGeometries(writer, value as IList<IGeometry>, serializer);
        }

        private static void WriteGeometries(JsonWriter writer, IEnumerable<IGeometry> geometries, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var geometry in geometries)
                serializer.Serialize(writer, geometry);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "geometries"))
            {
                throw new Exception();
            }
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                throw new Exception();

            reader.Read();
            var geoms = new List<IGeometry>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var obj = (JObject)serializer.Deserialize(reader);
                var geometryType = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), obj.Value<string>("type"));

                switch (geometryType)
                {
                    case GeoJsonObjectType.Point:
                        geoms.Add(_factory.CreatePoint(ToCoordinate(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.LineString:
                        geoms.Add(_factory.CreateLineString(ToCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.Polygon:
                        geoms.Add(CreatePolygon(ToListOfCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.MultiPoint:
                        geoms.Add(_factory.CreateMultiPoint(ToCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.MultiLineString:
                        geoms.Add(CreateMultiLineString(ToListOfCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.MultiPolygon:
                        geoms.Add(CreateMultiPolygon(ToListOfListOfCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.GeometryCollection:
                        throw new NotSupportedException();

                }
                reader.Read();
            }
            return geoms;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<IGeometry>).IsAssignableFrom(objectType);
        }

        private IMultiLineString CreateMultiLineString(List<Coordinate[]> coordinates)
        {
            var strings = new ILineString[coordinates.Count];
            for (var i = 0; i < coordinates.Count; i++)
                strings[i] = _factory.CreateLineString(coordinates[i]);
            return _factory.CreateMultiLineString(strings);
        }

        private IPolygon CreatePolygon(List<Coordinate[]> coordinates)
        {
            var shell = _factory.CreateLinearRing(coordinates[0]);
            var rings = new ILinearRing[coordinates.Count - 1];
            for (var i = 1; i < coordinates.Count; i++)
                rings[i - 1] = _factory.CreateLinearRing(coordinates[i]);
            return _factory.CreatePolygon(shell, rings);
        }

        private IMultiPolygon CreateMultiPolygon(List<List<Coordinate[]>> coordinates)
        {
            var polygons = new IPolygon[coordinates.Count];
            for (var i = 0; i < coordinates.Count; i++)
                polygons[i] = CreatePolygon(coordinates[i]);
            return _factory.CreateMultiPolygon(polygons);
        }
        
        private static Coordinate ToCoordinate(JArray array)
            {
                var c = new Coordinate {X = (Double) array[0], Y = (Double) array[1]};
                if (array.Count > 2)
                    c.Z = (Double)array[2];
                return c;
            }

            public static Coordinate[] ToCoordinates(JArray array)
            {
                var c = new Coordinate[array.Count];
                for (var i = 0; i < array.Count; i++)
                {
                    c[i] = ToCoordinate((JArray) array[i]);
                }
                return c;
            }
            public static List<Coordinate[]> ToListOfCoordinates(JArray array)
            {
                var c = new List<Coordinate[]>();
                for (var i = 0; i < array.Count; i++)
                    c.Add(ToCoordinates((JArray)array[i]));
                return c;
            }
            public static List<List<Coordinate[]>> ToListOfListOfCoordinates(JArray array)
            {
                var c = new List<List<Coordinate[]>>();
                for (var i = 0; i < array.Count; i++)
                    c.Add(ToListOfCoordinates((JArray)array[i]));
                return c;
            }
    }
}