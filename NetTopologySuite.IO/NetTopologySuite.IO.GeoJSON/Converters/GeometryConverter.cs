using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    public class GeometryConverter : JsonConverter
    {
        private readonly IGeometryFactory _factory;

        public GeometryConverter() : this(GeometryFactory.Default) { }

        public GeometryConverter(IGeometryFactory geometryFactory)
        {
            _factory = geometryFactory;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IGeometry geom = value as IGeometry;
            if (geom == null)
                return;

            writer.WriteStartObject();

            GeoJsonObjectType geomType = ToGeoJsonObject(geom);
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
                    IPolygon poly = geom as IPolygon;
                    Debug.Assert(poly != null);
                    serializer.Serialize(writer, PolygonCoordiantes(poly));
                    break;

                case GeoJsonObjectType.MultiPolygon:
                    IMultiPolygon mpoly = geom as IMultiPolygon;
                    Debug.Assert(mpoly != null);
                    List<List<Coordinate[]>> list = new List<List<Coordinate[]>>();
                    foreach (IPolygon mempoly in mpoly.Geometries)
                        list.Add(PolygonCoordiantes(mempoly));
                    serializer.Serialize(writer, list);
                    break;

                case GeoJsonObjectType.GeometryCollection:
                    IGeometryCollection gc = geom as IGeometryCollection;
                    Debug.Assert(gc != null);
                    serializer.Serialize(writer, gc.Geometries);
                    break;
                default:
                    List<Coordinate[]> coordinates = new List<Coordinate[]>();
                    foreach (IGeometry geometry in ((IGeometryCollection)geom).Geometries)
                        coordinates.Add(geometry.Coordinates);
                    serializer.Serialize(writer, coordinates);
                    break;
            }

            writer.WriteEndObject();
        }



        private GeoJsonObjectType ToGeoJsonObject(IGeometry geom)
        {
            if (geom     is IPoint)
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

        private static GeoJsonObjectType GetType(JsonReader reader)
        {
            
            return (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), (string)reader.Value, true);
        }

        private static ArrayList ReadCoordinates(JsonReader reader)
        {
            var coords = new ArrayList();
            while (reader.Read())
            {               
                if (reader.TokenType == JsonToken.StartArray)
                {
                    coords.Add(ReadCoordinates(reader));
                }
                else if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }                
                if (reader.Value != null) {
                    coords.Add(reader.Value);
                }
            }
            return coords;
        }

        private ArrayList ParseGeomCollection(JsonReader reader)
        {
            var geometries = new ArrayList();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    geometries.Add(ParseGeometry(reader));
                }
            }           
            return geometries;
        }

        private static Coordinate GetPointCoordinate(IList list)
        {
            var c = new Coordinate();
            c.X = Convert.ToDouble(list[0]);
            c.Y = Convert.ToDouble(list[1]); ;
            return c;
        }

        private static Coordinate[] GetLineStringCoordinates(IEnumerable list)
        {
            var coordinates = new List<Coordinate>();
            foreach (ArrayList coord in list)
            {
                coordinates.Add(GetPointCoordinate(coord));
            }
            return coordinates.ToArray();
        }

        private static List<Coordinate[]> GetPolygonCoordinates(IEnumerable list)
        {
            var coordinates = new List<Coordinate[]>();
            foreach (ArrayList coord in list)
            {
                coordinates.Add(GetLineStringCoordinates(coord));
            }
            return coordinates;
        }

        private static IEnumerable<List<Coordinate[]>> GetMultiPolygonCoordinates(IEnumerable list)
        {
            var coordinates = new List<List<Coordinate[]>>();
            foreach (ArrayList coord in list)
            {
                coordinates.Add(GetPolygonCoordinates(coord));
            }
            return coordinates;
        }

        private static IGeometry[] GetGeometries(IEnumerable list)
        {           
            var geometries = new List<IGeometry>();
            foreach (IGeometry geom in list)
            {
                geometries.Add(geom);
            }
            return geometries.ToArray();
        }


        private IGeometry ParseGeometry(JsonReader reader)
        {            
            GeoJsonObjectType? geometryType = null;
            ArrayList coords = null;
            while (reader.Read())
            {         
                if (reader.TokenType == JsonToken.EndObject)
                {
                    //we are at the end of the geometry block, do not read further
                    break;
                }

                //read the tokens, type may come before coordinates or geometries as pr spec
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if ((String)reader.Value == "type" && geometryType == null)
                    {
                        //get the type name
                        reader.Read();
                        geometryType = GetType(reader);
                    }
                    else if ((String)reader.Value == "geometries")
                    {                      
                        //only geom collection has "geometries"
                        reader.Read();  //read past start array tag                        
                        coords = ParseGeomCollection(reader);                     
                    }
                    else if ((String)reader.Value == "coordinates")
                    {
                        reader.Read(); //read past start array tag
                        coords = ReadCoordinates(reader);                        
                    }
                }
            }
            if (coords == null || geometryType == null)
            {
                return null;
            }
            
            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                    return _factory.CreatePoint(GetPointCoordinate(coords));
                case GeoJsonObjectType.LineString:
                    return _factory.CreateLineString(GetLineStringCoordinates(coords));
                case GeoJsonObjectType.Polygon:
                    return CreatePolygon(GetPolygonCoordinates(coords));
                case GeoJsonObjectType.MultiPoint:
                    return _factory.CreateMultiPoint(GetLineStringCoordinates(coords));
                case GeoJsonObjectType.MultiLineString:                    
                    var strings = new List<ILineString>();
                    foreach (var multiLineStringCoordinate in GetPolygonCoordinates(coords))
                    {
                        strings.Add(_factory.CreateLineString(multiLineStringCoordinate));
                    }
                    return _factory.CreateMultiLineString(strings.ToArray());
                case GeoJsonObjectType.MultiPolygon:                    
                    var polygons = new List<IPolygon>();
                    foreach (var multiPolygonCoordinate in GetMultiPolygonCoordinates(coords))
                    {
                        polygons.Add(CreatePolygon(multiPolygonCoordinate));
                    }
                    return _factory.CreateMultiPolygon(polygons.ToArray());
                case GeoJsonObjectType.GeometryCollection:
                    return _factory.CreateGeometryCollection(GetGeometries(coords));
            }
            return null;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ParseGeometry(reader);
        }

        private static List<Coordinate[]> PolygonCoordiantes(IPolygon polygon)
        {
            List<Coordinate[]> res = new List<Coordinate[]>();
            res.Add(polygon.Shell.Coordinates);
            foreach (ILineString interiorRing in polygon.InteriorRings)
                res.Add(interiorRing.Coordinates);
            return res;
        }

        private IPolygon CreatePolygon(IList<Coordinate[]> coordinatess)
        {
            ILinearRing shell = _factory.CreateLinearRing(coordinatess[0]);
            List<ILinearRing> rings = new List<ILinearRing>();
            for (int i = 1; i < coordinatess.Count; i++)
                rings.Add(_factory.CreateLinearRing(coordinatess[i]));
            return _factory.CreatePolygon(shell, rings.ToArray());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType);
        }
    }

    public class GeometryArrayConverter : JsonConverter
    {
        private readonly IGeometryFactory _factory;

        public GeometryArrayConverter() : this(GeometryFactory.Default) { }

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
            foreach (IGeometry geometry in geometries)
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
            List<IGeometry> geoms = new List<IGeometry>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                JObject obj = (JObject)serializer.Deserialize(reader);
                GeoJsonObjectType geometryType = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), obj.Value<string>("type"), true);

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
            ILineString[] strings = new ILineString[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
                strings[i] = _factory.CreateLineString(coordinates[i]);
            return _factory.CreateMultiLineString(strings);
        }

        private IPolygon CreatePolygon(List<Coordinate[]> coordinates)
        {
            ILinearRing shell = _factory.CreateLinearRing(coordinates[0]);
            ILinearRing[] rings = new ILinearRing[coordinates.Count - 1];
            for (int i = 1; i < coordinates.Count; i++)
                rings[i - 1] = _factory.CreateLinearRing(coordinates[i]);
            return _factory.CreatePolygon(shell, rings);
        }

        private IMultiPolygon CreateMultiPolygon(List<List<Coordinate[]>> coordinates)
        {
            IPolygon[] polygons = new IPolygon[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
                polygons[i] = CreatePolygon(coordinates[i]);
            return _factory.CreateMultiPolygon(polygons);
        }

        private static Coordinate ToCoordinate(JArray array)
        {
            Coordinate c = new Coordinate { X = (Double)array[0], Y = (Double)array[1] };
            if (array.Count > 2)
                c.Z = (Double)array[2];
            return c;
        }

        public static Coordinate[] ToCoordinates(JArray array)
        {
            Coordinate[] c = new Coordinate[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                c[i] = ToCoordinate((JArray)array[i]);
            }
            return c;
        }
        public static List<Coordinate[]> ToListOfCoordinates(JArray array)
        {
            List<Coordinate[]> c = new List<Coordinate[]>();
            for (int i = 0; i < array.Count; i++)
                c.Add(ToCoordinates((JArray)array[i]));
            return c;
        }
        public static List<List<Coordinate[]>> ToListOfListOfCoordinates(JArray array)
        {
            List<List<Coordinate[]>> c = new List<List<Coordinate[]>>();
            for (int i = 0; i < array.Count; i++)
                c.Add(ToListOfCoordinates((JArray)array[i]));
            return c;
        }
    }
}