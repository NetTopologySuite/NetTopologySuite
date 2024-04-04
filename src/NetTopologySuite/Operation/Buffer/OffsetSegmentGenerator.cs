using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Generates segments which form an offset curve.
    /// Supports all end cap and join options
    /// provided for buffering.
    /// This algorithm implements various heuristics to
    /// produce smoother, simpler curves which are
    /// still within a reasonable tolerance of the
    /// true curve.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class OffsetSegmentGenerator
    {
        /// <summary>
        /// Factor controlling how close offset segments can be to
        /// skip adding a fillet or mitre.
        /// This eliminates very short fillet segments,
        /// reduces the number of offset curve vertices.
        /// and improves the robustness of mitre construction.
        /// </summary>
        private const double OffsetSegmentSeparationFactor = 0.05;

        /// <summary>
        /// Factor controlling how close curve vertices on inside turns can be to be snapped
        /// </summary>
        private const double InsideTurnVertexSnapDistanceFactor = 1.0E-3;

        /// <summary>
        /// Factor which controls how close curve vertices can be to be snapped
        /// </summary>
        private const double CurveVertexSnapDistanceFactor = 1.0E-6;

        /// <summary>
        /// Factor which determines how short closing segs can be for round buffers
        /// </summary>
        private const int MaxClosingSegLenFactor = 80;

        /// <summary>
        /// The max error of approximation (distance) between a quad segment and the true fillet curve
        /// </summary>
        private double _maxCurveSegmentError;

        /// <summary>
        /// The angle quantum with which to approximate a fillet curve
        /// (based on the input # of quadrant segments)
        /// </summary>
        private readonly double _filletAngleQuantum;

        /// <summary>
        /// The Closing Segment Length Factor controls how long
        /// "closing segments" are.  Closing segments are added
        /// at the middle of inside corners to ensure a smoother
        /// boundary for the buffer offset curve.
        /// In some cases (particularly for round joins with default-or-better
        /// quantization) the closing segments can be made quite short.
        /// This substantially improves performance (due to fewer intersections being created).
        /// <list type="bullet">
        /// <item><description>A closingSegFactor of 0 results in lines to the corner vertex</description></item>
        /// <item><description>A closingSegFactor of 1 results in lines halfway to the corner vertex</description></item>
        /// <item><description> A closingSegFactor of 80 results in lines 1/81 of the way to the corner vertex
        /// (this option is reasonable for the very common default situation of round joins
        /// and quadrantSegs >= 8)</description></item>
        /// </list>
        /// </summary>
        private readonly int _closingSegLengthFactor = 1;

        private OffsetSegmentString _segList;
        private double _distance;
        private readonly PrecisionModel _precisionModel;
        private readonly BufferParameters _bufParams;
        private readonly LineIntersector _li;

        private Coordinate _s0, _s1, _s2;
        private readonly LineSegment _seg0 = new LineSegment();
        private readonly LineSegment _seg1 = new LineSegment();
        private readonly LineSegment _offset0 = new LineSegment();
        private readonly LineSegment _offset1 = new LineSegment();
        private Position _side = Position.On;
        private bool _hasNarrowConcaveAngle;

        public OffsetSegmentGenerator(PrecisionModel precisionModel,
            BufferParameters bufParams, double distance)
        {
            _precisionModel = precisionModel;
            _bufParams = bufParams;

            // compute intersections in full precision, to provide accuracy
            // the points are rounded as they are inserted into the curve line
            _li = new RobustLineIntersector();

            int quadSegs = bufParams.QuadrantSegments;
            if (quadSegs < 1) quadSegs = 1;
            _filletAngleQuantum = AngleUtility.PiOver2 / quadSegs;

            /*
             * Non-round joins cause issues with short closing segments, so don't use
             * them. In any case, non-round joins only really make sense for relatively
             * small buffer distances.
             */
            if (bufParams.QuadrantSegments >= 8
                && bufParams.JoinStyle == JoinStyle.Round)
                _closingSegLengthFactor = MaxClosingSegLenFactor;
            Init(distance);
        }

        /// <summary>
        /// Gets whether the input has a narrow concave angle
        /// (relative to the offset distance).
        /// In this case the generated offset curve will contain self-intersections
        /// and heuristic closing segments.
        /// This is expected behaviour in the case of buffer curves.
        /// For pure offset curves,
        /// the output needs to be further treated
        /// before it can be used.
        /// </summary>
        public bool HasNarrowConcaveAngle => _hasNarrowConcaveAngle;

        private void Init(double distance)
        {
            _distance = Math.Abs(distance);
            _maxCurveSegmentError = _distance * (1 - Math.Cos(_filletAngleQuantum / 2.0));
            _segList = new OffsetSegmentString {
                PrecisionModel = _precisionModel,
                // Choose the min vertex separation as a small fraction of the offset distance.
                MinimumVertexDistance = _distance * CurveVertexSnapDistanceFactor
            };
        }

        [Obsolete("Use InitSideSegments(Coordinate, Coordinate, Geometries.Position)")]
        public void InitSideSegments(Coordinate s1, Coordinate s2, Positions side) =>
            InitSideSegments(s1, s1, new Position((int)side));

        public void InitSideSegments(Coordinate s1, Coordinate s2, Position side)
        {
            _s1 = s1;
            _s2 = s2;
            _side = side;
            _seg1.SetCoordinates(s1, s2);
            ComputeOffsetSegment(_seg1, side, _distance, _offset1);
        }

        public Coordinate[] GetCoordinates()
        {
            var pts = _segList.GetCoordinates();
            return pts;
        }

        public void CloseRing()
        {
            _segList.CloseRing();
        }

        public void AddSegments(Coordinate[] pt, bool isForward)
        {
            _segList.AddPts(pt, isForward);
        }

        public void AddFirstSegment()
        {
            _segList.AddPt(_offset1.P0);
        }

        /// <summary>
        /// Add last offset point
        /// </summary>
        public void AddLastSegment()
        {
            _segList.AddPt(_offset1.P1);
        }

        //private static double MAX_CLOSING_SEG_LEN = 3.0;

        public void AddNextSegment(Coordinate p, bool addStartPoint)
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
                    if (addStartPoint) _segList.AddPt(_offset0.P1);
                    _segList.AddPt(_offset1.P0);
                }
                else
                {
                    AddCornerFillet(_s1, _offset0.P1, _offset1.P0, OrientationIndex.Clockwise, _distance);
                }
            }
        }

        /// <summary>
        /// Adds the offset points for an outside (convex) turn
        /// </summary>
        private void AddOutsideTurn(OrientationIndex orientation, bool addStartPoint)
        {
            /*
             * Heuristic: If offset endpoints are very close together,
             * (which happens for nearly-parallel segments),
             * use an endpoint as the single offset corner vertex.
             * This eliminates very short single-segment joins
             * and reduces the number of offset curve vertices.
             * This also avoids robustness problems with computing mitre corners 
             * for nearly-parallel segments.
             */
            if (_offset0.P1.Distance(_offset1.P0) < _distance * OffsetSegmentSeparationFactor)
            {
                //-- use endpoint of longest segment, to reduce change in area
                double segLen0 = _s0.Distance(_s1);
                double segLen1 = _s1.Distance(_s2);
                var offsetPt = (segLen0 > segLen1) ? _offset0.P1 : _offset1.P0;
                _segList.AddPt(offsetPt);
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
                if (addStartPoint)
                {
                    _segList.AddPt(_offset0.P1);
                }
                AddCornerFillet(_s1, _offset0.P1, _offset1.P0, orientation, _distance);
                _segList.AddPt(_offset1.P0);
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
                _segList.AddPt(_li.GetIntersection(0));
            }
            else
            {
                /*
                 * If no intersection is detected,
                 * it means the angle is so small and/or the offset so
                 * large that the offsets segments don't intersect.
                 * In this case we must
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
                 * (But not too short, since that would defeat its purpose).
                 * This is the purpose of the closingSegFactor heuristic value.
                 */

                /*
                 * The intersection test above is vulnerable to robustness errors; i.e. it
                 * may be that the offsets should intersect very close to their endpoints,
                 * but aren't reported as such due to rounding. To handle this situation
                 * appropriately, we use the following test: If the offset points are very
                 * close, don't add closing segments but simply use one of the offset
                 * points
                 */
                _hasNarrowConcaveAngle = true;
                //System.out.println("NARROW ANGLE - distance = " + distance);
                if (_offset0.P1.Distance(_offset1.P0) < _distance
                    * InsideTurnVertexSnapDistanceFactor)
                {
                    _segList.AddPt(_offset0.P1);
                }
                else
                {
                    // add endpoint of this segment offset
                    _segList.AddPt(_offset0.P1);

                    /*
                     * Add "closing segment" of required length.
                     */
                    if (_closingSegLengthFactor > 0)
                    {
                        var mid0 = new Coordinate((_closingSegLengthFactor * _offset0.P1.X + _s1.X) / (_closingSegLengthFactor + 1),
                            (_closingSegLengthFactor * _offset0.P1.Y + _s1.Y) / (_closingSegLengthFactor + 1));
                        _segList.AddPt(mid0);
                        var mid1 = new Coordinate((_closingSegLengthFactor * _offset1.P0.X + _s1.X) / (_closingSegLengthFactor + 1),
                           (_closingSegLengthFactor * _offset1.P0.Y + _s1.Y) / (_closingSegLengthFactor + 1));
                        _segList.AddPt(mid1);
                    }
                    else
                    {
                        /*
                         * This branch is not expected to be used except for testing purposes.
                         * It is equivalent to the JTS 1.9 logic for closing segments
                         * (which results in very poor performance for large buffer distances)
                         */
                        _segList.AddPt(_s1);
                    }

                    //*/
                    // add start point of next segment offset
                    _segList.AddPt(_offset1.P0);
                }
            }
        }

        /// <summary>
        /// Compute an offset segment for an input segment on a given side and at a given distance.
        /// The offset points are computed in full double precision, for accuracy.
        /// </summary>
        /// <param name="seg">The segment to offset</param>
        /// <param name="side">The side of the segment <see cref="Positions"/> the offset lies on</param>
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
        /// Add an end cap around point <paramref name="p1"/>, terminating a line segment coming from <paramref name="p0"/>
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public void AddLineEndCap(Coordinate p0, Coordinate p1)
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
                    _segList.AddPt(offsetL.P1);
                    AddDirectedFillet(p1, angle + AngleUtility.PiOver2, angle - AngleUtility.PiOver2, OrientationIndex.Clockwise, _distance);
                    _segList.AddPt(offsetR.P1);
                    break;
                case EndCapStyle.Flat:
                    // only offset segment points are added
                    _segList.AddPt(offsetL.P1);
                    _segList.AddPt(offsetR.P1);
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
                    _segList.AddPt(squareCapLOffset);
                    _segList.AddPt(squareCapROffset);
                    break;
            }
        }

        /// <summary>
        /// Adds a mitre join connecting two convex offset segments.
        /// The mitre is beveled if it exceeds the mitre limit factor.
        /// The mitre limit is intended to prevent extremely long corners occurring.
        /// If the mitre limit is very small it can cause unwanted artifacts around fairly flat corners.
        /// This is prevented by using a simple bevel join in this case.
        /// In other words, the limit prevents the corner from getting too long,
        /// but it won't force it to be very short/flat.
        /// </summary>
        /// <param name="cornerPt">A corner point</param>
        /// <param name="offset0">The first offset segment</param>
        /// <param name="offset1">The second offset segment</param>
        /// <param name="distance">The offset distance</param>
        private void AddMitreJoin(Coordinate cornerPt,
            LineSegment offset0,
            LineSegment offset1,
            double distance)
        {
            double mitreLimitDistance = _bufParams.MitreLimit * distance;
            /*
             * First try a non-beveled join.
             * Compute the intersection point of the lines determined by the offsets.
             * Parallel or collinear lines will return a null point ==> need to be beveled
             * 
             * Note: This computation is unstable if the offset segments are nearly collinear.
             * However, this situation should have been eliminated earlier by the check
             * for whether the offset segment endpoints are almost coincident
             */
            var intPt = IntersectionComputer.Intersection(offset0.P0, offset0.P1, offset1.P0, offset1.P1);
            if (intPt != null && intPt.Distance(cornerPt) <= mitreLimitDistance)
            {
                _segList.AddPt(intPt);
                return;
            }
            /*
             * In case the mitre limit is very small, try a plain bevel.
             * Use it if it's further than the limit.
             */
            double bevelDist = DistanceComputer.PointToSegment(cornerPt, offset0.P1, offset1.P0);
            if (bevelDist >= mitreLimitDistance)
            {
                AddBevelJoin(offset0, offset1);
                return;
            }
            /*
             * Have to construct a limited mitre bevel.
             */
            AddLimitedMitreJoin(offset0, offset1, distance, mitreLimitDistance);
        }

        /// <summary>
        /// Adds a limited mitre join connecting two convex offset segments.
        /// A limited mitre join is beveled at the distance
        /// determined by the mitre limit factor,
        /// or as a standard bevel join, whichever is further.
        /// </summary>
        /// <param name="offset0">The first offset segment</param>
        /// <param name="offset1">The second offset segment</param>
        /// <param name="distance">The offset distance</param>
        /// <param name="mitreLimitDistance">The mitre limit distance</param>
        private void AddLimitedMitreJoin(
            LineSegment offset0,
            LineSegment offset1,
            double distance,
            double mitreLimitDistance)
        {
            var cornerPt = _seg0.P1;

            // oriented angle of the corner formed by segments
            double angInterior = AngleUtility.AngleBetweenOriented(_seg0.P0, cornerPt, _seg1.P1);
            // half of the interior angle
            double angInterior2 = angInterior / 2;

            // direction of bisector of the interior angle between the segments
            double dir0 = AngleUtility.Angle(cornerPt, _seg0.P0);
            double dirBisector = AngleUtility.Normalize(dir0 + angInterior2);

            // midpoint of the bevel segment
            var bevelMidPt = Project(cornerPt, -mitreLimitDistance, dirBisector);

            // direction of bevel segment (at right angle to corner bisector)
            double dirBevel = AngleUtility.Normalize(dirBisector + AngleUtility.PiOver2);

            // compute the candidate bevel segment by projecting both sides of the midpoint
            var bevel0 = Project(bevelMidPt, distance, dirBevel);
            var bevel1 = Project(bevelMidPt, distance, dirBevel + Math.PI);

            // compute actual bevel segment between the offset lines
            var bevelInt0 = IntersectionComputer.LineSegment(offset0.P0, offset0.P1, bevel0, bevel1);
            var bevelInt1 = IntersectionComputer.LineSegment(offset1.P0, offset1.P1, bevel0, bevel1);

            //-- add the limited bevel, if it intersects the offsets
            if (bevelInt0 != null && bevelInt1 != null)
            {
                _segList.AddPt(bevelInt0);
                _segList.AddPt(bevelInt1);
                return;
            }
            /*
             * If the corner is very flat or the mitre limit is very small
             * the limited bevel segment may not intersect the offsets.
             * In this case just bevel the join.
             */
            AddBevelJoin(offset0, offset1);
        }

        /// <summary>
        /// Projects a point to a given distance in a given direction angle.
        /// </summary>
        /// <param name="pt">The point to project</param>
        /// <param name="d">The projection distance</param>
        /// <param name="dir">The direction angle (in radians)</param>
        /// <returns>The projected point</returns>
        private static Coordinate Project(Coordinate pt, double d, double dir)
        {
            double x = pt.X + d * AngleUtility.CosSnap(dir);
            double y = pt.Y + d * AngleUtility.SinSnap(dir);
            return new Coordinate(x, y);
        }

        /// <summary>
        /// Adds a bevel join connecting the two offset segments
        /// around a reflex corner.
        /// </summary>
        /// <param name="offset0">The first offset segment</param>
        /// <param name="offset1">The second offset segment</param>
        private void AddBevelJoin(
            LineSegment offset0,
            LineSegment offset1)
        {
            _segList.AddPt(offset0.P1);
            _segList.AddPt(offset1.P0);
        }

        /// <summary>
        /// Add points for a circular fillet around a convex corner.
        /// Adds the start and end points
        /// </summary>
        /// <param name="p">Base point of curve</param>
        /// <param name="p0">Start point of fillet curve</param>
        /// <param name="p1">Endpoint of fillet curve</param>
        /// <param name="direction">The orientation of the fillet</param>
        /// <param name="radius">The radius of the fillet</param>
        private void AddCornerFillet(Coordinate p, Coordinate p0, Coordinate p1, OrientationIndex direction, double radius)
        {
            double dx0 = p0.X - p.X;
            double dy0 = p0.Y - p.Y;
            double startAngle = Math.Atan2(dy0, dx0);
            double dx1 = p1.X - p.X;
            double dy1 = p1.Y - p.Y;
            double endAngle = Math.Atan2(dy1, dx1);

            if (direction == OrientationIndex.Clockwise)
            {
                if (startAngle <= endAngle) startAngle += AngleUtility.PiTimes2;
            }
            else
            {    // direction == COUNTERCLOCKWISE
                if (startAngle >= endAngle) startAngle -= AngleUtility.PiTimes2;
            }
            _segList.AddPt(p0);
            AddDirectedFillet(p, startAngle, endAngle, direction, radius);
            _segList.AddPt(p1);
        }

        /// <summary>
        /// Adds points for a circular fillet arc
        /// between two specified angles.
        /// The start and end point for the fillet are not added -
        /// the caller must add them if required.
        /// </summary>
        /// <param name="p">The center point</param>
        /// <param name="direction">Is -1 for a <see cref="OrientationIndex.Clockwise"/> angle, 1 for a <see cref="OrientationIndex.CounterClockwise"/> angle</param>
        /// <param name="startAngle">The start angle of the fillet</param>
        /// <param name="endAngle">The end angle of the fillet</param>
        /// <param name="radius">The radius of the fillet</param>
        private void AddDirectedFillet(Coordinate p, double startAngle, double endAngle, OrientationIndex direction, double radius)
        {
            int directionFactor = direction == OrientationIndex.Clockwise ? -1 : 1;

            double totalAngle = Math.Abs(startAngle - endAngle);
            int nSegs = (int)(totalAngle / _filletAngleQuantum + 0.5);

            if (nSegs < 1) return;    // no segments because angle is less than increment - nothing to do!

            // choose angle increment so that each segment has equal length
            double angleInc = totalAngle / nSegs;
            var pt = new Coordinate();
            for (int i = 0; i < nSegs; i++)
            {
                double angle = startAngle + directionFactor * i * angleInc;
                pt.X = p.X + radius * AngleUtility.CosSnap(angle);
                pt.Y = p.Y + radius * AngleUtility.SinSnap(angle);
                _segList.AddPt(pt);
            }
        }

        /// <summary>
        /// Creates a <see cref="OrientationIndex.Clockwise"/> circle around a point
        /// </summary>
        public void CreateCircle(Coordinate p)
        {
            // add start point
            var pt = new Coordinate(p.X + _distance, p.Y);
            _segList.AddPt(pt);
            AddDirectedFillet(p, 0.0, AngleUtility.PiTimes2, OrientationIndex.Clockwise, _distance);
            _segList.CloseRing();
        }

        /// <summary>
        /// Creates a <see cref="OrientationIndex.Clockwise"/> square around a point
        /// </summary>
        public void CreateSquare(Coordinate p)
        {
            _segList.AddPt(new Coordinate(p.X + _distance, p.Y + _distance));
            _segList.AddPt(new Coordinate(p.X + _distance, p.Y - _distance));
            _segList.AddPt(new Coordinate(p.X - _distance, p.Y - _distance));
            _segList.AddPt(new Coordinate(p.X - _distance, p.Y + _distance));
            _segList.CloseRing();
        }
    }
}
