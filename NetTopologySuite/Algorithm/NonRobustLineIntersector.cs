using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A non-robust version of <see cref="LineIntersector{TCoordinate}"/>.
    /// </summary>   
    /// <typeparam name="TCoordinate">The coordinate type to use.</typeparam>
    public class NonRobustLineIntersector<TCoordinate> : LineIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
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

        protected NonRobustLineIntersector(ICoordinateFactory<TCoordinate> factory)
            : base(factory) { }

        public override Intersection<TCoordinate> ComputeIntersection(TCoordinate p, Pair<TCoordinate> line)
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

            Boolean isProper;

            TCoordinate p1 = line.First;
            TCoordinate p2 = line.Second;

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
                return new Intersection<TCoordinate>(new Pair<TCoordinate>(p, p), line);
            }

            // Point lies on line - check to see whether it lies in line segment.

            Double dist = rParameter(line, p);

            if (dist < 0.0 || dist > 1.0)
            {
                return new Intersection<TCoordinate>(new Pair<TCoordinate>(p, p), line);
            }

            isProper = true;

            if (p.Equals(p1) || p.Equals(p2))
            {
                isProper = false;
            }

            return new Intersection<TCoordinate>(p, new Pair<TCoordinate>(p, p), line, false, false, isProper);
        }

        protected override Intersection<TCoordinate> ComputeIntersectInternal(Pair<TCoordinate> line0, Pair<TCoordinate> line1)
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

            Boolean isProper;

            TCoordinate p1 = line0.First;
            TCoordinate p2 = line0.Second;

            TCoordinate p3 = line1.First;
            TCoordinate p4 = line1.Second;

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
                return new Intersection<TCoordinate>(line0, line1);
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
                return new Intersection<TCoordinate>(line0, line1);
            }

            /*
            *  Line segments intersect: compute intersection point.
            */
            Double denom = a1 * b2 - a2 * b1;

            if (denom == 0)
            {
                return computeCollinearIntersection(line0, line1);
            }

            Double x = (b1 * c2 - b2 * c1) / denom;
            Double y = (a2 * c1 - a1 * c2) / denom;

            TCoordinate intersection = CoordinateFactory.Create(x, y);

            // check if this is a proper intersection BEFORE truncating values,
            // to avoid spurious equality comparisons with endpoints
            isProper = true;

            if (intersection.Equals(p1) || intersection.Equals(p2) 
                || intersection.Equals(p3) || intersection.Equals(p4))
            {
                isProper = false;
            }

            // truncate computed point to precision grid            
            if (PrecisionModel != null)
            {
                PrecisionModel.MakePrecise(intersection);
            }

            return new Intersection<TCoordinate>(intersection, line0, line1, false, false, isProper);
        }

        private static Intersection<TCoordinate> computeCollinearIntersection(Pair<TCoordinate> line0, Pair<TCoordinate> line1)
        {
            TCoordinate p1 = line0.First;
            TCoordinate p2 = line0.Second;

            TCoordinate p3 = line1.First;
            TCoordinate p4 = line1.Second;

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
            r3 = rParameter(line0, p3);
            r4 = rParameter(line0, p4);

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
                return new Intersection<TCoordinate>(line0, line1);
            }

            TCoordinate intersection0, intersection1;

            // check for single point intersection
            if (q4.Equals(p1))
            {
                return new Intersection<TCoordinate>(p1, line0, line1, false, false, false);
            }

            if (q3.Equals(p2))
            {
                return new Intersection<TCoordinate>(p2, line0, line1, false, false, false);
            }

            // intersection MUST be a segment - compute endpoints
            intersection0 = p1;

            if (t3 > r1)
            {
                intersection0 = q3;
            }

            intersection1 = p2;

            if (t4 < r2)
            {
                intersection1 = q4;
            }

            return new Intersection<TCoordinate>(intersection0, intersection1,
                                                 line0, line1, false, false, false);
        }

        /// <summary> 
        /// RParameter computes the parameter for the point p
        /// in the parameterized equation
        /// of the line from p1 to p2.
        /// This is equal to the 'distance' of p along p1-p2.
        /// </summary>
        private static Double rParameter(Pair<TCoordinate> line, TCoordinate p)
        {
            TCoordinate p1 = line.First;
            TCoordinate p2 = line.Second;

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