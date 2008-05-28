using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a line segment defined by two <typeparamref name="TCoordinate"/>s.
    /// Provides methods to compute various geometric properties
    /// and relationships of line segments.
    /// This class is designed to be easily mutable (to the extent of
    /// having its contained points public).
    /// This supports a common pattern of reusing a single LineSegment
    /// object as a way of computing segment properties on the
    /// segments defined by arrays or lists of <typeparamref name="TCoordinate"/>s.
    /// </summary>
    /// <typeparam name="TCoordinate">The coordinate type to use.</typeparam>
    [Serializable]
    public struct LineSegment<TCoordinate> : IEquatable<LineSegment<TCoordinate>>, 
                                             IComparable<LineSegment<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly TCoordinate _p0, _p1;

        public LineSegment(TCoordinate p0, TCoordinate p1)
        {
            _p0 = p0;
            _p1 = p1;
        }

        public LineSegment(Pair<TCoordinate> coordinates)
        {
            _p0 = coordinates.First;
            _p1 = coordinates.Second;
        }

        public LineSegment(LineSegment<TCoordinate> ls) : this(ls._p0, ls._p1) { }

        public Pair<TCoordinate> Points
        {
            get { return new Pair<TCoordinate>(P0, P1); }
        }

        public TCoordinate P0
        {
            get { return _p0; }
        }

        public TCoordinate P1
        {
            get { return _p1; }
        }

        public TCoordinate this[Int32 index]
        {
            get
            {
                checkIndex(index);

                if (index == 0)
                {
                    return P0;
                }
                else
                {
                    return P1;
                }
            }
        }

        public static LineSegment<TCoordinate> SetCoordinates(TCoordinate p0, TCoordinate p1)
        {
            LineSegment<TCoordinate> newSegment =  new LineSegment<TCoordinate>(p0, p1);
            return newSegment;
        }

        /// <summary>
        /// Computes the length of the line segment.
        /// </summary>
        /// <returns>The length of the line segment.</returns>
        public Double Length
        {
            get { return P0.Distance(P1); }
        }

        /// <summary> 
        /// Tests whether the segment is horizontal.
        /// </summary>
        /// <returns><see langword="true"/> if the segment is horizontal.</returns>
        public Boolean IsHorizontal
        {
            get { return P0[Ordinates.Y] == P1[Ordinates.Y]; }
        }

        /// <summary>
        /// Tests whether the segment is vertical.
        /// </summary>
        /// <returns><see langword="true"/> if the segment is vertical.</returns>
        public Boolean IsVertical
        {
            get { return P0[Ordinates.X] == P1[Ordinates.X]; }
        }

        /// <summary> 
        /// Determines the orientation of a LineSegment relative to this segment.
        /// The concept of orientation is specified as follows:
        /// Given two line segments A and L,
        /// A is to the left of a segment L if A lies wholly in the
        /// closed half-plane lying to the left of L
        /// A is to the right of a segment L if A lies wholly in the
        /// closed half-plane lying to the right of L
        /// otherwise, A has indeterminate orientation relative to L. This
        /// happens if A is collinear with L or if A crosses the line determined by L.
        /// </summary>
        /// <param name="seg">The <c>LineSegment</c> to compare.</param>
        /// <returns>
        /// 1 if <c>seg</c> is to the left of this segment,        
        /// -1 if <c>seg</c> is to the right of this segment,
        /// 0 if <c>seg</c> has indeterminate orientation relative to this segment.
        /// </returns>
        public Int32 OrientationIndex(LineSegment<TCoordinate> seg)
        {
            Int32 orient0 = CGAlgorithms<TCoordinate>.OrientationIndex(P0, P1, seg.P0);
            Int32 orient1 = CGAlgorithms<TCoordinate>.OrientationIndex(P0, P1, seg.P1);

            // this handles the case where the points are Curve or collinear
            if (orient0 >= 0 && orient1 >= 0)
            {
                return Math.Max(orient0, orient1);
            }

            // this handles the case where the points are R or collinear
            if (orient0 <= 0 && orient1 <= 0)
            {
                return Math.Max(orient0, orient1);
            }

            // points lie on opposite sides ==> indeterminate orientation
            return 0;
        }

        /// <summary> 
        /// Reverses the direction of the line segment.
        /// </summary>
        public LineSegment<TCoordinate> Reversed
        {
            get
            {
                return new LineSegment<TCoordinate>(_p1, _p0);
            }
        }

        /// <summary> 
        /// Puts the line segment into a normalized form.
        /// This is useful for using line segments in maps and indexes when
        /// topological equality rather than exact equality is desired.
        /// </summary>
        public LineSegment<TCoordinate> Normalized
        {
            get
            {
                if (P1.CompareTo(P0) < 0)
                {
                    return Reversed;
                }
                else
                {
                    return this;
                }
            }
        }

        /// <returns> 
        /// The angle this segment makes with the x-axis (in radians).
        /// </returns>
        public Double Angle
        {
            get { return Math.Atan2(P1[Ordinates.Y] - P0[Ordinates.Y], P1[Ordinates.X] - P0[Ordinates.X]); }
        }

        /// <summary> 
        /// Computes the distance between this line segment and another one.
        /// </summary>
        public Double Distance(LineSegment<TCoordinate> ls)
        {
            return CGAlgorithms<TCoordinate>.DistanceLineLine(P0, P1, ls.P0, ls.P1);
        }

        /// <summary> 
        /// Computes the distance between this line segment and a point.
        /// </summary>
        public Double Distance(TCoordinate p)
        {
            return CGAlgorithms<TCoordinate>.DistancePointLine(p, P0, P1);
        }

        /// <summary> 
        /// Computes the perpendicular distance between the (infinite) line defined
        /// by this line segment and a point.
        /// </summary>
        public Double DistancePerpendicular(TCoordinate p)
        {
            return CGAlgorithms<TCoordinate>.DistancePointLinePerpendicular(p, P0, P1);
        }

        /// <summary>
        /// Compute the projection factor for the projection of the point p
        /// onto this <c>LineSegment</c>. The projection factor is the constant k
        /// by which the vector for this segment must be multiplied to
        /// equal the vector for the projection of p.
        /// </summary>
        public Double ProjectionFactor(TCoordinate p)
        {
            if (p.Equals(P0))
            {
                return 0.0;
            }

            if (p.Equals(P1))
            {
                return 1.0;
            }

            // Otherwise, use comp.graphics.algorithms Frequently Asked Questions method
            /*     	          AC dot AB
                        r = ------------
                              ||AB||^2
                        r has the following meaning:
                        r=0 Point = A
                        r=1 Point = B
                        r<0 Point is on the backward extension of AB
                        r>1 Point is on the forward extension of AB
                        0<r<1 Point is interior to AB
            */
            Double dx = P1[Ordinates.X] - P0[Ordinates.X];
            Double dy = P1[Ordinates.Y] - P0[Ordinates.Y];
            Double len2 = dx * dx + dy * dy;
            Double r = ((p[Ordinates.X] - P0[Ordinates.X]) * dx 
                + (p[Ordinates.Y] - P0[Ordinates.Y]) * dy) / len2;
            return r;
        }

        /// <summary> 
        /// Compute the projection of a point onto the line determined
        /// by this line segment.
        /// Note that the projected point
        /// may lie outside the line segment.  If this is the case,
        /// the projection factor will lie outside the range [0.0, 1.0].
        /// </summary>
        public TCoordinate Project(TCoordinate p, ICoordinateFactory<TCoordinate> coordFactory)
        {
            if (p.Equals(P0) || p.Equals(P1))
            {
                return coordFactory.Create(p);
            }

            Double r = ProjectionFactor(p);
            Double x = P0[Ordinates.X] + r * (P1[Ordinates.X] - P0[Ordinates.X]);
            Double y = P0[Ordinates.Y] + r * (P1[Ordinates.Y] - P0[Ordinates.Y]);

            TCoordinate coord = coordFactory.Create(x, y);
            return coord;
        }

        /// <summary> 
        /// Project a line segment onto this line segment and return the resulting
        /// line segment.  The returned line segment will be a subset of
        /// the target line line segment.  This subset may be null, if
        /// the segments are oriented in such a way that there is no projection.
        /// Note that the returned line may have zero length (i.e. the same endpoints).
        /// This can happen for instance if the lines are perpendicular to one another.
        /// </summary>
        /// <param name="seg">The line segment to project.</param>
        /// <returns>The projected line segment, or <see langword="null" /> if there is no overlap.</returns>
        public LineSegment<TCoordinate>? Project(LineSegment<TCoordinate> seg, ICoordinateFactory<TCoordinate> coordFactory)
        {
            Double pf0 = ProjectionFactor(seg.P0);
            Double pf1 = ProjectionFactor(seg.P1);

            // check if segment projects at all
            if (pf0 >= 1.0 && pf1 >= 1.0)
            {
                return null;
            }

            if (pf0 <= 0.0 && pf1 <= 0.0)
            {
                return null;
            }

            TCoordinate newp0 = Project(seg.P0, coordFactory);

            if (pf0 < 0.0)
            {
                newp0 = P0;
            }

            if (pf0 > 1.0)
            {
                newp0 = P1;
            }

            TCoordinate newp1 = Project(seg.P1, coordFactory);

            if (pf1 < 0.0)
            {
                newp1 = P0;
            }

            if (pf1 > 1.0)
            {
                newp1 = P1;
            }

            return new LineSegment<TCoordinate>(newp0, newp1);
        }

        /// <summary> 
        /// Computes the closest point on this line segment to another point.
        /// </summary>
        /// <param name="p">The point to find the closest point to.</param>
        /// <returns>
        /// A Coordinate which is the closest point on the line segment to the point p.
        /// </returns>
        public TCoordinate ClosestPoint(TCoordinate p, ICoordinateFactory<TCoordinate> coordFactory)
        {
            Double factor = ProjectionFactor(p);

            if (factor > 0 && factor < 1)
            {
                return Project(p, coordFactory);
            }

            Double dist0 = P0.Distance(p);
            Double dist1 = P1.Distance(p);

            if (dist0 < dist1)
            {
                return P0;
            }
            else
            {
                return P1;
            }
        }

        /// <summary>
        /// Computes the closest points on a line segment.
        /// </summary>
        /// <returns>
        /// A pair of Coordinates which are the closest points on the line segments.
        /// </returns>
        public IEnumerable<TCoordinate> ClosestPoints(LineSegment<TCoordinate> line, IGeometryFactory<TCoordinate> geoFactory)
        {
            // test for intersection
            TCoordinate intersection = Intersection(line, geoFactory);

            if (Coordinates<TCoordinate>.IsEmpty(intersection))
            {
                yield return intersection;
                yield return intersection;
                yield break;
            }

            /*
            *  if no intersection closest pair contains at least one endpoint.
            * Test each endpoint in turn.
            */
            Double minDistance;
            Double dist;

            TCoordinate closestPoint1;
            TCoordinate closestPoint2;

            ICoordinateFactory<TCoordinate> coordinateFactory = geoFactory.CoordinateFactory;

            TCoordinate close00 = ClosestPoint(line.P0, coordinateFactory);
            minDistance = close00.Distance(line.P0);
            closestPoint1 = close00;
            closestPoint2 = line.P0;

            TCoordinate close01 = ClosestPoint(line.P1, coordinateFactory);
            dist = close01.Distance(line.P1);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint1 = close01;
                closestPoint2 = line.P1;
            }

            TCoordinate close10 = line.ClosestPoint(P0, coordinateFactory);
            dist = close10.Distance(P0);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint1 = P0;
                closestPoint2 = close10;
            }

            TCoordinate close11 = line.ClosestPoint(P1, coordinateFactory);
            dist = close11.Distance(P1);

            if (dist < minDistance)
            {
                closestPoint1 = P1;
                closestPoint2 = close11;
            }
            
            yield return closestPoint1;
            yield return closestPoint2;
        }

        /// <summary>
        /// Computes an intersection point between two segments, if there is one.
        /// </summary>
        /// <remarks>
        /// There may be 0, 1 or many intersection points between two segments.
        /// If there are 0, a default <typeparamref name="TCoordinate"/> is returned. 
        /// If there is 1 or more, a single one is returned (chosen at the discretion 
        /// of the algorithm).  If more information is required about the details of the 
        /// intersection, the <see cref="RobustLineIntersector{TCoordinate}"/> class should 
        /// be used, which returns an <see cref="Intersection{TCoordinate}"/> instance,
        /// which contains a variety of contextual data about the intersection.
        /// </remarks>
        /// <returns>
        /// An intersection point, or a default 
        /// <typeparamref name="TCoordinate"/> if there is none.
        /// </returns>
        public TCoordinate Intersection(LineSegment<TCoordinate> line, IGeometryFactory<TCoordinate> geoFactory)
        {
            LineIntersector<TCoordinate> li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geoFactory);
            Intersection<TCoordinate> intersection = li.ComputeIntersection(P0, P1, line.P0, line.P1);

            if (intersection.HasIntersection)
            {
                return intersection.GetIntersectionPoint(0);
            }

            return default(TCoordinate);
        }

        public override Int32 GetHashCode()
        {
            return 37 + _p0.GetHashCode() ^ 17 + _p1.GetHashCode();
        }

        /// <summary>  
        /// Returns <see langword="true"/> if <paramref name="other"/> 
        /// has the same values for its points.
        /// </summary>
        /// <param name="other">A <see cref="LineSegment{TCoordinate}"/> with
        /// which to do the comparison.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public override Boolean Equals(Object other)
        {
            if (!(other is LineSegment<TCoordinate>))
            {
                return false;
            }

            return Equals((LineSegment<TCoordinate>)other);
        }

        public Boolean Equals(LineSegment<TCoordinate> other)
        {
            return _p0.Equals(other._p0) && _p1.Equals(other._p1);
        }

        public static Boolean operator ==(LineSegment<TCoordinate> left, LineSegment<TCoordinate> right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(LineSegment<TCoordinate> left, LineSegment<TCoordinate> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Uses the standard lexicographic ordering for the points in the LineSegment.
        /// </summary>
        /// <param name="other">
        /// The <see cref="LineSegment{TCoordinate}"/> with which this <c>LineSegment</c>
        /// is being compared.
        /// </param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>LineSegment</c>
        /// is less than, equal to, or greater than the specified <c>LineSegment</c>.
        /// </returns>
        public Int32 CompareTo(LineSegment<TCoordinate> other)
        {
            Int32 comp0 = P0.CompareTo(other.P0);

            if (comp0 != 0)
            {
                return comp0;
            }

            return P1.CompareTo(other.P1);
        }

        /// <summary>
        /// Returns <see langword="true"/> if <c>other</c> is
        /// topologically equal to this LineSegment (e.g. irrespective
        /// of orientation).
        /// </summary>
        /// <param name="other">
        /// A <c>LineSegment</c> with which to do the comparison.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <c>other</c> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public Boolean EqualsTopologically(LineSegment<TCoordinate> other)
        {
            return
                P0.Equals(other.P0) && P1.Equals(other.P1) ||
                P0.Equals(other.P1) && P1.Equals(other.P0);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("LINESTRING( ");
            sb.Append(P0[Ordinates.X]).Append(" ");
            sb.Append(P0[Ordinates.Y]).Append(", ");
            sb.Append(P1[Ordinates.X]).Append(" ");
            sb.Append(P1[Ordinates.Y]).Append(")");
            return sb.ToString();
        }

        private static void checkIndex(Int32 index)
        {
            if (index != 0 && index != 1)
            {
                throw new ArgumentOutOfRangeException("index", index,
                                                      "Index must be 0 or 1.");
            }
        }
    }
}