using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Triangulate.QuadEdge
{

    /// <summary>
    /// Algorithms for computing values and predicates
    /// associated with triangles.
    /// </summary>
    /// <remarks>
    /// For some algorithms extended-precision
    /// implementations are provided, which are more robust
    /// (i.e. they produce correct answers in more cases).
    /// Also, some more robust formulations of
    /// some algorithms are provided, which utilize
    /// normalization to the origin.
    /// </remarks>
    /// <author>Martin Davis</author>
    public static class TrianglePredicate
    {

        /// <summary>
        /// Tests if a point is inside the circle defined by
        /// the triangle with vertices a, b, c (oriented counter-clockwise).
        /// This test uses simple
        /// double-precision arithmetic, and thus is not 100% robust.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns>true if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleNonRobust(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            bool isInCircle =
                  (a.X*a.X + a.Y*a.Y)*TriArea(b, c, p)
                - (b.X*b.X + b.Y*b.Y)*TriArea(a, c, p)
                + (c.X*c.X + c.Y*c.Y)*TriArea(a, b, p)
                - (p.X*p.X + p.Y*p.Y)*TriArea(a, b, c)
                > 0;
            return isInCircle;
        }

        /// <summary>
        /// Tests if a point is inside the circle defined by
        /// the triangle with vertices a, b, c (oriented counter-clockwise).
        /// </summary>
        /// <remarks>
        /// <para> This test uses simple
        /// double-precision arithmetic, and thus is not 100% robust.
        /// However, by using normalization to the origin
        /// it provides improved robustness and increased performance.</para>
        /// <para>Based on code by J.R.Shewchuk.</para>
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns>true if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleNormalized(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            double adx = a.X - p.X;
            double ady = a.Y - p.Y;
            double bdx = b.X - p.X;
            double bdy = b.Y - p.Y;
            double cdx = c.X - p.X;
            double cdy = c.Y - p.Y;

            double abdet = adx*bdy - bdx*ady;
            double bcdet = bdx*cdy - cdx*bdy;
            double cadet = cdx*ady - adx*cdy;
            double alift = adx*adx + ady*ady;
            double blift = bdx*bdx + bdy*bdy;
            double clift = cdx*cdx + cdy*cdy;

            double disc = alift*bcdet + blift*cadet + clift*abdet;
            return disc > 0;
        }

        /// <summary>
        /// Computes twice the area of the oriented triangle (a, b, c), i.e., the area is positive if the
        /// triangle is oriented counterclockwise.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>The area of the triangle defined by the points a, b, c</returns>
        private static double TriArea(Coordinate a, Coordinate b, Coordinate c)
        {
            return (b.X - a.X)*(c.Y - a.Y)
                 - (b.Y - a.Y)*(c.X - a.X);
        }

        /// <summary>
        /// Tests if a point is inside the circle defined by
        /// the triangle with vertices a, b, c (oriented counter-clockwise).
        /// </summary>
        /// <remarks>
        /// This method uses more robust computation.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns>true if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleRobust(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            //checkRobustInCircle(a, b, c, p);
            //    return isInCircleNonRobust(a, b, c, p);
            return IsInCircleNormalized(a, b, c, p);
        }

        /// <summary>
        /// Tests if a point is inside the circle defined by
        /// the triangle with vertices a, b, c (oriented counter-clockwise).
        /// </summary>
        /// <remarks>
        /// The computation uses <see cref="DD"/> arithmetic for robustness, but a faster approach.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns>true if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleDDFast(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            var aTerm = (DD.Sqr(a.X) + DD.Sqr(a.Y)) * TriAreaDDFast(b, c, p);
            var bTerm = (DD.Sqr(b.X) + DD.Sqr(b.Y)) * TriAreaDDFast(a, c, p);
            var cTerm = (DD.Sqr(c.X) + DD.Sqr(c.Y)) * TriAreaDDFast(a, b, p);
            var pTerm = (DD.Sqr(p.X) + DD.Sqr(p.Y)) * TriAreaDDFast(a, b, c);

            var sum = aTerm - bTerm + cTerm - pTerm;
            bool isInCircle = sum.ToDoubleValue() > 0;

            return isInCircle;
        }

        /// <summary>
        /// Computes twice the area of the oriented triangle (a, b, c), i.e., the area
        /// is positive if the triangle is oriented counterclockwise.
        /// </summary>
        /// <remarks>
        /// The computation uses {@link DD} arithmetic for robustness.
        /// </remarks>
        /// <param name="a">a vertex of the triangle</param>
        /// <param name="b">a vertex of the triangle</param>
        /// <param name="c">a vertex of the triangle</param>
        /// <returns>The area of a triangle defined by the points a, b and c</returns>
        private static DD TriAreaDDFast(
            Coordinate a, Coordinate b, Coordinate c)
        {

            var t1 = (DD.ValueOf(b.X)-a.X) * (DD.ValueOf(c.Y)- a.Y);
            var t2 = (DD.ValueOf(b.Y)-a.Y) * (DD.ValueOf(c.X) -a.X);

            return t1 - t2;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsInCircleDDNormalized(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            var adx = DD.ValueOf(a.X)-p.X;
            var ady = DD.ValueOf(a.Y)-p.Y;
            var bdx = DD.ValueOf(b.X)-p.X;
            var bdy = DD.ValueOf(b.Y)-p.Y;
            var cdx = DD.ValueOf(c.X)-p.X;
            var cdy = DD.ValueOf(c.Y)-p.Y;

            var abdet = adx*bdy - bdx*ady;
            var bcdet = bdx*cdy - cdx*bdy;
            var cadet = cdx*ady - adx*cdy;
            var alift = adx*adx + ady*ady;
            var blift = bdx*bdx + bdy*bdy;
            var clift = cdx*cdx + cdy*cdy;

            var sum = alift * bcdet + blift* cadet + clift * abdet;

            bool isInCircle = sum.ToDoubleValue() > 0;

            return isInCircle;
        }

        /// <summary>
        /// Computes the inCircle test using distance from the circumcentre.
        /// Uses standard double-precision arithmetic.
        /// </summary>
        /// <remarks>
        /// In general this doesn't
        /// appear to be any more robust than the standard calculation. However, there
        /// is at least one case where the test point is far enough from the
        /// circumcircle that this test gives the correct answer.
        /// <pre>
        /// LINESTRING (1507029.9878 518325.7547, 1507022.1120341457 518332.8225183258,
        /// 1507029.9833 518325.7458, 1507029.9896965567 518325.744909031)
        /// </pre>
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns>The area of a triangle defined by the points a, b and c</returns>
        public static bool IsInCircleCC(Coordinate a, Coordinate b, Coordinate c,
                                        Coordinate p)
        {
            var cc = Triangle.Circumcentre(a, b, c);
            double ccRadius = a.Distance(cc);
            double pRadiusDiff = p.Distance(cc) - ccRadius;
            return pRadiusDiff <= 0;
        }

    }
}
