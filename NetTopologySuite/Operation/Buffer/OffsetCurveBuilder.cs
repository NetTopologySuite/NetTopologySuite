using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes the raw offset curve for a
    /// single <see cref="Geometry{TCoordinate}"/> component (ring, line or point).
    /// A raw offset curve line is not noded -
    /// it may contain self-intersections (and usually will).
    /// The final buffer polygon is computed by forming a topological graph
    /// of all the noded raw curves and tracing outside contours.
    /// The points in the raw curve are rounded to the required precision model.
    /// </summary>
    public class OffsetCurveBuilder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// The default number of facets into which to divide a fillet of 90 degrees.
        /// A value of 8 gives less than 2% max error in the buffer distance.
        /// For a max error smaller of 1%, use QS = 12
        /// </summary>
        public const Int32 DefaultQuadrantSegments = 8;

        //private static readonly ICoordinate[] arrayTypeCoordinate = new ICoordinate[0];
        private readonly LineIntersector<TCoordinate> _li;

        /*
        * The angle quantum with which to approximate a fillet curve
        * (based on the input # of quadrant segments)
        */
        private readonly Double _filletAngleQuantum;

        /*
        * the max error of approximation between a quad segment and the true fillet curve
        */
        private Double _maxCurveSegmentError = 0.0;

        private readonly ICoordinateFactory<TCoordinate> _coordinateFactory;
        private readonly ICoordinateSequenceFactory<TCoordinate> _sequenceFactory;
        private readonly List<TCoordinate> _coordinates = new List<TCoordinate>();
        private Double _distance = 0.0;
        private readonly IPrecisionModel<TCoordinate> _precisionModel;
        private BufferStyle _endCapStyle = BufferStyle.CapRound;
        private TCoordinate _s0, _s1, _s2;
        private LineSegment<TCoordinate> _seg0 = new LineSegment<TCoordinate>();
        private LineSegment<TCoordinate> _seg1 = new LineSegment<TCoordinate>();
        private LineSegment<TCoordinate> _offset0 = new LineSegment<TCoordinate>();
        private LineSegment<TCoordinate> _offset1 = new LineSegment<TCoordinate>();
        private Positions _side = 0;

        public OffsetCurveBuilder(IPrecisionModel<TCoordinate> precisionModel)
            : this(precisionModel, DefaultQuadrantSegments) { }

        public OffsetCurveBuilder(IPrecisionModel<TCoordinate> precisionModel, Int32 quadrantSegments)
            : this(precisionModel, quadrantSegments, Coordinates<TCoordinate>.DefaultCoordinateSequenceFactory)
        { }

        public OffsetCurveBuilder(IPrecisionModel<TCoordinate> precisionModel, 
            Int32 quadrantSegments, ICoordinateSequenceFactory<TCoordinate> sequenceFactory)
        {
            _coordinateFactory = sequenceFactory.CoordinateFactory;
            _sequenceFactory = sequenceFactory;
            _precisionModel = precisionModel;

            // compute intersections in full precision, to provide accuracy
            // the points are rounded as they are inserted into the curve line
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector();

            Int32 limitedQuadSegs = quadrantSegments < 1 ? 1 : quadrantSegments;
            _filletAngleQuantum = Math.PI / 2.0 / limitedQuadSegs;
        }

        public BufferStyle EndCapStyle
        {
            get { return _endCapStyle; }
            set { _endCapStyle = value; }
        }

        /// <summary>
        /// This method handles single points as well as lines.
        /// Lines are assumed to not be closed (the function will not
        /// fail for closed lines, but will generate superfluous line caps).
        /// </summary>
        /// <returns> A set of coordinate sets.</returns>
        public IEnumerable<ICoordinateSequence<TCoordinate>> GetLineCurve(IEnumerable<TCoordinate> inputPts, Double distance)
        {
            // a zero or negative width buffer of a line/point is empty
            if (distance <= 0.0)
            {
                yield break;
            }

            init(distance);

            if (!Slice.CountGreaterThan(inputPts, 1))
            {
                TCoordinate start = Slice.GetFirst(inputPts);

                switch (_endCapStyle)
                {
                    case BufferStyle.CapRound:
                        addCircle(start, distance);
                        break;
                    case BufferStyle.CapSquare:
                        addSquare(start, distance);
                        break;
                    default:
                        // default is for buffer to be empty (e.g. for a butt line cap);
                        break;
                }
            }
            else
            {
                computeLineBufferCurve(inputPts);
            }

            yield return _sequenceFactory.Create(_coordinates);
        }

        /// <summary>
        /// This method handles the degenerate cases of single points and lines,
        /// as well as rings.
        /// </summary>
        /// <returns>A set of coordinate sets.</returns>
        public IEnumerable<ICoordinateSequence<TCoordinate>> GetRingCurve(IEnumerable<TCoordinate> inputPts, Positions side, Double distance)
        {
            init(distance);

            if (Slice.CountGreaterThan(inputPts, 1))
            {
                foreach (ICoordinateSequence<TCoordinate> coordinates in GetLineCurve(inputPts, distance))
                {
                    yield return coordinates;
                }
            }
            else
            {
                // optimize creating ring for for zero distance
                if (distance == 0.0)
                {
                    yield return _sequenceFactory.Create(inputPts);
                }
                else
                {
                    computeRingBufferCurve(inputPts, side);

                    yield return _sequenceFactory.Create(coordinatesAsRing());
                }
            }
        }

        //private static ICoordinate[] CopyCoordinates(ICoordinate[] pts)
        //{
        //    ICoordinate[] copy = new ICoordinate[pts.Length];
        //    for (Int32 i = 0; i < copy.Length; i++)
        //    {
        //        copy[i] = new Coordinate(pts[i]);
        //    }
        //    return copy;
        //}

        private void init(Double distance)
        {
            _distance = distance;
            _maxCurveSegmentError = distance * (1 - Math.Cos(_filletAngleQuantum / 2.0));
            //_coordinates = new ArrayList();
        }

        private IEnumerable<TCoordinate> coordinatesAsRing()
        {
            TCoordinate start = Slice.GetFirst(_coordinates);
            yield return start;

            TCoordinate last = default(TCoordinate);

            foreach (TCoordinate coordinate in Slice.StartAt(_coordinates, 1))
            {
                last = coordinate;
                yield return coordinate;
            }

            if (!last.Equals(start))
            {
                yield return start;
            }
        }

        private void computeLineBufferCurve(IEnumerable<TCoordinate> inputPts)
        {
            Pair<TCoordinate> initPoints = Slice.GetPair(inputPts).Value;

            // compute points for left side of line
            initSideSegments(initPoints.First, initPoints.Second, Positions.Left);

            foreach (TCoordinate point in Slice.StartAt(inputPts, 2))
            {
                addNextSegment(point, true);
            }

            addLastSegment();

            Pair<TCoordinate> lastPoints = Slice.GetLastPair(inputPts).Value;

            // add line cap for end of line
            addLineEndCap(lastPoints.First, lastPoints.Second);

            // compute points for right side of line
            initSideSegments(lastPoints.Second, lastPoints.First, Positions.Left);

            foreach (TCoordinate point in Slice.ReverseStartAt(inputPts, 2))
            {
                addNextSegment(point, true);
            }

            addLastSegment();

            // add line cap for start of line
            addLineEndCap(initPoints.Second, initPoints.First);
            closePoints();
        }

        private void computeRingBufferCurve(IEnumerable<TCoordinate> inputPts, Positions side)
        {
            TCoordinate start = Slice.GetFirst(inputPts);
            TCoordinate nextToLast = Slice.GetLastPair(inputPts).Value.First;

            initSideSegments(nextToLast, start, side);

            Int32 pointIndex = 0;

            foreach (TCoordinate coordinate in Slice.StartAt(inputPts, 1))
            {
                addNextSegment(coordinate, pointIndex == 0);
                pointIndex++;
            }

            closePoints();
        }

        private void addCoordinate(TCoordinate pt)
        {
            TCoordinate bufPt = _precisionModel.MakePrecise(pt);
            
            // don't add duplicate points
            TCoordinate lastPt = default(TCoordinate);

            if (Slice.IsEmpty(_coordinates))
            {
                lastPt = Slice.GetLast(_coordinates);
            }

            if (!Coordinates<TCoordinate>.IsEmpty(lastPt) && bufPt.Equals(lastPt))
            {
                return;
            }

            _coordinates.Add(bufPt);
        }

        private void closePoints()
        {
            if (!Slice.CountGreaterThan(_coordinates, 1))
            {
                return;
            }

            TCoordinate startPt = Slice.GetFirst(_coordinates);
            TCoordinate lastPt = Slice.GetLast(_coordinates);

            // JTSFIX: last2Pt isn't used...
            //TCoordinate last2Pt = default(TCoordinate);

            //if (_coordinates.Count >= 2)
            //{
            //    last2Pt = _coordinates[_coordinates.Count - 2];
            //}

            if (startPt.Equals(lastPt))
            {
                return;
            }

            _coordinates.Add(startPt);
        }

        private void initSideSegments(TCoordinate s1, TCoordinate s2, Positions side)
        {
            _s1 = s1;
            _s2 = s2;
            _side = side;
            _seg1 = new LineSegment<TCoordinate>(s1, s2);
            _offset1 = computeOffsetSegment(_seg1, side, _distance);
        }

        private void addNextSegment(TCoordinate p, Boolean addStartPoint)
        {
            // s0-s1-s2 are the coordinates of the previous segment and the current one
            _s0 = _s1;
            _s1 = _s2;
            _s2 = p;

            _seg0 = new LineSegment<TCoordinate>(_s0, _s1);
            _offset0 = computeOffsetSegment(_seg0, _side, _distance);

            _seg1 = new LineSegment<TCoordinate>(_s1, _s2);
            _offset1 = computeOffsetSegment(_seg1, _side, _distance);

            // do nothing if points are equal
            if (_s1.Equals(_s2))
            {
                return;
            }

            Orientation orientation = CGAlgorithms<TCoordinate>.ComputeOrientation(_s0, _s1, _s2);
            
            Boolean outsideTurn =
                (orientation == Orientation.Clockwise && _side == Positions.Left)
                || (orientation == Orientation.CounterClockwise && _side == Positions.Right);

            if (orientation == 0) // lines are collinear
            {
                Intersection<TCoordinate> intersection = _li.ComputeIntersection(_s0, _s1, _s1, _s2);
                LineIntersectionType intersectionType = intersection.IntersectionType;

                /*
                * if numInt is < 2, the lines are parallel and in the same direction.
                * In this case the point can be ignored, since the offset lines will also be
                * parallel.
                */
                if ((Int32)intersectionType >= 2)
                {
                    /*
                    * segments are collinear but reversing.  Have to add an "end-cap" fillet
                    * all the way around to other direction
                    * This case should ONLY happen for LineStrings, so the orientation is always CW.
                    * (Polygons can never have two consecutive segments which are parallel but reversed,
                    * because that would be a self intersection.
                    */
                    addFillet(_s1, _offset0.P1, _offset1.P0, Orientation.Clockwise, _distance);
                }
            }
            else if (outsideTurn)
            {
                // add a fillet to connect the endpoints of the offset segments
                if (addStartPoint)
                {
                    addCoordinate(_offset0.P1);
                }

                addFillet(_s1, _offset0.P1, _offset1.P0, orientation, _distance);
                addCoordinate(_offset1.P0);
            }
            else // inside turn
            {
                /*
                 * add intersection point of offset segments (if any)
                 */
                Intersection<TCoordinate> intersection = _li.ComputeIntersection(_offset0.P0, _offset0.P1, _offset1.P0, _offset1.P1);

                if (intersection.HasIntersection)
                {
                    addCoordinate(intersection.GetIntersectionPoint(0));
                }
                else
                {
                    /*
                    * If no intersection, it means the angle is so small and/or the offset so large
                    * that the offsets segments don't intersect.
                    * In this case we must add a offset joining curve to make sure the buffer line
                    * is continuous and tracks the buffer correctly around the corner.
                    * Note that the joining curve won't appear in the final buffer.
                    *
                    * The intersection test above is vulnerable to robustness errors;
                    * i.e. it may be that the offsets should intersect very close to their
                    * endpoints, but don't due to rounding.  To handle this situation
                    * appropriately, we use the following test:
                    * If the offset points are very close, don't add a joining curve
                    * but simply use one of the offset points
                    */
                    if (_offset0.P1.Distance(_offset1.P0) < _distance / 1000.0)
                    {
                        addCoordinate(_offset0.P1);
                    }
                    else
                    {
                        // add endpoint of this segment offset
                        addCoordinate(_offset0.P1);
                        // <FIX> MD - add in center point of corner, to make sure offset closer lines have correct topology
                        addCoordinate(_s1);
                        addCoordinate(_offset1.P0);
                    }
                }
            }
        }

        /// <summary>
        /// Add last offset point.
        /// </summary>
        private void addLastSegment()
        {
            addCoordinate(_offset1.P1);
        }

        /// <summary>
        /// Compute an offset segment for an input segment on a given side and at a given distance.
        /// The offset points are computed in full Double precision, for accuracy.
        /// </summary>
        /// <param name="seg">The segment to offset.</param>
        /// <param name="side">The side of the segment the offset lies on.</param>
        /// <param name="distance">The offset distance.</param>
        /// <returns>The points computed for the offset segment.</returns>
        private LineSegment<TCoordinate> computeOffsetSegment(LineSegment<TCoordinate> seg, Positions side, Double distance)
        {
            Int32 sideSign = side == Positions.Left ? 1 : -1;
            Double dx = seg.P1[Ordinates.X] - seg.P0[Ordinates.X];
            Double dy = seg.P1[Ordinates.Y] - seg.P0[Ordinates.Y];
            Double len = Math.Sqrt(dx * dx + dy * dy);

            // u is the vector that is the length of the offset, 
            // in the direction of the segment
            Double ux = sideSign * distance * dx / len;
            Double uy = sideSign * distance * dy / len;

            TCoordinate p0 = _coordinateFactory.Create(seg.P0[Ordinates.X] - uy, seg.P0[Ordinates.Y] + ux);
            TCoordinate p1 = _coordinateFactory.Create(seg.P1[Ordinates.X] - uy, seg.P1[Ordinates.Y] + ux);

            return new LineSegment<TCoordinate>(p0, p1);
        }

        /// <summary>
        /// Add an end cap around point p1, terminating a line segment coming from p0.
        /// </summary>
        private void addLineEndCap(TCoordinate p0, TCoordinate p1)
        {
            LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(p0, p1);

            LineSegment<TCoordinate> offsetL = 
                computeOffsetSegment(seg, Positions.Left, _distance);

            LineSegment<TCoordinate> offsetR = 
                computeOffsetSegment(seg, Positions.Right, _distance);

            Double dx = p1[Ordinates.X] - p0[Ordinates.X];
            Double dy = p1[Ordinates.Y] - p0[Ordinates.Y];
            Double angle = Math.Atan2(dy, dx);

            switch (_endCapStyle)
            {
                case BufferStyle.CapRound:
                    // add offset seg points with a fillet between them
                    addCoordinate(offsetL.P1);
                    addFillet(p1, angle + Math.PI / 2, angle - Math.PI / 2, Orientation.Clockwise, _distance);
                    addCoordinate(offsetR.P1);
                    break;

                case BufferStyle.CapButt:
                    // only offset segment points are added
                    addCoordinate(offsetL.P1);
                    addCoordinate(offsetR.P1);
                    break;

                case BufferStyle.CapSquare:
                    // add a square defined by extensions of the offset segment endpoints
                    Double sideOffsetX = Math.Abs(_distance) * Math.Cos(angle);
                    Double sideOffsetY = Math.Abs(_distance) * Math.Sin(angle);

                    TCoordinate squareCapLOffset = _coordinateFactory.Create(
                        offsetL.P1[Ordinates.X] + sideOffsetX,
                        offsetL.P1[Ordinates.Y] + sideOffsetY);

                    TCoordinate squareCapROffset = _coordinateFactory.Create(
                        offsetR.P1[Ordinates.X] + sideOffsetX,
                        offsetR.P1[Ordinates.Y] + sideOffsetY);

                    addCoordinate(squareCapLOffset);
                    addCoordinate(squareCapROffset);
                    break;

                default:
                    break;
            }
        }

        /// <param name="p">Base point of curve.</param>
        /// <param name="p0">Start point of fillet curve.</param>
        /// <param name="p1">Endpoint of fillet curve.</param>
        private void addFillet(TCoordinate p, TCoordinate p0, TCoordinate p1, Orientation direction, Double distance)
        {
            Double dx0 = p0[Ordinates.X] - p[Ordinates.X];
            Double dy0 = p0[Ordinates.Y] - p[Ordinates.Y];
            Double startAngle = Math.Atan2(dy0, dx0);
            Double dx1 = p1[Ordinates.X] - p[Ordinates.X];
            Double dy1 = p1[Ordinates.Y] - p[Ordinates.Y];
            Double endAngle = Math.Atan2(dy1, dx1);

            if (direction == Orientation.Clockwise)
            {
                if (startAngle <= endAngle)
                {
                    startAngle += 2.0 * Math.PI;
                }
            }
            else // direction == CounterClockwise
            {
                if (startAngle >= endAngle)
                {
                    startAngle -= 2.0 * Math.PI;
                }
            }

            addCoordinate(p0);
            addFillet(p, startAngle, endAngle, direction, distance);
            addCoordinate(p1);
        }

        /// <summary>
        /// Adds points for a fillet.  The start and end point for the fillet are not added -
        /// the caller must add them if required.
        /// </summary>
        /// <param name="direction">Is -1 for a CW angle, 1 for a CCW angle.</param>
        private void addFillet(TCoordinate p, Double startAngle, Double endAngle, Orientation direction, Double distance)
        {
            Int32 directionFactor = direction == Orientation.Clockwise ? -1 : 1;

            Double totalAngle = Math.Abs(startAngle - endAngle);
            Int32 segmentCount = (Int32)(totalAngle / _filletAngleQuantum + 0.5);

            if (segmentCount < 1)
            {
                return; // no segments because angle is less than increment - nothing to do!
            }

            Double initAngle, currAngleIncrement;

            // choose angle increment so that each segment has equal length
            initAngle = 0.0;
            currAngleIncrement = totalAngle / segmentCount;

            Double currAngle = initAngle;

            Double x = p[Ordinates.X];
            Double y = p[Ordinates.Y];

            while (currAngle < totalAngle)
            {
                Double angle = startAngle + directionFactor * currAngle;
                x = x + distance * Math.Cos(angle);
                y = y + distance * Math.Sin(angle);
                addCoordinate(_coordinateFactory.Create(x, y));
                currAngle += currAngleIncrement;
            }
        }

        /// <summary>
        /// Adds a CW circle around a point.
        /// </summary>
        private void addCircle(TCoordinate p, Double distance)
        {
            // add start point
            TCoordinate pt = _coordinateFactory.Create(p[Ordinates.X] + distance, p[Ordinates.Y]);
            addCoordinate(pt);
            addFillet(p, 0.0, 2.0 * Math.PI, Orientation.Clockwise, distance);
        }

        /// <summary>
        /// Adds a CW square around a point
        /// </summary>
        private void addSquare(ICoordinate p, Double distance)
        {
            Double x = p[Ordinates.X];
            Double y = p[Ordinates.Y];

            // add four corners of square
            addCoordinate(_coordinateFactory.Create(x + distance, y + distance));
            addCoordinate(_coordinateFactory.Create(x + distance, y - distance));
            addCoordinate(_coordinateFactory.Create(x - distance, y - distance));
            addCoordinate(_coordinateFactory.Create(x - distance, y + distance));

            // add end point
            addCoordinate(_coordinateFactory.Create(x + distance, y + distance));
        }
    }
}