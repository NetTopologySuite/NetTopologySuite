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
            //System.Console.WriteLine("--------------------------------");
            //System.Console.WriteLine("Computing e by Taylor series");
            var testE = ComputeEByTaylorSeries();
            var err = Math.Abs(testE.Subtract(DD.E).ToDoubleValue());
            //System.Console.WriteLine("Difference from DoubleDouble.E = " + err);
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
            //System.Console.WriteLine("--------------------------------");
            //System.Console.WriteLine("Computing Pi by Machin's rule");
            var testE = ComputePiByMachin();
            var err = Math.Abs(testE.Subtract(DD.PI).ToDoubleValue());
            //System.Console.WriteLine("Difference from DoubleDouble.PI = " + err);
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
            //System.Console.WriteLine("Computed DD.atan(): " + at
            //                        +"    Math.atan = " + Math.Atan(x.ToDoubleValue()));
            return at;
        }
    }
}