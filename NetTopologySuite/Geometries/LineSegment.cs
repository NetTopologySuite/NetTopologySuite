using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a line segment defined by two <c>Coordinate</c>s.
    /// Provides methods to compute various geometric properties
    /// and relationships of line segments.
    /// This class is designed to be easily mutable (to the extent of
    /// having its contained points public).
    /// This supports a common pattern of reusing a single LineSegment
    /// object as a way of computing segment properties on the
    /// segments defined by arrays or lists of <c>Coordinate</c>s.
    /// </summary>    
    [Serializable]
    public class LineSegment<TCoordinate> : IEquatable<LineSegment<TCoordinate>>, IComparable<LineSegment<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private TCoordinate _p0, _p1;

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

        public TCoordinate P1
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        public TCoordinate P0
        {
            get { return _p0; }
            set { _p0 = value; }
        }

        public LineSegment(LineSegment<TCoordinate> ls) : this(ls._p0, ls._p1) { }

        public LineSegment() : this(default(TCoordinate), default(TCoordinate)) { }

        public TCoordinate GetCoordinate(Int32 i)
        {
            if (i == 0)
            {
                return P0;
            }

            return P1;
        }

        public void SetCoordinates(LineSegment<TCoordinate> ls)
        {
            SetCoordinates(ls.P0, ls.P1);
        }

        public void SetCoordinates(TCoordinate p0, TCoordinate p1)
        {
            P0 = new TCoordinate(p0[Ordinates.X], p0[Ordinates.Y]);
            P1 = new TCoordinate(p1[Ordinates.X], p1[Ordinates.Y]);
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
        public void Reverse()
        {
            TCoordinate temp = P0;
            P0 = P1;
            P1 = temp;
        }

        /// <summary> 
        /// Puts the line segment into a normalized form.
        /// This is useful for using line segments in maps and indexes when
        /// topological equality rather than exact equality is desired.
        /// </summary>
        public void Normalize()
        {
            if (P1.CompareTo(P0) < 0)
            {
                Reverse();
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
        public TCoordinate Project(TCoordinate p)
        {
            if (p.Equals(P0) || p.Equals(P1))
            {
                return new TCoordinate(p);
            }

            Double r = ProjectionFactor(p);
            Double x = P0[Ordinates.X] + r * (P1[Ordinates.X] - P0[Ordinates.X]);
            Double y = P0[Ordinates.Y] + r * (P1[Ordinates.Y] - P0[Ordinates.Y]);

            TCoordinate coord = new TCoordinate(x, y);
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
        public LineSegment<TCoordinate> Project(LineSegment<TCoordinate> seg)
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

            TCoordinate newp0 = Project(seg.P0);

            if (pf0 < 0.0)
            {
                newp0 = P0;
            }

            if (pf0 > 1.0)
            {
                newp0 = P1;
            }

            TCoordinate newp1 = Project(seg.P1);

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
        public TCoordinate ClosestPoint(TCoordinate p)
        {
            Double factor = ProjectionFactor(p);

            if (factor > 0 && factor < 1)
            {
                return Project(p);
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
        public IEnumerable<TCoordinate> ClosestPoints(LineSegment<TCoordinate> line)
        {
            // test for intersection
            TCoordinate intersection = Intersection(line);

            if (intersection != null)
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

            TCoordinate close00 = ClosestPoint(line.P0);
            minDistance = close00.Distance(line.P0);
            closestPoint1 = close00;
            closestPoint2 = line.P0;

            TCoordinate close01 = ClosestPoint(line.P1);
            dist = close01.Distance(line.P1);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint1 = close01;
                closestPoint2 = line.P1;
            }

            TCoordinate close10 = line.ClosestPoint(P0);
            dist = close10.Distance(P0);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint1 = P0;
                closestPoint2 = close10;
            }

            TCoordinate close11 = line.ClosestPoint(P1);
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
        /// There may be 0, 1 or many intersection points between two segments.
        /// If there are 0, null is returned. If there is 1 or more, a single one
        /// is returned (chosen at the discretion of the algorithm).  If
        /// more information is required about the details of the intersection,
        /// the {RobustLineIntersector} class should be used.
        /// </summary>
        /// <returns> An intersection point, or <see langword="null" /> if there is none.</returns>
        public TCoordinate Intersection(LineSegment<TCoordinate> line)
        {
            LineIntersector<TCoordinate> li = new RobustLineIntersector<TCoordinate>();
            li.ComputeIntersection(P0, P1, line.P0, line.P1);

            if (li.HasIntersection)
            {
                return li.GetIntersection(0);
            }

            return default(TCoordinate);
        }

        /// <summary>  
        /// Returns <see langword="true"/> if <c>o</c> has the same values for its points.
        /// </summary>
        /// <param name="o">A <c>LineSegment</c> with which to do the comparison.</param>
        /// <returns>
        /// <see langword="true"/> if <c>o</c> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public override Boolean Equals(object o)
        {
            return Equals(o as LineSegment<TCoordinate>);
        }

        public Boolean Equals(LineSegment<TCoordinate> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return _p0.Equals(other._p0) && _p1.Equals(other._p1);
        }

        public static Boolean operator ==(LineSegment<TCoordinate> left, LineSegment<TCoordinate> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null))
            {
                return false;
            }

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
        /// <param name="o">
        /// The <c>LineSegment</c> with which this <c>LineSegment</c>
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
    }
}