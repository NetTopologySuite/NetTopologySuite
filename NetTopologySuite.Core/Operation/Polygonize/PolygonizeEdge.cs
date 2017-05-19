using GeoAPI.Geometries;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    ///     An edge of a polygonization graph.
    /// </summary>
    public class PolygonizeEdge : Edge
    {
        /// <summary>
        /// </summary>
        /// <param name="line"></param>
        public PolygonizeEdge(ILineString line)
        {
            Line = line;
        }

        /// <summary>
        /// </summary>
        public ILineString Line { get; }
    }
}