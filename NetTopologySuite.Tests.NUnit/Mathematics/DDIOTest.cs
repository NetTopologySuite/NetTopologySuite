using System;
using System.Globalization;
using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    /// <summary>
    /// Tests I/O for <see cref="DD"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDIOTest
    {

        [Test]
        public void TestStandardNotation()
        {
            // standard cases
            CheckStandardNotation(1.0, "1.0");
            CheckStandardNotation(0.0, "0.0");

            // cases where hi is a power of 10 and lo is negative
            CheckStandardNotation(DD.ValueOf(1e12).Subtract(DD.ValueOf(1)),	"999999999999.0");
            CheckStandardNotation(DD.ValueOf(1e14).Subtract(DD.ValueOf(1)),	"99999999999999.0");
            CheckStandardNotation(DD.ValueOf(1e16).Subtract(DD.ValueOf(1)),	"9999999999999999.0");

            DD num8Dec = DD.ValueOf(-379363639).Divide(
                DD.ValueOf(100000000));
            CheckStandardNotation(num8Dec, "-3.79363639");

            CheckStandardNotation(new DD(-3.79363639, 8.039137357367426E-17),
                "-3.7936363900000000000000000");

            CheckStandardNotation(DD.ValueOf(34).Divide(
                DD.ValueOf(1000)), "0.034");
            CheckStandardNotation(1.05e3, "1050.0");
            CheckStandardNotation(0.34, "0.34000000000000002442490654175344");
            CheckStandardNotation(DD.ValueOf(34).Divide(
                DD.ValueOf(100)), "0.34");
            CheckStandardNotation(14, "14.0");
        }

        private static void CheckStandardNotation(double x, String expectedStr) {
            CheckStandardNotation(DD.ValueOf(x), expectedStr);
        }

        private static void CheckStandardNotation(DD x, String expectedStr) {
            String xStr = x.ToStandardNotation();
            //System.Console.WriteLine("Standard Notation: " + xStr);
            Assert.AreEqual(expectedStr, xStr);
        }

        [Test]
        public void TestSciNotation() {
            CheckSciNotation(0.0, "0.0E0");
            CheckSciNotation(1.05e10, "1.05E10");
            CheckSciNotation(0.34, "3.4000000000000002442490654175344E-1");
            CheckSciNotation(
                DD.ValueOf(34).Divide(DD.ValueOf(100)), "3.4E-1");
            CheckSciNotation(14, "1.4E1");
        }

        private static void CheckSciNotation(double x, String expectedStr) {
            CheckSciNotation(DD.ValueOf(x), expectedStr);
        }

        private static void CheckSciNotation(DD x, String expectedStr) {
            var xStr = x.ToSciNotation();
            //System.Console.WriteLine("Sci Notation: " + xStr);
            Assert.AreEqual(xStr, expectedStr);
        }

        [Test]
        public void TestParse() {
            CheckParse("0", 0, 1e-32);
            CheckParse("1", 1, 1e-32);
            CheckParse("100", 100, 1e-32);
            CheckParse("-1", -1, 1e-32);
            CheckParse("-100", -100, 1e-32);
            CheckParse("-123", -123, 1e-32);

            CheckParse(".1", 0.1, 1e-32);
            CheckParse("0", 0, 1e-32);
            CheckParse("1", 1, 1e-32);
            CheckParse("1.05e10", 1.05E10, 1e-32);
            CheckParse("-1.05e10", -1.05E10, 1e-32);
            CheckParse("1.05e-10", DD.ValueOf(105d).Divide(
                DD.ValueOf(100d)).Divide(DD.ValueOf(1.0E10)), 1e-32);
            CheckParse("-1.05e-10", DD.ValueOf(105d).Divide(
                    DD.ValueOf(100d)).Divide(DD.ValueOf(1.0E10))
                .Negate(), 1e-32);

        /**
		 * The Java double-precision constant 1.4 gives rise to a value which
		 * differs from the exact binary representation down around the 17th decimal
		 * place. Thus it will not compare exactly to the DoubleDouble
		 * representation of the same number. To avoid this, compute the expected
		 * value using full DD precision.
		 */
            CheckParse("1.4",
                DD.ValueOf(14).Divide(DD.ValueOf(10)), 1e-30);

            // 39.5D can be converted to an exact FP representation
            CheckParse("39.5", 39.5, 1e-30);
            CheckParse("-39.5", -39.5, 1e-30);
        }

        private static void CheckParse(String str, double expectedVal, double errBound)
        {
            CheckParse(str, new DD(expectedVal), errBound);
        }

        private static void CheckParse(String str, DD expectedVal,
            double relErrBound) {
            DD xdd = DD.Parse(str);
            var err = (xdd - expectedVal).ToDoubleValue();
            var relErr = err / xdd.ToDoubleValue();

            //System.Console.WriteLine(("Parsed= " + xdd + " rel err= " + relErr);

            Assert.IsTrue(err <= relErrBound,
                string.Format(NumberFormatInfo.InvariantInfo,
                    "Parsing '" + str + "' results in " + xdd.ToString() + " ( "
                              + xdd.Dump() + ") != " + expectedVal + "\n  err =" + err + ", relerr =" + relErr));
        }

        [Test]
        public void TestParseError() {
            CheckParseError("-1.05E2w");
            CheckParseError("%-1.05E2w");
            CheckParseError("-1.0512345678t");
        }

        private static void CheckParseError(String str) {
            var foundParseError = false;
            try {
                DD.Parse(str);
            } catch (FormatException ex) {
                foundParseError = true;
            }
            Assert.IsTrue(foundParseError);
        }

        [Test]
        public void TestRepeatedSqrt()
        {
            WriteRepeatedSqrt(DD.ValueOf(1.0));
            WriteRepeatedSqrt(DD.ValueOf(.999999999999));
            WriteRepeatedSqrt(DD.PI.Divide(DD.ValueOf(10)));
        }

        /// <summary>
        /// This routine simply tests for robustness of the ToString function.
        /// </summary>
        private static void WriteRepeatedSqrt(DD xdd)
        {
            int count = 0;
            while (xdd.ToDoubleValue() > 1e-300) {
                count++;
                //if (count == 100)
                //    count = count;
                double x = xdd.ToDoubleValue();
                DD xSqrt = xdd.Sqrt();
                String s = xSqrt.ToString();
                //System.Console.WriteLine((count + ": " + s);

                DD xSqrt2 = DD.Parse(s);
                DD xx = xSqrt2.Multiply(xSqrt2);
                double err = Math.Abs(xx.ToDoubleValue() - x);
                //assertTrue(err < 1e-10);

                xdd = xSqrt;

                // square roots converge on 1 - stop when very close
                DD distFrom1DD = xSqrt.Subtract(DD.ValueOf(1.0));
                double distFrom1 = distFrom1DD.ToDoubleValue();
                if (Math.Abs(distFrom1) < 1.0e-40)
                    break;
            }
        }

        public void testRepeatedSqr()
        {
            WriteRepeatedSqr(DD.ValueOf(.9));
            WriteRepeatedSqr(DD.PI.Divide(DD.ValueOf(10)));
        }

        /// <summary>
        /// This routine simply tests for robustness of the toString function.
        /// </summary>
        static void WriteRepeatedSqr(DD xdd)
        {
            if (xdd.GreaterOrEqualThan(DD.ValueOf(1)))
                throw new ArgumentException("Argument must be < 1");

            int count = 0;
            while (xdd.ToDoubleValue() > 1e-300) {
                count++;
                double x = xdd.ToDoubleValue();
                DD xSqr = xdd.Sqr();
                String s = xSqr.ToString();
                //System.Console.WriteLine(count + ": " + s);

                DD xSqr2 = DD.Parse(s);

                xdd = xSqr;
            }
        }

        [Test]
        public void TestIOSquaresStress() {
            for (int i = 1; i < 10000; i++) {
                WriteAndReadSqrt(i);
            }
        }

        /// <summary>
        /// Tests that printing values with many decimal places works.
        /// This tests the correctness and robustness of both output and input.
        /// </summary>
        static void WriteAndReadSqrt(double x) {
            DD xdd = DD.ValueOf(x);
            DD xSqrt = xdd.Sqrt();
            String s = xSqrt.ToString();
            //System.Console.WriteLine(s);

            DD xSqrt2 = DD.Parse(s);
            DD xx = xSqrt2 * xSqrt2;
            String xxStr = xx.ToString();
            //System.Console.WriteLine("==>  " + xxStr);

            DD xx2 = DD.Parse(xxStr);
            double err = Math.Abs(xx2.ToDoubleValue() - x);
            Assert.IsTrue(err < 1e-10);
        }

    }
}