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
            var ipt = 0;
            var pts = new Coordinate[4 * nSide + 1];
            var maxx = minx + nSide * segLen;
            var maxy = miny + nSide * segLen;
            for (i = 0; i < nSide; i++)
            {
                var x = minx + i * segLen;
                var y = miny;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                var x = maxx;
                var y = miny + i * segLen;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                var x = maxx - i * segLen;
                var y = maxy;
                pts[ipt++] = new Coordinate(x, y);
            }
            for (i = 0; i < nSide; i++)
            {
                var x = minx;
                var y = maxy - i * segLen;
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
            var pts = CreateCircle(basex, basey, size, nPts);
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
            var pts = new Coordinate[nPts + 1];
            var iPt = 0;
            var len = size / 2.0;
            for (var i = 0; i < nPts; i++)
            {
                var ang = i * (2 * Math.PI / nPts);
                var x = len * Math.Cos(ang) + basex;
                var y = len * Math.Sin(ang) + basey;
                var pt = new Coordinate(x, y);
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
            var pts = CreateBox(minx, minx, nSide, segLen);
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
            var armBaseLen = size / 2 - armLen;
            if (armBaseLen < 0) armBaseLen = 0.5;
            var angInc = 2 * Math.PI / nArms;
            var nArmPt = nPts / nArms;
            if (nArmPt < 5) nArmPt = 5;
            var nPts2 = nArmPt * nArms;
            var pts = new Coordinate[nPts2 + 1];
            var iPt = 0;
            var starAng = 0.0;
            for (var iArm = 0; iArm < nArms; iArm++)
            {
                for (var iArmPt = 0; iArmPt < nArmPt; iArmPt++)
                {
                    var ang = iArmPt * (2 * Math.PI / nArmPt);
                    var len = armLen * (1 - Math.Cos(ang) / 2) + armBaseLen;
                    var x = len * Math.Cos(starAng + iArmPt * angInc / nArmPt) + basex;
                    var y = len * Math.Sin(starAng + iArmPt * angInc / nArmPt) + basey;
                    var pt = new Coordinate(x, y);
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
            var pts = CreateSineStar(basex, basey, size, armLen, nArms, nPts);
            var ring = fact.CreateLinearRing(pts);
            var poly = fact.CreatePolygon(ring, null);
            return poly;
        }
    }
}
