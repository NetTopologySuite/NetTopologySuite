using System;
using System.Runtime.Remoting.Messaging;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TopoGeometryConverter : GeometryConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (value == null)
                throw new ArgumentNullException("value");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            if (!(value is IGeometry))
            {
                string s = String.Format("IGeometry expected but was {0}", value.GetType().Name);
                throw new ArgumentException(s);
            }

            IGeometry geom = (IGeometry) value;
            OgcGeometryType type = geom.OgcGeometryType;
            switch (type)
            {
                case OgcGeometryType.Point:
                    Serialize((IPoint) geom, writer, serializer);
                    break;
                case OgcGeometryType.LineString:
                    break;
                case OgcGeometryType.Polygon:
                    break;
                case OgcGeometryType.MultiPoint:
                    break;
                case OgcGeometryType.MultiLineString:
                    break;
                case OgcGeometryType.MultiPolygon:
                    break;
                case OgcGeometryType.GeometryCollection:
                    break;
                default:
                    string s = string.Format("type unsupported: {0}", type);
                    throw new ArgumentOutOfRangeException(s);
                    
            }
        }

        private void Serialize(IPoint point, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName("coordinates");
            writer.WriteStartArray();
            writer.WriteValue(point.X);
            writer.WriteValue(point.Y);
            if (!Double.IsNaN(point.Z))
                writer.WriteValue(point.Z);            
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("used only in serialization");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType);
        }        
    }
}