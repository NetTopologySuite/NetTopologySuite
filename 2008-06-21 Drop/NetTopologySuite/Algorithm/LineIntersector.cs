using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;
using GeoAPI.Diagnostics;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A <see cref="LineIntersector{TCoordinate}"/> is an algorithm that can 
    /// both test whether two line segments intersect and compute the 
    /// <see cref="Intersection{TCoordinate}"/> point if they do.
    /// </summary>
    /// <remarks>
    /// The intersection point may be computed in a precise or non-precise manner.
    /// Computing it precisely involves rounding it to an integer.  (This assumes
    /// that the input coordinates have been made precise by scaling them to
    /// an integer grid.)
    /// </remarks>
    public abstract class LineIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
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
        /// <paramref name="line"/>.
        /// (E.g. p = (139, 9) p1 = (139, 10), p2 = (280, 1) produces a distance of 0.0, 
        /// which is incorrect). My hypothesis is that the function is safe to use for 
        /// points which are the result of rounding points which lie on the line, but not 
        /// safe to use for truncated points.
        /// </remarks>
        public static Double ComputeEdgeDistance(TCoordinate p, Pair<TCoordinate> line)
        {
            Double dx = Math.Abs(line.Second[Ordinates.X] - line.First[Ordinates.X]);
            Double dy = Math.Abs(line.Second[Ordinates.Y] - line.First[Ordinates.Y]);

            Double distance;

            if (p.Equals(line.First))
            {
                distance = 0.0;
            }
            else if (p.Equals(line.Second))
            {
                distance = dx > dy ? dx : dy;
            }
            else
            {
                Double pdx = Math.Abs(p[Ordinates.X] - line.First[Ordinates.X]);
                Double pdy = Math.Abs(p[Ordinates.Y] - line.First[Ordinates.Y]);

                distance = dx > dy ? pdx : pdy;

                // HACK: to ensure that non-endpoints always have a non-zero distance
                if (distance == 0.0 && !p.Equals(line.First))
                {
                    distance = Math.Max(pdx, pdy);
                }
            }

            Assert.IsTrue(!(distance == 0.0 && !p.Equals(line.First)), 
                          "Bad distance calculation");
            return distance;
        }

        /// <summary> 
        /// Computes the "edge distance" of an intersection point p along a segment.
        /// </summary>
        /// <seealso cref="ComputeEdgeDistance(TCoordinate, Pair{TCoordinate})"/>
        public static Double ComputeEdgeDistance(TCoordinate p, LineSegment<TCoordinate> line)
        {
            return ComputeEdgeDistance(p, line.Points);
        }

        /// <summary>
        /// This function is non-robust, since it may compute the square of large numbers.
        /// Currently not sure how to improve this.
        /// </summary>
        public static Double NonRobustComputeEdgeDistance(TCoordinate p, 
                                                          Pair<TCoordinate> line)
        {
            Double dx = p[Ordinates.X] - line.First[Ordinates.X];
            Double dy = p[Ordinates.Y] - line.First[Ordinates.Y];
            Double dist = Math.Sqrt(dx * dx + dy * dy); // dummy value
            Assert.IsTrue(!(dist == 0.0 && !p.Equals(line.First)), 
                          "Invalid distance calculation");
            return dist;
        }

        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly ICoordinateFactory<TCoordinate> _coordFactory;
        private IPrecisionModel<TCoordinate> _precisionModel;

        // The indexes of the endpoints of the intersection lines, in order along
        // the corresponding line
        //private Int32[] _inputLine1EndpointIndexes;
        //private Int32[] _inputLine2EndpointIndexes;

        protected LineIntersector(IGeometryFactory<TCoordinate> factory)
        {
            _geoFactory = factory;
            _coordFactory = factory.CoordinateFactory;
            _precisionModel = factory.PrecisionModel;
        }

        protected ICoordinateFactory<TCoordinate> CoordinateFactory
        {
            get { return _coordFactory; }
        }

        /// <summary> 
        /// Force computed intersection to be rounded to a given precision model.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the precision model is set, computed intersection coordinates will be made 
        /// precise using <see cref="IPrecisionModel{TCoordinate}.MakePrecise(TCoordinate)"/>.
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
        /// Gets the <see cref="IGeometryFactory{TCoordinate}"/> instance.
        /// </summary>
        public IGeometryFactory<TCoordinate> GeometryFactory
        {
            get { return _geoFactory; }
        }

        /// <summary> 
        /// Compute the intersection of a point p and the line p1-p2.
        /// This function computes the boolean value of the "has intersection" test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of <paramref name="p"/>.
        /// </summary>
        /// <param name="p">
        /// The coordinate to test for intersection with the given <paramref name="line"/>.
        /// </param>
        /// <param name="line">
        /// The line to test for intersection with.
        /// </param>
        /// <returns>
        /// An <see cref="Intersection{TCoordinate}"/> instance
        /// describing the relationship of <paramref name="p"/> with 
        /// <paramref name="line"/>.
        /// </returns>
        public abstract Intersection<TCoordinate> ComputeIntersection(TCoordinate p, 
                                                                      Pair<TCoordinate> line);

        /// <summary>
        /// Computes the intersection of the lines <c>p1-p2</c> and <c>p3-p4</c>.
        /// This function computes both the boolean value of the "has intersection" test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        /// <param name="p1">The first point of the line segment <c>p1-p2</c>.</param>
        /// <param name="p2">The second point of the line segment <c>p1-p2</c>.</param>
        /// <param name="p3">The first point of the line segment <c>p3-p4</c>.</param>
        /// <param name="p4">The second point of the line segment <c>p3-p4</c>.</param>
        /// <returns>
        /// An <see cref="Intersection{TCoordinate}"/> instance
        /// describing the relationship of the two lines.
        /// </returns>
        public Intersection<TCoordinate> ComputeIntersection(TCoordinate p1, 
                                                             TCoordinate p2, 
                                                             TCoordinate p3, 
                                                             TCoordinate p4)
        {
            return ComputeIntersectInternal(new Pair<TCoordinate>(p1, p2), 
                                            new Pair<TCoordinate>(p3, p4));
        }

        /// <summary>
        /// Computes the intersection of the line segments with endpoint pairs
        /// <paramref name="line0"/> and <paramref name="line1"/>.
        /// This function computes both the boolean value of the "has intersection" test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        /// <param name="line0">
        /// The endpoints of the first line segment to test for intersection.
        /// </param>
        /// <param name="line1">
        /// The endpoints of the second line segment to test for intersection.
        /// </param>
        /// <returns>
        /// An <see cref="Intersection{TCoordinate}"/> instance
        /// describing the relationship of the two line segments.
        /// </returns>
        public Intersection<TCoordinate> ComputeIntersection(Pair<TCoordinate> line0, 
                                                             Pair<TCoordinate> line1)
        {
            return ComputeIntersectInternal(line0, line1);
        }

        /// <summary>
        /// Computes the intersection of the line segments 
        /// <paramref name="line0"/> and <paramref name="line1"/>.
        /// This function computes both the boolean value of the "has intersection" test
        /// and the (approximate) value of the intersection point itself (if there is one).
        /// </summary>
        /// <param name="line0">
        /// The first line segment to test for intersection.
        /// </param>
        /// <param name="line1">
        /// The second line segment to test for intersection.
        /// </param>
        /// <returns>
        /// An <see cref="Intersection{TCoordinate}"/> instance
        /// describing the relationship of the two line segments.
        /// </returns>
        public Intersection<TCoordinate> ComputeIntersection(LineSegment<TCoordinate> line0, 
                                                             LineSegment<TCoordinate> line1)
        {
            return ComputeIntersectInternal(line0.Points, line1.Points);
        }

        protected abstract Intersection<TCoordinate> ComputeIntersectInternal(Pair<TCoordinate> line0, 
                                                                              Pair<TCoordinate> line1);

        protected static Pair<Boolean> ComputeIntersectionLineDirections(Intersection<TCoordinate> intersection)
        {
            Boolean directionLine0 = ComputeIntersectionLineDirection(intersection, 0);
            Boolean directionLine1 = ComputeIntersectionLineDirection(intersection, 1);

            return new Pair<Boolean>(directionLine0, directionLine1);
        }

        protected static Boolean ComputeIntersectionLineDirection(Intersection<TCoordinate> intersection, 
                                                                  Int32 segmentIndex)
        {
            //Int32[] indexes = GetIndexesForSegmentIndex(segmentIndex);

            Double dist0 = intersection.GetEdgeDistance(segmentIndex, 0);
            Double dist1 = intersection.GetEdgeDistance(segmentIndex, 1);

            return dist0 <= dist1;
        }

        //protected Int32[] GetIndexesForSegmentIndex(Int32 segmentIndex)
        //{
        //    if (segmentIndex < 0 || segmentIndex > 1)
        //    {
        //        throw new ArgumentOutOfRangeException("segmentIndex", segmentIndex,
        //                                              "Parameter 'segmentIndex' must be 0 or 1.");
        //    }

        //    return segmentIndex == 0
        //               ? _inputLine1EndpointIndexes
        //               : _inputLine2EndpointIndexes;
        //}
    }
}