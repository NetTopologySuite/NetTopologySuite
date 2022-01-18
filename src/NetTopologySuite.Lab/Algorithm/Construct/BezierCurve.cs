using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Creates a curved line or polygon using Bezier Curves
    /// defined by the segments of the input.
    /// </summary>
    public class BezierCurve
    {
        /// <summary>
        /// Creates a curved line or polygon using Bezier Curves
        /// defined by the segments of the input.
        /// </summary>
        /// <param name="geom">The geometry defining the curve</param>
        /// <param name="alpha">A roundness parameter (0 = linear, 1 = round, 2 = distorted)</param>
        /// <returns>A curved line or polygon using Bezier Curves</returns>
        public static Geometry Create(Geometry geom, double alpha)
        {
            var curve = new BezierCurve(geom, alpha);
            return curve.GetResult();
        }

        private readonly double _minSegmentLength = 0.0;
        private readonly int _numVerticesPerSegment = 10;

        private readonly Geometry _inputGeom;
        private readonly double _alpha;
        private readonly GeometryFactory _geomFactory;
  
        private Coordinate[] bezierCurvePts;
        private CubicBezierInterpolationParam[] interpolationParam;

        /// <summary>
        /// Creates a new Bezier Curve instance.
        /// </summary>
        /// <param name="geom">The geometry defining curve</param>
        /// <param name="alpha">A roundness parameter (0 = linear, 1 = round, 2 = distorted)</param>
        BezierCurve(Geometry geom, double alpha)
        {
            _inputGeom = geom;
            if (alpha < 0.0) alpha = 0;
            _alpha = alpha;
            _geomFactory = geom.Factory;
        }

        public Geometry GetResult()
        {
            bezierCurvePts = new Coordinate[_numVerticesPerSegment];
            interpolationParam = CubicBezierInterpolationParam.Compute(_numVerticesPerSegment);

            if (_inputGeom is LineString ls)
                return BezierLine(ls);
            if (_inputGeom is Polygon pg)
                return BezierPolygon(pg);
            return null;
        }

        private LineString BezierLine(LineString ls)
        {
            var coords = ls.Coordinates;

           var control = ControlPoints(coords, false, _alpha);

            int N = coords.Length;
            var curvePts = new List<Coordinate>();
            for (int i = 0; i < N - 1; i++)
            {
                double len = coords[i].Distance(coords[i + 1]);
                if (len < _minSegmentLength)
                {
                    // segment too short - copy input coordinate
                    curvePts.Add(new Coordinate(coords[i]));

                }
                else
                {
                    CubicBezier(coords[i], coords[i + 1], control[i][1], control[i + 1][0],
                        interpolationParam, bezierCurvePts);

                    int copyN = i < N - 1 ? bezierCurvePts.Length - 1 : bezierCurvePts.Length;
                    for (int k = 0; k < copyN; k++)
                    {
                        curvePts.Add(bezierCurvePts[k]);
                    }
                }
            }
            curvePts.Add(coords[N - 1]);
            return _geomFactory.CreateLineString(curvePts.ToArray());
        }

        private Polygon BezierPolygon(Polygon poly)
        {
            var coords = poly.ExteriorRing.Coordinates;
            int N = coords.Length - 1;

            var controlPoints = ControlPoints(coords, true, _alpha);
            var curvePts = new List<Coordinate>();
            for (int i = 0; i < N; i++)
            {
                int next = (i + 1) % N;

                double len = coords[i].Distance(coords[next]);
                if (len < _minSegmentLength)
                {
                    // segment too short - copy input coordinate
                    curvePts.Add(new Coordinate(coords[i]));

                }
                else
                {
                    CubicBezier(coords[i], coords[next], controlPoints[i][1], controlPoints[next][0],
                        interpolationParam, bezierCurvePts);

                    int copyN = i < N - 1 ? bezierCurvePts.Length - 1 : bezierCurvePts.Length;
                    for (int k = 0; k < copyN; k++)
                    {
                        curvePts.Add(bezierCurvePts[k]);
                    }
                }
            }

            var shell = _geomFactory.CreateLinearRing(curvePts.ToArray());
            return _geomFactory.CreatePolygon(shell, null);
        }


        private Coordinate[][] ControlPoints(Coordinate[] coords, bool isRing, double alpha)
        {
            int N = isRing ? coords.Length - 1 : coords.Length;
            double a1 = 1 - alpha;
            var ctrl = new Coordinate[N][];

            var v1 = coords[0];
            var v2 = coords[1];
            if (isRing)
            {
                v1 = coords[N - 1];
                v2 = coords[0];
            }

            double mid1x = (v1.X + v2.X) / 2.0;
            double mid1y = (v1.Y + v2.Y) / 2.0;
            double len1 = v1.Distance(v2);

            int start = isRing ? 0 : 1;
            int end = isRing ? N : N - 1;
            for (int i = start; i < end; i++)
            {
                v1 = coords[i];
                v2 = coords[i + 1];

                double mid0x = mid1x;
                double mid0y = mid1y;
                mid1x = (v1.X + v2.X) / 2.0;
                mid1y = (v1.Y + v2.Y) / 2.0;

                double len0 = len1;
                len1 = v1.Distance(v2);

                double p = len0 / (len0 + len1);
                double anchorx = mid0x + p * (mid1x - mid0x);
                double anchory = mid0y + p * (mid1y - mid0y);
                double xdelta = anchorx - v1.X;
                double ydelta = anchory - v1.Y;

                ctrl[i] = new Coordinate[2];
                ctrl[i][0] = new Coordinate(
                    a1 * (v1.X - mid0x + xdelta) + mid0x - xdelta,
                    a1 * (v1.Y - mid0y + ydelta) + mid0y - ydelta);

                ctrl[i][1] = new Coordinate(
                    a1 * (v1.X - mid1x + xdelta) + mid1x - xdelta,
                    a1 * (v1.Y - mid1y + ydelta) + mid1y - ydelta);
                //System.out.println(WKTWriter.toLineString(v[1], ctrl[i][0]));
                //System.out.println(WKTWriter.toLineString(v[1], ctrl[i][1]));
            }
            /*
             * For a line, 
             * use mirrored control points for start and end vertex,
             * to produce a symmetric curve for the first and last segments.
             */
            if (!isRing)
            {
                ctrl[0][1] = MirrorControlPoint(ctrl[1][0], coords[1], coords[0]);
                ctrl[N - 1][0] = MirrorControlPoint(ctrl[N - 2][1], coords[N - 1], coords[N - 2]);
            }
            return ctrl;
        }

        private static Coordinate MirrorControlPoint(Coordinate c, Coordinate p0, Coordinate p1)
        {
            double vlinex = p1.X - p0.X;
            double vliney = p1.Y - p0.Y;
            // rotate line vector by 90
            double vrotx = -vliney;
            double vroty = vlinex;

            double midx = (p0.X + p1.X) / 2;
            double midy = (p0.Y + p1.Y) / 2;

            return ReflectPointInLine(c, new Coordinate(midx, midy), new Coordinate(midx + vrotx, midy + vroty));
        }

        private static Coordinate ReflectPointInLine(Coordinate p, Coordinate p0, Coordinate p1)
        {
            double vx = p1.X - p0.X;
            double vy = p1.Y - p0.Y;
            double x = p0.X - p.X;
            double y = p0.Y - p.Y;
            double r = 1 / (vx * vx + vy * vy);
            double rx = p.X + 2 * (x - x * vx * vx * r - y * vx * vy * r);
            double ry = p.Y + 2 * (y - y * vy * vy * r - x * vx * vy * r);
            return new Coordinate(rx, ry);
        }

        /// <summary>
        /// Calculates vertices along a cubic Bezier curve.
        /// </summary>
        /// <param name="start">The start point</param>
        /// <param name="end">The end point</param>
        /// <param name="ctrl1">The first control point</param>
        /// <param name="ctrl2">The second control point</param>
        /// <param name="ip">Interpolation parameters</param>
        /// <param name="curve">An array to hold generated points</param>
        private void CubicBezier(Coordinate start,
            Coordinate end, Coordinate ctrl1,
            Coordinate ctrl2, CubicBezierInterpolationParam[] ip, Coordinate[] curve)
        {

            int n = curve.Length;
            curve[0] = new Coordinate(start);
            curve[n - 1] = new Coordinate(end);

            for (int i = 1; i < n - 1; i++)
            {
                var c = new Coordinate
                {
                    X = (ip[i].t[0] * start.X + ip[i].t[1] * ctrl1.X + ip[i].t[2] * ctrl2.X + ip[i].t[3] * end.X) / ip[i].tsum,
                    Y = (ip[i].t[0] * start.Y + ip[i].t[1] * ctrl1.Y + ip[i].t[2] * ctrl2.Y + ip[i].t[3] * end.Y) / ip[i].tsum
                };

                curve[i] = c;
            }
        }

        private sealed class CubicBezierInterpolationParam
        {
            public double[] t = new double[4];
            public double tsum;

            /**
             * Gets the interpolation parameters for a Bezier curve approximated by the
             * given number of vertices.
             *
             * @param n number of vertices
             * @return array of {@code InterpPoint} objects holding the parameter values
             */
            public static CubicBezierInterpolationParam[] Compute(int n)
            {
                var param = new CubicBezierInterpolationParam[n];

                for (int i = 0; i < n; i++)
                {
                    double t = (double)i / (n - 1);
                    double tc = 1.0 - t;

                    param[i] = new CubicBezierInterpolationParam();
                    param[i].t[0] = tc * tc * tc;
                    param[i].t[1] = 3.0 * tc * tc * t;
                    param[i].t[2] = 3.0 * tc * t * t;
                    param[i].t[3] = t * t * t;
                    param[i].tsum = param[i].t[0] + param[i].t[1] + param[i].t[2] + param[i].t[3];
                }
                return param;
            }
        }

    }
}
