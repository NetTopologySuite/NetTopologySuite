using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// An edge of a <c>LineMergeGraph</c>. The <c>marked</c> field indicates
    /// whether this Edge has been logically deleted from the graph.
    /// </summary>
    public class LineMergeEdge : Edge
    {
        private ILineString line;

        /// <summary>
        /// Constructs a LineMergeEdge with vertices given by the specified LineString.
        /// </summary>
        /// <param name="line"></param>
        public LineMergeEdge(ILineString line)
        {
            this.line = line;
        }

        /// <summary>
        /// Returns the LineString specifying the vertices of this edge.
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
