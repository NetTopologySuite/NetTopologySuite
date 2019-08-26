using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the minimum diameter of a <see cref="Geometry"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum diameter is defined to be the
    /// width of the smallest band that contains the point,
    /// where a band is a strip of the plane defined
    /// by two parallel lines.
    /// This can be thought of as the smallest hole that the point can be
    /// moved through, with a single rotation.
    /// </para>
    /// <para>
    /// The first step in the algorithm is computing the convex hull of the Geometry.
    /// If the input Geometry is known to be convex, a hint can be supplied to
    /// avoid this computation.
    /// </para>
    /// <para>
    /// This class can also be used to compute a line segment representing
    /// the minimum diameter, the supporting line segment of the minimum diameter,
    /// and a minimum rectangle enclosing the input geometry.
    /// This rectangle will
    /// have width equal to the minimum diameter, and have one side
    /// parallel to the supporting segment.
    /// </para>
    /// </remarks>
    /// <seealso cref="ConvexHull"/>
    public class MinimumDiameter
    {
        /// <summary>
        /// Gets the minimum rectangle enclosing a geometry.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <returns>The minimum rectangle enclosing the geometry</returns>
        public static Geometry GetMinimumRectangle(Geometry geom)
        {
            return (new MinimumDiameter(geom)).GetMinimumRectangle();
        }

        /// <summary>
        /// Gets the minimum diameter enclosing a geometry.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <returns>The length of the minimum diameter of the geometry</returns>
        public static Geometry GetMinimumDiameter(Geometry geom)
        {
            return (new MinimumDiameter(geom)).Diameter;
        }

        private readonly Geometry _inputGeom;
        private readonly bool _isConvex;

        private Coordinate[] _convexHullPts;
        private LineSegment _minBaseSeg = new LineSegment();
        private Coordinate _minWidthPt;
        private int _minPtIndex;
        private double _minWidth;

        /// <summary>
        /// Compute a minimum diameter for a given <see cref="Geometry"/>.
        /// </summary>
        /// <param name="inputGeom">a Geometry.</param>
        public MinimumDiameter(Geometry inputGeom)
            : this(inputGeom, false) { }

        /// <summary>
        /// Compute a minimum diameter for a giver <c>Geometry</c>,
        /// with a hint if
        /// the Geometry is convex
        /// (e.g. a convex Polygon or LinearRing,
        /// or a two-point LineString, or a Point).
        /// </summary>
        /// <param name="inputGeom">a Geometry which is convex.</param>
        /// <param name="isConvex"><c>true</c> if the input point is convex.</param>
        public MinimumDiameter(Geometry inputGeom, bool isConvex)
        {
            _inputGeom = inputGeom;
            _isConvex = isConvex;
        }

        /// <summary>
        /// Gets the length of the minimum diameter of the input Geometry.
        /// </summary>
        /// <returns>The length of the minimum diameter.</returns>
        public double Length
        {
            get
            {
                ComputeMinimumDiameter();
                return _minWidth;
            }
        }

        /// <summary>
        /// Gets the <c>Coordinate</c> forming one end of the minimum diameter.
        /// </summary>
        /// <returns>A coordinate forming one end of the minimum diameter.</returns>
        public Coordinate WidthCoordinate
        {
            get
            {
                ComputeMinimumDiameter();
                return _minWidthPt;
            }
        }

        /// <summary>
        /// Gets the segment forming the base of the minimum diameter.
        /// </summary>
        /// <returns>The segment forming the base of the minimum diameter.</returns>
        public LineString SupportingSegment
        {
            get
            {
                ComputeMinimumDiameter();
                return _inputGeom.Factory.CreateLineString(new[] { _minBaseSeg.P0, _minBaseSeg.P1 });
            }
        }

        /// <summary>
        /// Gets a <c>LineString</c> which is a minimum diameter.
        /// </summary>
        /// <returns>A <c>LineString</c> which is a minimum diameter.</returns>
        public LineString Diameter
        {
            get
            {
                ComputeMinimumDiameter();

                // return empty linearRing if no minimum width calculated
                if (_minWidthPt == null)
                {
                    //Coordinate[] nullCoords = null;
                    return _inputGeom.Factory.CreateLineString();
                }

                var basePt = _minBaseSeg.Project(_minWidthPt);
                return _inputGeom.Factory.CreateLineString(new[] { basePt, _minWidthPt });
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void ComputeMinimumDiameter()
        {
            // check if computation is cached
            if (_minWidthPt != null)
                return;

            if (_isConvex) ComputeWidthConvex(_inputGeom);
            else
            {
                var convexGeom = (new ConvexHull(_inputGeom)).GetConvexHull();
                ComputeWidthConvex(convexGeom);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="convexGeom"></param>
        private void ComputeWidthConvex(Geometry convexGeom)
        {
            if (convexGeom is Polygon)
                _convexHullPts = ((Polygon) convexGeom).ExteriorRing.Coordinates;
            else
                _convexHullPts = convexGeom.Coordinates;

            // special cases for lines or points or degenerate rings
            if (_convexHullPts.Length == 0)
            {
                _minWidth = 0.0;
                _minWidthPt = null;
                _minBaseSeg = null;
            }
            else if (_convexHullPts.Length == 1)
            {
                _minWidth = 0.0;
                _minWidthPt = _convexHullPts[0];
                _minBaseSeg.P0 = _convexHullPts[0];
                _minBaseSeg.P1 = _convexHullPts[0];
            }
            else if (_convexHullPts.Length == 2 || _convexHullPts.Length == 3)
            {
                _minWidth = 0.0;
                _minWidthPt = _convexHullPts[0];
                _minBaseSeg.P0 = _convexHullPts[0];
                _minBaseSeg.P1 = _convexHullPts[1];
            }
            else
                ComputeConvexRingMinDiameter(_convexHullPts);
        }

        /// <summary>
        /// Compute the width information for a ring of <c>Coordinate</c>s.
        /// Leaves the width information in the instance variables.
        /// </summary>
        /// <param name="pts"></param>
        private void ComputeConvexRingMinDiameter(Coordinate[] pts)
        {
            // for each segment in the ring
            _minWidth = double.MaxValue;
            int currMaxIndex = 1;

            var seg = new LineSegment();
            // compute the max distance for all segments in the ring, and pick the minimum
            for (int i = 0; i < pts.Length - 1; i++)
            {
                seg.P0 = pts[i];
                seg.P1 = pts[i + 1];
                currMaxIndex = FindMaxPerpDistance(pts, seg, currMaxIndex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="seg"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private int FindMaxPerpDistance(Coordinate[] pts, LineSegment seg, int startIndex)
        {
            double maxPerpDistance = seg.DistancePerpendicular(pts[startIndex]);
            double nextPerpDistance = maxPerpDistance;
            int maxIndex = startIndex;
            int nextIndex = maxIndex;
            while (nextPerpDistance >= maxPerpDistance)
            {
                maxPerpDistance = nextPerpDistance;
                maxIndex = nextIndex;

                nextIndex = NextIndex(pts, maxIndex);
                nextPerpDistance = seg.DistancePerpendicular(pts[nextIndex]);
            }

            // found maximum width for this segment - update global min dist if appropriate
            if (maxPerpDistance < _minWidth)
            {
                _minPtIndex = maxIndex;
                _minWidth = maxPerpDistance;
                _minWidthPt = pts[_minPtIndex];
                _minBaseSeg = new LineSegment(seg);
            }
            return maxIndex;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static int NextIndex(Coordinate[] pts, int index)
        {
            index++;
            if (index >= pts.Length) index = 0;
            return index;
        }

        /// <summary>
        /// Gets the minimum rectangular <see cref="Polygon"/> which encloses the input geometry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The rectangle has width equal to the minimum diameter, and a longer length.
        /// If the convex hull of the input is degenerate (a line or point) a <see cref="LineString"/> or <see cref="Point"/> is returned.
        /// </para>
        /// <para>
        /// The minimum rectangle can be used as an extremely generalized representation for the given geometry.
        /// </para>
        /// </remarks>
        /// <returns>The minimum rectangle enclosing the input (or a line or point if degenerate)</returns>
        public Geometry GetMinimumRectangle()
        {
            ComputeMinimumDiameter();

            // check if minimum rectangle is degenerate (a point or line segment)
            if (_minWidth == 0.0)
            {
                if (_minBaseSeg.P0.Equals2D(_minBaseSeg.P1))
                {
                    return _inputGeom.Factory.CreatePoint(_minBaseSeg.P0);
                }
                return _minBaseSeg.ToGeometry(_inputGeom.Factory);
            }

            // deltas for the base segment of the minimum diameter
            double dx = _minBaseSeg.P1.X - _minBaseSeg.P0.X;
            double dy = _minBaseSeg.P1.Y - _minBaseSeg.P0.Y;

            /*
            double c0 = computeC(dx, dy, minBaseSeg.p0);
            double c1 = computeC(dx, dy, minBaseSeg.p1);
            */

            double minPara = double.MaxValue;
            double maxPara = -double.MaxValue;
            double minPerp = double.MaxValue;
            double maxPerp = -double.MaxValue;

            // compute maxima and minima of lines parallel and perpendicular to base segment
            for (int i = 0; i < _convexHullPts.Length; i++)
            {

                double paraC = ComputeC(dx, dy, _convexHullPts[i]);
                if (paraC > maxPara) maxPara = paraC;
                if (paraC < minPara) minPara = paraC;

                double perpC = ComputeC(-dy, dx, _convexHullPts[i]);
                if (perpC > maxPerp) maxPerp = perpC;
                if (perpC < minPerp) minPerp = perpC;
            }

            // compute lines along edges of minimum rectangle
            var maxPerpLine = ComputeSegmentForLine(-dx, -dy, maxPerp);
            var minPerpLine = ComputeSegmentForLine(-dx, -dy, minPerp);
            var maxParaLine = ComputeSegmentForLine(-dy, dx, maxPara);
            var minParaLine = ComputeSegmentForLine(-dy, dx, minPara);

            // compute vertices of rectangle (where the para/perp max & min lines intersect)
            var p0 = maxParaLine.LineIntersection(maxPerpLine);
            var p1 = minParaLine.LineIntersection(maxPerpLine);
            var p2 = minParaLine.LineIntersection(minPerpLine);
            var p3 = maxParaLine.LineIntersection(minPerpLine);

            var shell = _inputGeom.Factory.CreateLinearRing(
                new[] { p0, p1, p2, p3, p0 });
            return _inputGeom.Factory.CreatePolygon(shell);

        }

        private static double ComputeC(double a, double b, Coordinate p)
        {
            return a * p.Y - b * p.X;
        }

        private static LineSegment ComputeSegmentForLine(double a, double b, double c)
        {
            Coordinate p0;
            Coordinate p1;
            /*
            * Line eqn is ax + by = c
            * Slope is a/b.
            * If slope is steep, use y values as the inputs
            */
            if (Math.Abs(b) > Math.Abs(a))
            {
                p0 = new Coordinate(0.0, c / b);
                p1 = new Coordinate(1.0, c / b - a / b);
            }
            else
            {
                p0 = new Coordinate(c / a, 0.0);
                p1 = new Coordinate(c / a - b / a, 1.0);
            }
            return new LineSegment(p0, p1);
        }

    }
}
