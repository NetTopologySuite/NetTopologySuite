using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Represents a list of contiguous line segments, and supports noding the segments.
    /// The line segments are represented by an array of <see cref="Coordinate" />s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// <see cref="NodedSegmentString" />s can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    /// </summary>
    public class NodedSegmentString : INodableSegmentString
    {
        /// <summary>
        /// Gets the <see cref="ISegmentString"/>s which result from splitting this string at node points.
        /// </summary>
        /// <param name="segStrings">A collection of NodedSegmentStrings</param>
        /// <returns>A collection of NodedSegmentStrings representing the substrings</returns>
        public static IList<ISegmentString> GetNodedSubstrings(IList<ISegmentString> segStrings)
        {
            IList<ISegmentString> resultEdgelist = new List<ISegmentString>();
            GetNodedSubstrings(segStrings, resultEdgelist);
            return resultEdgelist;
        }

        /// <summary>
        /// Adds the noded <see cref="ISegmentString"/>s which result from splitting this string at node points.
        /// </summary>
        /// <param name="segStrings">A collection of NodedSegmentStrings</param>
        /// <param name="resultEdgelist">A list which will collect the NodedSegmentStrings representing the substrings</param>
        public static void GetNodedSubstrings(IList<ISegmentString> segStrings, IList<ISegmentString> resultEdgelist)
        {
            foreach (var obj in segStrings)
            {
                var ss = (NodedSegmentString) obj;
                ss.NodeList.AddSplitEdges(resultEdgelist);
            }
        }

        private readonly SegmentNodeList _nodeList;
        private readonly Coordinate[] _pts;

        /// <summary>
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="pts">The vertices of the segment string.</param>
        /// <param name="data">The user-defined data of this segment string (may be null).</param>
        public NodedSegmentString(Coordinate[] pts, Object data)
        {
            _nodeList = new SegmentNodeList(this);

            _pts = pts;
            Context = data;
        }

        /// <summary>
        /// Gets/Sets the user-defined data for this segment string.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SegmentNodeList NodeList
        {
            get { return _nodeList; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int Count
        {
            get { return _pts.Length; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Coordinate GetCoordinate(int i) 
        { 
            return _pts[i]; 
        }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate[] Coordinates
        {
            get { return _pts; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsClosed
        {
            get { return _pts[0].Equals2D(_pts[_pts.Length - 1]); }
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
            return index == _pts.Length - 1 ? 
                Octants.Null :
                SafeOctant(GetCoordinate(index), GetCoordinate(index+1));
        }


        private static Octants SafeOctant(Coordinate p0, Coordinate p1)
        {
  	        if (p0.Equals2D(p1)) return Octants.Zero;
  	        return Octant.GetOctant(p0, p1);
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
        /// of the <see cref="NodedSegmentString" /> is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        /// <param name="li"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="geomIndex"></param>
        /// <param name="intIndex"></param>
        public void AddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
        {
            Coordinate intPt = new Coordinate(li.GetIntersection(intIndex));
            AddIntersection(intPt, segmentIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        public void AddIntersection(Coordinate intPt, int segmentIndex)
        {
            var normalizedSegmentIndex = segmentIndex;
            // normalize the intersection point location
            var nextSegIndex = normalizedSegmentIndex + 1;
            if(nextSegIndex < _pts.Length)
            {
                var nextPt = _pts[nextSegIndex];
              
                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                    normalizedSegmentIndex = nextSegIndex;                
            }

            // Add the intersection point to edge intersection list.
            /*var ei = */_nodeList.Add(intPt, normalizedSegmentIndex);
        }

        public LineSegment this[Int32 index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
#if PCL
                    throw new ArgumentOutOfRangeException("index", "Parameter must be greater than or equal to 0 and less than TotalItemCount.");
#else
                    throw new ArgumentOutOfRangeException("index", index,
                                                          "Parameter must be greater than or equal to 0 and less than TotalItemCount.");
#endif
                }

                return new LineSegment(_pts[index], _pts[index + 1]);
            }
            set
            {
                throw new NotSupportedException(
                    "Setting line segments in a ISegmentString not supported.");
            }
        }
        public override String ToString()
        {
            return WKTWriter.ToLineString(new CoordinateArraySequence(_pts));
        }

    }
}
