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
    /// Json Serializer with support for TopoJSON object structure.    
    /// For more information about TopoJSON format, 
    /// see: https://github.com/mbostock/topojson/wiki/Introduction
    /// </summary>
    public class TopoJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopoJsonSerializer"/> class.
        /// </summary>
        public TopoJsonSerializer() :this(GeometryFactory.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopoJsonSerializer"/> class.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        public TopoJsonSerializer(IGeometryFactory geometryFactory)
        {
            base.Converters.Add(new TopoGeometryConverter(geometryFactory));
        }
    }

    /// <summary>
    /// Represents a TopoJSON Writer allowing for serialization of various TopoJSON elements 
    /// or any object containing TopoJSON elements.
    /// </summary>
    public class TopoJsonWriter
    {
        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public string Write(IGeometry geometry)
        {
            if (geometry == null) 
                throw new ArgumentNullException("geometry");

            TopoJsonSerializer g = new TopoJsonSerializer(geometry.Factory);
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, geometry);
            return sb.ToString();
        }

        /// <summary>
        /// Writes any specified object.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns></returns>
        public string Write(object value)
        {
            TopoJsonSerializer g = new TopoJsonSerializer();
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, value);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a TopoJSON Reader allowing for deserialization of various TopoJSON elements 
    /// or any object containing TopoJSON elements.
    /// </summary>
    public class TopoJsonReader
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
            TopoJsonSerializer g = new TopoJsonSerializer();
            using (StringReader sr = new StringReader(json))
            {
                return g.Deserialize<TObject>(new JsonTextReader(sr));
            }
        }
    }    
}
