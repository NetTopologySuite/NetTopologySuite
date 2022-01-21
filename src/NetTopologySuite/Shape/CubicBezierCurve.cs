using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System;

namespace NetTopologySuite.Shape
{
    /// <summary>
    /// Creates a curved geometry by replacing the segments
    /// of the input with Cubic Bezier Curves.
    /// </summary>
    /// <remarks>
    /// The Bezier control points are determined from the segments of the geometry
    /// and the alpha control parameter.
    /// The Bezier Curves are created to be C2-continuous (smooth)
    /// at each input vertex.
    /// <para/>
    /// The result is not guaranteed to be valid, since large alpha values
    /// may cause self-intersections.
    /// </remarks>
    public class CubicBezierCurve
    {
        /// <summary>
        /// Creates a curved geometry using linearized Cubic Bezier Curves
        /// defined by the segments of the input.
        /// </summary>
        /// <param name="geom">The geometry defining the curve</param>
        /// <param name="alpha">A roundness parameter (0 is linear, 1 is round, >1 is increasingly curved)</param>
        /// <returns>A curved geometry</returns>
        public static Geometry Create(Geometry geom, double alpha)
        {
            var curve = new CubicBezierCurve(geom, alpha);
            return curve.GetResult();
        }

        /**
         * Creates a geometry using linearized Cubic Bezier Curves
         * defined by the segments of the input, with a skew factor
         * affecting the shape at each vertex.
         * 
         * @param geom the geometry defining the curve
         * @param alpha curviness parameter (0 is linear, 1 is round, >1 is increasingly curved)
         * @param skew the skew parameter (0 is none, positive skews towards longer side, negative towards shorter
         * @return  the curved geometry
         */
        public static Geometry Create(Geometry geom, double alpha, double skew)
        {
            var curve = new CubicBezierCurve(geom, alpha);
            curve.Skew = skew;
            return curve.GetResult();
        }
        private readonly double _minSegmentLength = 0.0;
        private readonly int _numVerticesPerSegment = 16;

        private readonly Geometry _inputGeom;
        private readonly double _alpha;
        private double _skew;
        private readonly GeometryFactory _geomFactory;
  
        private Coordinate[] bezierCurvePts;
        private CubicBezierInterpolationParam[] interpolationParam;

        /// <summary>
        /// Creates a newinstance.
        /// </summary>
        /// <param name="geom">The geometry defining curve</param>
        /// <param name="alpha">A roundness parameter (0 = linear, 1 = round, 2 = distorted)</param>
        CubicBezierCurve(Geometry geom, double alpha)
        {
            _inputGeom = geom;
            //if (alpha < 0.0) alpha = 0;
            _alpha = alpha;
            _geomFactory = geom.Factory;
        }

        /// <summary>
        /// Gets or sets a skew factor influencing the shape of the curve corners.
        /// 0 is no skew, positive skews towards longer edges, negative skews towards shorter.
        /// </summary>
        /// <returns>The skew value</returns>
        public double Skew { get; set; }

        /// <summary>
        /// Gets the computed Bezier curve geometry
        /// </summary>
        /// <returns>The curved geometry</returns>
        public Geometry GetResult()
        {
            bezierCurvePts = new Coordinate[_numVerticesPerSegment];
            interpolationParam = CubicBezierInterpolationParam.Compute(_numVerticesPerSegment);

            return GeometryMapper.FlatMap(_inputGeom, Dimension.Curve,
                new GeometryMapper.MapOp( (geom) => {
                    if (geom is LineString)
                        return BezierLine((LineString)geom);
                    if (geom is Polygon ) 
                        return BezierPolygon((Polygon)geom);
            
                    //-- Points
                    return geom.Copy();
                }));
        }

        private LineString BezierLine(LineString ls)
        {
            var coords = ls.Coordinates;
            var control = ControlPoints(coords, false, _alpha);
            int N = coords.Length;
            var curvePts = new CoordinateList();
            for (int i = 0; i < N - 1; i++)
            {
                AddCurve(coords[i], coords[i + 1], control[i][1], control[i + 1][0], curvePts);
            }
            curvePts.Add(coords[N - 1], false);
            return _geomFactory.CreateLineString(curvePts.ToCoordinateArray());
        }

        private LinearRing BezierRing(LinearRing ring)
        {
            var coords = ring.Coordinates;
            var control = ControlPoints(coords, true, _alpha);
            var curvePts = new CoordinateList();
            int N = coords.Length - 1;
            for (int i = 0; i < N; i++)
            {
                int next = (i + 1) % N;
                AddCurve(coords[i], coords[next], control[i][1], control[next][0], curvePts);
            }
            curvePts.CloseRing();

            return _geomFactory.CreateLinearRing(curvePts.ToCoordinateArray());
        }

        private Polygon BezierPolygon(Polygon poly)
        {
            var shell = BezierRing((LinearRing)poly.ExteriorRing);
            LinearRing[] holes = null;
            if (poly.NumInteriorRings > 0)
            {
                holes = new LinearRing[poly.NumInteriorRings];
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    holes[i] = BezierRing((LinearRing)poly.GetInteriorRingN(i));
                }
            }
            return _geomFactory.CreatePolygon(shell, holes);
        }

