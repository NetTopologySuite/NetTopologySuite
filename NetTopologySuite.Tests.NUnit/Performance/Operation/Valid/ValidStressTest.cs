using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Valid
{
    /// <summary>
    /// Stress-tests <see cref="IsValidOp"/> on invalid polygons with many intersections.
    /// by running it on an invalid MultiPolygon with many intersections.
    /// In NTS 1.14 and earlier this takes a very long time to run,
    /// since all intersections are computed before the invalid result is returned.
    /// In fact it is only necessary to detect a single intersection in order
    /// to determine invalidity.
    /// </summary>
    /// <author>mdavis</author>
    public class ValidStressTest
    {

        public static int SIZE = 10000;

        private static readonly GeometryFactory geomFact = new GeometryFactory();

        [Test,Ignore("Runs without end")]
        public void run()
        {
            var env = new Envelope(0, 100, 0, 100);
            var mp = CrossingCombs(env);
            //System.out.println(mp);
            var isValid = mp.IsValid;
            Debug.WriteLine("Is Valid = " + isValid);
        }

        private IMultiPolygon CrossingCombs(Envelope env)
        {
            var comb1 = Comb(env, SIZE);
            var centre = env.Centre;
            var trans = AffineTransformation.RotationInstance(0.5 * Math.PI, centre.X, centre.Y);
            var comb2 = (IPolygon)trans.Transform(comb1);
            var mp = geomFact.CreateMultiPolygon(new [] { comb1, comb2 });
            return mp;
        }

        static IPolygon Comb(Envelope env, int nArms)
        {
            int npts = 4*(nArms - 1) + 2 + 2 + 1;
            Coordinate[] pts = new Coordinate[npts];
            double armWidth = env.Width/(2*nArms - 1);
            double armLen = env.Height - armWidth;

            double xBase = env.MinX;
            double yBase = env.MinY;

            int ipts = 0;
            for (int i = 0; i < nArms; i++)
            {
                double x1 = xBase + i*2*armWidth;
                double y1 = yBase + armLen + armWidth;
                pts[ipts++] = new Coordinate(x1, y1);
                pts[ipts++] = new Coordinate(x1 + armWidth, y1);
                if (i < nArms - 1)
                {
                    pts[ipts++] = new Coordinate(x1 + armWidth, yBase + armWidth);
                    pts[ipts++] = new Coordinate(x1 + 2*armWidth, yBase + armWidth);
                }
            }
            pts[ipts++] = new Coordinate(env.MaxX, yBase);
            pts[ipts++] = new Coordinate(xBase, yBase);
            pts[ipts] = new Coordinate(pts[0]);

            return GeomFact.CreatePolygon(pts);
        }

    }
}