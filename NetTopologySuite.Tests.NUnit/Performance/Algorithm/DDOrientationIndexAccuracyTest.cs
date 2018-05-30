using System;
using System.Globalization;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    /// <summary>
    /// Test the accuracy of DD orientation index computation,
    /// using the built-in double value conversion and an experimental
    /// conversion approach with better decimal accuracy.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDOrientationIndexAccuracyTest : GeometryTestCase
    {

        [Test]
        public void TestRightTriangleForDeterminant()
        {
            CheckLine45(1, 100, 100);
        }

        private void CheckLine45(int width, int nPts, double precision)
        {
            Coordinate p1 = new Coordinate(0, width);
            Coordinate p2 = new Coordinate(width, 0);
            for (int i = 0; i <= nPts; i++)
            {
                var d = width / (double) nPts;
                var q = new Coordinate(0.0 + i * d, width - i * d);
                var pm = new PrecisionModel(precision);
                pm.MakePrecise(q);
                CheckPointOnSeg(p1, p2, q);
            }
        }

        private void CheckPointOnSeg(Coordinate p1, Coordinate p2, Coordinate q)
        {
            var ddStd = OrientationDet(p1, p2, q, DD_STD);
            var ddDec = OrientationDet(p1, p2, q, DD_DEC);

            Console.WriteLine("  Pt: " + WKTWriter.ToPoint(q) + "  seg: " + WKTWriter.ToLineString(p1, p2)
                              + " --- DDstd = " + ddStd + " --- DDdec = " + ddDec
            );
        }

        public static DD OrientationDet(Coordinate p1, Coordinate p2, Coordinate q, Func<double, DD> convert)
        {
            // normalize coordinates
            DD dx1 = convert(p2.X) + convert(-p1.X);
            DD dy1 = convert(p2.Y) + convert(-p1.Y);
            DD dx2 = convert(q.X) + convert(-p2.X);
            DD dy2 = convert(q.Y) + convert(-p2.Y);

            // sign of determinant - unrolled for performance
            return dx1 * dy2 - dy1 * dx2;
        }

        /*
        private const bool UseAccurateConversion = false;

        private static DD convertToDD(double x)
        {
            if (UseAccurateConversion)
            {
                // convert more accurately to DD from decimal representation
                // very slow though - should be a better way
                return DD.ValueOf(x + "");
            }

            // current built-in conversion - introduces jitter
            return DD.ValueOf(x);
        }
        */

        static DD DD_STD(double x)
        {
            var res = DD.ValueOf(x);
            //Console.WriteLine($"STD: {x.ToString(NumberFormatInfo.InvariantInfo)} -> {res.} ({res.Dump()})");
            return res;
        }

        static DD DD_DEC(double x)
        {
            var res = DD.ValueOf(x.ToString("R", NumberFormatInfo.InvariantInfo));
            //Console.WriteLine($"DEC: {x} -> {res} ({res.Dump()})");
            return res;
        }

    }
}