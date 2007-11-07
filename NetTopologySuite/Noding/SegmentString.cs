using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents a list of contiguous line segments, and supports noding the segments.
    /// The line segments are represented by an array of <see cref="Coordinate" />s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// <see cref="SegmentString" />s can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    /// </summary>
    public class SegmentString
    {
        public static IList GetNodedSubstrings(IList segStrings)
        {
            IList resultEdgelist = new ArrayList();
            GetNodedSubstrings(segStrings, resultEdgelist);
            return resultEdgelist;
        }

        public static void GetNodedSubstrings(IList segStrings, IList resultEdgelist)
        {
            foreach (object obj in segStrings)
            {
                SegmentString ss = (SegmentString) obj;
                ss.NodeList.AddSplitEdges(resultEdgelist);
            }
        }

        private SegmentNodeList nodeList = null;
        private ICoordinate[] pts;
        private object data;

        /// <summary>
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="pts">The vertices of the segment string.</param>
        /// <param name="data">The user-defined data of this segment string (may be null).</param>
        public SegmentString(ICoordinate[] pts, Object data)
        {
            nodeList = new SegmentNodeList(this);

            this.pts = pts;
            this.data = data;
        }

        /// <summary>
        /// Gets or sets the user-defined data for this segment string.
        /// </summary>
        public object Data
        {
            get { return data; }
            set { data = value; }
        }

        public SegmentNodeList NodeList
        {
            get { return nodeList; }
        }

        public Int32 Count
        {
            get { return pts.Length; }
        }

        public ICoordinate GetCoordinate(Int32 i)
        {
            return pts[i];
        }

        public ICoordinate[] Coordinates
        {
            get { return pts; }
        }

        public Boolean IsClosed
        {
            get { return pts[0].Equals(pts[pts.Length - 1]); }
        }

        /// <summary>
        ///  Gets the octant of the segment starting at vertex <c>index</c>.
        /// </summary>
        /// <param name="index">
        /// The index of the vertex starting the segment.  
        /// Must not be the last index in the vertex list
        /// </param>
        /// <returns>The octant of the segment at the vertex</returns>
        public Octants GetSegmentOctant(Int32 index)
        {
            if (index == pts.Length - 1)
            {
                return Octants.Null;
            }
            return Octant.GetOctant(GetCoordinate(index), GetCoordinate(index + 1));
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.   
        /// </summary>
        public void AddIntersections(LineIntersector li, Int32 segmentIndex, Int32 geomIndex)
        {
            for (Int32 i = 0; i < li.IntersectionNum; i++)
            {
                AddIntersection(li, segmentIndex, geomIndex, i);
            }
        }

        /// <summary>
        /// Add an <see cref="SegmentNode" /> for intersection intIndex.
        /// An intersection that falls exactly on a vertex
        /// of the <see cref="SegmentString" /> is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        public void AddIntersection(LineIntersector li, Int32 segmentIndex, Int32 geomIndex, Int32 intIndex)
        {
            ICoordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            AddIntersection(intPt, segmentIndex);
        }

        public void AddIntersection(ICoordinate intPt, Int32 segmentIndex)
        {
            Int32 normalizedSegmentIndex = segmentIndex;
            // normalize the intersection point location
            Int32 nextSegIndex = normalizedSegmentIndex + 1;
            if (nextSegIndex < pts.Length)
            {
                ICoordinate nextPt = pts[nextSegIndex];

                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                }
            }

            // Add the intersection point to edge intersection list.
            SegmentNode ei = nodeList.Add(intPt, normalizedSegmentIndex);
        }
    }
}