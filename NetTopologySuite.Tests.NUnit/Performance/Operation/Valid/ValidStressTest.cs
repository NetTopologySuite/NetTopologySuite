using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Valid
{
    /// <summary>
    /// Stress-tests <see cref="IsValidOp"/> on invalid polygons with many intersections.
    /// by running it on an invalid MultiPolygon with many intersections.
    /// In NTS 1.14 and earlier this takes a very long time to run,
    /// since all intersections are computed before the invalid result is returned.
    /// In fact it is only necessary to detect a single intersection in order
    /// to determine invalidity, and this provides much faster performance.
    /// </summary>
    /// <author>mdavis</author>
    public class ValidStressTest
    {

        //public static int SIZE = 10000;

        private static readonly GeometryFactory geomFact = new GeometryFactory();

        [Test]
        public void runComb()
        {
            const int size = 400;
            var env = new Envelope(0, 100, 0, 100);
            var geom = Comb.CrossedComb(env, size, geomFact);
            //System.Console.WriteLine(geom);
            checkValid("Crossed combs (size = " + size + ")", geom);
        }

        [Test]
        public void runStarCrossRing()
        {
            const int size = 1000;
            var env = new Envelope(0, 100, 0, 100);
            var poly = StarCross.Star(env, size, geomFact);
            var geom = poly.Boundary;
            //System.Console.WriteLine(geom);
            checkValid("StarCross " + geom.GeometryType + "   (size = " + size + ")", geom);
        }

        [Test]
        public void runStarCrossPoly()
        {
            const int size = 1000;
            var env = new Envelope(0, 100, 0, 100);
            var geom = StarCross.Star(env, size, geomFact);
            //System.out.println(geom);
            checkValid("StarCross " + geom.GeometryType + "   (size = " + size + ")", geom);
        }

        private void checkValid(string name, IGeometry g)
        {
            System.Console.WriteLine("Running " + name);
            var sw = new Stopwatch();
            sw.Start();
            var isValid = g.IsValid;
            sw.Stop();
            System.Console.WriteLine("Is Valid = {0}, Ticks: {1:N0}", isValid, sw.ElapsedTicks);
        }
    }

    internal class StarCross
    {
        public static IPolygon Star(Envelope env, int nSeg, IGeometryFactory geomFact)
        {
            Coordinate[] pts = new Coordinate[nSeg + 1];
            Coordinate centre = env.Centre;
            double len = 0.5 * System.Math.Min(env.Height, env.Width);
            double angInc = System.Math.PI + 2 * System.Math.PI / nSeg;

            double ang = 0;
            for (int i = 0; i < nSeg; i++)
            {
                double x = centre.X + len * System.Math.Cos(ang);
                double y = centre.X + len * System.Math.Sin(ang);
                pts[i] = new Coordinate(x, y);
                ang += angInc;
            }
            pts[nSeg] = new Coordinate(pts[0]);
            return geomFact.CreatePolygon(pts);
        }
    }

    /// <summary>
    /// Creates comb-like geometries.
    /// Crossed combs provide a geometry with a very high ratio of intersections to edges.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class Comb
    {

        public static IMultiPolygon CrossedComb(Envelope env, int size, GeometryFactory geomFact)
        {
            var comb1 = CreateComb(env, size, geomFact);
            var centre = env.Centre;
            var trans = AffineTransformation.RotationInstance(0.5 * System.Math.PI, centre.X, centre.Y);
            var comb2 = (IPolygon)trans.Transform(comb1);
            var mp = geomFact.CreateMultiPolygon(new [] { comb1, comb2 });
            return mp;
        }

        private static IPolygon CreateComb(Envelope env, int nArms, IGeometryFactory geomFact)
        {
            int npts = 4 * (nArms - 1) + 2 + 2 + 1;
            Coordinate[] pts = new Coordinate[npts];
            double armWidth = env.Width / (2 * nArms - 1);
            double armLen = env.Height - armWidth;

            double xBase = env.MinX;
            double yBase = env.MinY;

            int ipts = 0;
            for (int i = 0; i < nArms; i++)
            {
                double x1 = xBase + i * 2 * armWidth;
                double y1 = yBase + armLen + armWidth;
                pts[ipts++] = new Coordinate(x1, y1);
                pts[ipts++] = new Coordinate(x1 + armWidth, y1);
                if (i < nArms - 1)
                {
                    pts[ipts++] = new Coordinate(x1 + armWidth, yBase + armWidth);
                    pts[ipts++] = new Coordinate(x1 + 2 * armWidth, yBase + armWidth);
                }
            }
            pts[ipts++] = new Coordinate(env.MaxX, yBase);
            pts[ipts++] = new Coordinate(xBase, yBase);
            pts[ipts++] = new Coordinate(pts[0]);

            return geomFact.CreatePolygon(pts);
        }

    }
}