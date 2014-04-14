using GeoAPI.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Json Serializer with support for TopoJSON object structure.    
    /// For more information about TopoJSON format, 
    /// see: https://github.com/mbostock/topojson/wiki/Introduction
    /// </summary>
    public class TopoJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopoJsonSerializer"/> class.
        /// </summary>
        /// <param name="factory">The <see cref="IGeometryFactory"/> to use.</param>
        public TopoJsonSerializer(IGeometryFactory factory)
        {
            base.Converters.Add(new ArcsConverter());
            base.Converters.Add(new TransformConverter());
            base.Converters.Add(new ObjectsConverter());
            base.Converters.Add(new TopoObjectConverter());
            base.Converters.Add(new AttributesTableConverter());
            base.Converters.Add(new DataConverter(factory));
            // TODO: envelope
        }
    }
}
