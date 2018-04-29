using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Tests.NUnit.Performance.Mathematics
{
    public class TriPredicate
    {
        /// <summary>
        /// Tests if a point is inside the circle defined by the points a, b, c. 
        /// This test uses simple
        /// double-precision arithmetic, and thus may not be robust.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns><value>true</value> if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircle(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            var isInCircle =
                (a.X * a.X + a.Y * a.Y) * TriArea(b, c, p)
                - (b.X * b.X + b.Y * b.Y) * TriArea(a, c, p)
                + (c.X * c.X + c.Y * c.Y) * TriArea(a, b, p)
                - (p.X * p.X + p.Y * p.Y) * TriArea(a, b, c)
                > 0;
            return isInCircle;
        }

        /// <summary>
        /// Computes twice the area of the oriented triangle (a, b, c), i.e., the area is positive if the
        /// triangle is oriented counterclockwise.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <returns>Twice the area of the oriented triangle</returns>
        private static double TriArea(Coordinate a, Coordinate b, Coordinate c)
        {
            return (b.X - a.X) * (c.Y - a.Y)
                   - (b.Y - a.Y) * (c.X - a.X);
        }


        /// <summary>
        /// Tests if a point is inside the circle defined by the points a, b, c. 
        /// This test uses robust computation.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns><value>true</value> if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleRobust(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            //checkRobustInCircle(a, b, c, p);
            return IsInCircleDD(a, b, c, p);
        }

        /// <summary>
        /// Tests if a point is inside the circle defined by the points a, b, c. 
        /// The computation uses <see cref="NetTopologySuite.Mathematics.DD"/> arithmetic for robustness.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns><value>true</value> if this point is inside the circle defined by the points a, b, c</returns>
        [Obsolete]
        public static bool IsInCircleDD(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            DD px = new DD(p.X);
            DD py = new DD(p.Y);
            DD ax = new DD(a.X);
            DD ay = new DD(a.Y);
            DD bx = new DD(b.X);
            DD by = new DD(b.Y);
            DD cx = new DD(c.X);
            DD cy = new DD(c.Y);

            DD aTerm = (ax.Multiply(ax).Add(ay.Multiply(ay)))
                .Multiply(TriAreaDD(bx, by, cx, cy, px, py));
            DD bTerm = (bx.Multiply(bx).Add(by.Multiply(by)))
                .Multiply(TriAreaDD(ax, ay, cx, cy, px, py));
            DD cTerm = (cx.Multiply(cx).Add(cy.Multiply(cy)))
                .Multiply(TriAreaDD(ax, ay, bx, by, px, py));
            DD pTerm = (px.Multiply(px).Add(py.Multiply(py)))
                .Multiply(TriAreaDD(ax, ay, bx, by, cx, cy));

            DD sum = aTerm.Subtract(bTerm).Add(cTerm).Subtract(pTerm);
            var isInCircle = sum.ToDoubleValue() > 0;

            return isInCircle;
        }

        /// <summary>
        /// Tests if a point is inside the circle defined by the points a, b, c. 
        /// The computation uses <see cref="NetTopologySuite.Mathematics.DD"/> arithmetic for robustness.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns><value>true</value> if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleDD2(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            DD aTerm = (DD.Sqr(a.X) + DD.Sqr(a.Y)) * TriAreaDD2(b, c, p);
            DD bTerm = (DD.Sqr(b.X) + DD.Sqr(b.Y)) * TriAreaDD2(a, c, p);
            DD cTerm = (DD.Sqr(c.X) + DD.Sqr(c.Y)) * TriAreaDD2(a, b, p);
            DD pTerm = (DD.Sqr(p.X) + DD.Sqr(p.Y)) * TriAreaDD2(a, b, c);

            DD sum = aTerm - bTerm + cTerm - pTerm;
            var isInCircle = sum.ToDoubleValue() > 0;

            return isInCircle;
        }

        /// <summary>
        /// Tests if a point is inside the circle defined by the points a, b, c. 
        /// The computation uses <see cref="NetTopologySuite.Mathematics.DD"/> arithmetic for robustness.
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns><value>true</value> if this point is inside the circle defined by the points a, b, c</returns>
        public static bool IsInCircleDD3(
            Coordinate a, Coordinate b, Coordinate c,
            Coordinate p)
        {
            DD adx = DD.ValueOf(a.X) - (p.X);
            DD ady = DD.ValueOf(a.Y) - (p.Y);
            DD bdx = DD.ValueOf(b.X) - (p.X);
            DD bdy = DD.ValueOf(b.Y) - (p.Y);
            DD cdx = DD.ValueOf(c.X) - (p.X);
            DD cdy = DD.ValueOf(c.Y) - (p.Y);

            DD abdet = adx * bdy - (bdx * ady);
            DD bcdet = bdx * cdy - (cdx * bdy);
            DD cadet = cdx * ady - (adx * cdy);
            DD alift = adx * adx - (ady * ady);
            DD blift = bdx * bdx - (bdy * bdy);
            DD clift = cdx * cdx - (cdy * cdy);

            DD sum = alift * bcdet + blift * cadet + clift * abdet;

            var isInCircle = sum.ToDoubleValue() > 0;

            return isInCircle;
        }

        /// <summary>
        /// Computes twice the area of the oriented triangle (a, b, c), i.e., the area
        /// is positive if the triangle is oriented counterclockwise.
        /// The computation uses <see cref="DD"/> arithmetic for robustness.
        /// </summary>
        /// <param name="ax">The x ordinate of a vertex of the triangle</param>
        /// <param name="ay">The y ordinate of a vertex of the triangle</param>
        /// <param name="bx">The x ordinate of a vertex of the triangle</param>
        /// <param name="by">The y ordinate of a vertex of the triangle</param>
        /// <param name="cx">The x ordinate of a vertex of the triangle</param>
        /// <param name="cy">The y ordinate of a vertex of the triangle</param>
        /// <returns>Twice the area of the oriented triangle</returns>
        [Obsolete]
        public static DD TriAreaDD(DD ax, DD ay,
                                   DD bx, DD by, DD cx, DD cy)
        {
            return (bx.Subtract(ax).Multiply(cy.Subtract(ay)).Subtract(by.Subtract(ay)
                                                                         .Multiply(cx.Subtract(ax))));
        }

        public static DD TriAreaDD2(
            Coordinate a, Coordinate b, Coordinate c)
        {

            DD t1 = (DD.ValueOf(b.X) - a.X) * (DD.ValueOf(c.Y) - a.Y);
            DD t2 = (DD.ValueOf(b.Y) - a.Y) * (DD.ValueOf(c.X) - a.X);

            return t1 - t2;
        }

        /// <summary>
        /// Computes the inCircle test using distance from the circumcentre. 
        /// Uses standard double-precision arithmetic.
        /// <para/>
        /// In general this doesn't
        /// appear to be any more robust than the standard calculation. However, there
        /// is at least one case where the test point is far enough from the
        /// circumcircle that this test gives the correct answer. 
        /// <pre>
        /// LINESTRING
        /// (1507029.9878 518325.7547, 1507022.1120341457 518332.8225183258,
        ///  1507029.9833 518325.7458, 1507029.9896965567 518325.744909031)</pre>
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point to test</param>
        /// <returns><value>true</value> if this point is inside the circle defined by the points a, b, c</returns>
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
            var nonRobustInCircle = IsInCircle(a, b, c, p);
            var isInCircleDD = TriPredicate.IsInCircleDD(a, b, c, p);
            var isInCircleCC = TriPredicate.IsInCircleCC(a, b, c, p);

            Coordinate circumCentre = Triangle.Circumcentre(a, b, c);
            Console.WriteLine("p radius diff a = "
                              + Math.Abs(p.Distance(circumCentre) - a.Distance(circumCentre))
                              / a.Distance(circumCentre));

            if (nonRobustInCircle != isInCircleDD || nonRobustInCircle != isInCircleCC)
            {
                Console.WriteLine("inCircle robustness failure (double result = "
                                  + nonRobustInCircle
                                  + ", DD result = " + isInCircleDD
                                  + ", CC result = " + isInCircleCC + ")");
                Console.WriteLine(WKTWriter.ToLineString(new CoordinateArraySequence(
                                                             new Coordinate[] { a, b, c, p })));
                Console.WriteLine("Circumcentre = " + WKTWriter.ToPoint(circumCentre)
                                  + " radius = " + a.Distance(circumCentre));
                Console.WriteLine("p radius diff a = "
                                  + Math.Abs(p.Distance(circumCentre) / a.Distance(circumCentre) - 1));
                Console.WriteLine("p radius diff b = "
                                  + Math.Abs(p.Distance(circumCentre) / b.Distance(circumCentre) - 1));
                Console.WriteLine("p radius diff c = "
                                  + Math.Abs(p.Distance(circumCentre) / c.Distance(circumCentre) - 1));
                Console.WriteLine();
            }
        }


    }
}