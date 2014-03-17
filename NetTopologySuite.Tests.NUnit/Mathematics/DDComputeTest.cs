using System;
using NUnit.Framework;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    /// <summary>
    /// Various tests involving computing known mathematical quantities
    /// using the basic <see cref="DD"/> arithmetic operations.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDComputeTest
    {
        [TestAttribute]
        public void TestEByTaylorSeries()
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Computing e by Taylor series");
            var testE = ComputeEByTaylorSeries();
            var err = Math.Abs(testE.Subtract(DD.E).ToDoubleValue());
            Console.WriteLine("Difference from DoubleDouble.E = " + err);
            Assert.IsTrue(err < 64*DD.Epsilon);
        }
        
        /// <summary>
        /// Uses Taylor series to compute e
        /// <para/>
        /// e = 1 + 1 + 1/2! + 1/3! + 1/4! + ...
        /// </summary>
        /// <returns>An approximation to e</returns>
        private static DD ComputeEByTaylorSeries()
        {
            var s = DD.ValueOf(2.0);
            var t = DD.ValueOf(1.0);
            var n = 1.0;
            var i = 0;

            while (t.ToDoubleValue() > DD.Epsilon)
            {
                i++;
                n += 1.0;
                t = t.Divide(DD.ValueOf(n));
                s = s.Add(t);
                Console.WriteLine(i + ": " + s);
            }
            return s;
        }

        [TestAttribute]
        public void TestPiByMachin()
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Computing Pi by Machin's rule");
            var testE = ComputePiByMachin();
            var err = Math.Abs(testE.Subtract(DD.PI).ToDoubleValue());
            Console.WriteLine("Difference from DoubleDouble.PI = " + err);
            Assert.IsTrue(err < 8*DD.Epsilon);
        }

        /// <summary>
        /// Uses Machin's arctangent formula to compute Pi:
        /// <para/>
        /// Pi / 4  =  4 arctan(1/5) - arctan(1/239)
        /// </summary>
        /// <returns>An approximation to Pi</returns>
        private static DD ComputePiByMachin()
        {
            var t1 = DD.ValueOf(1.0).Divide(DD.ValueOf(5.0));
            var t2 = DD.ValueOf(1.0).Divide(DD.ValueOf(239.0));

            var pi4 = (DD.ValueOf(4.0)
                .Multiply(ArcTan(t1)))
                .Subtract(ArcTan(t2));
            var pi = DD.ValueOf(4.0).Multiply(pi4);
            Console.WriteLine("Computed value = " + pi);
            return pi;
        }

        /// <summary>
        /// Computes the arctangent based on the Taylor series expansion
        /// <para/>
        /// arctan(x) = x - x^3 / 3 + x^5 / 5 - x^7 / 7 + ...
        /// </summary>
        /// <param name="x">The argument</param>
        /// <returns>An approximation to the arctangent of the input</returns>
        private static DD ArcTan(DD x)
        {
            var t = x;
            var t2 = t.Sqr();
            var at = new DD(0.0);
            var two = new DD(2.0);
            var k = 0;
            var d = new DD(1.0);
            var sign = 1;
            while (t.ToDoubleValue() > DD.Epsilon)
            {
                k++;
                at = sign < 0 ? at.Subtract(t.Divide(d)) : at.Add(t.Divide(d));

                d = d.Add(two);
                t = t.Multiply(t2);
                sign = -sign;
            }
            Console.WriteLine("Computed DD.atan(): " + at
                              + "    Math.atan = " + Math.Atan(x.ToDoubleValue()));
            return at;
        }
    }

    /// <summary>
    /// Tests I/O for <see cref="DD"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
public class DDIOTest 
{

    [TestAttribute]
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
		Console.WriteLine("Standard Notation: " + xStr);
		Assert.AreEqual(expectedStr, xStr);
	}

    [TestAttribute]
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
		Console.WriteLine("Sci Notation: " + xStr);
		Assert.AreEqual(xStr, expectedStr);
	}

    [TestAttribute]
	public void TestParse() {
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
		double err = xdd.Subtract(expectedVal).ToDoubleValue();
		double relErr = err / xdd.ToDoubleValue();

		Console.WriteLine("Parsed= " + xdd + " rel err= " + relErr);

		Assert.IsTrue(err <= relErrBound);
	}

    [TestAttribute]
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

    [TestAttribute]
	public void TestRepeatedSqrt()
	{
		WriteRepeatedSqrt(DD.ValueOf(1.0));
		WriteRepeatedSqrt(DD.ValueOf(.999999999999));
		WriteRepeatedSqrt(DD.PI.Divide(DD.ValueOf(10)));
	}
	
	/**
	 * This routine simply tests for robustness of the toString function.
	 * 
	 * @param xdd
	 */
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
//			System.out.println(count + ": " + s);

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
	
	/**
	 * This routine simply tests for robustness of the toString function.
	 * 
	 * @param xdd
	 */
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
			Console.WriteLine(count + ": " + s);

			DD xSqr2 = DD.Parse(s);
	
			xdd = xSqr;
		}
	}
	
    [TestAttribute]
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
//		System.out.println(s);

		DD xSqrt2 = DD.Parse(s);
		DD xx = xSqrt2 * xSqrt2;
		String xxStr = xx.ToString();
//		System.out.println("==>  " + xxStr);

		DD xx2 = DD.Parse(xxStr);
		double err = Math.Abs(xx2.ToDoubleValue() - x);
		Assert.IsTrue(err < 1e-10);
	}

}
}