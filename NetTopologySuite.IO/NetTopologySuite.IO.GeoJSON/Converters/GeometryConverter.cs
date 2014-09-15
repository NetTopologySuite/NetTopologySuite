using System;
using System.Collections.Generic;
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
                    {
                        serializer.Serialize(writer, geom.Coordinate);
                        break;
                    }

                case GeoJsonObjectType.LineString:
                case GeoJsonObjectType.MultiPoint:
                    {
                        serializer.Serialize(writer, geom.Coordinates);
                        break;
                    }

                case GeoJsonObjectType.Polygon:
                    {
                        IPolygon poly = (IPolygon)geom;
                        List<Coordinate[]> coords = PolygonCoords(poly);
                        serializer.Serialize(writer, coords);
                        break;
                    }

                case GeoJsonObjectType.MultiPolygon:
                    {
                        IMultiPolygon mpoly = (IMultiPolygon)geom;
                        List<List<Coordinate[]>> list = new List<List<Coordinate[]>>();
                        foreach (IPolygon poly in mpoly.Geometries)
                        {
                            List<Coordinate[]> coords = PolygonCoords(poly);
                            list.Add(coords);
                        }
                        serializer.Serialize(writer, list);
                        break;
                    }

                case GeoJsonObjectType.GeometryCollection:
                    {
                        IGeometryCollection coll = (IGeometryCollection)geom;
                        serializer.Serialize(writer, coll.Geometries);
                        break;
                    }
                default:
                    {
                        List<Coordinate[]> list = new List<Coordinate[]>();
                        IGeometryCollection coll = (IGeometryCollection)geom;
                        foreach (IGeometry geometry in coll.Geometries)
                        {
                            Coordinate[] coords = geometry.Coordinates;
                            list.Add(coords);
                        }
                        serializer.Serialize(writer, list);
                        break;
                    }
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

        private static List<object> ReadCoordinates(JsonReader reader)
        {
            List<object> coords = new List<object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    List<object> list = ReadCoordinates(reader);
                    coords.Add(list);
                }
                else if (reader.TokenType == JsonToken.EndArray)
                    break;

                object c = reader.Value;
                if (c != null)
                    coords.Add(c);
            }
            return coords;
        }

        private List<IGeometry> ParseGeomCollection(JsonReader reader)
        {
            List<IGeometry> list = new List<IGeometry>();
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.StartObject)
                    continue;

                IGeometry g = ParseGeometry(reader);
                list.Add(g);
            }
            return list;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ParseGeometry(reader);
        }

        public IGeometry ParseGeometry(JsonReader reader)
        {
            GeoJsonObjectType? geometryType = null;
            List<object> coords = null;
            List<IGeometry> geoms = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    // we are at the end of the geometry block, do not read further
                    break;
                }

                // read the tokens, type may come before coordinates or geometries as pr spec
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if ((string)reader.Value == "type" && geometryType == null)
                    {
                        // get the type name
                        reader.Read();
                        geometryType = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), (string)reader.Value, true);
                    }
                    else if ((String)reader.Value == "geometries")
                    {
                        // only geom collection has "geometries"
                        reader.Read();  //read past start array tag                        
                        geoms = ParseGeomCollection(reader);
                    }
                    else if ((String)reader.Value == "coordinates")
                    {
                        reader.Read(); // read past start array tag
                        coords = ReadCoordinates(reader);
                    }
                }
            }

            // check data readed correctly
            if (geometryType == null)
                return null;
            switch (geometryType)
            {
                case GeoJsonObjectType.GeometryCollection:
                    if (geoms == null)
                        return null;
                    break;

                default:
                    if (coords == null)
                        return null;
                    break;
            }

            // build geom
            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                {
                    Coordinate coordinate = PointCoords(coords);
                    return _factory.CreatePoint(coordinate);
                }

                case GeoJsonObjectType.LineString:
                {
                    Coordinate[] coordinates = LineStringCoords(coords);
                    return _factory.CreateLineString(coordinates);
                }

                case GeoJsonObjectType.Polygon:
                {
                    List<Coordinate[]> coordinates = PolygonCoords(coords);
                    return CreatePolygon(coordinates);
                }

                case GeoJsonObjectType.MultiPoint:
                {
                    Coordinate[] coordinates = LineStringCoords(coords);
                    return _factory.CreateMultiPoint(coordinates);
                }

                case GeoJsonObjectType.MultiLineString:
                {
                    List<ILineString> list = new List<ILineString>();
                    foreach (Coordinate[] coordinates in PolygonCoords(coords))
                    {
                        ILineString lineString = _factory.CreateLineString(coordinates);
                        list.Add(lineString);
                    }
                    return _factory.CreateMultiLineString(list.ToArray());
                }

                case GeoJsonObjectType.MultiPolygon:
                {
                    List<IPolygon> list = new List<IPolygon>();
                    foreach (List<Coordinate[]> coordinates in MultiPolygonCoords(coords))
                    {
                        IPolygon polygon = CreatePolygon(coordinates);
                        list.Add(polygon);
                    }
                    return _factory.CreateMultiPolygon(list.ToArray());
                    ;
                }

                case GeoJsonObjectType.GeometryCollection:
                {
// ReSharper disable once PossibleNullReferenceException
                    return _factory.CreateGeometryCollection(geoms.ToArray());
                }
            }
            return null;
        }

        private static Coordinate PointCoords(List<object> list)
        {
            Coordinate coordinate = new Coordinate();
            coordinate.X = (double)list[0];
            coordinate.Y = (double)list[1];
            return coordinate;
        }

        private static Coordinate[] LineStringCoords(IEnumerable<object> list)
        {
            List<Coordinate> coordinates = new List<Coordinate>();
            foreach (List<object> item in list)
            {
                Coordinate coordinate = PointCoords(item);
                coordinates.Add(coordinate);
            }
            return coordinates.ToArray();
        }

        private static List<Coordinate[]> PolygonCoords(IEnumerable<object> list)
        {
            List<Coordinate[]> coordinates = new List<Coordinate[]>();
            foreach (List<object> item in list)
            {
                Coordinate[] arr = LineStringCoords(item);
                coordinates.Add(arr);
            }
            return coordinates;
        }

        private static List<Coordinate[]> PolygonCoords(IPolygon polygon)
        {
            List<Coordinate[]> list = new List<Coordinate[]>();
            list.Add(polygon.Shell.Coordinates);
            foreach (ILineString hole in polygon.InteriorRings)
                list.Add(hole.Coordinates);
            return list;
        }

        private static IEnumerable<List<Coordinate[]>> MultiPolygonCoords(IEnumerable<object> list)
        {
            List<List<Coordinate[]>> coordinates = new List<List<Coordinate[]>>();
            foreach (List<object> item in list)
            {
                List<Coordinate[]> arrs = PolygonCoords(item);
                coordinates.Add(arrs);
            }
            return coordinates;
        }

        private IPolygon CreatePolygon(List<Coordinate[]> list)
        {
            ILinearRing shell = _factory.CreateLinearRing(list[0]);
            List<ILinearRing> holes = new List<ILinearRing>();
            for (int i = 1; i < list.Count; i++)
            {
                ILinearRing hole = _factory.CreateLinearRing(list[i]);
                holes.Add(hole);
            }
            return _factory.CreatePolygon(shell, holes.ToArray());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType);
        }
    }
}