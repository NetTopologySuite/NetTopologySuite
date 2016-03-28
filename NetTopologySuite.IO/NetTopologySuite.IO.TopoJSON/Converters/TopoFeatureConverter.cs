using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TopoFeatureConverter : CoordinateConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null)
                throw new ArgumentNullException("value");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (!(value is IFeature))
            {
                string s = String.Format("IFeature expected but was {0}", value.GetType().Name);
                throw new ArgumentException(s);
            }

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("Topology");

            writer.WritePropertyName("objects");
            writer.WriteStartObject();
            writer.WritePropertyName("data");
            IEnumerable<Coordinate[]> arcs = WriteFeature(writer, serializer, (IFeature)value);
            writer.WriteEndObject();

            WriteArcs(writer, serializer, arcs);
            writer.WriteEndObject();
        }

        private IEnumerable<Coordinate[]> WriteFeature(JsonWriter writer, JsonSerializer serializer, IFeature feature)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (feature == null)
                throw new ArgumentNullException("feature");

            Coordinate[][] arcs;
            writer.WriteStartObject();
            IGeometry geom = feature.Geometry;
            writer.WritePropertyName("type");
            writer.WriteValue(geom.GeometryType);
            OgcGeometryType type = geom.OgcGeometryType;
            switch (type)
            {
                case OgcGeometryType.Point:
                    WriteGeom(writer, serializer, (IPoint)geom);
                    arcs = null;
                    break;
                case OgcGeometryType.LineString:
                    arcs = WriteGeom(writer, (ILineString)geom);
                    break;
                case OgcGeometryType.Polygon:
                    arcs = WriteGeom(writer, (IPolygon)geom);
                    break;
                default:
                    string err = String.Format("type unsupported: {0}", type);
                    throw new ArgumentOutOfRangeException(err);
            }
            // properties
            writer.WritePropertyName("properties");
            serializer.Serialize(writer, feature.Attributes);

            writer.WriteEndObject();
            return arcs;
        }

        private void WriteGeom(JsonWriter writer, JsonSerializer serializer, IPoint pt)
        {
            if (pt == null)
                throw new ArgumentNullException("pt");

            writer.WritePropertyName("coordinates");
            WriteJsonCoordinate(writer, pt.Coordinate, serializer);
        }

        private Coordinate[][] WriteGeom(JsonWriter writer, ILineString ls)
        {
            if (ls == null)
                throw new ArgumentNullException("ls");

            writer.WritePropertyName("arcs");
            writer.WriteStartArray();
            writer.WriteValue(0);
            writer.WriteEndArray();
            return new[] { ls.Coordinates };
        }

        private Coordinate[][] WriteGeom(JsonWriter writer, IPolygon poly)
        {
            if (poly == null)
                throw new ArgumentNullException("poly");

            int numHoles = poly.NumInteriorRings;
            Coordinate[][] arr = new Coordinate[numHoles + 1][];
            writer.WritePropertyName("arcs");
            writer.WriteStartArray();
            writer.WriteStartArray();
            // shell            
            writer.WriteValue(0);
            arr[0] = poly.Shell.Coordinates;
            // holes
            for (int i = 0; i < numHoles; i++)
            {
                writer.WriteValue(i + 1);
                arr[i + 1] = poly.GetInteriorRingN(i).Coordinates;
            }
            writer.WriteEndArray();
            writer.WriteEndArray();
            return arr;
        }

        private void WriteArcs(JsonWriter writer, JsonSerializer serializer, IEnumerable<Coordinate[]> arcs)
        {
            if (arcs == null)
                return;

            writer.WritePropertyName("arcs");
            WriteJsonCoordinatesEnumerable(writer, arcs, serializer);            
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("used only in serialization");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IFeature).IsAssignableFrom(objectType);
        }
    }
}