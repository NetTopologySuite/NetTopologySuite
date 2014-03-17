using System;
using NUnit.Framework;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    /// <summary>
    /// Tests basic arithmetic operations for <see cref="DD"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDBasicTest
    {

        [TestAttribute]
        public void TestNaN()
        {
            Assert.IsTrue(DD.IsNaN(DD.ValueOf(1).Divide(DD.ValueOf(0))));
            Assert.IsTrue(DD.IsNaN(DD.ValueOf(1).Multiply(DD.NaN)));
        }

        [TestAttribute]
        public void TestAddMult2()
        {
            CheckAddMult2(new DD(3));
            CheckAddMult2(DD.PI);
        }

        [TestAttribute]
        public void TestMultiplyDivide()
        {
            CheckMultiplyDivide(DD.PI, DD.E, 1e-30);
            CheckMultiplyDivide(DD.TwoPi, DD.E, 1e-30);
            CheckMultiplyDivide(DD.PiHalf, DD.E, 1e-30);
            CheckMultiplyDivide(new DD(39.4), new DD(10), 1e-30);
        }

        [TestAttribute]
        public void TestDivideMultiply()
        {
            CheckDivideMultiply(DD.PI, DD.E, 1e-30);
            CheckDivideMultiply(new DD(39.4), new DD(10), 1e-30);
        }

        [TestAttribute]
        public void TestSqrt()
        {
            // the appropriate error bound is determined empirically
            CheckSqrt(DD.PI, 1e-30);
            CheckSqrt(DD.E, 1e-30);
            CheckSqrt(new DD(999.0), 1e-28);
        }

        private void CheckSqrt(DD x, double errBound)
        {
            DD sqrt = x.Sqrt();
            DD x2 = sqrt.Multiply(sqrt);
            CheckErrorBound("Sqrt", x, x2, errBound);
        }

        [TestAttribute]
        public void TestTrunc()
        {
            CheckTrunc(DD.ValueOf(1e16).Subtract(DD.ValueOf(1)),
                       DD.ValueOf(1e16).Subtract(DD.ValueOf(1)));
            // the appropriate error bound is determined empirically
            CheckTrunc(DD.PI, DD.ValueOf(3));
            CheckTrunc(DD.ValueOf(999.999), DD.ValueOf(999));

            CheckTrunc(DD.E.Negate(), DD.ValueOf(-2));
            CheckTrunc(DD.ValueOf(-999.999), DD.ValueOf(-999));
        }

        private static void CheckTrunc(DD x, DD expected)
        {
            DD trunc = x.Truncate();
            var isEqual = trunc.Equals(expected);
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
            //  	checkBinomialSquare(1e16, 1.0);

            CheckBinomialSquare(1e14, 291.0);
            CheckBinomialSquare(5e14, 291.0);
            CheckBinomialSquare(5e14, 345291.0);
        }

        private static void CheckAddMult2(DD dd)
        {
            DD sum = dd.Add(dd);
            DD prod = dd.Multiply(new DD(2.0));
            CheckErrorBound("AddMult2", sum, prod, 0.0);
        }

        private static void CheckMultiplyDivide(DD a, DD b, double errBound)
        {
            DD a2 = a.Multiply(b).Divide(b);
            CheckErrorBound("MultiplyDivide", a, a2, errBound);
        }

        private static void CheckDivideMultiply(DD a, DD b, double errBound)
        {
            DD a2 = a.Divide(b).Multiply(b);
            CheckErrorBound("DivideMultiply", a, a2, errBound);
        }

        private static DD Delta(DD x, DD y)
        {
            return x.Subtract(y).Abs();
        }

        private static void CheckErrorBound(String tag, DD x, DD y, double errBound)
        {
            DD err = x.Subtract(y).Abs();
            Console.WriteLine(tag + " err=" + err);
            var isWithinEps = err.ToDoubleValue() <= errBound;
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
            var aPlusb = add.Add(bdd);
            var abSq = aPlusb.Multiply(aPlusb);
            //  	System.out.println("(a+b)^2 = " + abSq);

            // expansion
            var a2DD = add.Multiply(add);
            var b2DD = bdd.Multiply(bdd);
            var ab = add.Multiply(bdd);
            var sum = b2DD.Add(ab).Add(ab);

            //  	System.out.println("2ab+b^2 = " + sum);

            var diff = abSq.Subtract(a2DD);
            //  	System.out.println("(a+b)^2 - a^2 = " + diff);

            var delta = diff.Subtract(sum);

            Console.WriteLine("\nA = " + a + ", B = " + b);
            Console.WriteLine("[DD]     2ab+b^2 = " + sum
                              + "   (a+b)^2 - a^2 = " + diff
                              + "   delta = " + delta);
            PrintBinomialSquareDouble(a, b);

            var isSame = diff.Equals(sum);
            Assert.IsTrue(isSame);
            var isDeltaZero = delta.IsZero;
            Assert.IsTrue(isDeltaZero);
        }

        private static void PrintBinomialSquareDouble(double a, double b)
        {
            var sum = 2*a*b + b*b;
            var diff = (a + b)*(a + b) - a*a;
            Console.WriteLine("[double] 2ab+b^2= " + sum
                              + "   (a+b)^2-a^2= " + diff
                              + "   delta= " + (sum - diff));
        }

        [TestAttribute]
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
            var aPlusb = add.Add(bdd);
            var aSubb = add.Subtract(bdd);
            var abProd = aPlusb.Multiply(aSubb);
            //  	System.out.println("(a+b)^2 = " + abSq);

            // expansion
            var a2DD = add.Multiply(add);
            var b2DD = bdd.Multiply(bdd);

            //  	System.out.println("2ab+b^2 = " + sum);

            // this should equal b^2
            var diff = abProd.Subtract(a2DD).Negate();
            //  	System.out.println("(a+b)^2 - a^2 = " + diff);

            var delta = diff.Subtract(b2DD);

            Console.WriteLine("\nA = " + a + ", B = " + b);
            Console.WriteLine("[DD] (a+b)(a-b) = " + abProd
                              + "   -((a^2 - b^2) - a^2) = " + diff
                              + "   delta = " + delta);
            //  	printBinomialSquareDouble(a,b);

            var isSame = diff.Equals(b2DD);
            Assert.IsTrue(isSame);
            var isDeltaZero = delta.IsZero;
            Assert.IsTrue(isDeltaZero);
        }


        private static void CheckReciprocal(double x, double errBound)
        {
            var xdd = new DD(x);
            var rr = xdd.Reciprocal().Reciprocal();

            var err = xdd.Subtract(rr).ToDoubleValue();

            Console.WriteLine("DD Recip = " + xdd
                              + " DD delta= " + err
                              + " double recip delta= " + (x - 1.0/(1.0/x)));

            Assert.IsTrue(err <= errBound);
        }

        private static void CheckPow(double x, int exp, double errBound)
        {
            var xdd = new DD(x);
            var pow = xdd.Pow(exp);
            Console.WriteLine("Pow(" + x + ", " + exp + ") = " + pow);
            var pow2 = SlowPow(xdd, exp);

            double err = pow.Subtract(pow2).ToDoubleValue();

            var isOK = err < errBound;
            if (!isOK)
                Console.WriteLine("Test slowPow value " + pow2);

            Assert.IsTrue(err <= errBound);
        }

        private static DD SlowPow(DD x, int exp)
        {
            if (exp == 0)
                return DD.ValueOf(1.0);

            var n = Math.Abs(exp);
            // MD - could use binary exponentiation for better precision & speed
            var pow = new DD(x);
            for (int i = 1; i < n; i++)
            {
                pow = pow.Multiply(x);
            }
            if (exp < 0)
            {
                return pow.Reciprocal();
            }
            return pow;
        }
    }
}