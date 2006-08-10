using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// An edge of a <c>LineMergeGraph</c>. The <c>marked</c> field indicates
    /// whether this Edge has been logically deleted from the graph.
    /// </summary>
    public class LineMergeEdge : Edge
    {
        private LineString line;

        /// <summary>
        /// Constructs a LineMergeEdge with vertices given by the specified LineString.
        /// </summary>
        /// <param name="line"></param>
        public LineMergeEdge(LineString line)
        {
            this.line = line;
        }

        /// <summary>
        /// Returns the LineString specifying the vertices of this edge.
        /// </summary>
        public virtual LineString Line
        {
            get
            {
                return line;
            }
        }

        /*

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LineMergeEdge: " + line.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is LineMergeEdge))
                return false;
            if (!base.Equals(obj))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            LineMergeEdge other = obj as LineMergeEdge;
            return Equals(other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        protected virtual bool Equals(LineMergeEdge other)
        {
            return Line.Equals(other.Line);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>       
        public override int GetHashCode()
        {
            int result = 29 * base.GetHashCode();
            result += 14 + 29 * Line.GetHashCode();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator == (LineMergeEdge a, LineMergeEdge b)
        {
            return Object.Equals(a, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(LineMergeEdge a, LineMergeEdge b)
        {
            return !(a == b);
        }

        */
    }
}
