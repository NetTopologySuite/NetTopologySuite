using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// An edge of a polygonization graph.
    /// </summary>
    public class PolygonizeEdge : Edge
    {
        private readonly LineString line;

        /// <summary>
        ///
        /// </summary>
        /// <param name="line"></param>
        public PolygonizeEdge(LineString line)
        {
            this.line = line;
        }

        /// <summary>
        ///
        /// </summary>
        public LineString Line => line;
    }
}
