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
    /// and the alpha control parameter controlling curvedness, and
    /// the optional skew parameter controlling the shape of the curve at vertices.
    /// The Bezier Curves are created to be C2-continuous (smooth)
    /// at each input vertex.
    /// <para/>
    /// Alternatively, the Bezier control points can be supplied explicitly.
    /// <para/>
    /// The result is not guaranteed to be valid, since large alpha values
    /// may cause self-intersections.
    /// </remarks>
    public class CubicBezierCurve
    {
        /// <summary>
        /// Creates a geometry of linearized Cubic Bezier Curves
        /// defined by the segments of the input and a parameter
        /// controlling how curved the result should be.
        /// </summary>
        /// <param name="geom">The geometry defining the curve</param>
        /// <param name="alpha">A curvedness parameter (0 is linear, 1 is round, >1 is increasingly curved)</param>
        /// <returns>The linearized curved geometry</returns>
        public static Geometry Create(Geometry geom, double alpha)
        {
            var curve = new CubicBezierCurve(geom, alpha);
            return curve.GetResult();
        }

        /// <summary>
        /// Creates a geometry of linearized Cubic Bezier Curves
        /// defined by the segments of the inputand a parameter
        /// controlling how curved the result should be, with a skew factor
        /// affecting the curve shape at each vertex.
        /// </summary>
        /// <param name="geom">The geometry defining the curve</param>
        /// <param name="alpha">The curvedness parameter (0 is linear, 1 is round, >1 is increasingly curved)</param>
        /// <param name="skew">The skew parameter (0 is none, positive skews towards longer side, negative towards shorter</param>
        /// <returns>The linearized curved geometry</returns>
        public static Geometry Create(Geometry geom, double alpha, double skew)
        {
            var curve = new CubicBezierCurve(geom, alpha, skew);
            return curve.GetResult();
        }

        /// <summary>
        /// Creates a geometry of linearized Cubic Bezier Curves
        /// defined by the segments of the input
        /// and a list (or lists) of control points.
        /// </summary>
        /// <remarks>
        /// Typically the control point geometry
        /// is a <see cref="LineString"/> or <see cref="MultiLineString"/>
        /// containing an element for each line or ring in the input geometry.
        /// The list of control points for each linear element must contain two
        /// vertices for each segment (and thus <code>2 * npts - 2</code>).
        /// </remarks>
        /// <param name="geom">The geometry defining the curve</param>
        /// <param name="controlPoints">A geometry containing the control point elements.</param>
        /// <returns>The linearized curved geometry</returns>
        public static Geometry Create(Geometry geom, Geometry controlPoints)
        {
            var curve = new CubicBezierCurve(geom, controlPoints);
            try
            {
                return curve.GetResult();
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(nameof(controlPoints), e);
            }
        }

        private readonly double _minSegmentLength = 0.0;
        private readonly int _numVerticesPerSegment = 16;

        private readonly Geometry _inputGeom;
        private readonly double _alpha = -1;
        private double _skew;
        private readonly Geometry _controlPoints;
        private readonly GeometryFactory _geomFactory;
        private int _controlPointIndex;
  
        private Coordinate[] bezierCurvePts;
        private double[][] interpolationParam;

        /// <summary>
        /// Creates a new instance producing a Bezier curve defined by a geometry
        /// and an alpha curvedness value.
        /// </summary>
        /// <param name="geom">The geometry defining curve</param>
        /// <param name="alpha">A curvedness parameter (0 = linear, 1 = round, 2 = distorted)</param>
        CubicBezierCurve(Geometry geom, double alpha)
        {
            _inputGeom = geom;
            _geomFactory = geom.Factory;
            if (alpha < 0.0) alpha = 0;
            _alpha = alpha;
        }

        /// <summary>
        /// Creates a new instance producing a Bezier curve defined by a geometry,
        /// an alpha curvedness value, and a skew factor.
        /// </summary>
        /// <param name="geom">The geometry defining curve</param>
        /// <param name="alpha">curvedness parameter (0 is linear, 1 is round, >1 is increasingly curved)</param>
        /// <param name="skew">The skew parameter (0 is none, positive skews towards longer side, negative towards shorter</param>
        CubicBezierCurve(Geometry geom, double alpha, double skew)
        {
            _inputGeom = geom;
            _geomFactory = geom.Factory;
            if (alpha < 0.0) alpha = 0;
            _alpha = alpha;
            _skew = skew;
        }

        /// <summary>
        /// Creates a new instance producing a Bezier curve defined by a geometry,
        /// and a list (or lists) of control points.
        /// </summary><remarks>
        /// <para/>
        /// Typically the control point geometry
        /// is a <see cref="LineString"/> or <see cref="MultiLineString"/>
        /// containing an element for each line or ring in the input geometry.
        /// The list of control points for each linear element must contain two
        /// vertices for each segment (and thus <code>2 * npts - 2</code>).
        /// </remarks>
        CubicBezierCurve(Geometry geom, Geometry controlPoints)
        {
            _inputGeom = geom;
            _geomFactory = geom.Factory;
            _controlPoints = controlPoints;
        }

        /// <summary>
        /// Gets the computed Bezier curve geometry
        /// </summary>
        /// <returns>The curved geometry</returns>
        public Geometry GetResult()
        {
            bezierCurvePts = new Coordinate[_numVerticesPerSegment];
            interpolationParam = ComputeIterpolationParameters(_numVerticesPerSegment);

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
            var curvePts = BezierCurve(coords, false);
            curvePts.Add(coords[coords.Length - 1].Copy(), false);
            return _geomFactory.CreateLineString(curvePts.ToCoordinateArray());
        }

        private LinearRing BezierRing(LinearRing ring)
        {
            var coords = ring.Coordinates;
            var curvePts = BezierCurve(coords, true);
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

        private CoordinateList BezierCurve(Coordinate[] coords, bool isRing)
        {
            var control = ControlPoints(coords, isRing);
            var curvePts = new CoordinateList();
            for (int i = 0; i < coords.Length - 1; i++)
            {
                int ctrlIndex = 2 * i;
                AddCurve(coords[i], coords[i + 1], control[ctrlIndex], control[ctrlIndex + 1], curvePts);
            }
            return curvePts;
        }

        private Coordinate[] ControlPoints(Coordinate[] coords, bool isRing)
        {
            if (_controlPoints != null)
            {
                if (_controlPointIndex >= _controlPoints.NumGeometries)
                {
                    throw new InvalidOperationException("Too few control point elements");
                }
                var ctrlPtsGeom = _controlPoints.GetGeometryN(_controlPointIndex);
                var ctrlPts = ctrlPtsGeom.Coordinates;

                int expectedNum1 = 2 * coords.Length - 2;
                int expectedNum2 = isRing ? coords.Length - 1 : coords.Length;
                if (expectedNum1 != ctrlPts.Length && expectedNum2 != ctrlPts.Length)
                {
                    throw new InvalidOperationException(
                        string.Format("Wrong number of control points for element {0} - expected {1} or {2}, found {3}",
                            _controlPointIndex, expectedNum1, expectedNum2, ctrlPts.Length
                              ));
                }
                _controlPointIndex++;
                return ctrlPts;
            }
            return ControlPoints(coords, isRing, _alpha, _skew);
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

        //-- chosen to make curve at right-angle corners roughly circular
        private const double CIRCLE_LEN_FACTOR = 3.0 / 8.0;

        /// <summary>
        /// Creates control points for each vertex of curve.
        /// The control points are collinear with each vertex,
        /// thus providing C1-continuity.
        /// By default the control vectors are the same length,
        /// which provides C2-continuity(same curvature on each
        /// side of vertex.
        /// The alpha parameter controls the length of the control vectors.
        /// Alpha = 0 makes the vectors zero-length, and hence flattens the curves.
        /// Alpha = 1 makes the curve at right angles roughly circular.
        /// Alpha > 1 starts to distort the curve and may introduce self-intersections.
        /// <para/>
        /// The control point array contains a pair of coordinates for each input segment.
        /// </summary>
        private static Coordinate[] ControlPoints(Coordinate[] coords, bool isRing, double alpha, double skew)
        {
            int N = coords.Length;
            int start = 1;
            int end = N - 1;

            if (isRing)
            {
                N = coords.Length - 1;
                start = 0;
                end = N;
            }

            int nControl = 2 * coords.Length - 2;
            var ctrl = new Coordinate[nControl];

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
                if (skew != 0)
                {
                    double stretch = Math.Abs(dist0 - dist1) / Math.Max(dist0, dist1);
                    int skewIndex = dist0 > dist1 ? 0 : 1;
                    if (skew < 0) skewIndex = 1 - skewIndex;
                    if (skewIndex == 0)
                    {
                        stretch0 += Math.Abs(skew) * stretch;
                    }
                    else
                    {
                        stretch1 += Math.Abs(skew) * stretch;
                    }
                }
                var ctl0 = AngleUtility.Project(v1, ang0, stretch0 * len);
                var ctl1 = AngleUtility.Project(v1, ang1, stretch1 * len);

                int index = 2 * i - 1;
                // for a ring case the first control point is for last segment
                int i0 = index < 0 ? nControl - 1 : index;
                ctrl[i0] = ctl0;
                ctrl[index + 1] = ctl1;

                //System.out.println(WKTWriter.toLineString(v1, ctl0));
                //System.out.println(WKTWriter.toLineString(v1, ctl1));
            }
            if (!isRing)
            {
                SetLineEndControlPoints(coords, ctrl);
            }
            return ctrl;
        }

        /// <summary>
        /// Sets the end control points for a line.
        /// Produce a symmetric curve for the first and last segments
        /// by using mirrored control points for start and end vertex.
        /// </summary>
        /// <param name="coords">The coordinates</param>
        /// <param name="ctrl">The control points</param>
        private static void SetLineEndControlPoints(Coordinate[] coords, Coordinate[] ctrl)
        {
            int N = ctrl.Length;

            ctrl[0] = MirrorControlPoint(ctrl[1], coords[1], coords[0]);
            ctrl[N - 1] = MirrorControlPoint(ctrl[N - 2],
                coords[coords.Length - 1], coords[coords.Length - 2]);
        }

        /// <summary>
        /// Creates a control point aimed at the control point at the opposite end of the segment.
        /// </summary>
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
        /// <param name="p0">The start point</param>
        /// <param name="p1">The end point</param>
        /// <param name="ctrl1">The first control point</param>
        /// <param name="ctrl2">The second control point</param>
        /// <param name="param">A set of interpolation parameters</param>
        /// <param name="curve">An array to hold generated points.</param>
        private static void CubicBezier(Coordinate p0,
            Coordinate p1, Coordinate ctrl1,
            Coordinate ctrl2, double[][] param,
            Coordinate[] curve)
        {

            int n = curve.Length;
            curve[0] = new Coordinate(p0);
            curve[n - 1] = new Coordinate(p1);

            for (int i = 1; i < n - 1; i++)
            {
                var c = new Coordinate();
                double sum = param[i][0] + param[i][1] + param[i][2] + param[i][3];
                c.X = param[i][0] * p0.X + param[i][1] * ctrl1.X + param[i][2] * ctrl2.X + param[i][3] * p1.X;
                c.X /= sum;
                c.Y = param[i][0] * p0.Y + param[i][1] * ctrl1.Y + param[i][2] * ctrl2.Y + param[i][3] * p1.Y;
                c.Y /= sum;

                curve[i] = c;
            }
        }

        /// <summary>
        /// Gets the interpolation parameters for a Bezier curve approximated by a
        /// given number of vertices.
        /// </summary>
        /// <param name="n">The number of vertices</param>
        /// <returns>An array of double[4] holding the parameter values</returns>
        private static double[][] ComputeIterpolationParameters(int n)
        {
            double[][] param = new double[n][];
            for (int i = 0; i < n; i++)
            {
                param[i] = new double[4];
                double t = (double)i / (n - 1);
                double tc = 1.0 - t;

                param[i][0] = tc * tc * tc;
                param[i][1] = 3.0 * tc * tc * t;
                param[i][2] = 3.0 * tc * t * t;
                param[i][3] = t * t * t;
            }
            return param;
        }


    }
}
