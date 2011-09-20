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
        /**
         * The coordinates of the vertices of the triangle
         */
        private Coordinate _p0, _p1, _p2;

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public Coordinate P0
        {
            get { return _p0; }
            set { _p0 = value; }
        }

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public Coordinate P1
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public Coordinate P2
        {
            get { return _p2; }
            set { _p2 = value; }
        }

        ///<summary>
        /// Tests whether a triangle is acute.
        /// </summary>
        /// <remarks>
        /// <para>A triangle is acute iff all interior angles are acute.</para>
        /// <para>This is a strict test - right triangles will return <c>false</c>
        /// A triangle which is not acute is either right or obtuse.
        /// </para>
        /// Note: this implementation is not robust for angles very close to 90 degrees.
        ///</remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>True if the triangle is acute.</returns>
        public static Boolean IsAcute(Coordinate a, Coordinate b, Coordinate c)
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
        public static HCoordinate PerpendicularBisector(Coordinate a, Coordinate b)
        {
            // returns the perpendicular bisector of the line segment ab
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            HCoordinate l1 = new HCoordinate(a.X + dx / 2.0, a.Y + dy / 2.0, 1.0);
            HCoordinate l2 = new HCoordinate(a.X - dy + dx / 2.0, a.Y + dx + dy / 2.0, 1.0);
            return new HCoordinate(l1, l2);
        }

        ///<summary>Computes the circumcentre of a triangle.</summary>
        /// <remarks>
        /// The circumcentre is the centre of the circumcircle, 
        /// the smallest circle which encloses the triangle.
        /// It is also the common intersection point of the
        /// perpendicular bisectors of the sides of the triangle,
        /// and is the only point which has equal distance to all three
        /// vertices of the triangle.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The circumcentre of the triangle</returns>
        public static Coordinate Circumcentre(Coordinate a, Coordinate b, Coordinate c)
        {
            // compute the perpendicular bisector of chord ab
            HCoordinate cab = PerpendicularBisector(a, b);
            // compute the perpendicular bisector of chord bc
            HCoordinate cbc = PerpendicularBisector(b, c);
            // compute the intersection of the bisectors (circle radii)
            HCoordinate hcc = new HCoordinate(cab, cbc);
            Coordinate cc;
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
        /// The <c>InCentre</c> of a triangle is the point which is equidistant
        /// from the sides of the triangle.
        /// It is also the point at which the bisectors of the triangle's angles meet.
        /// It is the centre of the triangle's <c>InCircle</c>, which is the unique circle 
        /// that is tangent to each of the triangle's three sides.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The point which is the incentre of the triangle</returns>
        public static Coordinate InCentreFn(Coordinate a, Coordinate b, Coordinate c)
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
        /// The centroid always lies within the triangle.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        ///<returns>The centroid of the triangle</returns>
        public static Coordinate Centroid(Coordinate a, Coordinate b, Coordinate c)
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
        public static double LongestSideLength(Coordinate a, Coordinate b, Coordinate c)
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
        public static Coordinate AngleBisector(Coordinate a, Coordinate b, Coordinate c)
        {
            /*
             * Uses the fact that the lengths of the parts of the split segment
             * are proportional to the lengths of the adjacent triangle sides
             */
            double len0 = b.Distance(a);
            double len2 = b.Distance(c);
            double frac = len0 / (len0 + len2);
            double dx = c.X - a.X;
            double dy = c.Y - a.Y;

            Coordinate splitPt = new Coordinate(a.X + frac * dx,
                                                a.Y + frac * dy);
            return splitPt;
        }

        ///<summary>
        /// Computes the 2D area of a triangle.
        /// The area value is always non-negative.
        ///</summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The area of the triangle</returns>
        /// <seealso cref="SignedArea"/>
        public static double Area(Coordinate a, Coordinate b, Coordinate c)
        {
            return Math.Abs(
                  a.X * (c.Y - b.Y)
                + b.X * (a.Y - c.Y)
                + c.X * (b.Y - a.Y))
                / 2.0;
        }

        ///<summary>
        /// Computes the signed 2D area of a triangle.
        ///</summary>
        /// <remarks>
        /// <para>
        /// The area value is positive if the triangle is oriented CW,
        /// and negative if it is oriented CCW.
        /// </para>
        /// <para>
        /// The signed area value can be used to determine point orientation, but 
        /// the implementation in this method is susceptible to round-off errors.  
        /// Use <see cref="CGAlgorithms.OrientationIndex"/> for robust orientation
        /// calculation.
        /// </para>
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The area of the triangle</returns>
        /// <seealso cref="Area"/>
        /// <seealso cref="CGAlgorithms.OrientationIndex"/>

        public static double SignedArea(Coordinate a, Coordinate b, Coordinate c)
        {
            /*
             * Uses the formula 1/2 * | u x v |
             * where
             * 	u,v are the side vectors of the triangle
             *  x is the vector cross-product
             * For 2D vectors, this formual simplifies to the expression below
             */
            return ((c.X - a.X) * (b.Y - a.Y) - (b.X - a.X) * (c.Y - a.Y)) / 2;
        }

        ///<summary>
        /// Computes the 3D area of a triangle. 
        /// The value computed is alway non-negative.
        ///</summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The 3D area of the triangle</returns>
        public static double Area3D(Coordinate a, Coordinate b, Coordinate c)
        {
            /*
             * Uses the formula 1/2 * | u x v |
             * where
             * 	u,v are the side vectors of the triangle
             *  x is the vector cross-product
             */
            // side vectors u and v
            double ux = b.X - a.X;
            double uy = b.Y - a.Y;
            double uz = b.Z - a.Z;

            double vx = c.X - a.X;
            double vy = c.Y - a.Y;
            double vz = c.Z - a.Z;

            // cross-product = u x v 
            double crossx = uy * vz - uz * vy;
            double crossy = uz * vx - ux * vz;
            double crossz = ux * vy - uy * vx;

            // tri area = 1/2 * | u x v |
            double absSq = crossx * crossx + crossy * crossy + crossz * crossz;
            double area3D = Math.Sqrt(absSq) / 2;

            return area3D;
        }

        /// <summary>
        /// Creates a new triangle with the given vertices.
        /// </summary>
        /// <param name="p0">A vertex</param>
        /// <param name="p1">A vertex</param>
        /// <param name="p2">A vertex</param>
        public Triangle(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
        }

        /// <summary>
        /// Gets the <c>InCentre</c> of this triangle
        /// </summary>
        /// <remarks>The <c>InCentre</c> of a triangle is the point which is equidistant
        /// from the sides of the triangle.
        /// This is also the point at which the bisectors of the angles meet.
        /// It is the centre of the triangle's <c>InCircle</c>,
        /// which is the unique circle that is tangent to each of the triangle's three sides.
        /// </remarks>
        /// <returns>
        /// The point which is the InCentre of the triangle.
        /// </returns>
        public Coordinate InCentre
        {
            get { return InCentreFn(P0, P1, P2); }
        }
    }
}
