using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A non-robust version of <see cref="LineIntersector{TCoordinate}"/>.
    /// </summary>   
    /// <typeparam name="TCoordinate">The coordinate type to use.</typeparam>
    public class NonRobustLineIntersector<TCoordinate> : LineIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <returns> 
        /// <see langword="true"/> if both numbers are positive or if both numbers are negative, 
        /// <c>false</c> if both numbers are zero.
        /// </returns>
        public static Boolean IsSameSignAndNonZero(Double a, Double b)
        {
            if (a == 0 || b == 0)
            {
                return false;
            }

            return (a < 0 && b < 0) || (a > 0 && b > 0);
        }

        public override void ComputeIntersection(TCoordinate p, TCoordinate p1, TCoordinate p2)
        {
            Double a1;
            Double b1;
            Double c1;
            /*
            *  Coefficients of line eqns.
            */

            Double r;
            /*
            *  'Sign' values
            */

            IsProper = false;

            /*
            *  Compute a1, b1, c1, where line joining points 1 and 2
            *  is "a1 x  +  b1 y  +  c1  =  0".
            */
            a1 = p2[Ordinates.Y] - p1[Ordinates.Y];
            b1 = p1[Ordinates.X] - p2[Ordinates.X];
            c1 = p2[Ordinates.X] * p1[Ordinates.Y] - p1[Ordinates.X] * p2[Ordinates.Y];

            /*
            *  Compute r3 and r4.
            */
            r = a1 * p[Ordinates.X] + b1 * p[Ordinates.Y] + c1;

            // if r != 0 the point does not lie on the line
            if (r != 0)
            {
                IntersectionType = LineIntersectionType.DoesNotIntersect;
                return;
            }

            // Point lies on line - check to see whether it lies in line segment.

            Double dist = RParameter(p1, p2, p);

            if (dist < 0.0 || dist > 1.0)
            {
                IntersectionType = LineIntersectionType.DoesNotIntersect;
                return;
            }

            IsProper = true;

            if (p.Equals(p1) || p.Equals(p2))
            {
                IsProper = false;
            }

            IntersectionType = LineIntersectionType.DoesIntersect;
        }

        public override LineIntersectionType ComputeIntersect(TCoordinate p1, TCoordinate p2, TCoordinate p3, TCoordinate p4)
        {
            Double a1;
            Double b1;
            Double c1;

            Double a2;
            Double b2;
            Double c2;
            /*
            *  Coefficients of line eqns.
            */

            Double r1;
            Double r2;
            Double r3;
            Double r4;
            /*
            *  'Sign' values
            */

            IsProper = false;

            /*
            *  Compute a1, b1, c1, where line joining points 1 and 2
            *  is "a1 x  +  b1 y  +  c1  =  0".
            */
            a1 = p2[Ordinates.Y] - p1[Ordinates.Y];
            b1 = p1[Ordinates.X] - p2[Ordinates.X];
            c1 = p2[Ordinates.X] * p1[Ordinates.Y] - p1[Ordinates.X] * p2[Ordinates.Y];

            /*
            *  Compute r3 and r4.
            */
            r3 = a1 * p3[Ordinates.X] + b1 * p3[Ordinates.Y] + c1;
            r4 = a1 * p4[Ordinates.X] + b1 * p4[Ordinates.Y] + c1;

            /*
            *  Check signs of r3 and r4.  If both point 3 and point 4 lie on
            *  same side of line 1, the line segments do not intersect.
            */
            if (r3 != 0 && r4 != 0 && IsSameSignAndNonZero(r3, r4))
            {
                return LineIntersectionType.DoesNotIntersect;
            }

            /*
            *  Compute a2, b2, c2
            */
            a2 = p4[Ordinates.Y] - p3[Ordinates.Y];
            b2 = p3[Ordinates.X] - p4[Ordinates.X];
            c2 = p4[Ordinates.X] * p3[Ordinates.Y] - p3[Ordinates.X] * p4[Ordinates.Y];

            /*
            *  Compute r1 and r2
            */
            r1 = a2 * p1[Ordinates.X] + b2 * p1[Ordinates.Y] + c2;
            r2 = a2 * p2[Ordinates.X] + b2 * p2[Ordinates.Y] + c2;

            /*
            *  Check signs of r1 and r2.  If both point 1 and point 2 lie
            *  on same side of second line segment, the line segments do
            *  not intersect.
            */
            if (r1 != 0 && r2 != 0 && IsSameSignAndNonZero(r1, r2))
            {
                return LineIntersectionType.DoesNotIntersect;
            }

            /*
            *  Line segments intersect: compute intersection point.
            */
            Double denom = a1 * b2 - a2 * b1;

            if (denom == 0)
            {
                return ComputeCollinearIntersection(p1, p2, p3, p4);
            }

            Double x = (b1 * c2 - b2 * c1) / denom;
            Double y = (a2 * c1 - a1 * c2) / denom;
            PointA = new TCoordinate(x, y);

            // check if this is a proper intersection BEFORE truncating values,
            // to avoid spurious equality comparisons with endpoints
            IsProper = true;

            if (PointA.Equals(p1) || PointA.Equals(p2) || PointA.Equals(p3) || PointA.Equals(p4))
            {
                IsProper = false;
            }

            // truncate computed point to precision grid            
            if (PrecisionModel != null)
            {
                PrecisionModel.MakePrecise(PointA);
            }

            return LineIntersectionType.DoesIntersect;
        }

        private LineIntersectionType ComputeCollinearIntersection(TCoordinate p1, TCoordinate p2, TCoordinate p3, TCoordinate p4)
        {
            Double r1;
            Double r2;
            Double r3;
            Double r4;

            TCoordinate q3;
            TCoordinate q4;

            Double t3;
            Double t4;

            r1 = 0;
            r2 = 1;
            r3 = RParameter(p1, p2, p3);
            r4 = RParameter(p1, p2, p4);

            // make sure p3-p4 is in same direction as p1-p2
            if (r3 < r4)
            {
                q3 = p3;
                t3 = r3;
                q4 = p4;
                t4 = r4;
            }
            else
            {
                q3 = p4;
                t3 = r4;
                q4 = p3;
                t4 = r3;
            }

            // check for no intersection
            if (t3 > r2 || t4 < r1)
            {
                return LineIntersectionType.DoesNotIntersect;
            }

            // check for single point intersection
            if (q4.Equals(p1))
            {
                PointA = p1;
                return LineIntersectionType.DoesIntersect;
            }

            if (q3.Equals(p2))
            {
                PointA = p2;
                return LineIntersectionType.DoesIntersect;
            }

            // intersection MUST be a segment - compute endpoints
            PointA = p1;

            if (t3 > r1)
            {
                PointA = q3;
            }

            PointB = p2;

            if (t4 < r2)
            {
                PointB = q4;
            }

            return LineIntersectionType.Collinear;
        }

        /// <summary> 
        /// RParameter computes the parameter for the point p
        /// in the parameterized equation
        /// of the line from p1 to p2.
        /// This is equal to the 'distance' of p along p1-p2.
        /// </summary>
        private Double RParameter(TCoordinate p1, TCoordinate p2, TCoordinate p)
        {
            // compute maximum delta, for numerical stability
            // also handle case of p1-p2 being vertical or horizontal
            Double r;
            Double dx = Math.Abs(p2[Ordinates.X] - p1[Ordinates.X]);
            Double dy = Math.Abs(p2[Ordinates.Y] - p1[Ordinates.Y]);

            if (dx > dy)
            {
                r = (p[Ordinates.X] - p1[Ordinates.X]) / (p2[Ordinates.X] - p1[Ordinates.X]);
            }
            else
            {
                r = (p[Ordinates.Y] - p1[Ordinates.Y]) / (p2[Ordinates.Y] - p1[Ordinates.Y]);
            }

            return r;
        }
    }
}