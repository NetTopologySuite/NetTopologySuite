using System;
using GeoAPI.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    public class TopoGeometryConverter : JsonConverter
    {
        private readonly IGeometryFactory factory;

        public TopoGeometryConverter(IGeometryFactory factory)
        {
            if (factory == null) 
                throw new ArgumentNullException("factory");

            this.factory = factory;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IGeometry geom = value as IGeometry;
            if (geom == null)
                return;

            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType);
        }
    }
}
