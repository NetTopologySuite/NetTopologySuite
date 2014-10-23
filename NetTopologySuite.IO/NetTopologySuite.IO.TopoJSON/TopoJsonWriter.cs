using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a TopoJSON Writer, uesd to serialize TopoJSON elements.
    /// </summary>
    public class TopoJsonWriter
    {
        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="features">The feature.</param>
        /// <returns></returns>
        public string Write(FeatureCollection features)
        {
            if (features == null)
                throw new ArgumentNullException("features");

            IGeometryFactory factory = GeometryFactory.Default;
            JsonSerializer g = new TopoJsonSerializer(factory);
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, features);
            return sb.ToString();
        }
    }
}