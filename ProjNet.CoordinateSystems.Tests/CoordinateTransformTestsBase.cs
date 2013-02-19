using System;
using System.Globalization;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.UnitTests
{
    public class CoordinateTransformTestsBase
    {
        protected readonly CoordinateSystemFactory CoordinateSystemFactory = new CoordinateSystemFactory();
        protected readonly CoordinateTransformationFactory CoordinateTransformationFactory = new CoordinateTransformationFactory();

        protected bool Verbose { get; set; }

        protected bool ToleranceLessThan(double[] p1, double[] p2, double tolerance)
        {
            var d0 = Math.Abs(p1[0] - p2[0]);
            var d1 = Math.Abs(p1[1] - p2[1]);
            if (p1.Length > 2 && p2.Length > 2)
            {
                var d2 = Math.Abs(p1[2] - p2[2]);
                if (Verbose)
                    Console.WriteLine("Tolerance {3} dx: {0}, dy: {1}, dz {2}", d0, d1, d2, tolerance);
                return d0 < tolerance && d1 < tolerance && d2 < tolerance;
            }
            Console.WriteLine();
            if (Verbose)
                Console.WriteLine("Tolerance {2} dx: {0}, dy: {1}", d0, d1, tolerance);
            return d0 < tolerance && d1 < tolerance;
        }

        protected string TransformationError(string projection, double[] pExpected, double[] pResult, bool reverse = false)
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 "{6} {7} transformation outside tolerance!\n\tExpected [{0}, {1}],\n\tgot      [{2}, {3}],\n\tdelta    [{4}, {5}]",
                                 pExpected[0], pExpected[1], 
                                 pResult[0], pResult[1], 
                                 pExpected[0]-pResult[0], pExpected[1]-pResult[1],
                                 projection, reverse ? "reverse" : "forward");
        }
    }
}