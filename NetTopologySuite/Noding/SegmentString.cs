using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents a list of contiguous line segments, and supports noding the segments.
    /// The line segments are represented by an set of <typeparamref name="TCoordinate"/>s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// <see cref="NodedSegmentString{TCoordinate}" />s can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    /// </summary>
    public class NodedSegmentString<TCoordinate> : IList<LineSegment<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        public static IEnumerable<NodedSegmentString<TCoordinate>> GetNodedSubstrings(IEnumerable<NodedSegmentString<TCoordinate>> segStrings)
        {
            foreach (NodedSegmentString<TCoordinate> ss in segStrings)
            {
                foreach (NodedSegmentString<TCoordinate> segmentString in ss.Nodes.CreateSplitEdges())
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

        private static readonly Object _nullData = new Object();
        private readonly SegmentNodeList<TCoordinate> _nodeList = null;
        private readonly ICoordinateSequence<TCoordinate> _coordinates;
        private Object _context;

        /// <summary>
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="coordinates">The vertices of the segment string.</param>
        /// <param name="data">The user-defined data of this segment string (may be null).</param>
        public NodedSegmentString(ICoordinateSequence<TCoordinate> coordinates, Object data)
        {
            _nodeList = new SegmentNodeList<TCoordinate>(this);
            _coordinates = coordinates;
            _context = data;
        }

        public override Int32 GetHashCode()
        {
            Int32 hashCode = 7199369;

            hashCode ^= _nodeList.GetHashCode();
            hashCode ^= _coordinates.GetHashCode();
            hashCode ^= (_context ?? _nullData).GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Gets or sets the user-defined context for this segment string.
        /// </summary>
        public Object Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public SegmentNodeList<TCoordinate> Nodes
        {
            get { return _nodeList; }
        }

        public Int32 Count
        {
            get { return _coordinates.Count - 1; }
        }

        public LineSegment<TCoordinate> this[Int32 index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException("index", index,
                                                          "Parameter must be greater than or equal to 0 and less than Count.");
                }

                return new LineSegment<TCoordinate>(_coordinates[index], _coordinates[index + 1]);
            }
            set
            {
                throw new NotSupportedException(
                    "Setting line segments in a SegmentString not supported.");
            }
        }

        public ICoordinateSequence<TCoordinate> Coordinates
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
        /// Gets the octant of the segment starting at vertex <paramref name="index"/>.
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

            LineSegment<TCoordinate> segment = this[index];
            return Octant.GetOctant(segment);
        }

        /// <summary>
        /// Adds edge intersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.   
        /// </summary>
        public void AddIntersections(Intersection<TCoordinate> intersection, Int32 segmentIndex, Int32 geomIndex)
        {
            Int32 intersections = (Int32)intersection.IntersectionDegree;

            for (Int32 i = 0; i < intersections; i++)
            {
                AddIntersection(intersection, segmentIndex, geomIndex, i);
            }
        }

        /// <summary>
        /// Add a <see cref="SegmentNode{TCoordinate}" /> for intersection
        /// <paramref name="intersectionPointIndex"/>.
        /// </summary>
        /// <remarks>
        /// An intersection that falls exactly on a vertex
        /// of the <see cref="NodedSegmentString{TCoordinate}" /> is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </remarks>
        public void AddIntersection(Intersection<TCoordinate> intersection, 
            Int32 segmentIndex, Int32 geomIndex, Int32 intersectionPointIndex)
        {
            TCoordinate intersectionPoint = intersection.GetIntersectionPoint(intersectionPointIndex);
            AddIntersection(intersectionPoint, segmentIndex);
        }

        public void AddIntersection(TCoordinate intersectionPoint, Int32 segmentIndex)
        {
            Int32 normalizedSegmentIndex = segmentIndex;
            // normalize the intersection point location
            Int32 nextSegmentIndex = normalizedSegmentIndex + 1;

            if (nextSegmentIndex < _coordinates.Count)
            {
                TCoordinate nextPt = _coordinates[nextSegmentIndex];

                // Normalize segment index if intersectionPoint falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intersectionPoint.Equals(nextPt))
                {
                    normalizedSegmentIndex = nextSegmentIndex;
                }
            }

            // Add the intersection point to edge intersection list.
            _nodeList.Add(intersectionPoint, normalizedSegmentIndex);
        }

        #region IList<TCoordinate> Members

        public Int32 IndexOf(LineSegment<TCoordinate> item)
        {
            Int32 indexP0 = _coordinates.IndexOf(item.P0);

            if (indexP0 == Count - 1)
            {
                return -1;
            }

            if (_coordinates[indexP0 + 1].Equals(item.P1))
            {
                return indexP0;
            }
            else
            {
                return -1;
            }
        }

        void IList<LineSegment<TCoordinate>>.Insert(Int32 index, LineSegment<TCoordinate> item)
        {
            throw new NotSupportedException("Insertion not supported, since SegmentString is sorted.");
        }

        void IList<LineSegment<TCoordinate>>.RemoveAt(Int32 index)
        {
            throw new NotSupportedException("Removal of segments not supported.");
        }

        #endregion

        #region ICollection<TCoordinate> Members

        void ICollection<LineSegment<TCoordinate>>.Add(LineSegment<TCoordinate> item)
        {
            throw new NotSupportedException("Use 'AddIntersection' method instead.");
        }

        public void Clear()
        {
            _coordinates.Clear();
        }

        public Boolean Contains(LineSegment<TCoordinate> item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(LineSegment<TCoordinate>[] array, Int32 arrayIndex)
        {
            _coordinates.CopyTo(array, arrayIndex);
        }

        public Boolean IsReadOnly
        {
            get { return true; }
        }

        Boolean ICollection<LineSegment<TCoordinate>>.Remove(LineSegment<TCoordinate> item)
        {
            throw new NotSupportedException("Removal of segments not supported.");
        }

        #endregion

        #region IEnumerable<TCoordinate> Members

        public IEnumerator<LineSegment<TCoordinate>> GetEnumerator()
        {
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(_coordinates))
            {
                yield return new LineSegment<TCoordinate>(pair);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}