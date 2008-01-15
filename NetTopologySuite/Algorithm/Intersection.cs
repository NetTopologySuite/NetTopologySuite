using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    public struct Intersection<TCoordinate> : IEnumerable<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly LineIntersectionType _result;
        private readonly TCoordinate _intersectP0;
        private readonly TCoordinate _intersectP1;
        private readonly Pair<TCoordinate> _line0;
        private readonly Boolean _flipLine0;
        private readonly Pair<TCoordinate> _line1;
        private readonly Boolean _flipLine1;
        private readonly Boolean _isProper;

        public Intersection(Pair<TCoordinate> line0, Pair<TCoordinate> line1)
            : this(LineIntersectionType.DoesNotIntersect, Coordinates<TCoordinate>.Empty, 
                    Coordinates<TCoordinate>.Empty, line0, line1, false, false, false)
        { }

        public Intersection(TCoordinate intersectionPoint, Pair<TCoordinate> line0, 
            Pair<TCoordinate> line1, Boolean flipLine0, Boolean flipLine1, Boolean isProper)
            : this(LineIntersectionType.Intersects, intersectionPoint, Coordinates<TCoordinate>.Empty, 
                   line0, line1, flipLine0, flipLine1, isProper)
        { }

        public Intersection(TCoordinate intersectionPoint0, TCoordinate intersectionPoint1,
            Pair<TCoordinate> line0, Pair<TCoordinate> line1, Boolean flipLine0,
            Boolean flipLine1, Boolean isProper) 
            : this(LineIntersectionType.Collinear, intersectionPoint0, intersectionPoint1,
                   line0, line1, flipLine0, flipLine1, isProper)
        { }

        private Intersection(LineIntersectionType type, TCoordinate intersectionPoint0, TCoordinate intersectionPoint1,
            Pair<TCoordinate> line0, Pair<TCoordinate> line1, Boolean flipLine0, 
            Boolean flipLine1, Boolean isProper)
        {
            _result = type;
            _intersectP0 = intersectionPoint0;
            _intersectP1 = intersectionPoint1;
            _line0 = line0;
            _line1 = line1;
            _flipLine0 = flipLine0;
            _flipLine1 = flipLine1;
            _isProper = isProper;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(_line0.First).Append("-");
            buffer.Append(_line0.Second).Append(" ");
            buffer.Append(_line1.First).Append("-");
            buffer.Append(_line1.Second).Append(" : ");

            if (IsEndPoint)
            {
                buffer.Append(" endpoint");
            }

            if (_isProper)
            {
                buffer.Append(" proper");
            }

            if (IsCollinear)
            {
                buffer.Append(" collinear");
            }

            return buffer.ToString();
        }

        public Boolean IsCollinear
        {
            get { return _result == LineIntersectionType.Collinear; }
        }

        /// <summary> 
        /// Tests whether the input geometries intersect.
        /// </summary>
        /// <returns><see langword="true"/> if the input geometries intersect.</returns>
        public Boolean HasIntersection
        {
            get { return _result != LineIntersectionType.DoesNotIntersect; }
        }

        /// <summary>
        /// Returns the number of intersection points found.  This will be either 0, 1 or 2.
        /// </summary>
        public LineIntersectionType IntersectionType
        {
            get { return _result; }
        }

        /// <summary> 
        /// Returns the intersection point at index <paramref name="intersectionIndex"/>.
        /// </summary>
        /// <param name="intersectionIndex">Index of the intersection: 0 or 1.</param>
        /// <returns>
        /// The intersection point at intersection <paramref name="intersectionIndex"/>.
        /// </returns>
        public TCoordinate GetIntersectionPoint(Int32 intersectionIndex)
        {
            if (intersectionIndex != 0 && intersectionIndex != 1)
            {
                throw new ArgumentOutOfRangeException("intersectionIndex",
                                                      intersectionIndex,
                                                      "Index must be 0 or 1.");
            }

            return intersectionIndex == 0 ? _intersectP0 : _intersectP1;
        }

        /// <summary> 
        /// Test whether a point is a intersection point of two line segments.
        /// Note that if the intersection is a line segment, this method only tests for
        /// equality with the endpoints of the intersection segment.
        /// It does not return true if the input point is internal to the intersection segment.
        /// </summary>
        /// <returns><see langword="true"/> if the input point is one of the intersection points.</returns>
        public Boolean IsIntersection(TCoordinate coordinate)
        {
            if (Coordinates<TCoordinate>.IsEmpty(coordinate))
            {
                return false;
            }

            return coordinate.Equals(_intersectP0) || coordinate.Equals(_intersectP1);
        }

        /// <summary> 
        /// Tests whether either intersection point is an interior point of one of the input segments.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if either intersection point is in the interior of one of the input segment.
        /// </returns>
        public Boolean IsInteriorIntersection()
        {
            return IsInteriorIntersection(0) || IsInteriorIntersection(1);
        }

        /// <summary>
        /// Tests whether either intersection point is an interior point 
        /// of the specified input segment.
        /// </summary>
        /// <returns> 
        /// <see langword="true"/> if either intersection point is in the 
        /// interior of the input segment.
        /// </returns>
        public Boolean IsInteriorIntersection(Int32 inputLineIndex)
        {
            Pair<TCoordinate> line = GetLineForIndex(inputLineIndex);

            return !_intersectP0.Equals(line.First) && !_intersectP0.Equals(line.Second);
        }

        /// <summary>
        /// Gets a value indicating whether an intersection is proper.
        /// </summary>
        /// <remarks>
        /// The intersection between two line segments is considered proper if
        /// they intersect in a single point in the interior of both segments
        /// (e.g. the intersection is a single point and is not equal to any of the endpoints). 
        /// The intersection between a point and a line segment is considered proper
        /// if the point lies in the interior of the segment 
        /// (e.g. is not equal to either of the endpoints).
        /// </remarks>
        /// <returns><see langword="true"/> if the intersection is proper.</returns>
        public Boolean IsProper
        {
            get { return HasIntersection && _isProper; }
        }

        public Boolean IsEndPoint
        {
            get { return HasIntersection && !_isProper; }
        }

        /// <summary> 
        /// Computes the intersection point number <paramref name="intersectionIndex"/> 
        /// in the direction of a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">Index of the line segment: 0 or 1.</param>
        /// <param name="intersectionIndex">Index of the intersection: 0 or 1.</param>
        /// <returns>
        /// The intersection point at index <paramref name="intersectionIndex"/> 
        /// in the direction of the specified input line segment.
        /// </returns>
        public TCoordinate GetIntersectionAlongSegment(Int32 segmentIndex, Int32 intersectionIndex)
        {
            Int32 index = getSegmentIntersectionIndex(segmentIndex, intersectionIndex);
            Pair<TCoordinate> line = GetLineForIndex(segmentIndex);
            return index == 0 ? line.First : line.Second;
        }

        /// <summary>
        /// Computes the index of the intersection point at index 
        /// <paramref name="intersectionIndex"/> in the direction of
        /// a specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">Index of the line segment: 0 or 1.</param>
        /// <param name="intersectionIndex">Index of the intersection: 0 or 1.</param>
        /// <returns>
        /// The index of the intersection point along the segment (0 or 1).
        /// </returns>
        public Int32 GetIndexAlongSegment(Int32 segmentIndex, Int32 intersectionIndex)
        {
            if (segmentIndex != 0 && segmentIndex != 1)
            {
                throw new ArgumentOutOfRangeException("segmentIndex", segmentIndex,
                                                      "Segment index must be 0 or 1.");
            }

            if (intersectionIndex != 0 && intersectionIndex != 1)
            {
                throw new ArgumentOutOfRangeException("intersectionIndex", intersectionIndex,
                                                      "Intersection index must be 0 or 1.");
            }

            return getSegmentIntersectionIndex(segmentIndex, intersectionIndex);
        }

        /// <summary> 
        /// Computes the "edge distance" of an intersection point along the specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">Index of the line segment: 0 or 1.</param>
        /// <param name="intersectionIndex">Index of the intersection: 0 or 1.</param>
        /// <returns>The edge distance of the intersection point.</returns>
        public Double GetEdgeDistance(Int32 segmentIndex, Int32 intersectionIndex)
        {
            Pair<TCoordinate> line = GetLineForIndex(segmentIndex);
            TCoordinate intersectionPoint = GetIntersectionPoint(intersectionIndex);
            Double dist = LineIntersector<TCoordinate>.ComputeEdgeDistance(intersectionPoint, line);
            return dist;
        }

        public TCoordinate PointA
        {
            get { return _intersectP0; }
        }

        public TCoordinate PointB
        {
            get { return _intersectP1; }
        }

        #region IEnumerable<TCoordinate> Members

        public IEnumerator<TCoordinate> GetEnumerator()
        {
            if (!Coordinates<TCoordinate>.IsEmpty(_intersectP0))
            {
                yield return _intersectP0;
            }

            if (!Coordinates<TCoordinate>.IsEmpty(_intersectP1))
            {
                yield return _intersectP1;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private Int32 getSegmentIntersectionIndex(Int32 segmentIndex, Int32 intersectionIndex)
        {
            // TODO: measure the performance of this technique vs. array lookup
            // never can tell anymore with today's processors...

            Boolean flip = getFlip(segmentIndex);

            if (intersectionIndex == 0)
            {
                return flip ? 1 : 0;
            }
            else
            {
                return flip ? 0 : 1;
            }
        }

        private Boolean getFlip(Int32 segmentIndex)
        {
            return segmentIndex == 0 ? _flipLine0 : _flipLine1;
        }

        //private void setFlip(Int32 segmentIndex, Boolean flip)
        //{
        //    Debug.Assert(segmentIndex == 0 || segmentIndex == 1);

        //    if (segmentIndex == 0)
        //    {
        //        _flipLine0 = flip;
        //    }
        //    else
        //    {
        //        _flipLine1 = flip;
        //    }
        //}

        public Pair<TCoordinate> GetLineForIndex(Int32 segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex > 1)
            {
                throw new ArgumentOutOfRangeException("segmentIndex", segmentIndex,
                                                      "Parameter 'segmentIndex' must be 0 or 1.");
            }

            return segmentIndex == 0
                       ? _line0
                       : _line1;
        }
    }
}
