using System;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    public enum LineIntersectionType
    {
        // These numbers indicate the number of intersections
        // which are present in the intersection between two lines
        // or segments. Do not reorder.
        DoesNotIntersect = 0,
        DoesIntersect = 1,
        Collinear = 2
    }

    /// <summary> 
    /// A LineIntersector is an algorithm that can both test whether
    /// two line segments intersect and compute the intersection point
    /// if they do.
    /// </summary>
    /// <remarks>
    /// The intersection point may be computed in a precise or non-precise manner.
    /// Computing it precisely involves rounding it to an integer.  (This assumes
    /// that the input coordinates have been made precise by scaling them to
    /// an integer grid.)
    /// </remarks>
    public abstract class LineIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Computes the "edge distance" of an intersection point p along a segment.
        /// </summary>
        /// <remarks>
        /// The edge distance is a metric of the point along the edge.
        /// The metric used is a robust and easy to compute metric function.
        /// It is not equivalent to the usual Euclidean metric.
        /// It relies on the fact that either the x or the y ordinates of the
        /// points in the edge are unique, depending on whether the edge is longer in
        /// the horizontal or vertical direction.
        /// NOTE: This function may produce incorrect distances
        /// for inputs where <paramref name="p"/> is not precisely on 
        /// <paramref name="p1"/><c>-</c><paramref name="p2"/>
        /// (E.g. p = (139, 9) p1 = (139, 10), p2 = (280, 1) produces a distance of 0.0, 
        /// which is incorrect). My hypothesis is that the function is safe to use for 
        /// points which are the result of rounding points which lie on the line, but not 
        /// safe to use for truncated points.
        /// </remarks>
        public static Double ComputeEdgeDistance(TCoordinate p, TCoordinate p1, TCoordinate p2)
        {
            Double dx = Math.Abs(p2[Ordinates.X] - p1[Ordinates.X]);
            Double dy = Math.Abs(p2[Ordinates.Y] - p1[Ordinates.Y]);

            Double distance;

            if (p.Equals(p1))
            {
                distance = 0.0;
            }
            else if (p.Equals(p2))
            {
                if (dx > dy)
                {
                    distance = dx;
                }
                else
                {
                    distance = dy;
                }
            }
            else
            {
                Double pdx = Math.Abs(p[Ordinates.X] - p1[Ordinates.X]);
                Double pdy = Math.Abs(p[Ordinates.Y] - p1[Ordinates.Y]);

                if (dx > dy)
                {
                    distance = pdx;
                }
                else
                {
                    distance = pdy;
                }

                // HACK: to ensure that non-endpoints always have a non-zero distance
                if (distance == 0.0 && !p.Equals(p1))
                {
                    distance = Math.Max(pdx, pdy);
                }
            }

            Assert.IsTrue(!(distance == 0.0 && !p.Equals(p1)), "Bad distance calculation");
            return distance;
        }

        /// <summary>
        /// This function is non-robust, since it may compute the square of large numbers.
        /// Currently not sure how to improve this.
        /// </summary>
        public static Double NonRobustComputeEdgeDistance(TCoordinate p, TCoordinate p1, TCoordinate p2)
        {
            Double dx = p[Ordinates.X] - p1[Ordinates.X];
            Double dy = p[Ordinates.Y] - p1[Ordinates.Y];
            Double dist = Math.Sqrt(dx * dx + dy * dy); // dummy value
            Assert.IsTrue(!(dist == 0.0 && !p.Equals(p1)), "Invalid distance calculation");
            return dist;
        }

        private LineIntersectionType _result;
        private readonly TCoordinate[] _points = new TCoordinate[2];
        private readonly TCoordinate[] _inputLine1 = new TCoordinate[2];
        private readonly TCoordinate[] _inputLine2 = new TCoordinate[2];
        private Boolean _isProper;
        //private TCoordinate _pa;
        //private TCoordinate _pb;

        // The indexes of the endpoints of the intersection lines, in order along
        // the corresponding line
        private Int32[] _inputLine1EndpointIndexes;
        private Int32[] _inputLine2EndpointIndexes;

        private IPrecisionModel<TCoordinate> _precisionModel = null;

        public LineIntersector()
        {
            _points[0] = new TCoordinate();
            _points[1] = new TCoordinate();
            // alias the intersection points for ease of reference
            //_pa = _points[0];
            //_pb = _points[1];
            _result = 0;
        }

        /// <summary> 
        /// Force computed intersection to be rounded to a given precision model.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the precision model is set, computed intersection coordinates will be made 
        /// precise using <see cref="IPrecisionModel{TCoordinate}.MakePrecise"/>.
        /// </para>
        /// <para>
        /// No getter is provided, because the precision model is not required to be 
        /// specified.
        /// </para>
        /// </remarks>
        public IPrecisionModel<TCoordinate> PrecisionModel
        {
            protected get { return _precisionModel; }
            set { _precisionModel = value; }
        }

        /// <summary> 
        /// Compute the intersection of a point p and the line p1-p2.
        /// This function computes the Boolean value of the hasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of <c>p</c>.
        /// </summary>
        public abstract void ComputeIntersection(TCoordinate p, TCoordinate p1, TCoordinate p2);

        protected Boolean IsCollinear
        {
            get { return _result == LineIntersectionType.Collinear; }
        }

        /// <summary>
        /// Computes the intersection of the lines p1-p2 and p3-p4.
        /// This function computes both the Boolean value of the hasIntersection test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        public void ComputeIntersection(TCoordinate p1, TCoordinate p2, TCoordinate p3, TCoordinate p4)
        {
            _inputLine1[0] = p1;
            _inputLine1[1] = p2;

            _inputLine2[0] = p3;
            _inputLine2[1] = p4;

            _result = ComputeIntersect(p1, p2, p3, p4);
        }

        public abstract LineIntersectionType ComputeIntersect(TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2);

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(_inputLine1[0]).Append("-");
            buffer.Append(_inputLine1[0]).Append(" ");
            buffer.Append(_inputLine2[0]).Append("-");
            buffer.Append(_inputLine2[0]).Append(" : ");

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
            protected set { _result = value; }
        }

        /// <summary> 
        /// Returns the intersection point at index <paramref name="intersectionIndex"/>.
        /// </summary>
        /// <param name="intersectionIndex">Index of the intersection: 0 or 1.</param>
        /// <returns>
        /// The intersection point at intersection <paramref name="intersectionIndex"/>.
        /// </returns>
        public TCoordinate GetIntersection(Int32 intersectionIndex)
        {
            return _points[intersectionIndex];
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
            for (Int32 i = 0; i < (Int32)_result; i++)
            {
                if (_points[i].Equals(coordinate))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 
        /// Tests whether either intersection point is an interior point of one of the input segments.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if either intersection point is in the interior of one of the input segment.
        /// </returns>
        public Boolean IsInteriorIntersection()
        {
            if (IsInteriorIntersection(0))
            {
                return true;
            }

            if (IsInteriorIntersection(1))
            {
                return true;
            }

            return false;
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
            for (Int32 i = 0; i < (Int32)_result; i++)
            {
                TCoordinate[] line = GetLineForIndex(inputLineIndex);

                if (!(_points[i].Equals(line[0]) || _points[i].Equals(line[1])))
                {
                    return true;
                }
            }

            return false;
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
            protected set { _isProper = value; }
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
            // lazily compute line intersection index array
            ComputeIntersectionLineIndexes();
            Int32[] indexes = GetIndexesForSegmentIndex(segmentIndex);
            return _points[indexes[intersectionIndex]];
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
            ComputeIntersectionLineIndexes();
            Int32[] indexes = GetIndexesForSegmentIndex(segmentIndex);
            return indexes[intersectionIndex];
        }

        /// <summary> 
        /// Computes the "edge distance" of an intersection point along the specified input line segment.
        /// </summary>
        /// <param name="segmentIndex">Index of the line segment: 0 or 1.</param>
        /// <param name="intersectionIndex">Index of the intersection: 0 or 1.</param>
        /// <returns>The edge distance of the intersection point.</returns>
        public Double GetEdgeDistance(Int32 segmentIndex, Int32 intersectionIndex)
        {
            TCoordinate[] line = GetLineForIndex(segmentIndex);

            Double dist = ComputeEdgeDistance(_points[intersectionIndex], line[0], line[1]);
            return dist;
        }

        protected Boolean IsEndPoint
        {
            get { return HasIntersection && !_isProper; }
        }

        protected void ComputeIntersectionLineIndexes()
        {
            if (_inputLine1EndpointIndexes == null)
            {
                _inputLine1EndpointIndexes = new Int32[2];
            }

            if (_inputLine2EndpointIndexes == null)
            {
                _inputLine2EndpointIndexes = new Int32[2];
            }

            ComputeIntersectionLineIndex(0);
            ComputeIntersectionLineIndex(1);
        }

        protected void ComputeIntersectionLineIndex(Int32 segmentIndex)
        {
            Int32[] indexes = GetIndexesForSegmentIndex(segmentIndex);

            Double dist0 = GetEdgeDistance(segmentIndex, 0);
            Double dist1 = GetEdgeDistance(segmentIndex, 1);

            if (dist0 > dist1)
            {
                indexes[0] = 0;
                indexes[1] = 1;
            }
            else
            {
                indexes[0] = 1;
                indexes[1] = 0;
            }
        }

        protected Int32[] GetIndexesForSegmentIndex(Int32 segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex > 1)
            {
                throw new ArgumentOutOfRangeException("segmentIndex", segmentIndex,
                                                      "Parameter 'segmentIndex' must be 0 or 1.");
            }

            return segmentIndex == 0
                       ? _inputLine1EndpointIndexes
                       : _inputLine2EndpointIndexes;
        }

        protected TCoordinate[] GetLineForIndex(Int32 segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex > 1)
            {
                throw new ArgumentOutOfRangeException("segmentIndex", segmentIndex,
                                                      "Parameter 'segmentIndex' must be 0 or 1.");
            }

            return segmentIndex == 0
                       ? _inputLine1
                       : _inputLine2;
        }

        protected TCoordinate PointA
        {
            get { return _points[0]; }
            set { _points[0] = value; }
        }

        protected TCoordinate PointB
        {
            get { return _points[1]; }
            set { _points[1] = value; }
        }
    }
}