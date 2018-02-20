using System;
using System.Collections;
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
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            GeoJsonObjectType geomType = ToGeoJsonObject(geom);
            writer.WritePropertyName("type");
            writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), geomType));

            switch (geomType)
            {
                case GeoJsonObjectType.Point:
                    if (serializer.NullValueHandling == NullValueHandling.Include || geom.Coordinate != null)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, geom.Coordinate);
                    }
                    break;
                case GeoJsonObjectType.LineString:
                case GeoJsonObjectType.MultiPoint:
                    var linealCoords = geom.Coordinates;
                    if (serializer.NullValueHandling == NullValueHandling.Include || linealCoords != null)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, linealCoords);
                    }
                    break;
                case GeoJsonObjectType.Polygon:
                    IPolygon poly = geom as IPolygon;
                    Debug.Assert(poly != null);
                    var polygonCoords = PolygonCoordinates(poly);
                    if (serializer.NullValueHandling == NullValueHandling.Include || polygonCoords != null)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, polygonCoords);
                    }
                    break;

                case GeoJsonObjectType.MultiPolygon:
                    IMultiPolygon mpoly = geom as IMultiPolygon;
                    Debug.Assert(mpoly != null);
                    var list = new List<List<Coordinate[]>>();
                    foreach (IPolygon mempoly in mpoly.Geometries)
                        list.Add(PolygonCoordinates(mempoly));
                    if (serializer.NullValueHandling == NullValueHandling.Include || list.Count > 0)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, list);
                    }
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
                    if (serializer.NullValueHandling == NullValueHandling.Include || coordinates.Count > 0)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, coordinates);
                    }
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

        private static GeoJsonObjectType GetType(JsonReader reader)
        {
            var res = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), (string)reader.Value, true);
            reader.Read();
            return res;
        }

        private static List<object> ReadCoordinates(JsonReader reader)
        {
            List<object> coords = new List<object>();
            var startArray = reader.TokenType == JsonToken.StartArray;
            reader.Read();

            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    coords.Add(ReadCoordinates(reader));
                }
                else if (reader.Value != null)
                {
                    coords.Add(reader.Value);
                    reader.Read();
                }
            }

            if (startArray)
            {
                Debug.Assert(reader.TokenType == JsonToken.EndArray);
                reader.Read();
            }

            return coords;
        }

        private List<object> ParseGeomCollection(JsonReader reader, JsonSerializer serializer)
        {
            List<object> geometries = new List<object>();
            while (reader.Read())
            {
                // Exit if we are at the end
                if (reader.TokenType == JsonToken.EndArray)
                {
                    reader.Read();
                    break;
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    geometries.Add(ParseGeometry(reader, serializer));
                }
            }
            return geometries;
        }

        private static Coordinate GetPointCoordinate(IList list)
        {
            Coordinate c = new Coordinate();
            c.X = Convert.ToDouble(list[0]);
            c.Y = Convert.ToDouble(list[1]);
            if (list.Count > 2)
                c.Z = Convert.ToDouble(list[2]);
            return c;
        }

        private static Coordinate[] GetLineStringCoordinates(IEnumerable list)
        {
            List<Coordinate> coordinates = new List<Coordinate>();
            foreach (List<object> coord in list)
            {
                coordinates.Add(GetPointCoordinate(coord));
            }
            return coordinates.ToArray();
        }

        private static List<Coordinate[]> GetPolygonCoordinates(IEnumerable list)
        {
            List<Coordinate[]> coordinates = new List<Coordinate[]>();
            foreach (List<object> coord in list)
            {
                coordinates.Add(GetLineStringCoordinates(coord));
            }
            return coordinates;
        }

        private static IEnumerable<List<Coordinate[]>> GetMultiPolygonCoordinates(IEnumerable list)
        {
            List<List<Coordinate[]>> coordinates = new List<List<Coordinate[]>>();
            foreach (List<object> coord in list)
            {
                coordinates.Add(GetPolygonCoordinates(coord));
            }
            return coordinates;
        }

        private static IGeometry[] GetGeometries(IEnumerable list)
        {
            List<IGeometry> geometries = new List<IGeometry>();
            foreach (IGeometry geom in list)
            {
                geometries.Add(geom);
            }
            return geometries.ToArray();
        }

        private IGeometry ParseGeometry(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonReaderException("Expected Start object '{' Token");

            // advance
            var read = reader.Read();

            GeoJsonObjectType? geometryType = null;
            List<object> coords = null;
            while (reader.TokenType == JsonToken.PropertyName)
            {
                //read the tokens, type may come before coordinates or geometries as pr spec
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    switch (prop)
                    {
                        case "type":
                            if (geometryType == null)
                            {
                                reader.Read();
                                geometryType = GetType(reader);
                            }
                            break;
                        case "geometries":
                            //only geom collection has "geometries"
                            reader.Read();  //read past start array tag                        
                            coords = ParseGeomCollection(reader, serializer);
                            break;
                        case "coordinates":
                            reader.Read(); //read past start array tag
                            coords = ReadCoordinates(reader);
                            break;
                        case "bbox":
                            // Read, but can't do anything with it, assigning Envelopes is impossible without reflection
                            var bbox = serializer.Deserialize<Envelope>(reader);
                            break;

                        default:
                            reader.Read();
                            var item = serializer.Deserialize(reader);
                            reader.Read();
                            break;

                    }
                }
            }

            if (read && reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");

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
                    List<ILineString> strings = new List<ILineString>();
                    foreach (Coordinate[] multiLineStringCoordinate in GetPolygonCoordinates(coords))
                    {
                        strings.Add(_factory.CreateLineString(multiLineStringCoordinate));
                    }
                    return _factory.CreateMultiLineString(strings.ToArray());
                case GeoJsonObjectType.MultiPolygon:
                    List<IPolygon> polygons = new List<IPolygon>();
                    foreach (List<Coordinate[]> multiPolygonCoordinate in GetMultiPolygonCoordinates(coords))
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
            return ParseGeometry(reader, serializer);
        }

        private static List<Coordinate[]> PolygonCoordinates(IPolygon polygon)
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
}