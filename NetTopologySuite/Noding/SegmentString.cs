using System;
using System.Collections;
using GeoAPI.Geometries;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        public static IList GetNodedSubstrings(IList segStrings)
        {
            IList resultEdgelist = new ArrayList();
            GetNodedSubstrings(segStrings, resultEdgelist);
            return resultEdgelist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="resultEdgelist"></param>
        public static void GetNodedSubstrings(IList segStrings, IList resultEdgelist)
        {
            foreach (var obj in segStrings)
            {
                var ss = (SegmentString) obj;
                ss.NodeList.AddSplitEdges(resultEdgelist);
            }
        }

        private readonly SegmentNodeList nodeList;
        private readonly ICoordinate[] pts;

        /// <summary>
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="pts">The vertices of the segment string.</param>
        /// <param name="data">The user-defined data of this segment string (may be null).</param>
        public SegmentString(ICoordinate[] pts, Object data)
        {
            nodeList = new SegmentNodeList(this);

            this.pts = pts;
            Data = data;
        }

        /// <summary>
        /// Gets/Sets the user-defined data for this segment string.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SegmentNodeList NodeList
        {
            get { return nodeList; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Count
        {
            get { return pts.Length; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ICoordinate GetCoordinate(int i) 
        { 
            return pts[i]; 
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get { return pts; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsClosed
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
        public Octants GetSegmentOctant(int index)
        {
            return index == pts.Length - 1 ? 
                Octants.Null :
                Octant.GetOctant(GetCoordinate(index), GetCoordinate(index + 1));
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.   
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        public void AddIntersections(LineIntersector li, int segmentIndex, int geomIndex)
        {
            for (var i = 0; i < li.IntersectionNum; i++)
                AddIntersection(li, segmentIndex, geomIndex, i);            
        }

        /// <summary>
        /// Add an <see cref="SegmentNode" /> for intersection intIndex.
        /// An intersection that falls exactly on a vertex
        /// of the <see cref="SegmentString" /> is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        /// <param name="intIndex"></param>
        public void AddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
        {
            ICoordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            AddIntersection(intPt, segmentIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        public void AddIntersection(ICoordinate intPt, int segmentIndex)
        {
            var normalizedSegmentIndex = segmentIndex;
            // normalize the intersection point location
            var nextSegIndex = normalizedSegmentIndex + 1;
            if(nextSegIndex < pts.Length)
            {
                var nextPt = pts[nextSegIndex];
              
                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                    normalizedSegmentIndex = nextSegIndex;                
            }

            // Add the intersection point to edge intersection list.
            var ei = nodeList.Add(intPt, normalizedSegmentIndex);
        }
    }
}
