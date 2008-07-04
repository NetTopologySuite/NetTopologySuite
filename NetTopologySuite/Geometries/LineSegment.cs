using System;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;

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
    public class LineSegment: IComparable
    {
        private ICoordinate p0 = null, p1 = null;

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate P1
        {
            get { return p1; }
            set { p1 = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate P0
        {
            get { return p0; }
            set { p0 = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public LineSegment(ICoordinate p0, ICoordinate p1) 
        {
            this.p0 = p0;
            this.p1 = p1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ls"></param>
        public LineSegment(LineSegment ls) : this(ls.p0, ls.p1) { }

        /// <summary>
        /// 
        /// </summary>
        public LineSegment() : this(new Coordinate(), new Coordinate()) { }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ICoordinate GetCoordinate(int i)
        {
            if (i == 0) return P0;
            return P1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ls"></param>
        public void SetCoordinates(LineSegment ls)
        {
            SetCoordinates(ls.P0, ls.P1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public void SetCoordinates(ICoordinate p0, ICoordinate p1)
        {
            this.P0.X = p0.X;
            this.P0.Y = p0.Y;
            this.P1.X = p1.X;
            this.P1.Y = p1.Y;
        }

        /// <summary>
        /// Computes the length of the line segment.
        /// </summary>
        /// <returns>The length of the line segment.</returns>
        public double Length
        {
            get
            {
                return P0.Distance(P1);
            }
        }

        /// <summary> 
        /// Tests whether the segment is horizontal.
        /// </summary>
        /// <returns><c>true</c> if the segment is horizontal.</returns>
        public bool IsHorizontal
        {
            get
            {
                return P0.Y == P1.Y;
            }
        }

        /// <summary>
        /// Tests whether the segment is vertical.
        /// </summary>
        /// <returns><c>true</c> if the segment is vertical.</returns>
        public bool IsVertical
        {
            get
            {
                return P0.X == P1.X;
            }
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
        public int OrientationIndex(LineSegment seg)
        {
            int orient0 = CGAlgorithms.OrientationIndex(P0, P1, seg.P0);
            int orient1 = CGAlgorithms.OrientationIndex(P0, P1, seg.P1);
            // this handles the case where the points are Curve or collinear
            if (orient0 >= 0 && orient1 >= 0)
                return Math.Max(orient0, orient1);
            // this handles the case where the points are R or collinear
            if (orient0 <= 0 && orient1 <= 0)
                return Math.Max(orient0, orient1);
            // points lie on opposite sides ==> indeterminate orientation
            return 0;
        }

        /// <summary> 
        /// Reverses the direction of the line segment.
        /// </summary>
        public void Reverse()
        {
            ICoordinate temp = P0;
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
                Reverse();
        }

        /// <returns> 
        /// The angle this segment makes with the x-axis (in radians).
        /// </returns>
        public double Angle
        {
            get
            {
               return Math.Atan2(P1.Y - P0.Y, P1.X - P0.X);
            }
        }

        /// <summary> 
        /// Computes the distance between this line segment and another one.
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public double Distance(LineSegment ls)
        {
            return CGAlgorithms.DistanceLineLine(P0, P1, ls.P0, ls.P1);
        }

        /// <summary> 
        /// Computes the distance between this line segment and a point.
        /// </summary>
        public double Distance(ICoordinate p)
        {
            return CGAlgorithms.DistancePointLine(p, P0, P1);
        }

        /// <summary> 
        /// Computes the perpendicular distance between the (infinite) line defined
        /// by this line segment and a point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double DistancePerpendicular(ICoordinate p)
        {
            return CGAlgorithms.DistancePointLinePerpendicular(p, P0, P1);
        }

        /// <summary>
        /// Compute the projection factor for the projection of the point p
        /// onto this <c>LineSegment</c>. The projection factor is the constant k
        /// by which the vector for this segment must be multiplied to
        /// equal the vector for the projection of p.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double ProjectionFactor(ICoordinate p)
        {
            if (p.Equals(P0)) return 0.0;
            if (p.Equals(P1)) return 1.0;

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
            double dx = P1.X - P0.X;
            double dy = P1.Y - P0.Y;
            double len2 = dx * dx + dy * dy;
            double r = ((p.X - P0.X) * dx + (p.Y - P0.Y) * dy) / len2;
            return r;
        }

        /// <summary> 
        /// Compute the projection of a point onto the line determined
        /// by this line segment.
        /// Note that the projected point
        /// may lie outside the line segment.  If this is the case,
        /// the projection factor will lie outside the range [0.0, 1.0].
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public ICoordinate Project(ICoordinate p)
        {
            if (p.Equals(P0) || p.Equals(P1)) 
                return new Coordinate(p);

            double r = ProjectionFactor(p);
            ICoordinate coord = new Coordinate();
            coord.X = P0.X + r * (P1.X - P0.X);
            coord.Y = P0.Y + r * (P1.Y - P0.Y);
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
        /// <returns>The projected line segment, or <c>null</c> if there is no overlap.</returns>
        public LineSegment Project(LineSegment seg)
        {
            double pf0 = ProjectionFactor(seg.P0);
            double pf1 = ProjectionFactor(seg.P1);
            // check if segment projects at all
            if (pf0 >= 1.0 && pf1 >= 1.0) return null;
            if (pf0 <= 0.0 && pf1 <= 0.0) return null;

            ICoordinate newp0 = Project(seg.P0);
            if (pf0 < 0.0) newp0 = P0;
            if (pf0 > 1.0) newp0 = P1;

            ICoordinate newp1 = Project(seg.P1);
            if (pf1 < 0.0) newp1 = P0;
            if (pf1 > 1.0) newp1 = P1;

            return new LineSegment(newp0, newp1);
        }

        /// <summary> 
        /// Computes the closest point on this line segment to another point.
        /// </summary>
        /// <param name="p">The point to find the closest point to.</param>
        /// <returns>
        /// A Coordinate which is the closest point on the line segment to the point p.
        /// </returns>
        public ICoordinate ClosestPoint(ICoordinate p)
        {
            double factor = ProjectionFactor(p);
            if (factor > 0 && factor < 1) 
                return Project(p);
            double dist0 = P0.Distance(p);
            double dist1 = P1.Distance(p);
            if (dist0 < dist1)
                return P0;
            else return P1;
        }

        /// <summary>
        /// Computes the closest points on a line segment.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>
        /// A pair of Coordinates which are the closest points on the line segments.
        /// </returns>
        public ICoordinate[] ClosestPoints(LineSegment line)
        {
            // test for intersection
            ICoordinate intPt = Intersection(line);
            if (intPt != null)
                return new ICoordinate[] { intPt, intPt };            

            /*
            *  if no intersection closest pair contains at least one endpoint.
            * Test each endpoint in turn.
            */
            ICoordinate[] closestPt = new ICoordinate[2];
            double minDistance = Double.MaxValue;
            double dist;

            ICoordinate close00 = ClosestPoint(line.P0);
            minDistance = close00.Distance(line.P0);
            closestPt[0] = close00;
            closestPt[1] = line.P0;

            ICoordinate close01 = ClosestPoint(line.P1);
            dist = close01.Distance(line.P1);
            if (dist < minDistance) 
            {
                minDistance = dist;
                closestPt[0] = close01;
                closestPt[1] = line.P1;
            }

            ICoordinate close10 = line.ClosestPoint(P0);
            dist = close10.Distance(P0);
            if (dist < minDistance) 
            {
                minDistance = dist;
                closestPt[0] = P0;
                closestPt[1] = close10;
            }

            ICoordinate close11 = line.ClosestPoint(P1);
            dist = close11.Distance(P1);
            if (dist < minDistance) 
            {
                minDistance = dist;
                closestPt[0] = P1;
                closestPt[1] = close11;
            }

            return closestPt;
        }

        /// <summary>
        /// Computes an intersection point between two segments, if there is one.
        /// There may be 0, 1 or many intersection points between two segments.
        /// If there are 0, null is returned. If there is 1 or more, a single one
        /// is returned (chosen at the discretion of the algorithm).  If
        /// more information is required about the details of the intersection,
        /// the {RobustLineIntersector} class should be used.
        /// </summary>
        /// <param name="line"></param>
        /// <returns> An intersection point, or <c>null</c> if there is none.</returns>
        public ICoordinate Intersection(LineSegment line)
        {
            LineIntersector li = new RobustLineIntersector();
            li.ComputeIntersection(P0, P1, line.P0, line.P1);
            if (li.HasIntersection)
            return li.GetIntersection(0);
            return null;
        }

        /// <summary>  
        /// Returns <c>true</c> if <c>o</c> has the same values for its points.
        /// </summary>
        /// <param name="o">A <c>LineSegment</c> with which to do the comparison.</param>
        /// <returns>
        /// <c>true</c> if <c>o</c> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public override bool Equals(object o) 
        {
            if (o == null)
                return false;
            if (!(o is LineSegment)) 
                return false;            
            LineSegment other = (LineSegment) o;
            return p0.Equals(other.p0) && p1.Equals(other.p1);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(LineSegment obj1, LineSegment obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(LineSegment obj1, LineSegment obj2)
        {
            return !(obj1 == obj2);
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
        public int CompareTo(object o) 
        {
            LineSegment other = (LineSegment) o;
            int comp0 = P0.CompareTo(other.P0);
            if (comp0 != 0) return comp0;
            return P1.CompareTo(other.P1);
        }

        /// <summary>
        /// Returns <c>true</c> if <c>other</c> is
        /// topologically equal to this LineSegment (e.g. irrespective
        /// of orientation).
        /// </summary>
        /// <param name="other">
        /// A <c>LineSegment</c> with which to do the comparison.
        /// </param>
        /// <returns>
        /// <c>true</c> if <c>other</c> is a <c>LineSegment</c>
        /// with the same values for the x and y ordinates.
        /// </returns>
        public bool EqualsTopologically(LineSegment other)
        {
            return
                P0.Equals(other.P0) && P1.Equals(other.P1) || 
                P0.Equals(other.P1) && P1.Equals(other.P0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("LINESTRING( ");
            sb.Append(P0.X).Append(" ");
            sb.Append(P0.Y).Append(", ");
            sb.Append(P1.X).Append(" ");
            sb.Append(P1.Y).Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
