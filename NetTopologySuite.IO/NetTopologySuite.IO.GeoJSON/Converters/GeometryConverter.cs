using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

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
                    serializer.Serialize(writer, PolygonCoordinates(poly));
                    break;

                case GeoJsonObjectType.MultiPolygon:
                    IMultiPolygon mpoly = geom as IMultiPolygon;
                    Debug.Assert(mpoly != null);
                    List<List<Coordinate[]>> list = new List<List<Coordinate[]>>();
                    foreach (IPolygon mempoly in mpoly.Geometries)
                        list.Add(PolygonCoordinates(mempoly));
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

            return (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), (string)reader.Value, true);
        }

        private static List<object> ReadCoordinates(JsonReader reader)
        {
            List<object> coords = new List<object>();
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
                if (reader.Value != null)
                {
                    coords.Add(reader.Value);
                }
            }
            return coords;
        }

        private List<object> ParseGeomCollection(JsonReader reader)
        {
            List<object> geometries = new List<object>();
            while (reader.Read())
            {
                // Exit if we are at the end
                if (reader.TokenType == JsonToken.EndArray)
                    break;

                if (reader.TokenType == JsonToken.StartObject)
                {
                    geometries.Add(ParseGeometry(reader));
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

        private IGeometry ParseGeometry(JsonReader reader)
        {
            GeoJsonObjectType? geometryType = null;
            List<object> coords = null;
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
            return ParseGeometry(reader);
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