using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    /// <summary>
    /// Tests the correctness of Orientation Index computed with DD arithmetic.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DDOrientationIndexCorrectTest : GeometryTestCase
    {
        /// <summary>
        /// This test captures a situation where
        /// the DD orientation apparently fails.
        /// <para/>
        /// According to the stated decimal representation
        /// the two orientations should be equal, but they are not.
        /// <para/>
        /// Even more disturbingly, the orientationIndexFilter
        /// handles this case, without dropping through to
        /// the actual DD code.  The result is the same, however.
        /// </summary>
        [Test]
        public void TestPointCloseToLine()
        {
            Coordinate[] pts =
            {
                new Coordinate(2.4829102, 48.8726807),
                new Coordinate(2.4832535, 48.8737106),
                new Coordinate(2.4830818249999997, 48.873195575)
            };
            var orientDD = RunDD("Orginal case", pts);
            //System.out.println("DD - Alt: " + orientDD);

            Coordinate[] ptsScale =
            {
                new Coordinate(24829102, 488726807),
                new Coordinate(24832535, 488737106),
                new Coordinate(24830818.249999997, 488731955.75)
            };
            var orientSC = RunDD("Scaled case", ptsScale);
            //System.out.println("DD - Alt: " + orientDD);

            /*
             * Same arrangement as above, but translated 
             * by removing digits before decimal point
             * to reduce numeric precision
             */
            Coordinate[] ptsLowPrec =
            {
                new Coordinate(0.4829102, 0.8726807),
                new Coordinate(0.4832535, 0.8737106),
                new Coordinate(0.4830818249999997, 0.873195575)
            };
            var orientLP = RunDD("Lower precision case", ptsLowPrec);

            /*
             * By adjusting the point slightly it lies exactly on the line
             */
            Coordinate[] ptOnLineScaled =
            {
                new Coordinate(24829102, 488726807),
                new Coordinate(24832535, 488737106),
                new Coordinate(24830818.25, 488731955.75)
            };
            var orientOLSC = RunDD("On-line scaled case", ptOnLineScaled);
            Assert.That(orientOLSC, Is.EqualTo(OrientationIndex.None));

            /*
             * By adjusting the point slightly it lies exactly on the line
             */
            Coordinate[] ptOnLine =
            {
                new Coordinate(2.4829102, 48.8726807),
                new Coordinate(2.4832535, 48.8737106),
                new Coordinate(2.483081825, 48.873195575)
            };
            var orientOL = RunDD("On-line case", ptOnLine);
            //assertTrue(orientOL == 0);

            // this fails in JTS also
            //Assert.That(orientDD == orientLP, Is.True, "Orignal index not equal to lower-precision index");

        }

        private OrientationIndex RunDD(string desc, Coordinate[] pts)
        {
            var orientDD = Orientation.Index(pts[0], pts[1], pts[2]);
            //int orientSD = ShewchuksDeterminant.orientationIndex(pts[0], pts[1], pts[2]); 
            //int orientAlt = orientationIndexAlt(pts[0], pts[1], pts[2]); 

            Console.WriteLine($"{desc} --------------");
            Console.WriteLine($"DD: {orientDD}");
            return orientDD;
        }

        public static int OrientationIndexAlt(Coordinate p1, Coordinate p2, Coordinate q)
        {
            // normalize coordinates
            var dx1 = ToDDAlt(p2.X) + ToDDAlt(-p1.X);
            var dy1 = ToDDAlt(p2.Y) + ToDDAlt(-p1.Y);
            var dx2 = ToDDAlt(q.X) + ToDDAlt(-p2.X);
            var dy2 = ToDDAlt(q.Y) + ToDDAlt(-p2.Y);

            // sign of determinant - unrolled for performance
            var det = dx1 * dy2 - dy1 * dx2;
            return det.Signum();
        }

        private static DD ToDDAlt(double x)
        {
            // convert more accurately to DD from decimal representation
            // very slow though - should be a better way
            return DD.ValueOf(x + "");
        }
    }

}
