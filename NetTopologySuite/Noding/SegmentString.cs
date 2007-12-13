using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents a list of contiguous line segments, and supports noding the segments.
    /// The line segments are represented by an set of <typeparamref name="TCoordinate"/>s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// <see cref="SegmentString{TCoordinate}" />s can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    /// </summary>
    public class SegmentString<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings(IEnumerable<SegmentString<TCoordinate>> segStrings)
        {
            foreach (SegmentString<TCoordinate> ss in segStrings)
            {
                foreach (SegmentString<TCoordinate> segmentString in ss.NodeList.GetSplitEdges())
                {
                    yield return segmentString;
                }
            }
        }

        //public static void GetNodedSubstrings(IList segStrings, IList resultEdgelist)
        //{
        //    foreach (object obj in segStrings)
        //    {
        //        SegmentString ss = (SegmentString) obj;
        //        ss.NodeList.AddSplitEdges(resultEdgelist);
        //    }
        //}

        private readonly SegmentNodeList<TCoordinate> _nodeList = null;
        private readonly ICoordinateSequence<TCoordinate> _coordinates;
        private Object _data;

        /// <summary>
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="coordinates">The vertices of the segment string.</param>
        /// <param name="data">The user-defined data of this segment string (may be null).</param>
        public SegmentString(ICoordinateSequence<TCoordinate> coordinates, Object data)
        {
            _nodeList = new SegmentNodeList<TCoordinate>(this);

            _coordinates = coordinates;
            _data = data;
        }

        /// <summary>
        /// Gets or sets the user-defined data for this segment string.
        /// </summary>
        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public SegmentNodeList<TCoordinate> NodeList
        {
            get { return _nodeList; }
        }

        public Int32 Count
        {
            get { return _coordinates.Count; }
        }

        public TCoordinate this[Int32 index]
        {
            get
            {
                return _coordinates[index];
            }
        }

        public IEnumerable<TCoordinate> Coordinates
        {
            get { return _coordinates; }
        }

        public Boolean IsClosed
        {
            get 
            { 
                return Slice.GetFirst(_coordinates).Equals(Slice.GetLast(_coordinates)); 
            }
        }

        /// <summary>
        /// Gets the octant of the segment starting at vertex <c>index</c>.
        /// </summary>
        /// <param name="index">
        /// The index of the vertex starting the segment.  
        /// Must not be the last index in the vertex list
        /// </param>
        /// <returns>The octant of the segment at the vertex.</returns>
        public Octants GetSegmentOctant(Int32 index)
        {
            if (index == _coordinates.Count - 1)
            {
                return Octants.Null;
            }

            return Octant.GetOctant(this[index], this[index + 1]);
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.   
        /// </summary>
        public void AddIntersections(LineIntersector<TCoordinate> li, Int32 segmentIndex, Int32 geomIndex)
        {
            Int32 intersection = (Int32)li.IntersectionType;

            for (Int32 i = 0; i < intersection; i++)
            {
                AddIntersection(li, segmentIndex, geomIndex, i);
            }
        }

        /// <summary>
        /// Add a <see cref="SegmentNode{TCoordinate}" /> for intersection
        /// <paramref name="intIndex"/>.
        /// </summary>
        /// <remarks>
        /// An intersection that falls exactly on a vertex
        /// of the <see cref="SegmentString{TCoordinate}" /> is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </remarks>
        public void AddIntersection(LineIntersector<TCoordinate> li, Int32 segmentIndex, Int32 geomIndex, Int32 intIndex)
        {
            TCoordinate intPt = new TCoordinate(li.GetIntersection(intIndex));
            AddIntersection(intPt, segmentIndex);
        }

        public void AddIntersection(TCoordinate intPt, Int32 segmentIndex)
        {
            Int32 normalizedSegmentIndex = segmentIndex;
            // normalize the intersection point location
            Int32 nextSegIndex = normalizedSegmentIndex + 1;

            if (nextSegIndex < _coordinates.Count)
            {
                TCoordinate nextPt = _coordinates[nextSegIndex];

                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                }
            }

            // Add the intersection point to edge intersection list.
            SegmentNode<TCoordinate> ei = _nodeList.Add(intPt, normalizedSegmentIndex);
        }
    }
}