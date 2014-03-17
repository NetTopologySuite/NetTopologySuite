using System;
using System.Diagnostics;
using NUnit.Framework;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Tests.NUnit.Performance.Mathematics
{
    /// <summary>
    /// Times evaluating floating-point expressions using 
    /// various extended precision APIs.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDExpressionPerformance
    {
        [TestAttribute]
        public void Test()
        {
            Run(1000000);
        }

        public void Run(int n)
        {
            if (n == 0) 
                n = 1000000;
            
            var doubleTime = RunDouble(n);
            var ddTime = RunDoubleDouble(n);
            var ddSelfTime = RunDoubleDoubleSelf(n);
            var bigDecTime = RunDecimal(n);

            Console.WriteLine("Decimal VS double performance factor = " + bigDecTime/doubleTime);
            Console.WriteLine("Decimal VS DD performance factor = " + bigDecTime/ddTime);

            Console.WriteLine("DD VS double performance factor = " + ddTime/doubleTime);
            Console.WriteLine("DD-Self VS double performance factor = " + ddSelfTime/doubleTime);

        }

        public double RunDouble(int nIter)
        {
            var det = 0d;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                var a = 9.0;
                const double factor = 10.0;

                var aMul = factor*a;
                var aDiv = a/factor;

                det = a*a - aMul*aDiv;
                //			Console.WriteLine(det);
            }
            sw.Stop();
            Console.WriteLine("double:          nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds / (double)nIter;
        }


        public double RunDecimal(int nIter)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < nIter; i++)
            {


                var a = new decimal(9.0);
                var factor = new decimal(10.0);
                var aMul = decimal.Multiply(factor, a);
                var aDiv = decimal.Round(decimal.Divide(a, factor), MidpointRounding.AwayFromZero);

                var det = decimal.Subtract(decimal.Multiply(a, a), decimal.Multiply(aMul, aDiv));
                //			Console.WriteLine(aDiv);
                //			Console.WriteLine(det);
            }
            sw.Stop();
            Console.WriteLine("BigDecimal:      nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double RunDoubleDouble(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {

                DD a = new DD(9.0);
                DD factor = new DD(10.0);
                DD aMul = factor.Multiply(a);
                DD aDiv = a.Divide(factor);

                DD det = a.Multiply(a)
                    .Subtract(aMul.Multiply(aDiv));
                //      Console.WriteLine(aDiv);
                //      Console.WriteLine(det);
            }
            sw.Stop();
            Console.WriteLine("DD:              nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }


        //  public double XrunDoubleDoubleSelf(int nIter)
        //  {
        //    Stopwatch sw = new Stopwatch();
        //    for (int i = 0; i < nIter; i++) {

        //      DD a = new DD(9.0);
        //      DD factor = new DD(10.0);
        //      DD aMul = factor.multiply(a);
        //      DD aDiv = a.divide(factor);

        //      DD det = a.multiply(a)
        //          .subtract(aMul.multiply(aDiv));
        ////      Console.WriteLine(aDiv);
        ////      Console.WriteLine(det);
        //    }
        //    sw.Stop();
        //    Console.WriteLine("DD:              nIter = " + nIter 
        //        + "   time = " + sw.ElapsedMilliseconds);
        //    return sw.ElapsedMilliseconds / (double) nIter;
        //  }

        //*
        public double RunDoubleDoubleSelf(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {

                double a = 9.0;
                double factor = 10.0;
                DD c = new DD(9.0);
                c*=factor;
                DD b = new DD(9.0);
                b/=factor;

                DD a2 = new DD(a);
                a2*=a;
                DD b2 = new DD(b);
                b2*=c;
                a2/=b2;
                DD det = a2;
                //			Console.WriteLine(aDiv);
                //			Console.WriteLine(det);
            }
            sw.Stop();
            Console.WriteLine("DD-Self:         nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }
    }
}