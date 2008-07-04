using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// An edge of a polygonization graph.
    /// </summary>
    public class PolygonizeEdge : Edge
    {
        private ILineString line;

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
        public ILineString Line
        {
            get
            {
                return line;
            }
        }
    }
}
