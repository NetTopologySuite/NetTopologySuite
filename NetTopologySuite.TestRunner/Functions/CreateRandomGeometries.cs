using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public static class CreateRandomGeometryFunctions
    {
        private static Random RND = new Random();

        public static IGeometry RandomPointsInGrid(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);

            int nCell = (int)Math.Sqrt(nPts) + 1;

            double xLen = env.Width / nCell;
            double yLen = env.Height / nCell;

            var pts = new List<IPoint>();

            for (int i = 0; i < nCell; i++)
            {
                for (int j = 0; j < nCell; j++)
                {
                    double x = env.MinX + i * xLen + xLen * RND.NextDouble();
                    double y = env.MinY + j * yLen + yLen * RND.NextDouble();
                    pts.Add(geomFact.CreatePoint(new Coordinate(x, y)));
                }
            }
            return geomFact.BuildGeometry(pts.ToArray());
        }

        public static IGeometry RandomPoints(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            double xLen = env.Width;
            double yLen = env.Height;

            var pts = new List<IPoint>();

            for (int i = 0; i < nPts; i++)
            {
                double x = env.MinX + xLen * RND.NextDouble();
                double y = env.MinY + yLen * RND.NextDouble();
                pts.Add(geomFact.CreatePoint(new Coordinate(x, y)));
            }
            return geomFact.BuildGeometry(pts.ToArray());
        }

        public static IGeometry RandomRadialPoints(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            double xLen = env.Width;
            double yLen = env.Height;
            double rMax = Math.Min(xLen, yLen) / 2.0;

            double centreX = env.MinX + xLen / 2;
            double centreY = env.MinY + yLen / 2;

            var pts = new List<IPoint>();

            for (int i = 0; i < nPts; i++)
            {
                double rand = RND.NextDouble();
                //use rand^2 to accentuate radial distribution
                double r = rMax * rand * rand;
                double ang = 2 * Math.PI * RND.NextDouble();
                double x = centreX + r * Math.Cos(ang);
                double y = centreY + r * Math.Sin(ang);
                pts.Add(geomFact.CreatePoint(new Coordinate(x, y)));
            }
            return geomFact.BuildGeometry(pts.ToArray());
        }

        /// <summary>
        /// Create Halton points using bases 2 and 3.
        /// </summary>
        public static IGeometry HaltonPoints(IGeometry g, int nPts)
        {
            return HaltonPointsWithBases(g, nPts, 2, 3);
        }

        /// <summary>
        /// Create Halton points using bases 5 and 7.
        /// </summary>
        public static IGeometry HaltonPoints57(IGeometry g, int nPts)
        {
            return HaltonPointsWithBases(g, nPts, 5, 7);
        }

        public static IGeometry HaltonPointsWithBases(IGeometry g, int nPts, int basei, int basej)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var pts = new Coordinate[nPts];
            double baseX = env.MinX;
            double baseY = env.MinY;

            int i = 0;
            while (i < nPts)
            {
                double x = baseX + env.Width * HaltonOrdinate(i + 1, basei);
                double y = baseY + env.Height * HaltonOrdinate(i + 1, basej);
                var p = new Coordinate(x, y);
                if (env.Contains(p))
                    pts[i++] = p.Copy();
            }
            return FunctionsUtil.GetFactoryOrDefault(g).CreateMultiPointFromCoords(pts);
        }

        private static double HaltonOrdinate(int index, int basis)
        {
            double result = 0;
            double f = 1.0 / basis;
            int i = index;
            while (i > 0)
            {
                result = result + f * (i % basis);
                i = (int)Math.Floor(i / (double)basis);
                f = f / basis;
            }
            return result;
        }

        public static IGeometry RandomSegments(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            double xLen = env.Width;
            double yLen = env.Height;

            var lines = new List<IGeometry>();

            for (int i = 0; i < nPts; i++)
            {
                double x0 = env.MinX + xLen * RND.NextDouble();
                double y0 = env.MinY + yLen * RND.NextDouble();
                double x1 = env.MinX + xLen * RND.NextDouble();
                double y1 = env.MinY + yLen * RND.NextDouble();
                lines.Add(geomFact.CreateLineString(new[]
                    {
                        new Coordinate(x0, y0), new Coordinate(x1, y1)
                    }));
            }
            return geomFact.BuildGeometry(lines);
        }

        public static IGeometry RandomSegmentsInGrid(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);

            int nCell = (int)Math.Sqrt(nPts) + 1;

            double xLen = env.Width / nCell;
            double yLen = env.Height / nCell;

            var lines = new List<IGeometry>();

            for (int i = 0; i < nCell; i++)
            {
                for (int j = 0; j < nCell; j++)
                {
                    double x0 = env.MinX + i * xLen + xLen * RND.NextDouble();
                    double y0 = env.MinY + j * yLen + yLen * RND.NextDouble();
                    double x1 = env.MinX + i * xLen + xLen * RND.NextDouble();
                    double y1 = env.MinY + j * yLen + yLen * RND.NextDouble();
                    lines.Add(geomFact.CreateLineString(new[]
                        {
                            new Coordinate(x0, y0), new Coordinate(x1, y1)
                        }));
                }
            }
            return geomFact.BuildGeometry(lines);
        }

        public static IGeometry RandomLineString(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            double width = env.Width;
            double hgt = env.Height;

            var pts = new Coordinate[nPts];

            for (int i = 0; i < nPts; i++)
            {
                double xLen = width * RND.NextDouble();
                double yLen = hgt * RND.NextDouble();
                pts[i] = RandomPtAround(env.Centre, xLen, yLen);
            }
            return geomFact.CreateLineString(pts);
        }

        public static IGeometry RandomRectilinearWalk(IGeometry g, int nPts)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            double xLen = env.Width;
            double yLen = env.Height;

            var pts = new Coordinate[nPts];

            bool xory = true;
            for (int i = 0; i < nPts; i++)
            {
                Coordinate pt;
                if (i == 0)
                {
                    pt = RandomPtAround(env.Centre, xLen, yLen);
                }
                else
                {
                    double dist = xLen * (RND.NextDouble() - 0.5);
                    double x = pts[i - 1].X;
                    double y = pts[i - 1].Y;
                    if (xory)
                    {
                        x += dist;
                    }
                    else
                    {
                        y += dist;
                    }
                    // switch orientation
                    xory = !xory;
                    pt = new Coordinate(x, y);
                }
                pts[i] = pt;
            }
            return geomFact.CreateLineString(pts);
        }

        private static int RandomQuadrant(int exclude)
        {
            while (true)
            {
                int quad = (int)(RND.NextDouble() * 4);
                if (quad > 3) quad = 3;
                if (quad != exclude) return quad;
            }
        }

        private static Coordinate RandomPtAround(Coordinate basePt, double xLen, double yLen)
        {
            double x0 = basePt.X + xLen * (RND.NextDouble() - 0.5);
            double y0 = basePt.Y + yLen * (RND.NextDouble() - 0.5);
            return new Coordinate(x0, y0);
        }
    }
}
