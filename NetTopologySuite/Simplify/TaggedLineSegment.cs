using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// A LineSegment which is tagged with its location in a <c>Geometry</c>.
    /// Used to index the segments in a point and recover the segment locations
    /// from the index.
    /// </summary>
    public class TaggedLineSegment : LineSegment
    {
        private Geometry parent;
        private int index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        public TaggedLineSegment(Coordinate p0, Coordinate p1, Geometry parent, int index) : base(p0, p1)
        {            
            this.parent = parent;
            this.index = index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public TaggedLineSegment(Coordinate p0, Coordinate p1) : this(p0, p1, null, -1) { }

        /// <summary>
        /// 
        /// </summary>
        public virtual Geometry Parent
        {
            get
            {
                return parent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Index
        {
            get
            {
                return index;
            }
        }
    }
}
