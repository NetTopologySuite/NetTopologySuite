using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a TopoJSON Reader allowing for deserialization of various TopoJSON elements 
    /// or any object containing TopoJSON elements.
    /// </summary>
    public class TopoJsonReader
    {
        private readonly IGeometryFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopoJsonReader"/> class.
        /// </summary>
        /// <param name="factory">The <see cref="IGeometryFactory"/> to use.</param>
        public TopoJsonReader(IGeometryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Reads the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>A <see cref="FeatureCollection"/> with all data contained</returns>
        public T Read<T>(string json)
        {
            TopoJsonSerializer g = new TopoJsonSerializer(_factory);
            using (StringReader sr = new StringReader(json))
            {
                return g.Deserialize<T>(new JsonTextReader(sr));
            }
        }
    }
}