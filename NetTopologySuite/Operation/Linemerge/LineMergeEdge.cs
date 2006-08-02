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
         * 
        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return "LineMergeEdge: " + line.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;
            if (!(obj is LineMergeEdge))
                return false;
            return Equals(obj as LineMergeEdge);
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
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            int result = 29;
            result = 14 + 29 * Line.GetHashCode();
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
            return a.Equals(b);
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
