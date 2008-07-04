using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// A non-robust version of <c>LineIntersector</c>.
    /// </summary>   
    public class NonRobustLineIntersector : LineIntersector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns> 
        /// <c>true</c> if both numbers are positive or if both numbers are negative, 
        /// <c>false</c> if both numbers are zero.
        /// </returns>
        public static bool IsSameSignAndNonZero(double a, double b) 
        {
            if (a == 0 || b == 0)             
                return false;            
            return (a < 0 && b < 0) || (a > 0 && b > 0);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public NonRobustLineIntersector() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public override void ComputeIntersection(ICoordinate p, ICoordinate p1, ICoordinate p2) 
        {
            double a1;
            double b1;
            double c1;
            /*
            *  Coefficients of line eqns.
            */

            double r;
            /*
            *  'Sign' values
            */

            isProper = false;

            /*
            *  Compute a1, b1, c1, where line joining points 1 and 2
            *  is "a1 x  +  b1 y  +  c1  =  0".
            */
            a1 = p2.Y - p1.Y;
            b1 = p1.X - p2.X;
            c1 = p2.X * p1.Y - p1.X * p2.Y;

            /*
            *  Compute r3 and r4.
            */
            r = a1 * p.X + b1 * p.Y + c1;

            // if r != 0 the point does not lie on the line
            if (r != 0) 
            {
                result = DontIntersect;
                return;
            }

            // Point lies on line - check to see whether it lies in line segment.

            double dist = RParameter(p1, p2, p);
            if (dist < 0.0 || dist > 1.0)
            {
                result = DontIntersect;
                return;
            }

            isProper = true;
            if (p.Equals(p1) || p.Equals(p2))             
                isProper = false;
            
            result = DoIntersect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public override int ComputeIntersect(ICoordinate p1, ICoordinate p2, ICoordinate p3, ICoordinate p4) 
        {
            double a1;
            double b1;
            double c1;            

            double a2;            
            double b2;            
            double c2;
            /*
            *  Coefficients of line eqns.
            */

            double r1;
            double r2;
            double r3;
            double r4;
            /*
            *  'Sign' values
            */
            
            isProper = false;

            /*
            *  Compute a1, b1, c1, where line joining points 1 and 2
            *  is "a1 x  +  b1 y  +  c1  =  0".
            */
            a1 = p2.Y - p1.Y;
            b1 = p1.X - p2.X;
            c1 = p2.X * p1.Y - p1.X * p2.Y;

            /*
            *  Compute r3 and r4.
            */
            r3 = a1 * p3.X + b1 * p3.Y + c1;
            r4 = a1 * p4.X + b1 * p4.Y + c1;

            /*
            *  Check signs of r3 and r4.  If both point 3 and point 4 lie on
            *  same side of line 1, the line segments do not intersect.
            */
            if (r3 != 0 && r4 != 0 && IsSameSignAndNonZero(r3, r4))             
                return DontIntersect;           

            /*
            *  Compute a2, b2, c2
            */
            a2 = p4.Y - p3.Y;
            b2 = p3.X - p4.X;
            c2 = p4.X * p3.Y - p3.X * p4.Y;

            /*
            *  Compute r1 and r2
            */
            r1 = a2 * p1.X + b2 * p1.Y + c2;
            r2 = a2 * p2.X + b2 * p2.Y + c2;

            /*
            *  Check signs of r1 and r2.  If both point 1 and point 2 lie
            *  on same side of second line segment, the line segments do
            *  not intersect.
            */
            if (r1 != 0 && r2 != 0 && IsSameSignAndNonZero(r1, r2)) 
                return DontIntersect;            

            /*
            *  Line segments intersect: compute intersection point.
            */
            double denom = a1 * b2 - a2 * b1;
            if (denom == 0) 
                return ComputeCollinearIntersection(p1, p2, p3, p4);
            
            double numX = b1 * c2 - b2 * c1;
            pa.X = numX / denom;

            double numY = a2 * c1 - a1 * c2;
            pa.Y = numY / denom;

            // check if this is a proper intersection BEFORE truncating values,
            // to avoid spurious equality comparisons with endpoints
            isProper = true;
            if (pa.Equals(p1) || pa.Equals(p2) || pa.Equals(p3) || pa.Equals(p4))             
                isProper = false;            

            // truncate computed point to precision grid            
            if (precisionModel != null) 
                precisionModel.MakePrecise(pa);
            
            return DoIntersect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        private int ComputeCollinearIntersection(ICoordinate p1, ICoordinate p2, ICoordinate p3, ICoordinate p4) 
        {
            double r1;
            double r2;
            double r3;
            double r4;

            ICoordinate q3;
            ICoordinate q4;
            
            double t3;
            double t4;
            
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
                return DontIntersect;
            
            // check for single point intersection
            if (q4 == p1) 
            {
                pa.CoordinateValue = p1;
                return DoIntersect;
            }
            if (q3 == p2)
            {
                pa.CoordinateValue = p2;
                return DoIntersect;
            }

            // intersection MUST be a segment - compute endpoints
            pa.CoordinateValue = p1;
            if (t3 > r1) pa.CoordinateValue = q3;

            pb.CoordinateValue = p2;
            if (t4 < r2) pb.CoordinateValue = q4;
            
            return Collinear;
        }

        /// <summary> 
        /// RParameter computes the parameter for the point p
        /// in the parameterized equation
        /// of the line from p1 to p2.
        /// This is equal to the 'distance' of p along p1-p2.
        /// </summary>
        private double RParameter(ICoordinate p1, ICoordinate p2, ICoordinate p)
        {
            // compute maximum delta, for numerical stability
            // also handle case of p1-p2 being vertical or horizontal
            double r;            
            double dx = Math.Abs(p2.X - p1.X);
            double dy = Math.Abs(p2.Y - p1.Y);

            if (dx > dy) 
                 r = (p.X - p1.X) / (p2.X - p1.X);            
            else r = (p.Y - p1.Y) / (p2.Y - p1.Y);            

            return r;
        }
    }
}
