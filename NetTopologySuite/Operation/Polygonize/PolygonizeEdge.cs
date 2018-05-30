using GeoAPI.Geometries;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// An edge of a polygonization graph.
    /// </summary>
    public class PolygonizeEdge : Edge
    {
        private readonly ILineString line;

        /// <summary>
        ///
        /// </summary>
        /// <param name="line"></param>
        public PolygonizeEdge(ILineString line)
        {
            this.line = line;
        }

        /// <summary>
        ///
        /// </summary>
        public ILineString Line => line;
    }
}
