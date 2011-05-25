using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a planar triangle, and provides methods for calculating various
    /// properties of triangles.
    /// </summary>
    public class Triangle
    {
        private ICoordinate _p0, _p1, _p2;

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public ICoordinate P0
        {
            get { return _p0; }
            set { _p0 = value; }
        }

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public ICoordinate P1
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public ICoordinate P2
        {
            get { return _p2; }
            set { _p2 = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Triangle(ICoordinate p0, ICoordinate p1, ICoordinate p2)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
        }

        ///<summary>
        /// Tests whether the triangle is acute.
        /// A triangle is acute iff all interior angles are acute.
        ///</summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>True if the triangle is acute.</returns>
        public static Boolean IsAcute(ICoordinate a, ICoordinate b, ICoordinate c)
        {
            if (!AngleUtility.IsAcute(a, b, c)) return false;
            if (!AngleUtility.IsAcute(b, c, a)) return false;
            if (!AngleUtility.IsAcute(c, a, b)) return false;
            return true;
        }

        ///<summary>
        /// Computes the line which is the perpendicular bisector of the
        ///</summary>
        /// <param name="a">A point</param>
        /// <param name="b">Another point</param>
        /// <returns>The perpendicular bisector, as an HCoordinate line segment a-b.</returns>
        public static HCoordinate PerpendicularBisector(ICoordinate a, ICoordinate b)
        {
            // returns the perpendicular bisector of the line segment ab
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            HCoordinate l1 = new HCoordinate(a.X + dx / 2.0, a.Y + dy / 2.0, 1.0);
            HCoordinate l2 = new HCoordinate(a.X - dy + dx / 2.0, a.Y + dx + dy / 2.0, 1.0);
            return new HCoordinate(l1, l2);
        }

        ///<summary>Computes the circumcentre of a triangle.</summary>
        /// <returns>The circumcentre is the centre of the circumcircle, the smallest circle which encloses the triangle.</returns>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The circumcentre of the triangle</returns>
        public static ICoordinate Circumcentre(Coordinate a, Coordinate b, Coordinate c)
        {
            // compute the perpendicular bisector of chord ab
            HCoordinate cab = PerpendicularBisector(a, b);
            // compute the perpendicular bisector of chord bc
            HCoordinate cbc = PerpendicularBisector(b, c);
            // compute the intersection of the bisectors (circle radii)
            HCoordinate hcc = new HCoordinate(cab, cbc);
            ICoordinate cc;
            try
            {
                cc = new Coordinate(hcc.GetX(), hcc.GetY());
            }
            catch (NotRepresentableException ex)
            {
                // MD - not sure what we can do to prevent this (robustness problem)
                // Idea - can we condition which edges we choose?
                throw new InvalidOperationException(ex.Message);
            }
            return cc;
        }

        ///<summary>
        /// Computes the incentre of a triangle.
        ///</summary>
        /// <remarks>
        /// The inCentre of a triangle is the point which is equidistant
        /// from the sides of the triangle.
        /// It is also the point at which the bisectors of the triangle's angles meet.
        /// It is the centre of the incircle, which is the unique circle 
        /// that is tangent to each of the triangle's three sides.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The point which is the incentre of the triangle</returns>
        public static ICoordinate InCentreFn(ICoordinate a, ICoordinate b, ICoordinate c)
        {
            // the lengths of the sides, labelled by their opposite vertex
            double len0 = b.Distance(c);
            double len1 = a.Distance(c);
            double len2 = a.Distance(b);
            double circum = len0 + len1 + len2;

            double inCentreX = (len0 * a.X + len1 * b.X + len2 * c.X) / circum;
            double inCentreY = (len0 * a.Y + len1 * b.Y + len2 * c.Y) / circum;
            return new Coordinate(inCentreX, inCentreY);
        }

        ///<summary>Computes the centroid (centre of mass) of a triangle.</summary>
        /// <remarks>
        /// This is also the point at which the triangle's three
        /// medians intersect (a triangle median is the segment from a vertex of the triangle to the
        /// midpoint of the opposite side).
        /// The centroid divides each median in a ratio of 2:1.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        ///<returns>The centroid of the triangle</returns>
        public static ICoordinate Centroid(ICoordinate a, ICoordinate b, ICoordinate c)
        {
            double x = (a.X + b.X + c.X) / 3;
            double y = (a.Y + b.Y + c.Y) / 3;
            return new Coordinate(x, y);
        }

        ///<summary>Computes the length of the longest side of a triangle</summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The length of the longest side of the triangle</returns>
        public static double LongestSideLength(ICoordinate a, ICoordinate b, ICoordinate c)
        {
            // ReSharper disable InconsistentNaming
            double lenAB = a.Distance(b);
            double lenBC = b.Distance(c);
            double lenCA = c.Distance(a);
            // ReSharper restore InconsistentNaming
            double maxLen = lenAB;
            if (lenBC > maxLen)
                maxLen = lenBC;
            if (lenCA > maxLen)
                maxLen = lenCA;
            return maxLen;
        }

        ///<summary>Computes the point at which the bisector of the angle ABC cuts the segment AC.</summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The angle bisector cut point</returns>
        public static ICoordinate AngleBisector(ICoordinate a, ICoordinate b, ICoordinate c)
        {
            /**
             * Uses the fact that the lengths of the parts of the split segment
             * are proportional to the lengths of the adjacent triangle sides
             */
            double len0 = b.Distance(a);
            double len2 = b.Distance(c);
            //double lenSeg = a.Distance(c);
            double frac = len0 / (len0 + len2);
            double dx = c.X - a.X;
            double dy = c.Y - a.Y;

            ICoordinate splitPt = new Coordinate(a.X + frac * dx,
                                                a.Y + frac * dy);
            return splitPt;
        }

        ///<summary>
        /// Computes the area of a triangle.
        ///</summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The area of the triangle</returns>
        public static double Area(ICoordinate a, ICoordinate b, ICoordinate c)
        {
            return Math.Abs(
                  a.X * (c.Y - b.Y)
                + b.X * (a.Y - c.Y)
                + c.X * (b.Y - a.Y))
                / 2.0;
        }

        /// <summary>
        /// The inCentre of a triangle is the point which is equidistant
        /// from the sides of the triangle.  This is also the point at which the bisectors
        /// of the angles meet.
        /// </summary>
        /// <returns>
        /// The point which is the InCentre of the triangle.
        /// </returns>
        public ICoordinate InCentre
        {
            get { return InCentreFn(P0, P1, P2); }
        }
    }
}
