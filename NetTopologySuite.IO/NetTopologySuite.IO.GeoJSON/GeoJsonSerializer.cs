using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class GeoJsonSerializer : JsonSerializer
    {
        public GeoJsonSerializer()
            :this(GeometryFactory.Default)
        {}
        
        public GeoJsonSerializer(IGeometryFactory geometryFactory)
        {
            base.Converters.Add(new GeometryConverter(geometryFactory));
            base.Converters.Add(new GeometryArrayConverter());
            base.Converters.Add(new CoordinateConverter());
            base.Converters.Add(new EnvelopeConverter());
        }
    }

    public class GeoJsonWriter
    {
        public string Write(IGeometry geometry)
        {
            var g = new GeoJsonSerializer(geometry.Factory);
            var sb = new StringBuilder();
            
            using (var sw = new StringWriter(sb))
                g.Serialize(sw, geometry);
            
            return sb.ToString();
        }
    }

    public class GeoJsonReader
    {
        public TGeometry Read<TGeometry> (string json)
            where TGeometry : class, IGeometry
        {
            var g = new GeoJsonSerializer();
            return g.Deserialize<TGeometry>(new JsonTextReader(new StringReader(json)));
        }

        public IGeometry Read( string json )
        {
            throw new NotSupportedException("You must call Read{TGeometry}(string json)");
        }
    }
}
