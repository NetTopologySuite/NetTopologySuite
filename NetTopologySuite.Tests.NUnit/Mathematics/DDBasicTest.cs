using System;
using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    /// <summary>
    /// Tests basic arithmetic operations for <see cref="DD"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDBasicTest
    {

        [Test]
        public void TestNaN()
        {
            Assert.IsTrue(DD.IsNaN(DD.ValueOf(1) / DD.ValueOf(0)));
            Assert.IsTrue(DD.IsNaN(DD.ValueOf(1) * DD.NaN));
        }

        [Test]
        public void TestAddMult2()
        {
            CheckAddMult2(new DD(3));
            CheckAddMult2(DD.PI);
        }

        [Test]
        public void TestMultiplyDivide()
        {
            CheckMultiplyDivide(DD.PI, DD.E, 1e-30);
            CheckMultiplyDivide(DD.TwoPi, DD.E, 1e-30);
            CheckMultiplyDivide(DD.PiHalf, DD.E, 1e-30);
            CheckMultiplyDivide(new DD(39.4), new DD(10), 1e-30);
        }

        [Test]
        public void TestDivideMultiply()
        {
            CheckDivideMultiply(DD.PI, DD.E, 1e-30);
            CheckDivideMultiply(new DD(39.4), new DD(10), 1e-30);
        }

        [Test]
        public void TestSqrt()
        {
            // the appropriate error bound is determined empirically
            CheckSqrt(DD.PI, 1e-30);
            CheckSqrt(DD.E, 1e-30);
            CheckSqrt(new DD(999.0), 1e-28);
        }

        private void CheckSqrt(DD x, double errBound)
        {
            var sqrt = x.Sqrt();
            var x2 = sqrt * sqrt;
            CheckErrorBound("Sqrt", x, x2, errBound);
        }

        [Test]
        public void TestTrunc()
        {
            CheckTrunc(DD.ValueOf(1e16) - DD.ValueOf(1),
                       DD.ValueOf(1e16) - DD.ValueOf(1));
            // the appropriate error bound is determined empirically
            CheckTrunc(DD.PI, DD.ValueOf(3));
            CheckTrunc(DD.ValueOf(999.999), DD.ValueOf(999));

            CheckTrunc(DD.E.Negate(), DD.ValueOf(-2));
            CheckTrunc(DD.ValueOf(-999.999), DD.ValueOf(-999));
        }

        private static void CheckTrunc(DD x, DD expected)
        {
            var trunc = x.Truncate();
            bool isEqual = trunc.Equals(expected);
            Assert.True(isEqual);
            isEqual = trunc == expected;
            Assert.True(isEqual);
        }

        public void TestPow()
        {
            CheckPow(0, 3, 16*DD.Epsilon);
            CheckPow(14, 3, 16*DD.Epsilon);
            CheckPow(3, -5, 16*DD.Epsilon);
            CheckPow(-3, 5, 16*DD.Epsilon);
            CheckPow(-3, -5, 16*DD.Epsilon);
            CheckPow(0.12345, -5, 1e5*DD.Epsilon);
        }

        public void TestReciprocal()
        {
            // error bounds are chosen to be "close enough" (i.e. heuristically)

            // for some reason many reciprocals are exact
            CheckReciprocal(3.0, 0);
            CheckReciprocal(99.0, 1e-29);
            CheckReciprocal(999.0, 0);
            CheckReciprocal(314159269.0, 0);
        }

        public void TestBinom()
        {
            CheckBinomialSquare(100.0, 1.0);
            CheckBinomialSquare(1000.0, 1.0);
            CheckBinomialSquare(10000.0, 1.0);
            CheckBinomialSquare(100000.0, 1.0);
            CheckBinomialSquare(1000000.0, 1.0);
            CheckBinomialSquare(1e8, 1.0);
            CheckBinomialSquare(1e10, 1.0);
            CheckBinomialSquare(1e14, 1.0);
            // Following call will fail, because it requires 32 digits of precision
            // checkBinomialSquare(1e16, 1.0);

            CheckBinomialSquare(1e14, 291.0);
            CheckBinomialSquare(5e14, 291.0);
            CheckBinomialSquare(5e14, 345291.0);
        }

        private static void CheckAddMult2(DD dd)
        {
            var sum = dd + dd;
            var prod = dd * new DD(2.0);
            CheckErrorBound("AddMult2", sum, prod, 0.0);
        }

        private static void CheckMultiplyDivide(DD a, DD b, double errBound)
        {
            var a2 = a * b / b;
            CheckErrorBound("MultiplyDivide", a, a2, errBound);
        }

        private static void CheckDivideMultiply(DD a, DD b, double errBound)
        {
            var a2 = a / b * b;
            CheckErrorBound("DivideMultiply", a, a2, errBound);
        }

        private static DD Delta(DD x, DD y)
        {
            return (x - y).Abs();
        }

        private static void CheckErrorBound(string tag, DD x, DD y, double errBound)
        {
            var err = (x - y).Abs();
            Console.WriteLine(tag + " err=" + err);
            bool isWithinEps = err.ToDoubleValue() <= errBound;
            Assert.True(isWithinEps);
        }

        /**
         * Computes (a+b)^2 in two different ways and compares the result.
         * For correct results, a and b should be integers.
         *
         * @param a
         * @param b
         */

        private static void CheckBinomialSquare(double a, double b)
        {
            // binomial square
            var add = new DD(a);
            var bdd = new DD(b);
            var aPlusb = add + bdd;
            var abSq = aPlusb * aPlusb;
            // System.out.println("(a+b)^2 = " + abSq);

            // expansion
            var a2DD = add * add;
            var b2DD = bdd * bdd;
            var ab = add * bdd;
            var sum = b2DD + ab + ab;

            // System.out.println("2ab+b^2 = " + sum);

            var diff = abSq - a2DD;
            // System.out.println("(a+b)^2 - a^2 = " + diff);

            var delta = diff - sum;

            // System.Console.WriteLine("\nA = " + a + ", B = " + b);
            // System.Console.WriteLine("[DD]     2ab+b^2 = " + sum
            //                          + "   (a+b)^2 - a^2 = " + diff
            //                          + "   delta = " + delta);
            PrintBinomialSquareDouble(a, b);

            bool isSame = diff.Equals(sum);
            Assert.IsTrue(isSame);
            bool isDeltaZero = delta.IsZero;
            Assert.IsTrue(isDeltaZero);
        }

        private static void PrintBinomialSquareDouble(double a, double b)
        {
            double sum = 2*a*b + b*b;
            double diff = (a + b)*(a + b) - a*a;
            // Console.WriteLine("[double] 2ab+b^2= " + sum
            //                   + "   (a+b)^2-a^2= " + diff
            //                   + "   delta= " + (sum - diff));
        }

        [Test]
        public void TestBinomial2()
        {
            CheckBinomial2(100.0, 1.0);
            CheckBinomial2(1000.0, 1.0);
            CheckBinomial2(10000.0, 1.0);
            CheckBinomial2(100000.0, 1.0);
            CheckBinomial2(1000000.0, 1.0);
            CheckBinomial2(1e8, 1.0);
            CheckBinomial2(1e10, 1.0);
            CheckBinomial2(1e14, 1.0);

            CheckBinomial2(1e14, 291.0);

            CheckBinomial2(5e14, 291.0);
            CheckBinomial2(5e14, 345291.0);
        }

        private static void CheckBinomial2(double a, double b)
        {
            // binomial product
            var add = new DD(a);
            var bdd = new DD(b);
            var aPlusb = add + bdd;
            var aSubb = add - bdd;
            var abProd = aPlusb * aSubb;
            // System.out.println("(a+b)^2 = " + abSq);

            // expansion
            var a2DD = add * add;
            var b2DD = bdd * bdd;

            // System.out.println("2ab+b^2 = " + sum);

            // this should equal b^2
            var diff = (abProd - a2DD).Negate();
            // System.out.println("(a+b)^2 - a^2 = " + diff);

            var delta = diff - b2DD;

            // System.Console.WriteLine("\nA = " + a + ", B = " + b);
            // System.Console.WriteLine("[DD] (a+b)(a-b) = " + abProd
            //                          + "   -((a^2 - b^2) - a^2) = " + diff
            //                          + "   delta = " + delta);
            // printBinomialSquareDouble(a,b);

            bool isSame = diff.Equals(b2DD);
            Assert.IsTrue(isSame);
            bool isDeltaZero = delta.IsZero;
            Assert.IsTrue(isDeltaZero);
        }

        private static void CheckReciprocal(double x, double errBound)
        {
            var xdd = new DD(x);
            var rr = xdd.Reciprocal().Reciprocal();

            double err = (xdd - rr).ToDoubleValue();

            // System.Console.WriteLine("DD Recip = " + xdd
            //                          + " DD delta= " + err
            //                          + " double recip delta= " + (x - 1.0/(1.0/x)));

            Assert.IsTrue(err <= errBound);
        }

        private static void CheckPow(double x, int exp, double errBound)
        {
            var xdd = new DD(x);
            var pow = xdd.Pow(exp);
            // System.Console.WriteLine("Pow(" + x + ", " + exp + ") = " + pow);
            var pow2 = SlowPow(xdd, exp);

            double err = (pow - pow2).ToDoubleValue();

            bool isOK = err < errBound;
            if (!isOK)
                Console.WriteLine("Test slowPow value " + pow2);

            Assert.IsTrue(err <= errBound);
        }

        private static DD SlowPow(DD x, int exp)
        {
            if (exp == 0)
                return DD.ValueOf(1.0);

            int n = Math.Abs(exp);
            // MD - could use binary exponentiation for better precision & speed
            var pow = new DD(x);
            for (int i = 1; i < n; i++)
            {
                pow *= x;
            }
            if (exp < 0)
            {
                return pow.Reciprocal();
            }
            return pow;
        }
    }
}
