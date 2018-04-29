using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryTestFactory
    {

        public static Coordinate[] CreateBox(
                              double minx, double miny,
                              int nSide,
                              double segLen)
        {
            int i;
            int ipt = 0;
            Coordinate[] pts = new Coordinate[4 * nSide + 1];

            double maxx = minx + nSide * segLen;
            double maxy = miny + nSide * segLen;

            for (i = 0; i < nSide; i++)
            {
                double x = minx + i * segLen;
                double y = miny;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                double x = maxx;
                double y = miny + i * segLen;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                double x = maxx - i * segLen;
                double y = maxy;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                double x = minx;
                double y = maxy - i * segLen;
                pts[ipt++] = new Coordinate(x, y);
            }
            pts[ipt++] = new Coordinate(pts[0]);

            return pts;
        }
        public static IPolygon CreateCircle(
                              IGeometryFactory fact,
                              double basex,
                              double basey,
                              double size,
                              int nPts)
        {
            Coordinate[] pts = CreateCircle(basex, basey, size, nPts);
            var ring = fact.CreateLinearRing(pts);
            var poly = fact.CreatePolygon(ring, null);
            return poly;
        }

        /// <summary>
        /// Creates a circle
        /// </summary>
        /// <param name="basex">The centre x coord</param>
        /// <param name="basey">The centre y coord</param>
        /// <param name="size">The size of the envelope of the star</param>
        /// <param name="nPts">The number of points in the star</param>
        public static Coordinate[] CreateCircle(
                              double basex,
                              double basey,
                              double size,
                              int nPts)
        {
            Coordinate[] pts = new Coordinate[nPts + 1];

            int iPt = 0;
            double len = size / 2.0;

            for (int i = 0; i < nPts; i++)
            {
                double ang = i * (2 * Math.PI / nPts);
                double x = len * Math.Cos(ang) + basex;
                double y = len * Math.Sin(ang) + basey;
                Coordinate pt = new Coordinate(x, y);
                pts[iPt++] = pt;
            }
            pts[iPt] = pts[0];
            return pts;
        }

        public static IPolygon CreateBox(
            IGeometryFactory fact,
                              double minx, double miny,
                              int nSide,
                              double segLen)
        {
            Coordinate[] pts = CreateBox(minx, minx, nSide, segLen);
            var ring = fact.CreateLinearRing(pts);
            var poly = fact.CreatePolygon(ring, null);
            return poly;
        }



        /// <summary>
        /// Creates a star from a "circular" sine wave
        /// </summary>
        /// <param name="basex">The centre x coord</param>
        /// <param name="basey">The centre y coord</param>
        /// <param name="size">The size of the envelope of the star</param>
        /// <param name="armLen">The length of an arm of the star</param>
        /// <param name="nArms">The number of arms of the star</param>
        /// <param name="nPts">The number of points in the star</param>
        public static Coordinate[] CreateSineStar(
                              double basex,
                              double basey,
                              double size,
                              double armLen,
                              int nArms,
                              int nPts)
        {
            double armBaseLen = size / 2 - armLen;
            if (armBaseLen < 0) armBaseLen = 0.5;

            double angInc = 2 * Math.PI / nArms;
            int nArmPt = nPts / nArms;
            if (nArmPt < 5) nArmPt = 5;

            int nPts2 = nArmPt * nArms;
            Coordinate[] pts = new Coordinate[nPts2 + 1];

            int iPt = 0;
            double starAng = 0.0;

            for (int iArm = 0; iArm < nArms; iArm++)
            {
                for (int iArmPt = 0; iArmPt < nArmPt; iArmPt++)
                {
                    double ang = iArmPt * (2 * Math.PI / nArmPt);
                    double len = armLen * (1 - Math.Cos(ang) / 2) + armBaseLen;
                    double x = len * Math.Cos(starAng + iArmPt * angInc / nArmPt) + basex;
                    double y = len * Math.Sin(starAng + iArmPt * angInc / nArmPt) + basey;
                    Coordinate pt = new Coordinate(x, y);
                    pts[iPt++] = pt;
                }
                starAng += angInc;
            }
            pts[iPt] = pts[0];
            return pts;
        }

        public static IPolygon CreateSineStar(
                              IGeometryFactory fact,
                              double basex,
                              double basey,
                              double size,
                              double armLen,
                              int nArms,
                              int nPts)
        {
            Coordinate[] pts = CreateSineStar(basex, basey, size, armLen, nArms, nPts);
            var ring = fact.CreateLinearRing(pts);
            var poly = fact.CreatePolygon(ring, null);
            return poly;
        }

    }
}