        private void AddCurve(Coordinate p0, Coordinate p1,
            Coordinate ctrl0, Coordinate crtl1,
            CoordinateList curvePts)
        {
            double len = p0.Distance(p1);
            if (len < _minSegmentLength)
            {
                // segment too short - copy input coordinate
                curvePts.Add(new Coordinate(p0));

            }
            else
            {
                CubicBezier(p0, p1, ctrl0, crtl1,
                    interpolationParam, bezierCurvePts);
                for (int i = 0; i < bezierCurvePts.Length - 1; i++)
                {
                    curvePts.Add(bezierCurvePts[i], false);
                }
            }
        }

        //-- makes curve at right-angle corners roughly circular
        private const double CIRCLE_LEN_FACTOR = 3.0 / 8.0;

        /**
         * Creates control points for each vertex of curve.
         * The control points are collinear with each vertex, 
         * thus providing C1-continuity.
         * By default the control vectors are the same length, 
         * which provides C2-continuity (same curvature on each
         * side of vertex.
         * The alpha parameter controls the length of the control vectors.
         * Alpha = 0 makes the vectors zero-length, and hence flattens the curves.
         * Alpha = 1 makes the curve at right angles roughly circular.
         * Alpha > 1 starts to distort the curve and may introduce self-intersections
         * 
         * @param coords
         * @param isRing
         * @param alpha determines the curviness
         * @return
         */
        private Coordinate[][] ControlPoints(Coordinate[] coords, bool isRing, double alpha)
        {
            int N = isRing ? coords.Length - 1 : coords.Length;
            var ctrl = new Coordinate[N][];
            ctrl[0] = new Coordinate[2];
            ctrl[N-1] = new Coordinate[2];

            int start = isRing ? 0 : 1;
            int end = isRing ? N : N - 1;
            for (int i = start; i < end; i++)
            {
                int iprev = i == 0 ? N - 1 : i - 1;
                var v0 = coords[iprev];
                var v1 = coords[i];
                var v2 = coords[i + 1];

                double interiorAng = AngleUtility.AngleBetweenOriented(v0, v1, v2);
                double orient = Math.Sign(interiorAng);
                double angBisect = AngleUtility.Bisector(v0, v1, v2);
                double ang0 = angBisect - orient * AngleUtility.PiOver2;
                double ang1 = angBisect + orient * AngleUtility.PiOver2;

                double dist0 = v1.Distance(v0);
                double dist1 = v1.Distance(v2);
                double lenBase = Math.Min(dist0, dist1);
                double intAngAbs = Math.Abs(interiorAng);

                //-- make acute corners sharper by shortening tangent vectors
                double sharpnessFactor = intAngAbs >= AngleUtility.PiOver2 ? 1 : intAngAbs / AngleUtility.PiOver2;

                double len = alpha * CIRCLE_LEN_FACTOR * sharpnessFactor * lenBase;
                double stretch0 = 1;
                double stretch1 = 1;
                if (Skew != 0)
                {
                    double stretch = Math.Abs(dist0 - dist1) / Math.Max(dist0, dist1);
                    int skewIndex = dist0 > dist1 ? 0 : 1;
                    if (Skew < 0) skewIndex = 1 - skewIndex;
                    if (skewIndex == 0)
                    {
                        stretch0 += Math.Abs(Skew) * stretch;
                    }
                    else
                    {
                        stretch1 += Math.Abs(Skew) * stretch;
                    }
                }
                var ctl0 = AngleUtility.Project(v1, ang0, stretch0 * len);
                var ctl1 = AngleUtility.Project(v1, ang1, stretch1 * len);

                if (ctrl[i] == null) ctrl[i] = new Coordinate[2];
                ctrl[i][0] = ctl0;
                ctrl[i][1] = ctl1;

                //System.out.println(WKTWriter.toLineString(v1, ctl0));
                //System.out.println(WKTWriter.toLineString(v1, ctl1));
            }
            if (!isRing)
            {
                SetLineEndControlPoints(coords, ctrl);
            }
            return ctrl;
        }

        /**
         * Sets the end control points for a line.
         * Produce a symmetric curve for the first and last segments
         * by using mirrored control points for start and end vertex.
         * 
         * @param coords
         * @param ctrl
         */
        private void SetLineEndControlPoints(Coordinate[] coords, Coordinate[][] ctrl)
        {
            int N = coords.Length;

            ctrl[0][1] = MirrorControlPoint(ctrl[1][0], coords[1], coords[0]);
            ctrl[N - 1][0] = MirrorControlPoint(ctrl[N - 2][1], coords[N - 1], coords[N - 2]);
        }

        /**
         * Creates a control point aimed at the control point at the opposite end of the segment.
         * 
         * Produces overly flat results, so not used currently.
         * 
         * @param c
         * @param p1
         * @param p0
         * @return
         */
        private static Coordinate AimedControlPoint(Coordinate c, Coordinate p1, Coordinate p0)
        {
            double len = p1.Distance(c);
            double ang = AngleUtility.Angle(p0, p1);
            return AngleUtility.Project(p0, ang, len);
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
