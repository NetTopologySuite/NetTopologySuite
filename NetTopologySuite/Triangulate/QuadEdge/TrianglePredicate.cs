using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
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
        /// double-precision arithmetic, and thus may not be robust.
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
        /// double-precision arithmetic, and thus is not 10% robust.
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
            DD px = DD.valueOf(p.X);
            DD py = DD.valueOf(p.Y);
            DD ax = DD.valueOf(a.X);
            DD ay = DD.valueOf(a.Y);
            DD bx = DD.valueOf(b.X);
            DD by = DD.valueOf(b.Y);
            DD cx = DD.valueOf(c.X);
            DD cy = DD.valueOf(c.Y);

            DD aTerm = (ax.multiply(ax).add(ay.multiply(ay)))
                .multiply(TriAreaDDSlow(bx, by, cx, cy, px, py));
            DD bTerm = (bx.multiply(bx).add(by.multiply(by)))
                .multiply(TriAreaDDSlow(ax, ay, cx, cy, px, py));
            DD cTerm = (cx.multiply(cx).add(cy.multiply(cy)))
                .multiply(TriAreaDDSlow(ax, ay, bx, by, px, py));
            DD pTerm = (px.multiply(px).add(py.multiply(py)))
                .multiply(TriAreaDDSlow(ax, ay, bx, by, cx, cy));

            DD sum = aTerm.subtract(bTerm).add(cTerm).subtract(pTerm);
            bool isInCircle = sum.doubleValue() > 0;

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
            return (bx.subtract(ax).multiply(cy.subtract(ay)).subtract(by.subtract(ay)
                                                                           .multiply(cx.subtract(ax))));
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
            DD aTerm = (DD.sqr(a.X).selfAdd(DD.sqr(a.Y)))
                .selfMultiply(TriAreaDDFast(b, c, p));
            DD bTerm = (DD.sqr(b.X).selfAdd(DD.sqr(b.Y)))
                .selfMultiply(TriAreaDDFast(a, c, p));
            DD cTerm = (DD.sqr(c.X).selfAdd(DD.sqr(c.Y)))
                .selfMultiply(TriAreaDDFast(a, b, p));
            DD pTerm = (DD.sqr(p.X).selfAdd(DD.sqr(p.Y)))
                .selfMultiply(TriAreaDDFast(a, b, c));

            DD sum = aTerm.selfSubtract(bTerm).selfAdd(cTerm).selfSubtract(pTerm);
            bool isInCircle = sum.doubleValue() > 0;

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

            DD t1 = DD.valueOf(b.X).selfSubtract(a.X)
                .selfMultiply(
                    DD.valueOf(c.Y).selfSubtract(a.Y));

            DD t2 = DD.valueOf(b.Y).selfSubtract(a.Y)
                .selfMultiply(
                    DD.valueOf(c.X).selfSubtract(a.X));

            return t1.selfSubtract(t2);
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
            DD adx = DD.valueOf(a.X).selfSubtract(p.X);
            DD ady = DD.valueOf(a.Y).selfSubtract(p.Y);
            DD bdx = DD.valueOf(b.X).selfSubtract(p.X);
            DD bdy = DD.valueOf(b.Y).selfSubtract(p.Y);
            DD cdx = DD.valueOf(c.X).selfSubtract(p.X);
            DD cdy = DD.valueOf(c.Y).selfSubtract(p.Y);

            DD abdet = adx.multiply(bdy).selfSubtract(bdx.multiply(ady));
            DD bcdet = bdx.multiply(cdy).selfSubtract(cdx.multiply(bdy));
            DD cadet = cdx.multiply(ady).selfSubtract(adx.multiply(cdy));
            DD alift = adx.multiply(adx).selfAdd(ady.multiply(ady));
            DD blift = bdx.multiply(bdx).selfAdd(bdy.multiply(bdy));
            DD clift = cdx.multiply(cdx).selfAdd(cdy.multiply(cdy));

            DD sum = alift.selfMultiply(bcdet)
                .selfAdd(blift.selfMultiply(cadet))
                .selfAdd(clift.selfMultiply(abdet));

            bool isInCircle = sum.doubleValue() > 0;

            return isInCircle;
        }

        /**
         * <p>
         * In general this doesn't
         * appear to be any more robust than the standard calculation. However, there
         * is at least one case where the test point is far enough from the
         * circumcircle that this test gives the correct answer. 
         * <pre>
         * LINESTRING
         * (1507029.9878 518325.7547, 1507022.1120341457 518332.8225183258,
         * 1507029.9833 518325.7458, 1507029.9896965567 518325.744909031)
         * </pre>
         * 
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @param p the point to test
         * @return true if this point is inside the circle defined by the points a, b, c
         */

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
            Console.WriteLine("p radius diff a = "
                              + Math.Abs(p.Distance(circumCentre) - a.Distance(circumCentre))
                              /a.Distance(circumCentre));

            if (nonRobustInCircle != isInCircleDD || nonRobustInCircle != isInCircleCC)
            {
                Console.WriteLine("inCircle robustness failure (double result = "
                                  + nonRobustInCircle
                                  + ", DD result = " + isInCircleDD
                                  + ", CC result = " + isInCircleCC + ")");
                Console.WriteLine(WKTWriter.ToLineString(new CoordinateArraySequence(new[] {a, b, c, p})));

                Console.WriteLine("Circumcentre = " + WKTWriter.ToPoint(circumCentre)
                                  + " radius = " + a.Distance(circumCentre));
                Console.WriteLine("p radius diff a = "
                                  + Math.Abs(p.Distance(circumCentre)/a.Distance(circumCentre) - 1));
                Console.WriteLine("p radius diff b = "
                                  + Math.Abs(p.Distance(circumCentre)/b.Distance(circumCentre) - 1));
                Console.WriteLine("p radius diff c = "
                                  + Math.Abs(p.Distance(circumCentre)/c.Distance(circumCentre) - 1));
                Console.WriteLine();
            }
        }


    }
}