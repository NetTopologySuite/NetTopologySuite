using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes the raw offset curve for a single <see cref="Geometry"/> component (ring, line or point).
    /// </summary>
    /// <remarks>
    /// A raw offset curve line is not noded - it may contain self-intersections (and usually will).
    /// The final buffer polygon is computed by forming a topological graph
    /// of all the noded raw curves and tracing outside contours.
    /// The points in the raw curve are rounded to the required precision model.
    /// </remarks>
    [Obsolete]
    public class OldOffsetCurveBuilder
    {
        /// <summary>
        /// The angle quantum with which to approximate a fillet curve
        /// (based on the input # of quadrant segments)
        /// </summary>
        private readonly double _filletAngleQuantum;

        /// <summary>
        /// The max error of approximation (distance) between a quad segment and the true fillet curve
        /// </summary>
        private double _maxCurveSegmentError;

        //private const double MinCurveVertexFactor = 1.0E-6;
        /// <summary>
        /// Factor which controls how close curve vertices can be to be snapped
        /// </summary>
        private const double CurveVertexSnapDistanceFactor = 1.0E-6;

        /// <summary>
        /// Factor which controls how close offset segments can be to
        /// skip adding a filler or mitre.
        /// </summary>
        private const double OffsetSegmentSeparationFactor = 1.0E-3;

        /// <summary>
        /// Factor which controls how close curve vertices on inside turns can be to be snapped
        /// </summary>
        private const double InsideTurnVertexSnapDistanceFactor = 1.0E-3;

        /// <summary>
        /// Factor which determines how short closing segs can be for round buffers
        /// </summary>
        private const int MaxClosingSegFraction = 80;

        private double _distance;
        private readonly PrecisionModel _precisionModel;

        private readonly BufferParameters _bufParams;


        /// <summary>
        /// The Closing Segment Factor controls how long
        /// "closing segments" are.  Closing segments are added
        /// at the middle of inside corners to ensure a smoother
        /// boundary for the buffer offset curve.<br/>
        /// In some cases (particularly for round joins with default-or-better
        /// quantization) the closing segments can be made quite short.
        /// This substantially improves performance (due to fewer intersections being created).
        /// <br/>
        /// A closingSegFactor of 0 results in lines to the corner vertex<br/>
        /// A closingSegFactor of 1 results in lines halfway to the corner vertex<br/>
        /// A closingSegFactor of 80 results in lines 1/81 of the way to the corner vertex
        /// (this option is reasonable for the very common default situation of round joins
        /// and quadrantSegs >= 8)
        /// </summary>
        private readonly int _closingSegFactor = 1;

        private OffsetCurveVertexList _vertexList;
        private readonly LineIntersector _li;

        public OldOffsetCurveBuilder(
                      PrecisionModel precisionModel,
                      BufferParameters bufParams
                      )
        {
            _precisionModel = precisionModel;
            _bufParams = bufParams;

            // compute intersections in full precision, to provide accuracy
            // the points are rounded as they are inserted into the curve line
            _li = new RobustLineIntersector();
            _filletAngleQuantum = Math.PI / 2.0 / bufParams.QuadrantSegments;

            /*
             * Non-round joins cause issues with short closing segments,
             * so don't use them.  In any case, non-round joins
             * only really make sense for relatively small buffer distances.
             */
            if (bufParams.QuadrantSegments >= 8
                && bufParams.JoinStyle == JoinStyle.Round)
                _closingSegFactor = MaxClosingSegFraction;
        }

        /// <summary>
        /// This method handles single points as well as lines.
        /// Lines are assumed to <b>not</b> be closed (the function will not
        /// fail for closed lines, but will generate superfluous line caps).
        /// </summary>
        /// <returns>a List of Coordinate[]</returns>
        public IList<Coordinate[]> GetLineCurve(Coordinate[] inputPts, double distance)
        {
            var lineList = new List<Coordinate[]>();
            // a zero or negative width buffer of a line/point is empty
            if (distance <= 0.0) return lineList;

            Init(distance);
            if (inputPts.Length <= 1)
            {
                switch (_bufParams.EndCapStyle)
                {
                    case EndCapStyle.Round:
                        AddCircle(inputPts[0], distance);
                        break;
                    case EndCapStyle.Square:
                        AddSquare(inputPts[0], distance);
                        break;
                    // default is for buffer to be empty (e.g. for a butt line cap);
                }
            }
            else
                ComputeLineBufferCurve(inputPts);

            // System.out.println(vertexList);

            var lineCoord = _vertexList.Coordinates;
            lineList.Add(lineCoord);
            return lineList;
        }

        /// <summary>
        /// This method handles the degenerate cases of single points and lines, as well as rings.
        /// </summary>
        /// <returns>a List of Coordinate[]</returns>
        [Obsolete("Use GetRingCurve(Coordinate[], Geometries.Position, double)")]
        public IList<Coordinate[]> GetRingCurve(Coordinate[] inputPts, Positions side, double distance)
            => GetRingCurve(inputPts, new Position((int) side), distance);

        /// <summary>
        /// This method handles the degenerate cases of single points and lines, as well as rings.
        /// </summary>
        /// <returns>a List of Coordinate[]</returns>
        public IList<Coordinate[]> GetRingCurve(Coordinate[] inputPts, Position side, double distance)
        {
            var lineList = new List<Coordinate[]>();
            Init(distance);
            if (inputPts.Length <= 2)
                return GetLineCurve(inputPts, distance);

            // optimize creating ring for for zero distance
            if (distance == 0.0)
            {
                lineList.Add(CopyCoordinates(inputPts));
                return lineList;
            }
            ComputeRingBufferCurve(inputPts, side);
            lineList.Add(_vertexList.Coordinates);
            return lineList;
        }

        private static Coordinate[] CopyCoordinates(Coordinate[] pts)
        {
            var copy = new Coordinate[pts.Length];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = pts[i].Copy();
            }
            return copy;
        }

        private void Init(double distance)
        {
            _distance = distance;
            _maxCurveSegmentError = distance * (1 - Math.Cos(_filletAngleQuantum / 2.0));
            _vertexList = new OffsetCurveVertexList();
            _vertexList.PrecisionModel = _precisionModel;
            /*
             * Choose the min vertex separation as a small fraction of the offset distance.
             */
            _vertexList.MinimumVertexDistance = distance * CurveVertexSnapDistanceFactor;
        }

        /// <summary>
        /// Use a value which results in a potential distance error which is
        /// significantly less than the error due to
        /// the quadrant segment discretization.
        /// For QS = 8 a value of 100 is reasonable.
        /// This should produce a maximum of 1% distance error.
        /// </summary>
        private const double SimplifyFactor = 400.0;

        /// <summary>
        /// Computes the distance tolerance to use during input
        /// line simplification.
        /// </summary>
        /// <param name="bufDistance">The buffer distance</param>
        /// <returns>The simplification tolerance</returns>
        private static double SimplifyTolerance(double bufDistance)
        {
            return bufDistance / SimplifyFactor;
        }

        private void ComputeLineBufferCurve(Coordinate[] inputPts)
        {
            double distTol = SimplifyTolerance(_distance);

            //--------- compute points for left side of line
            // Simplify the appropriate side of the line before generating
            var simp1 = BufferInputLineSimplifier.Simplify(inputPts, distTol);
            // MD - used for testing only (to eliminate simplification)
            // Coordinate[] simp1 = inputPts;

            int n1 = simp1.Length - 1;

            InitSideSegments(simp1[0], simp1[1], Position.Left);
            for (int i = 2; i <= n1; i++)
            {
                AddNextSegment(simp1[i], true);
            }
            AddLastSegment();
            // add line cap for end of line
            AddLineEndCap(simp1[n1 - 1], simp1[n1]);

            //---------- compute points for right side of line
            // Simplify the appropriate side of the line before generating
            var simp2 = BufferInputLineSimplifier.Simplify(inputPts, -distTol);
            // MD - used for testing only (to eliminate simplification)
            // Coordinate[] simp2 = inputPts;
            int n2 = simp2.Length - 1;

            // since we are traversing line in opposite order, offset position is still LEFT
            InitSideSegments(simp2[n2], simp2[n2 - 1], Position.Left);
            for (int i = n2 - 2; i >= 0; i--)
            {
                AddNextSegment(simp2[i], true);
            }
            AddLastSegment();
            // add line cap for start of line
            AddLineEndCap(simp2[1], simp2[0]);

            _vertexList.CloseRing();
        }

        private void OldcomputeLineBufferCurve(Coordinate[] inputPts)
        {
            int n = inputPts.Length - 1;

            // compute points for left side of line
            InitSideSegments(inputPts[0], inputPts[1], Position.Left);
            for (int i = 2; i <= n; i++)
            {
                AddNextSegment(inputPts[i], true);
            }
            AddLastSegment();
            // add line cap for end of line
            AddLineEndCap(inputPts[n - 1], inputPts[n]);

            // compute points for right side of line
            InitSideSegments(inputPts[n], inputPts[n - 1], Position.Left);
            for (int i = n - 2; i >= 0; i--)
            {
                AddNextSegment(inputPts[i], true);
            }
            AddLastSegment();
            // add line cap for start of line
            AddLineEndCap(inputPts[1], inputPts[0]);

            _vertexList.CloseRing();
        }

        private void ComputeRingBufferCurve(Coordinate[] inputPts, Position side)
        {
            // simplify input line to improve performance
            double distTol = SimplifyTolerance(_distance);
            // ensure that correct side is simplified
            if (side == Position.Right)
                distTol = -distTol;
            var simp = BufferInputLineSimplifier.Simplify(inputPts, distTol);
            // Coordinate[] simp = inputPts;

            int n = simp.Length - 1;
            InitSideSegments(simp[n - 1], simp[0], side);
            for (int i = 1; i <= n; i++)
            {
                bool addStartPoint = i != 1;
                AddNextSegment(simp[i], addStartPoint);
            }
            _vertexList.CloseRing();
        }

        private Coordinate _s0, _s1, _s2;
        private readonly LineSegment _seg0 = new LineSegment();
        private readonly LineSegment _seg1 = new LineSegment();
        private readonly LineSegment _offset0 = new LineSegment();
        private readonly LineSegment _offset1 = new LineSegment();
        private Position _side;

        private void InitSideSegments(Coordinate s1, Coordinate s2, Position side)
        {
            _s1 = s1;
            _s2 = s2;
            _side = side;
            _seg1.SetCoordinates(s1, s2);
            ComputeOffsetSegment(_seg1, side, _distance, _offset1);
        }

        //private static double _maxClosingSegLen = 3.0;

        private void AddNextSegment(Coordinate p, bool addStartPoint)
        {
            // s0-s1-s2 are the coordinates of the previous segment and the current one
            _s0 = _s1;
            _s1 = _s2;
            _s2 = p;
            _seg0.SetCoordinates(_s0, _s1);
            ComputeOffsetSegment(_seg0, _side, _distance, _offset0);
            _seg1.SetCoordinates(_s1, _s2);
            ComputeOffsetSegment(_seg1, _side, _distance, _offset1);

            // do nothing if points are equal
            if (_s1.Equals(_s2)) return;

            var orientation = Orientation.Index(_s0, _s1, _s2);
            bool outsideTurn =
                  (orientation == OrientationIndex.Clockwise && _side == Position.Left)
              || (orientation == OrientationIndex.CounterClockwise && _side == Position.Right);

            if (orientation == 0)
            { // lines are collinear
                AddCollinear(addStartPoint);
            }
            else if (outsideTurn)
            {
                AddOutsideTurn(orientation, addStartPoint);
            }
            else
            { // inside turn
                AddInsideTurn(orientation, addStartPoint);
            }
        }

        private void AddCollinear(bool addStartPoint)
        {
            /*
             * This test could probably be done more efficiently,
             * but the situation of exact collinearity should be fairly rare.
             */
            _li.ComputeIntersection(_s0, _s1, _s1, _s2);
            int numInt = _li.IntersectionNum;
            /*
             * if numInt is < 2, the lines are parallel and in the same direction. In
             * this case the point can be ignored, since the offset lines will also be
             * parallel.
             */
            if (numInt >= 2)
            {
                /*
                 * segments are collinear but reversing.
                 * Add an "end-cap" fillet
                 * all the way around to other direction This case should ONLY happen
                 * for LineStrings, so the orientation is always CW. (Polygons can never
                 * have two consecutive segments which are parallel but reversed,
                 * because that would be a self intersection.
                 *
                 */
                if (_bufParams.JoinStyle == JoinStyle.Bevel
                        || _bufParams.JoinStyle == JoinStyle.Mitre)
                {
                    if (addStartPoint) _vertexList.AddPt(_offset0.P1);
                    _vertexList.AddPt(_offset1.P0);
                }
                else
                {
                    AddFillet(_s1, _offset0.P1, _offset1.P0, OrientationIndex.Clockwise, _distance);
                }
            }
        }

        /// <summary>
        /// Adds the offset points for an outside (convex) turn
        /// </summary>
        /// <param name="orientation">
        /// </param>
        /// <param name="addStartPoint"></param>
        private void AddOutsideTurn(OrientationIndex orientation, bool addStartPoint)
        {
            /*
             * Heuristic: If offset endpoints are very close together,
             * just use one of them as the corner vertex.
             * This avoids problems with computing mitre corners in the case
             * where the two segments are almost parallel
             * (which is hard to compute a robust intersection for).
             */
            if (_offset0.P1.Distance(_offset1.P0) < _distance * OffsetSegmentSeparationFactor)
            {
                _vertexList.AddPt(_offset0.P1);
                return;
            }

            if (_bufParams.JoinStyle == JoinStyle.Mitre)
            {
                AddMitreJoin(_s1, _offset0, _offset1, _distance);
            }
            else if (_bufParams.JoinStyle == JoinStyle.Bevel)
            {
                AddBevelJoin(_offset0, _offset1);
            }
            else
            {
                // add a circular fillet connecting the endpoints of the offset segments
                if (addStartPoint) _vertexList.AddPt(_offset0.P1);
                // TESTING - comment out to produce beveled joins
                AddFillet(_s1, _offset0.P1, _offset1.P0, orientation, _distance);
                _vertexList.AddPt(_offset1.P0);
            }
        }

        /// <summary>
        /// Adds the offset points for an inside (concave) turn.
        /// </summary>
        /// <param name="orientation"></param>
        /// <param name="addStartPoint"></param>
        private void AddInsideTurn(OrientationIndex orientation, bool addStartPoint)
        {
            /*
             * add intersection point of offset segments (if any)
             */
            _li.ComputeIntersection(_offset0.P0, _offset0.P1, _offset1.P0, _offset1.P1);
            if (_li.HasIntersection)
            {
                _vertexList.AddPt(_li.GetIntersection(0));
            }
            else
            {
                /*
                 * If no intersection is detected, it means the angle is so small and/or the offset so
                 * large that the offsets segments don't intersect. In this case we must
                 * add a "closing segment" to make sure the buffer curve is continuous,
                 * fairly smooth (e.g. no sharp reversals in direction)
                 * and tracks the buffer correctly around the corner. The curve connects
                 * the endpoints of the segment offsets to points
                 * which lie toward the centre point of the corner.
                 * The joining curve will not appear in the final buffer outline, since it
                 * is completely internal to the buffer polygon.
                 *
                 * In complex buffer cases the closing segment may cut across many other
                 * segments in the generated offset curve.  In order to improve the
                 * performance of the noding, the closing segment should be kept as short as possible.
                 * (But not too short, since that would defeat it's purpose).
                 * This is the purpose of the closingSegFactor heuristic value.
                 *
                 * The intersection test above is vulnerable to robustness errors; i.e. it
                 * may be that the offsets should intersect very close to their endpoints,
                 * but aren't reported as such due to rounding. To handle this situation
                 * appropriately, we use the following test: If the offset points are very
                 * close, don't add closing segments but simply use one of the offset
                 * points
                 */
                if (_offset0.P1.Distance(_offset1.P0) < _distance * InsideTurnVertexSnapDistanceFactor)
                {
                    _vertexList.AddPt(_offset0.P1);
                }
                else
                {
                    /*
                     * Add "closing segment" of required length.
                     */
                    _vertexList.AddPt(_offset0.P1);

                    // add closing segments of required length
                    if (_closingSegFactor > 0)
                    {
                        var mid0 = new Coordinate((_closingSegFactor * _offset0.P1.X + _s1.X) / (_closingSegFactor + 1),
                            (_closingSegFactor * _offset0.P1.Y + _s1.Y) / (_closingSegFactor + 1));
                        _vertexList.AddPt(mid0);
                        var mid1 = new Coordinate((_closingSegFactor * _offset1.P0.X + _s1.X) / (_closingSegFactor + 1),
                           (_closingSegFactor * _offset1.P0.Y + _s1.Y) / (_closingSegFactor + 1));
                        _vertexList.AddPt(mid1);
                    }
                    else
                    {
                        /*
                         * This branch is not expected to be used except for testing purposes.
                         * It is equivalent to the JTS 1.9 logic for closing segments
                         * (which results in very poor performance for large buffer distances)
                         */
                        _vertexList.AddPt(_s1);
                    }

                    // add start point of next segment offset
                    _vertexList.AddPt(_offset1.P0);
                }
            }
        }

        /// <summary>
        /// Add last offset point
        /// </summary>
        private void AddLastSegment()
        {
            _vertexList.AddPt(_offset1.P1);
        }

        /// <summary>
        /// Compute an offset segment for an input segment on a given side and at a given distance.
        /// The offset points are computed in full double precision, for accuracy.
        /// </summary>
        /// <param name="seg">The segment to offset</param>
        /// <param name="side">The side of the segment (<see cref="Positions"/>) the offset lies on</param>
        /// <param name="distance">The offset distance</param>
        /// <param name="offset">The points computed for the offset segment</param>
        private static void ComputeOffsetSegment(LineSegment seg, Position side, double distance, LineSegment offset)
        {
            int sideSign = side == Position.Left ? 1 : -1;
            double dx = seg.P1.X - seg.P0.X;
            double dy = seg.P1.Y - seg.P0.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            // u is the vector that is the length of the offset, in the direction of the segment
            double ux = sideSign * distance * dx / len;
            double uy = sideSign * distance * dy / len;
            offset.P0.X = seg.P0.X - uy;
            offset.P0.Y = seg.P0.Y + ux;
            offset.P1.X = seg.P1.X - uy;
            offset.P1.Y = seg.P1.Y + ux;
        }

        /// <summary>
        /// Add an end cap around point p1, terminating a line segment coming from p0
        /// </summary>
        private void AddLineEndCap(Coordinate p0, Coordinate p1)
        {
            var seg = new LineSegment(p0, p1);

            var offsetL = new LineSegment();
            ComputeOffsetSegment(seg, Position.Left, _distance, offsetL);
            var offsetR = new LineSegment();
            ComputeOffsetSegment(seg, Position.Right, _distance, offsetR);

            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double angle = Math.Atan2(dy, dx);

            switch (_bufParams.EndCapStyle)
            {
                case EndCapStyle.Round:
                    // add offset seg points with a fillet between them
                    _vertexList.AddPt(offsetL.P1);
                    AddFillet(p1, angle + Math.PI / 2, angle - Math.PI / 2, OrientationIndex.Clockwise, _distance);
                    _vertexList.AddPt(offsetR.P1);
                    break;
                case EndCapStyle.Flat:
                    // only offset segment points are added
                    _vertexList.AddPt(offsetL.P1);
                    _vertexList.AddPt(offsetR.P1);
                    break;
                case EndCapStyle.Square:
                    // add a square defined by extensions of the offset segment endpoints
                    var squareCapSideOffset = new Coordinate();
                    squareCapSideOffset.X = Math.Abs(_distance) * Math.Cos(angle);
                    squareCapSideOffset.Y = Math.Abs(_distance) * Math.Sin(angle);

                    var squareCapLOffset = new Coordinate(
                        offsetL.P1.X + squareCapSideOffset.X,
                        offsetL.P1.Y + squareCapSideOffset.Y);
                    var squareCapROffset = new Coordinate(
                        offsetR.P1.X + squareCapSideOffset.X,
                        offsetR.P1.Y + squareCapSideOffset.Y);
                    _vertexList.AddPt(squareCapLOffset);
                    _vertexList.AddPt(squareCapROffset);
                    break;
            }
        }

        /// <summary>
        /// Adds a mitre join connecting the two reflex offset segments.
        /// The mitre will be beveled if it exceeds the mitre ratio limit.
        /// </summary>
        /// <param name="p">The base point</param>
        /// <param name="offset0">The first offset segment</param>
        /// <param name="offset1">The second offset segment</param>
        /// <param name="distance">The offset distance</param>
        private void AddMitreJoin(Coordinate p,
              LineSegment offset0,
              LineSegment offset1,
              double distance)
        {
            bool isMitreWithinLimit = true;
            Coordinate intPt;

            /*
             * This computation is unstable if the offset segments are nearly collinear.
             * Howver, this situation should have been eliminated earlier by the check for
             * whether the offset segment endpoints are almost coincident
             */
            try
            {
                intPt = HCoordinate.Intersection(offset0.P0,
                       offset0.P1, offset1.P0, offset1.P1);

                double mitreRatio = distance <= 0.0 ? 1.0
                        : intPt.Distance(p) / Math.Abs(distance);

                if (mitreRatio > _bufParams.MitreLimit)
                    isMitreWithinLimit = false;
            }
            catch (NotRepresentableException ex)
            {
                intPt = new Coordinate(0, 0);
                isMitreWithinLimit = false;
            }

            if (isMitreWithinLimit)
            {
                _vertexList.AddPt(intPt);
            }
            else
            {
                AddLimitedMitreJoin(offset0, offset1, distance, _bufParams.MitreLimit);
                // addBevelJoin(offset0, offset1);
            }
        }

        /// <summary>
        /// Adds a limited mitre join connecting the two reflex offset segments.
        /// </summary>
        /// <remarks>
        /// A limited mitre is a mitre which is beveled at the distance
        /// determined by the mitre ratio limit.
        /// </remarks>
        /// <param name="offset0">The first offset segment</param>
        /// <param name="offset1">The second offset segment</param>
        /// <param name="distance">The offset distance</param>
        /// <param name="mitreLimit">The mitre limit ratio</param>
        private void AddLimitedMitreJoin(
              LineSegment offset0,
              LineSegment offset1,
              double distance,
              double mitreLimit)
        {
            var basePt = _seg0.P1;

            double ang0 = AngleUtility.Angle(basePt, _seg0.P0);
            double ang1 = AngleUtility.Angle(basePt, _seg1.P1);

            // oriented angle between segments
            double angDiff = AngleUtility.AngleBetweenOriented(_seg0.P0, basePt, _seg1.P1);
            // half of the interior angle
            double angDiffHalf = angDiff / 2;

            // angle for bisector of the interior angle between the segments
            double midAng = AngleUtility.Normalize(ang0 + angDiffHalf);
            // rotating this by PI gives the bisector of the reflex angle
            double mitreMidAng = AngleUtility.Normalize(midAng + Math.PI);

            // the miterLimit determines the distance to the mitre bevel
            double mitreDist = mitreLimit * distance;
            // the bevel delta is the difference between the buffer distance
            // and half of the length of the bevel segment
            double bevelDelta = mitreDist * Math.Abs(Math.Sin(angDiffHalf));
            double bevelHalfLen = distance - bevelDelta;

            // compute the midpoint of the bevel segment
            double bevelMidX = basePt.X + mitreDist * Math.Cos(mitreMidAng);
            double bevelMidY = basePt.Y + mitreDist * Math.Sin(mitreMidAng);
            var bevelMidPt = new Coordinate(bevelMidX, bevelMidY);

            // compute the mitre midline segment from the corner point to the bevel segment midpoint
            var mitreMidLine = new LineSegment(basePt, bevelMidPt);

            // finally the bevel segment endpoints are computed as offsets from
            // the mitre midline
            var bevelEndLeft = mitreMidLine.PointAlongOffset(1.0, bevelHalfLen);
            var bevelEndRight = mitreMidLine.PointAlongOffset(1.0, -bevelHalfLen);

            if (_side == Position.Left)
            {
                _vertexList.AddPt(bevelEndLeft);
                _vertexList.AddPt(bevelEndRight);
            }
            else
            {
                _vertexList.AddPt(bevelEndRight);
                _vertexList.AddPt(bevelEndLeft);
            }
        }

        /// <summary>
        /// Adds a bevel join connecting the two offset segments around a reflex corner.
        /// </summary>
        /// <param name="offset0">The first offset segment</param>
        /// <param name="offset1">The second offset segment</param>
        private void AddBevelJoin(
              LineSegment offset0,
              LineSegment offset1)
        {
            _vertexList.AddPt(offset0.P1);
            _vertexList.AddPt(offset1.P0);
        }

        /// <summary>
        /// Add points for a circular fillet around a reflex corner. Adds the start and end points
        /// </summary>
        /// <param name="p">Base point of curve</param>
        /// <param name="p0">Start point of fillet curve</param>
        /// <param name="p1">Endpoint of fillet curve</param>
        /// <param name="direction">The orientation of the fillet</param>
        /// <param name="radius">The radius of the fillet</param>
        private void AddFillet(Coordinate p, Coordinate p0, Coordinate p1, OrientationIndex direction, double radius)
        {
            double dx0 = p0.X - p.X;
            double dy0 = p0.Y - p.Y;
            double startAngle = Math.Atan2(dy0, dx0);
            double dx1 = p1.X - p.X;
            double dy1 = p1.Y - p.Y;
            double endAngle = Math.Atan2(dy1, dx1);

            if (direction == OrientationIndex.Clockwise)
            {
                if (startAngle <= endAngle) startAngle += 2.0 * Math.PI;
            }
            else
            {    // direction == COUNTERCLOCKWISE
                if (startAngle >= endAngle) startAngle -= 2.0 * Math.PI;
            }
            _vertexList.AddPt(p0);
            AddFillet(p, startAngle, endAngle, direction, radius);
            _vertexList.AddPt(p1);
        }

        /// <summary>
        /// Adds points for a circular fillet arc between two specified angles.
        /// </summary>
        /// <remarks>
        /// The start and end point for the fillet are not added - the caller must add them if required.
        /// </remarks>
        /// <param name="p">The point around which to add the fillet points</param>
        /// <param name="startAngle">The start angle (in radians)</param>
        /// <param name="endAngle">The end angle (in radians)</param>
        /// <param name="direction">Is -1 for a CW angle, 1 for a CCW angle</param>
        /// <param name="radius">The radius of the fillet</param>
        private void AddFillet(Coordinate p, double startAngle, double endAngle, OrientationIndex direction, double radius)
        {
            int directionFactor = direction == OrientationIndex.Clockwise ? -1 : 1;

            double totalAngle = Math.Abs(startAngle - endAngle);
            int nSegs = (int)(totalAngle / _filletAngleQuantum + 0.5);

            if (nSegs < 1) return;    // no segments because angle is less than increment - nothing to do!

            double initAngle, currAngleInc;

            // choose angle increment so that each segment has equal length
            initAngle = 0.0;
            currAngleInc = totalAngle / nSegs;

            double currAngle = initAngle;
            var pt = new Coordinate();
            while (currAngle < totalAngle)
            {
                double angle = startAngle + directionFactor * currAngle;
                pt.X = p.X + radius * Math.Cos(angle);
                pt.Y = p.Y + radius * Math.Sin(angle);
                _vertexList.AddPt(pt);
                currAngle += currAngleInc;
            }
        }

        /// <summary>
        /// Adds a CW circle around a point
        /// </summary>
        private void AddCircle(Coordinate p, double distance)
        {
            // add start point
            var pt = new Coordinate(p.X + distance, p.Y);
            _vertexList.AddPt(pt);
            AddFillet(p, 0.0, 2.0 * Math.PI, OrientationIndex.Clockwise, distance);
        }

        /// <summary>
        /// Adds a CW square around a point
        /// </summary>
        private void AddSquare(Coordinate p, double distance)
        {
            // add start point
            _vertexList.AddPt(new Coordinate(p.X + distance, p.Y + distance));
            _vertexList.AddPt(new Coordinate(p.X + distance, p.Y - distance));
            _vertexList.AddPt(new Coordinate(p.X - distance, p.Y - distance));
            _vertexList.AddPt(new Coordinate(p.X - distance, p.Y + distance));
            _vertexList.AddPt(new Coordinate(p.X + distance, p.Y + distance));
        }
    }
}
