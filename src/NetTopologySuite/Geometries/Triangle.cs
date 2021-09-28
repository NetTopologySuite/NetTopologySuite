using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Represents a planar triangle, and provides methods for calculating various
    /// properties of triangles.
    /// </summary>
    public class Triangle
    {
        /*
         * The coordinates of the vertices of the triangle
         */
        private Coordinate _p0, _p1, _p2;

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public Coordinate P0
        {
            get => _p0;
            set => _p0 = value;
        }

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public Coordinate P1
        {
            get => _p1;
            set => _p1 = value;
        }

        /// <summary>
        /// A corner point of the triangle
        /// </summary>
        public Coordinate P2
        {
            get => _p2;
            set => _p2 = value;
        }

        /// <summary>
        /// Tests whether a triangle is acute. A triangle is acute if all interior
        /// angles are acute. This is a strict test - right triangles will return
        /// <tt>false</tt> A triangle which is not acute is either right or obtuse.
        /// <para/>
        /// Note: this implementation is not robust for angles very close to 90 degrees.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>True if the triangle is acute.</returns>
        public static bool IsAcute(Coordinate a, Coordinate b, Coordinate c)
        {
            if (!AngleUtility.IsAcute(a, b, c)) return false;
            if (!AngleUtility.IsAcute(b, c, a)) return false;
            if (!AngleUtility.IsAcute(c, a, b)) return false;
            return true;
        }

        /// <summary>
        /// Computes the line which is the perpendicular bisector of the
        /// </summary>
        /// <param name="a">A point</param>
        /// <param name="b">Another point</param>
        /// <returns>The perpendicular bisector, as an HCoordinate line segment a-b.</returns>
        public static HCoordinate PerpendicularBisector(Coordinate a, Coordinate b)
        {
            // returns the perpendicular bisector of the line segment ab
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            var l1 = new HCoordinate(a.X + dx / 2.0, a.Y + dy / 2.0, 1.0);
            var l2 = new HCoordinate(a.X - dy + dx / 2.0, a.Y + dx + dy / 2.0, 1.0);
            return new HCoordinate(l1, l2);
        }

        /// <summary>Computes the circumcentre of a triangle.</summary>
        /// <remarks>
        /// The circumcentre is the centre of the circumcircle,
        /// the smallest circle which encloses the triangle.
        /// It is also the common intersection point of the
        /// perpendicular bisectors of the sides of the triangle,
        /// and is the only point which has equal distance to all three
        /// vertices of the triangle.
        /// <para>
        /// The circumcentre does not necessarily lie within the triangle. For example,
        /// the circumcentre of an obtuse isosceles triangle lies outside the triangle.
        /// </para>
        /// <para>This method uses an algorithm due to J.R.Shewchuk which uses normalization
        /// to the origin to improve the accuracy of computation. (See <i>Lecture Notes
        /// on Geometric Robustness</i>, Jonathan Richard Shewchuk, 1999).
        /// </para>
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The circumcentre of the triangle</returns>
        public static Coordinate Circumcentre(Coordinate a, Coordinate b, Coordinate c)
        {
            double cx = c.X;
            double cy = c.Y;
            double ax = a.X - cx;
            double ay = a.Y - cy;
            double bx = b.X - cx;
            double by = b.Y - cy;

            double denom = 2 * Det(ax, ay, bx, by);
            double numx = Det(ay, ax * ax + ay * ay, by, bx * bx + by * by);
            double numy = Det(ax, ax * ax + ay * ay, bx, bx * bx + by * by);

            double ccx = cx - numx / denom;
            double ccy = cy + numy / denom;

            return new Coordinate(ccx, ccy);
        }

        /// <summary>
        /// Computes the circumcentre of a triangle. The circumcentre is the centre of
        /// the circumcircle, the smallest circle which encloses the triangle.It is
        /// also the common intersection point of the perpendicular bisectors of the
        /// sides of the triangle, and is the only point which has equal distance to
        /// all three vertices of the triangle.
        /// <para/>
        /// The circumcentre does not necessarily lie within the triangle. For example,
        /// the circumcentre of an obtuse isosceles triangle lies outside the triangle.
        /// <para/>
        /// This method uses <see cref="DD"/> extended-precision arithmetic to
        /// provide more accurate results than
        /// <see cref="Circumcentre(NetTopologySuite.Geometries.Coordinate,NetTopologySuite.Geometries.Coordinate,NetTopologySuite.Geometries.Coordinate)"/>
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The circumcentre of the triangle</returns>
        public static Coordinate CircumcentreDD(Coordinate a, Coordinate b, Coordinate c)
        {
            var ax = DD.ValueOf(a.X) - DD.ValueOf(c.X);
            var ay = DD.ValueOf(a.Y) - DD.ValueOf(c.Y);
            var bx = DD.ValueOf(b.X) - DD.ValueOf(c.X);
            var by = DD.ValueOf(b.Y) - DD.ValueOf(c.Y);

            var denom = DD.Determinant(ax, ay, bx, by) * DD.ValueOf(2);
            var asqr = ax.Sqr() + ay.Sqr();
            var bsqr = bx.Sqr() + by.Sqr();
            var numx = DD.Determinant(ay, asqr, by, bsqr);
            var numy = DD.Determinant(ax, asqr, bx, bsqr);

            var ccx = DD.ValueOf(c.X) - numx / denom;
            var ccy = DD.ValueOf(c.Y) + numy / denom;

            return new Coordinate(ccx.ToDoubleValue(), ccy.ToDoubleValue());
        }

        /// <summary>
        /// Computes the determinant of a 2x2 matrix. Uses standard double-precision
        /// arithmetic, so is susceptible to round-off error.
        /// </summary>
        /// <param name="m00">the [0,0] entry of the matrix</param>
        /// <param name="m01">the [0,1] entry of the matrix</param>
        /// <param name="m10">the [1,0] entry of the matrix</param>
        /// <param name="m11">the [1,1] entry of the matrix</param>
        /// <returns>The determinant</returns>
        private static double Det(double m00, double m01, double m10, double m11)
        {
            return m00 * m11 - m01 * m10;
        }

        /// <summary>
        /// Computes the incentre of a triangle.
        /// </summary>
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
        public static Coordinate InCentre(Coordinate a, Coordinate b, Coordinate c)
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

        /// <summary>Computes the centroid (centre of mass) of a triangle.</summary>
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
        /// <returns>The centroid of the triangle</returns>
        public static Coordinate Centroid(Coordinate a, Coordinate b, Coordinate c)
        {
            double x = (a.X + b.X + c.X) / 3;
            double y = (a.Y + b.Y + c.Y) / 3;
            return new Coordinate(x, y);
        }

        /// <summary>Computes the length of the longest side of a triangle</summary>
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

        /// <summary>Computes the point at which the bisector of the angle ABC cuts the segment AC.</summary>
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

            var splitPt = new Coordinate(a.X + frac * dx,
                                                a.Y + frac * dy);
            return splitPt;
        }

        /// <summary>
        /// Computes the 2D area of a triangle.
        /// The area value is always non-negative.
        /// </summary>
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

        /// <summary>
        /// Computes the signed 2D area of a triangle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The area value is positive if the triangle is oriented CW,
        /// and negative if it is oriented CCW.
        /// </para>
        /// <para>
        /// The signed area value can be used to determine point orientation, but
        /// the implementation in this method is susceptible to round-off errors.
        /// Use <see cref="Orientation.Index"/> for robust orientation
        /// calculation.
        /// </para>
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The area of the triangle</returns>
        /// <seealso cref="Area(Coordinate, Coordinate, Coordinate)"/>
        /// <seealso cref="Orientation.Index"/>

        public static double SignedArea(Coordinate a, Coordinate b, Coordinate c)
        {
            /*
             * Uses the formula 1/2 * | u x v |
             * where
             *  u,v are the side vectors of the triangle
             *  x is the vector cross-product
             * For 2D vectors, this formula simplifies to the expression below
             */
            return ((c.X - a.X) * (b.Y - a.Y) - (b.X - a.X) * (c.Y - a.Y)) / 2;
        }

        /// <summary>
        /// Computes the 3D area of a triangle.
        /// The value computed is always non-negative.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The 3D area of the triangle</returns>
        public static double Area3D(Coordinate a, Coordinate b, Coordinate c)
        {
            /*
             * Uses the formula 1/2 * | u x v |
             * where
             *  u,v are the side vectors of the triangle
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
        /// Computes the Z-value (elevation) of an XY point
        /// on a three-dimensional plane defined by a triangle
        /// whose vertices have Z-values.
        /// The defining triangle must not be degenerate
        /// (in other words, the triangle must enclose a
        /// non-zero area),
        /// and must not be parallel to the Z-axis.
        /// <para/>
        /// This method can be used to interpolate
        /// the Z-value of a point inside a triangle
        /// (for example, of a TIN facet with elevations on the vertices).
        /// </summary>
        /// <param name="p">The point to compute the Z-value of</param>
        /// <param name="v0">A vertex of a triangle, with a Z ordinate</param>
        /// <param name="v1">A vertex of a triangle, with a Z ordinate</param>
        /// <param name="v2">A vertex of a triangle, with a Z ordinate</param>
        /// <returns>The computed Z-value (elevation) of the point</returns>
        public static double InterpolateZ(Coordinate p, Coordinate v0, Coordinate v1, Coordinate v2)
        {
            double x0 = v0.X;
            double y0 = v0.Y;
            double a = v1.X - x0;
            double b = v2.X - x0;
            double c = v1.Y - y0;
            double d = v2.Y - y0;
            double det = a * d - b * c;
            double dx = p.X - x0;
            double dy = p.Y - y0;
            double t = (d * dx - b * dy) / det;
            double u = (-c * dx + a * dy) / det;
            double z = v0.Z + t * (v1.Z - v0.Z) + u * (v2.Z - v0.Z);
            return z;
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
        /// Computes the <c>InCentre</c> of this triangle
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
        public Coordinate InCentre()
        {
            return InCentre(P0, P1, P2);
        }

        /// <summary>
        /// Tests whether this triangle is acute. A triangle is acute if all interior
        /// angles are acute. This is a strict test - right triangles will return
        /// <tt>false</tt> A triangle which is not acute is either right or obtuse.
        /// <para/>
        /// Note: this implementation is not robust for angles very close to 90
        /// degrees.
        /// </summary>
        /// <returns><c>true</c> if this triangle is acute</returns>
        public bool IsAcute()
        {
            return IsAcute(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the circumcentre of this triangle. The circumcentre is the centre
        /// of the circumcircle, the smallest circle which encloses the triangle. It is
        /// also the common intersection point of the perpendicular bisectors of the
        /// sides of the triangle, and is the only point which has equal distance to
        /// all three vertices of the triangle.
        /// <para/>
        /// The circumcentre does not necessarily lie within the triangle.
        /// <para/>
        /// This method uses an algorithm due to J.R.Shewchuk which uses normalization
        /// to the origin to improve the accuracy of computation. (See <i>Lecture Notes
        /// on Geometric Robustness</i>, Jonathan Richard Shewchuk, 1999).
        /// </summary>
        /// <returns>The circumcentre of this triangle</returns>
        public Coordinate Circumcentre()
        {
            return Circumcentre(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the centroid (centre of mass) of this triangle. This is also the
        /// point at which the triangle's three medians intersect (a triangle median is
        /// the segment from a vertex of the triangle to the midpoint of the opposite
        /// side). The centroid divides each median in a ratio of 2:1.
        /// <para/>
        /// The centroid always lies within the triangle.
        /// </summary>
        /// <returns>The centroid of this triangle</returns>
        public Coordinate Centroid()
        {
            return Centroid(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the length of the longest side of this triangle
        /// </summary>
        /// <returns>The length of the longest side of this triangle</returns>
        public double LongestSideLength()
        {
            return LongestSideLength(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the 2D area of this triangle. The area value is always
        /// non-negative.
        /// </summary>
        /// <returns>The area of this triangle</returns>
        /// <seealso cref="SignedArea()"/>
        public double Area()
        {
            return Area(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the signed 2D area of this triangle. The area value is positive if
        /// the triangle is oriented CW, and negative if it is oriented CCW.
        /// <para/>
        /// The signed area value can be used to determine point orientation, but the
        /// implementation in this method is susceptible to round-off errors. Use
        /// <see cref="Orientation.Index(Coordinate, Coordinate, Coordinate)"/>
        /// for robust orientation calculation.
        /// </summary>
        /// <returns>The signed 2D area of this triangle</returns>
        /// <seealso cref="Orientation.Index(Coordinate, Coordinate, Coordinate)"/>
        public double SignedArea()
        {
            return SignedArea(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the 3D area of this triangle. The value computed is always
        /// non-negative.
        /// </summary>
        /// <returns>The 3D area of this triangle</returns>
        public double Area3D()
        {
            return Area3D(_p0, _p1, _p2);
        }

        /// <summary>
        /// Computes the Z-value (elevation) of an XY point on a three-dimensional
        /// plane defined by this triangle (whose vertices must have Z-values). This
        /// triangle must not be degenerate (in other words, the triangle must enclose
        /// a non-zero area), and must not be parallel to the Z-axis.
        /// <para/>
        /// This method can be used to interpolate the Z-value of a point inside this
        /// triangle (for example, of a TIN facet with elevations on the vertices).
        /// </summary>
        /// <param name="p">The point to compute the Z-value of</param>
        /// <returns>The computed Z-value (elevation) of the point</returns>
        public double InterpolateZ(Coordinate p)
        {
            if (p == null)
                throw new ArgumentNullException("p", "Supplied point is null.");
            return InterpolateZ(p, _p0, _p1, _p2);
        }

    }
}
