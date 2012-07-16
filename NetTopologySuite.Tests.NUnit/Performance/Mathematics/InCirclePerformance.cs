using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Tests.NUnit.Performance.Mathematics
{
    /**
     * Test performance of evaluating Triangle predicate computations
     * using 
     * various extended precision APIs.
     * 
     * @author Martin Davis
     *
     */

    [Category("Stress")]
    public class InCirclePerf
    {


        private readonly Coordinate _pa = new Coordinate(687958.05, 7460725.97);
        private readonly Coordinate _pb = new Coordinate(687957.43, 7460725.93);
        private readonly Coordinate _pc = new Coordinate(687957.58, 7460721);
        private readonly Coordinate _pp = new Coordinate(687958.13, 7460720.99);

        [Test]
        public void Test()
        {
            Console.WriteLine("InCircle perf");
            int n = 1000000;
            double doubleTime = RunDouble(n);
            double ddSelfTime = RunDDSelf(n);
            double ddSelf2Time = runDDSelf2(n);
            double ddTime = RunDD(n);
            //		double ddSelfTime = runDoubleDoubleSelf(10000000);

            Console.WriteLine("DD VS double performance factor      = " + ddTime/doubleTime);
            Console.WriteLine("DDSelf VS double performance factor  = " + ddSelfTime/doubleTime);
            Console.WriteLine("DDSelf2 VS double performance factor = " + ddSelf2Time/doubleTime);
        }

        public double RunDouble(int nIter)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircle(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("double:   nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double RunDD(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircleDD(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("DD:       nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double RunDDSelf(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircleDD2(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("DD-Self:  nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double runDDSelf2(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircleDD3(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("DD-Self2: nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        /**
         * Algorithms for computing values and predicates
         * associated with triangles.
         * For some algorithms extended-precision
         * versions are provided, which are more robust
         * (i.e. they produce correct answers in more cases).
         * These are used in triangulation algorithms.
         * 
         * @author Martin Davis
         *
         */

        public class TriPredicate
        {
            /**
             * Tests if a point is inside the circle defined by the points a, b, c. 
             * This test uses simple
             * double-precision arithmetic, and thus may not be robust.
             * 
             * @param a a vertex of the triangle
             * @param b a vertex of the triangle
             * @param c a vertex of the triangle
             * @param P the point to test
             * @return true if this point is inside the circle defined by the points a, b, c
             */

            public static bool IsInCircle(
                Coordinate a, Coordinate b, Coordinate c,
                Coordinate p)
            {
                var isInCircle =
                    (a.X*a.X + a.Y*a.Y)*TriArea(b, c, p)
                    - (b.X*b.X + b.Y*b.Y)*TriArea(a, c, p)
                    + (c.X*c.X + c.Y*c.Y)*TriArea(a, b, p)
                    - (p.X*p.X + p.Y*p.Y)*TriArea(a, b, c)
                    > 0;
                return isInCircle;
            }

            /**
             * Computes twice the area of the oriented triangle (a, b, c), i.e., the area is positive if the
             * triangle is oriented counterclockwise.
             * 
             * @param a a vertex of the triangle
             * @param b a vertex of the triangle
             * @param c a vertex of the triangle
             */

            private static double TriArea(Coordinate a, Coordinate b, Coordinate c)
            {
                return (b.X - a.X)*(c.Y - a.Y)
                       - (b.Y - a.Y)*(c.X - a.X);
            }

            /**
             * Tests if a point is inside the circle defined by the points a, b, c. 
             * This test uses robust computation.
             * 
             * @param a a vertex of the triangle
             * @param b a vertex of the triangle
             * @param c a vertex of the triangle
             * @param P the point to test
             * @return true if this point is inside the circle defined by the points a, b, c
             */

            public static bool IsInCircleRobust(
                Coordinate a, Coordinate b, Coordinate c,
                Coordinate p)
            {
                //checkRobustInCircle(a, b, c, p);
                return IsInCircleDD(a, b, c, p);
            }

            /**
             * Tests if a point is inside the circle defined by the points a, b, c. 
             * The computation uses {@link DD} arithmetic for robustness.
             * 
             * @param a a vertex of the triangle
             * @param b a vertex of the triangle
             * @param c a vertex of the triangle
             * @param P the point to test
             * @return true if this point is inside the circle defined by the points a, b, c
             */

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

            public static bool IsInCircleDD2(
                Coordinate a, Coordinate b, Coordinate c,
                Coordinate p)
            {
                DD aTerm = (DD.Sqr(a.X).SelfAdd(DD.Sqr(a.Y)))
                    .SelfMultiply(TriAreaDD2(b, c, p));
                DD bTerm = (DD.Sqr(b.X).SelfAdd(DD.Sqr(b.Y)))
                    .SelfMultiply(TriAreaDD2(a, c, p));
                DD cTerm = (DD.Sqr(c.X).SelfAdd(DD.Sqr(c.Y)))
                    .SelfMultiply(TriAreaDD2(a, b, p));
                DD pTerm = (DD.Sqr(p.X).SelfAdd(DD.Sqr(p.Y)))
                    .SelfMultiply(TriAreaDD2(a, b, c));

                DD sum = aTerm.SelfSubtract(bTerm).SelfAdd(cTerm).SelfSubtract(pTerm);
                var isInCircle = sum.ToDoubleValue() > 0;

                return isInCircle;
            }

            public static bool IsInCircleDD3(
                Coordinate a, Coordinate b, Coordinate c,
                Coordinate p)
            {
                DD adx = DD.ValueOf(a.X).SelfSubtract(p.X);
                DD ady = DD.ValueOf(a.Y).SelfSubtract(p.Y);
                DD bdx = DD.ValueOf(b.X).SelfSubtract(p.X);
                DD bdy = DD.ValueOf(b.Y).SelfSubtract(p.Y);
                DD cdx = DD.ValueOf(c.X).SelfSubtract(p.X);
                DD cdy = DD.ValueOf(c.Y).SelfSubtract(p.Y);

                DD abdet = adx.Multiply(bdy).SelfSubtract(bdx.Multiply(ady));
                DD bcdet = bdx.Multiply(cdy).SelfSubtract(cdx.Multiply(bdy));
                DD cadet = cdx.Multiply(ady).SelfSubtract(adx.Multiply(cdy));
                DD alift = adx.Multiply(adx).SelfSubtract(ady.Multiply(ady));
                DD blift = bdx.Multiply(bdx).SelfSubtract(bdy.Multiply(bdy));
                DD clift = cdx.Multiply(cdx).SelfSubtract(cdy.Multiply(cdy));

                DD sum = alift.SelfMultiply(bcdet)
                    .SelfAdd(blift.SelfMultiply(cadet))
                    .SelfAdd(clift.SelfMultiply(abdet));

                var isInCircle = sum.ToDoubleValue() > 0;

                return isInCircle;
            }

            /**
             * Computes twice the area of the oriented triangle (a, b, c), i.e., the area
             * is positive if the triangle is oriented counterclockwise.
             * The computation uses {@link DD} arithmetic for robustness.
             * 
             * @param ax the x ordinate of a vertex of the triangle
             * @param ay the y ordinate of a vertex of the triangle
             * @param bx the x ordinate of a vertex of the triangle
             * @param by the y ordinate of a vertex of the triangle
             * @param cx the x ordinate of a vertex of the triangle
             * @param cy the y ordinate of a vertex of the triangle
             */

            public static DD TriAreaDD(DD ax, DD ay,
                                       DD bx, DD by, DD cx, DD cy)
            {
                return (bx.Subtract(ax).Multiply(cy.Subtract(ay)).Subtract(by.Subtract(ay)
                                                                               .Multiply(cx.Subtract(ax))));
            }

            public static DD TriAreaDD2(
                Coordinate a, Coordinate b, Coordinate c)
            {

                DD t1 = DD.ValueOf(b.X).SelfSubtract(a.X)
                    .SelfMultiply(
                        DD.ValueOf(c.Y).SelfSubtract(a.Y));

                DD t2 = DD.ValueOf(b.Y).SelfSubtract(a.Y)
                    .SelfMultiply(
                        DD.ValueOf(c.X).SelfSubtract(a.X));

                return t1.SelfSubtract(t2);
            }

            /**
             * Computes the inCircle test using distance from the circumcentre. 
             * Uses standard double-precision arithmetic.
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

            public static bool IsInCircleCC(Coordinate a, Coordinate b, Coordinate c,
                                            Coordinate p)
            {
                Coordinate cc = Triangle.Circumcentre(a, b, c);
                double ccRadius = a.Distance(cc);
                double pRadiusDiff = p.Distance(cc) - ccRadius;
                return pRadiusDiff <= 0;
            }

            /**
             * Checks if the computed value for isInCircle is correct, using
             * double-double precision arithmetic.
             * 
             * @param a a vertex of the triangle
             * @param b a vertex of the triangle
             * @param c a vertex of the triangle
             * @param p the point to test
             */

            private static void CheckRobustInCircle(Coordinate a, Coordinate b, Coordinate c,
                                                    Coordinate p)
            {
                var nonRobustInCircle = IsInCircle(a, b, c, p);
                var isInCircleDD = TriPredicate.IsInCircleDD(a, b, c, p);
                var isInCircleCC = TriPredicate.IsInCircleCC(a, b, c, p);

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
                    Console.WriteLine(WKTWriter.ToLineString(new CoordinateArraySequence(
                                                                 new Coordinate[] {a, b, c, p})));
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
}