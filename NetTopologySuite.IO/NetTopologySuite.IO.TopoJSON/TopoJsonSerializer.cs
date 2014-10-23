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
            // used in serialization/deserialization
            base.Converters.Add(new ArcsConverter());
            base.Converters.Add(new TransformConverter());
            base.Converters.Add(new AttributesTableConverter());
            base.Converters.Add(new DataConverter(factory));
            base.Converters.Add(new EnvelopeConverter());
            base.Converters.Add(new ObjectsConverter());
            base.Converters.Add(new TopoObjectConverter());

            // used only in serialization
            base.Converters.Add(new TopoDatasetConverter());
            base.Converters.Add(new TopoFeaturesCollConverter());
            base.Converters.Add(new TopoFeatureConverter());
            base.Converters.Add(new TopoGeometryConverter());
            base.Converters.Add(new AttributesTableConverter());            
        }
    }
}
