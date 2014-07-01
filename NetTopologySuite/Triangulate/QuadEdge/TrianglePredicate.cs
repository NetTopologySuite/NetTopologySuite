using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Mathematics;
using Wintellect;

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
        /// The computation uses <see cref="DD"/> arithmetic for robustness.
        /// </remarks>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns>true if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleDDSlow(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            DD px = DD.ValueOf(p.X);
            DD py = DD.ValueOf(p.Y);
            DD ax = DD.ValueOf(a.X);
            DD ay = DD.ValueOf(a.Y);
            DD bx = DD.ValueOf(b.X);
            DD by = DD.ValueOf(b.Y);
            DD cx = DD.ValueOf(c.X);
            DD cy = DD.ValueOf(c.Y);

            DD aTerm = (ax.Multiply(ax).Add(ay.Multiply(ay)))
                .Multiply(TriAreaDDSlow(bx, by, cx, cy, px, py));
            DD bTerm = (bx.Multiply(bx).Add(by.Multiply(by)))
                .Multiply(TriAreaDDSlow(ax, ay, cx, cy, px, py));
            DD cTerm = (cx.Multiply(cx).Add(cy.Multiply(cy)))
                .Multiply(TriAreaDDSlow(ax, ay, bx, by, px, py));
            DD pTerm = (px.Multiply(px).Add(py.Multiply(py)))
                .Multiply(TriAreaDDSlow(ax, ay, bx, by, cx, cy));

            DD sum = aTerm.Subtract(bTerm).Add(cTerm).Subtract(pTerm);
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
        /// <param name="ax">x ordinate of a vertex of the triangle</param>
        /// <param name="ay">y ordinate of a vertex of the triangle</param>
        /// <param name="bx">x ordinate of a vertex of the triangle</param>
        /// <param name="by">y ordinate of a vertex of the triangle</param>
        /// <param name="cx">x ordinate of a vertex of the triangle</param>
        /// <param name="cy">y ordinate of a vertex of the triangle</param>
        /// <returns>The area of a triangle defined by the points a, b and c</returns>
        private static DD TriAreaDDSlow(DD ax, DD ay,
                                       DD bx, DD by, DD cx, DD cy)
        {
            return (bx.Subtract(ax).Multiply(cy.Subtract(ay)).Subtract(by.Subtract(ay)
                                                                           .Multiply(cx.Subtract(ax))));
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
            DD aTerm = (DD.Sqr(a.X) + DD.Sqr(a.Y)) * TriAreaDDFast(b, c, p);
            DD bTerm = (DD.Sqr(b.X) + DD.Sqr(b.Y)) * TriAreaDDFast(a, c, p);
            DD cTerm = (DD.Sqr(c.X) + DD.Sqr(c.Y)) * TriAreaDDFast(a, b, p);
            DD pTerm = (DD.Sqr(p.X) + DD.Sqr(p.Y)) * TriAreaDDFast(a, b, c);

            DD sum = aTerm - bTerm + cTerm - pTerm;
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

            DD t1 = (DD.ValueOf(b.X)-a.X) * (DD.ValueOf(c.Y)- a.Y);
            DD t2 = (DD.ValueOf(b.Y)-a.Y) * (DD.ValueOf(c.X) -a.X);

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
            DD adx = DD.ValueOf(a.X)-p.X;
            DD ady = DD.ValueOf(a.Y)-p.Y;
            DD bdx = DD.ValueOf(b.X)-p.X;
            DD bdy = DD.ValueOf(b.Y)-p.Y;
            DD cdx = DD.ValueOf(c.X)-p.X;
            DD cdy = DD.ValueOf(c.Y)-p.Y;

            DD abdet = adx*bdy - bdx*ady;
            DD bcdet = bdx*cdy - cdx*bdy;
            DD cadet = cdx*ady - adx*cdy;
            DD alift = adx*adx + ady*ady;
            DD blift = bdx*bdx + bdy*bdy;
            DD clift = cdx*cdx + cdy*cdy;

            DD sum = alift * bcdet + blift* cadet + clift * abdet;

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
            Coordinate cc = Triangle.Circumcentre(a, b, c);
            double ccRadius = a.Distance(cc);
            double pRadiusDiff = p.Distance(cc) - ccRadius;
            return pRadiusDiff <= 0;
        }

        /// <summary>
        /// Checks if the computed value for isInCircle is correct, using
        /// double-double precision arithmetic.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        private static void CheckRobustInCircle(Coordinate a, Coordinate b, Coordinate c,
                                                Coordinate p)
        {
            bool nonRobustInCircle = IsInCircleNonRobust(a, b, c, p);
            bool isInCircleDD = IsInCircleDDSlow(a, b, c, p);
            bool isInCircleCC = IsInCircleCC(a, b, c, p);

            Coordinate circumCentre = Triangle.Circumcentre(a, b, c);
            System.Diagnostics.Debug.WriteLine("p radius diff a = "
                              + Math.Abs(p.Distance(circumCentre) - a.Distance(circumCentre))
                              /a.Distance(circumCentre));

            if (nonRobustInCircle != isInCircleDD || nonRobustInCircle != isInCircleCC)
            {
                System.Diagnostics.Debug.WriteLine("inCircle robustness failure (double result = "
                                  + nonRobustInCircle
                                  + ", DD result = " + isInCircleDD
                                  + ", CC result = " + isInCircleCC + ")");
                System.Diagnostics.Debug.WriteLine(WKTWriter.ToLineString(new CoordinateArraySequence(new[] { a, b, c, p })));

                System.Diagnostics.Debug.WriteLine("Circumcentre = " + WKTWriter.ToPoint(circumCentre)
                                  + " radius = " + a.Distance(circumCentre));
                System.Diagnostics.Debug.WriteLine("p radius diff a = "
                                  + Math.Abs(p.Distance(circumCentre)/a.Distance(circumCentre) - 1));
                System.Diagnostics.Debug.WriteLine("p radius diff b = "
                                  + Math.Abs(p.Distance(circumCentre)/b.Distance(circumCentre) - 1));
                System.Diagnostics.Debug.WriteLine("p radius diff c = "
                                  + Math.Abs(p.Distance(circumCentre)/c.Distance(circumCentre) - 1));
                System.Diagnostics.Debug.WriteLine("");
            }
        }


    }
}