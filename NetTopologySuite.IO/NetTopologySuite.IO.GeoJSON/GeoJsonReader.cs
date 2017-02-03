using System.IO;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a GeoJSON Reader allowing for deserialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonReader
    {
        /// <summary>
        /// Reads the specified json.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public TObject Read<TObject>(string json)
            where TObject : class
        {
            JsonSerializer g = GeoJsonSerializer.CreateDefault();
            using (StringReader sr = new StringReader(json))
            {
                return g.Deserialize<TObject>(new JsonTextReader(sr));
            }
        }
    }
}