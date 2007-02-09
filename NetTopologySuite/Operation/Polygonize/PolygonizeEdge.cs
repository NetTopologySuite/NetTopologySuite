using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// An edge of a polygonization graph.
    /// </summary>
    public class PolygonizeEdge : Edge
    {
        private LineString line;

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
        public LineString Line
        {
            get
            {
                return line;
            }
        }
    }
}
