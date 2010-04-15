using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// Computes the raw offset curve for a
    /// single <see cref="IGeometry{TCoordinate}"/> component (ring, line or point).
    /// A raw offset curve line is not noded -
    /// it may contain self-intersections (and usually will).
    /// The final buffer polygon is computed by forming a topological graph
    /// of all the noded raw curves and tracing outside contours.
    /// The points in the raw curve are rounded to the required precision model.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class OffsetCurveBuilder_110<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {

        ///<summary>
        /// The angle quantum with which to approximate a fillet curve
        /// (based on the input # of quadrant segments)
        ///</summary>
        private Double _filletAngleQuantum;

        ///<summary>
        /// the max error of approximation (distance) between a quad segment and the true fillet curve
        /// </summary>
        private Double _maxCurveSegmentError = 0.0;

        ///<summary>
        /// Factor which controls how close curve vertices can be to be snapped
        /// </summary>
        private const Double CurveVertexSnapDistanceFactor = 1.0E-6;

        ///<summary>
        /// Factor which controls how close curve vertices on inside turns can be to be snapped 
        /// </summary>
        private const Double InsideTurnVertexSnapDistanceFactor = 1.0E-3;

        ///<summary>
        /// Factor which determines how short closing segs can be for round buffers
        ///</summary>
        private const Int32 MaxClosingSegFraction = 80;

        private Double _distance = 0.0;
        private IPrecisionModel<TCoordinate> _precisionModel;

        private BufferParameters _bufParams;

        ///<summary>
        /// The Closing Segment Factor controls how long
        /// "closing segments" are.  Closing segments are added
        /// at the middle of inside corners to ensure a smoother
        /// boundary for the buffer offset curve. 
        /// In some cases (particularly for round joins with default-or-better
        /// quantization) the closing segments can be made quite short.
        /// This substantially improves performance (due to fewer intersections being created).
        /// 
        /// A closingSegFactor of 0 results in lines to the corner vertex
        /// A closingSegFactor of 1 results in lines halfway to the corner vertex
        /// A closingSegFactor of 80 results in lines 1/81 of the way to the corner vertex
        /// (this option is reasonable for the very common default situation of round joins
        /// and quadrantSegs >= 8)
        /// </summary>
        private Int32 _closingSegFactor = 1;
        private OffsetCurveVertexList_110<TCoordinate> _vertexList;
        private readonly LineIntersector<TCoordinate> _li;
        private readonly IGeometryFactory<TCoordinate> _geomFact;
        private readonly ICoordinateFactory<TCoordinate> _coordFact;
        private readonly ICoordinateSequenceFactory<TCoordinate> _sequenceFactory;

        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geomFact">geometry factory to use</param>
        ///<param name="precisionModel">precision model</param>
        ///<param name="bufParams">paramters to compute the buffer</param>
        public OffsetCurveBuilder_110(
                IGeometryFactory<TCoordinate> geomFact,
                IPrecisionModel<TCoordinate> precisionModel,
                BufferParameters bufParams
                )
        {
            _geomFact = geomFact;
            _precisionModel = precisionModel;
            _bufParams = bufParams;
            _coordFact = geomFact.CoordinateFactory;
            _sequenceFactory = geomFact.CoordinateSequenceFactory;

            // compute intersections in full precision, to provide accuracy
            // the points are rounded as they are inserted into the curve line
            _li = new RobustLineIntersector<TCoordinate>(geomFact);
            _filletAngleQuantum = Math.PI / 2.0 / _bufParams.QuadrantSegments;

            /**
             * Non-round joins cause issues with short closing segments,
             * so don't use them.  In any case, non-round joins 
             * only really make sense for relatively small buffer distances.
             */
            if (_bufParams.QuadrantSegments >= 8
                && _bufParams.JoinStyle == BufferParameters.BufferJoinStyle.JoinRound)
                _closingSegFactor = MaxClosingSegFraction;
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
                yield break;

            Init(distance);
            if (!Slice.CountGreaterThan(inputPts, 1))
            {
                TCoordinate start = Slice.GetFirst(inputPts);
                switch (_bufParams.EndCapStyle)
                {
                    case BufferParameters.BufferEndCapStyle.CapRound:
                        AddCircle(start, distance);
                        break;
                    case BufferParameters.BufferEndCapStyle.CapSquare:
                        AddSquare(start, distance);
                        break;
                    // default is for buffer to be empty (e.g. for a butt line cap);
                }
            }
            else
                ComputeLineBufferCurve(inputPts);

            //System.out.println(vertexList);

            yield return _sequenceFactory.Create(_vertexList.GetCoordinates());
        }

        /**
         * This method handles the degenerate cases of single points and lines,
         * as well as rings.
         *
         * @return a List of Coordinate[]
         */
        public IEnumerable<ICoordinateSequence<TCoordinate>> GetRingCurve(IEnumerable<TCoordinate> inputPts, Positions side, double distance)
        {
            Init(distance);
            if (!Slice.CountGreaterThan(inputPts, 1))
                foreach (ICoordinateSequence<TCoordinate> coordinates in GetLineCurve(inputPts, distance))
                    yield return coordinates;

            // optimize creating ring for for zero distance
            if (distance == 0.0)
                yield return _sequenceFactory.Create(inputPts);
            else
            {
                ComputeRingBufferCurve(inputPts, side);
                yield return _sequenceFactory.Create(_vertexList.GetCoordinates());
            }
        }

        private static TCoordinate[] CopyCoordinates(TCoordinate[] pts)
        {
            TCoordinate[] copy = new TCoordinate[pts.Length];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = pts[i].Clone();
            }
            return copy;
        }

        private void Init(Double distance)
        {
            _distance = distance;
            _maxCurveSegmentError = distance * (1 - Math.Cos(_filletAngleQuantum / 2.0));
            _vertexList = new OffsetCurveVertexList_110<TCoordinate>(_geomFact);
            _vertexList.PrecisionModel = _precisionModel;
            /**
             * Choose the min vertex separation as a small fraction of the offset distance.
             */
            _vertexList.MinimumVertexDistance = distance*CurveVertexSnapDistanceFactor;
            if ( _vertexList.MinimumVertexDistance ==  0d)
                _vertexList.MinimumVertexDistance = 1E-10;
        }

        ///<summary>
        /// Use a value which results in a potential distance error which is
        /// significantly less than the error due to 
        /// the quadrant segment discretization.
        /// For QS = 8 a value of 400 is reasonable.
        ///</summary>
        private const double SimplifyFactor = 400.0;

        ///<summary>
        /// Computes the distance tolerance to use during input line simplification.
        ///</summary>
        /// <param name="bufDistance">the buffer distance</param>
        /// <returns>the simplification tolerance</returns>
        private static Double SimplifyTolerance(double bufDistance)
        {
            return bufDistance / SimplifyFactor;
        }

        private void ComputeLineBufferCurve(IEnumerable<TCoordinate> inputPts)
        {
            Double distTol = SimplifyTolerance(_distance);

            //--------- compute points for left side of line
            // Simplify the appropriate side of the line before generating
            IEnumerable<TCoordinate> simp1 = 
                BufferInputLineSimplifier_110<TCoordinate>.Simplify(inputPts, distTol);
            // MD - used for testing only (to eliminate simplification)
            //    Coordinate[] simp1 = inputPts;
            Pair<TCoordinate> initPoints = Slice.GetPair(simp1).Value;

            InitSideSegments(initPoints.First, initPoints.Second, Positions.Left);
            foreach (TCoordinate point in Enumerable.Skip(simp1, 2))
            {
                AddNextSegment(point, true);
            }
            AddLastSegment();
            // add line cap for end of line
            Pair<TCoordinate> lastPoints = Slice.GetLastPair(simp1).Value;
            AddLineEndCap(lastPoints.First, lastPoints.Second);

            //---------- compute points for right side of line
            // Simplify the appropriate side of the line before generating
            IEnumerable<TCoordinate> simp2 = 
                BufferInputLineSimplifier_110<TCoordinate>.Simplify(inputPts, -distTol);
            // MD - used for testing only (to eliminate simplification)
            //    Coordinate[] simp2 = inputPts;
            initPoints = Slice.GetPair(simp2).Value;
            // since we are traversing line in opposite order, offset position is still LEFT
            InitSideSegments(lastPoints.Second, lastPoints.First, Positions.Left);
            foreach (TCoordinate point in Slice.ReverseStartAt(simp2, 2))
            {
                AddNextSegment(point, true);
            }
            AddLastSegment();
            // add line cap for start of line
            lastPoints = Slice.GetPair(simp2).Value;
            AddLineEndCap(lastPoints.Second, lastPoints.First);

            _vertexList.CloseRing();
        }

        private void OldComputeLineBufferCurve(TCoordinate[] inputPts)
        {
            int n = inputPts.Length - 1;

            // compute points for left side of line
            InitSideSegments(inputPts[0], inputPts[1], Positions.Left);
            for (int i = 2; i <= n; i++)
            {
                AddNextSegment(inputPts[i], true);
            }
            AddLastSegment();
            // add line cap for end of line
            AddLineEndCap(inputPts[n - 1], inputPts[n]);

            // compute points for right side of line
            InitSideSegments(inputPts[n], inputPts[n - 1], Positions.Left);
            for (int i = n - 2; i >= 0; i--)
            {
                AddNextSegment(inputPts[i], true);
            }
            AddLastSegment();
            // add line cap for start of line
            AddLineEndCap(inputPts[1], inputPts[0]);

            _vertexList.CloseRing();
        }

        private void ComputeRingBufferCurve(IEnumerable<TCoordinate> inputPts, Positions side)
        {
            // simplify input line to improve performance
            Double distTol = SimplifyTolerance(_distance);
            // ensure that correct side is simplified
            if (side == Positions.Right)
                distTol = -distTol;
            List<TCoordinate> simp = new List<TCoordinate>(
                BufferInputLineSimplifier_110<TCoordinate>.Simplify(inputPts, distTol));
            //    Coordinate[] simp = inputPts;
            int n = simp.Count - 1;
            InitSideSegments(simp[n - 1], simp[0], side);
            for (int i = 1; i <= n; i++)
            {
                Boolean addStartPoint = i != 1;
                AddNextSegment(simp[i], addStartPoint);
            }
            _vertexList.CloseRing();
        }

        private TCoordinate _s0, _s1, _s2;
        private LineSegment<TCoordinate> _seg0 = new LineSegment<TCoordinate>();
        private LineSegment<TCoordinate> _seg1 = new LineSegment<TCoordinate>();
        private LineSegment<TCoordinate> _offset0 = new LineSegment<TCoordinate>();
        private LineSegment<TCoordinate> _offset1 = new LineSegment<TCoordinate>();
        private Positions _side = 0;

        private void InitSideSegments(TCoordinate s1, TCoordinate s2, Positions side)
        {
            _s1 = s1;
            _s2 = s2;
            _side = side;
            _seg1 = new LineSegment<TCoordinate>(s1, s2);
            _offset1 = ComputeOffsetSegment(_seg1, _side, _distance);
        }

        private static double _maxClosingSegLen = 3.0;

        private void AddNextSegment(TCoordinate p, Boolean addStartPoint)
        {
            // s0-s1-s2 are the coordinates of the previous segment and the current one
            _s0 = _s1;
            _s1 = _s2;
            _s2 = p;
            _seg0 = new LineSegment<TCoordinate>(_s0, _s1);
            _offset0 = ComputeOffsetSegment(_seg0, _side, _distance);
            _seg1 = new LineSegment<TCoordinate>(_s1, _s2);
            _offset1 = ComputeOffsetSegment(_seg1, _side, _distance);

            // do nothing if points are equal
            if (_s1.Equals(_s2)) return;

            Orientation orientation = CGAlgorithms<TCoordinate>.ComputeOrientation(_s0, _s1, _s2);
            Boolean outsideTurn =
                  (orientation == Orientation.Clockwise && _side == Positions.Left)
              || (orientation == Orientation.CounterClockwise && _side == Positions.Right);

            if (orientation == Orientation.Collinear)
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

        private void AddCollinear(Boolean addStartPoint)
  {
		Intersection<TCoordinate> ret = _li.ComputeIntersection(_s0, _s1, _s1, _s2);
		/**
		 * if numInt is &lt; 2, the lines are parallel and in the same direction. In
		 * this case the point can be ignored, since the offset lines will also be
		 * parallel.
		 */
		if (ret.IntersectionDegree == LineIntersectionDegrees.Collinear ) {
			/**
			 * segments are collinear but reversing. 
			 * Add an "end-cap" fillet
			 * all the way around to other direction This case should ONLY happen
			 * for LineStrings, so the orientation is always CW. (Polygons can never
			 * have two consecutive segments which are parallel but reversed,
			 * because that would be a self intersection.
			 * 
			 */
			if (_bufParams.JoinStyle == BufferParameters.BufferJoinStyle.JoinBevel 
					|| _bufParams.JoinStyle == BufferParameters.BufferJoinStyle.JoinMitre) {
				if (addStartPoint) _vertexList.Add(_offset0.P1);
				_vertexList.Add(_offset1.P0);
			}
			else {
				AddFillet(_s1, _offset0.P1, _offset1.P0, Orientation.Clockwise, _distance);
			}
		}
  }

        /**
         * Adds the offset points for an outside (convex) turn
         * 
         * @param orientation
         * @param addStartPoint
         */
        private void AddOutsideTurn(Orientation orientation, Boolean addStartPoint)
        {
            /**
             * If offset endpoints are very close together, just snap them together.
             * This avoids problems with computing mitre corners in degenerate cases.
             */
            if (_offset0.P1.Distance(_offset1.P0) < _distance * CurveVertexSnapDistanceFactor)
            {
                _vertexList.Add(_offset0.P1);
                return;
            }


            if (_bufParams.JoinStyle == BufferParameters.BufferJoinStyle.JoinMitre)
            {
                AddMitreJoin(_s1, _offset0, _offset1, _distance);
            }
            else if (_bufParams.JoinStyle == BufferParameters.BufferJoinStyle.JoinBevel)
            {
                AddBevelJoin(_offset0, _offset1);
            }
            else
            {
                // add a circular fillet connecting the endpoints of the offset segments
                if (addStartPoint) _vertexList.Add(_offset0.P1);
                // TESTING - comment out to produce beveled joins
                AddFillet(_s1, _offset0.P1, _offset1.P0, orientation, _distance);
                _vertexList.Add(_offset1.P0);
            }
        }

        ///<summary>
        /// Adds the offset points for an inside (concave) turn
        ///</summary>
        /// <param name="orientation"></param>
        /// <param name="addStartPoint"></param>
        private void AddInsideTurn(Orientation orientation, Boolean addStartPoint)
        {
            /**
             * add intersection point of offset segments (if any)
             */
            Intersection<TCoordinate> ret = _li.ComputeIntersection(_offset0.P0, _offset0.P1, _offset1.P0, _offset1.P1);
            if (ret.HasIntersection)
            {
                _vertexList.Add(ret.GetIntersectionPoint(0));//_li.getIntersection(0));
            }
            else
            {
                /**
                 * If no intersection is detected, it means the angle is so small and/or the offset so
                 * large that the offsets segments don't intersect. In this case we must
                 * add a "closing segment" to make sure the buffer line is continuous
                 * and tracks the buffer correctly around the corner. The curve connects
                 * the endpoints of the segment offsets to points
                 * which lie toward the centre point of the corner.
                 * The joining curve will not appear in the final buffer outline, since it
                 * is completely internal to the buffer polygon.
                 * 
                 * The intersection test above is vulnerable to robustness errors; i.e. it
                 * may be that the offsets should intersect very close to their endpoints,
                 * but aren't reported as such due to rounding. To handle this situation
                 * appropriately, we use the following test: If the offset points are very
                 * close, don't add closing segments but simply use one of the offset
                 * points
                 */
                if (_offset0.P1.Distance(_offset1.P0) < _distance
                    * InsideTurnVertexSnapDistanceFactor)
                {
                    _vertexList.Add(_offset0.P1);
                }
                else
                {
                    // add endpoint of this segment offset
                    _vertexList.Add(_offset0.P1);

                    // add closing segments of required length
                    if (_closingSegFactor > 0)
                    {
                        TCoordinate mid0 = _coordFact.Create(
                            (_closingSegFactor * _offset0.P1[Ordinates.X] + _s1[Ordinates.X]) / (_closingSegFactor + 1),
                            (_closingSegFactor * _offset0.P1[Ordinates.Y] + _s1[Ordinates.Y]) / (_closingSegFactor + 1));
                        _vertexList.Add(mid0);
                        TCoordinate mid1 = _coordFact.Create(
                            (_closingSegFactor * _offset1.P0[Ordinates.X] + _s1[Ordinates.X]) / (_closingSegFactor + 1),
                            (_closingSegFactor * _offset1.P0[Ordinates.Y] + _s1[Ordinates.Y]) / (_closingSegFactor + 1));
                        _vertexList.Add(mid1);
                    }
                    else
                    {
                        /**
                         * This branch is not expected to be used except for testing purposes.
                         * It is equivalent to the JTS 1.9 logic for closing segments
                         * (which results in very poor performance for large buffer distances)
                         */
                        _vertexList.Add(_s1);
                    }

                    //*/  
                    // add start point of next segment offset
                    _vertexList.Add(_offset1.P0);
                }
            }
        }

        /**
         * Add last offset point
         */
        private void AddLastSegment()
        {
            _vertexList.Add(_offset1.P1);
        }

        /**
         * Compute an offset segment for an input segment on a given side and at a given distance.
         * The offset points are computed in full double precision, for accuracy.
         *
         * @param seg the segment to offset
         * @param side the side of the segment ({@link Position}) the offset lies on
         * @param distance the offset distance
         * @param offset the points computed for the offset segment
         */
        private LineSegment<TCoordinate> ComputeOffsetSegment(LineSegment<TCoordinate> seg, Positions side, Double distance )//,  offset)
        {
            Int32 sideSign = side == Positions.Left ? 1 : -1;
            Double dx = seg.P1[Ordinates.X] - seg.P0[Ordinates.X];
            Double dy = seg.P1[Ordinates.Y] - seg.P0[Ordinates.Y];
            Double len = Math.Sqrt(dx * dx + dy * dy);
            // u is the vector that is the length of the offset, in the direction of the segment
            Double ux = sideSign * distance * dx / len;
            Double uy = sideSign * distance * dy / len;

            return new LineSegment<TCoordinate>(
                _coordFact.Create(seg.P0[Ordinates.X] - uy,seg.P0[Ordinates.Y] + ux),
                _coordFact.Create(seg.P1[Ordinates.X] - uy,seg.P1[Ordinates.Y] + ux));
        }

        /**
         * Add an end cap around point p1, terminating a line segment coming from p0
         */
        private void AddLineEndCap(TCoordinate p0, TCoordinate p1)
        {
            LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(p0, p1);

            LineSegment<TCoordinate> offsetL = ComputeOffsetSegment(seg, Positions.Left, _distance);
            LineSegment<TCoordinate> offsetR = ComputeOffsetSegment(seg, Positions.Right, _distance); 

            Double dx = p1[Ordinates.X] - p0[Ordinates.X];
            Double dy = p1[Ordinates.Y] - p0[Ordinates.Y];
            Double angle = Math.Atan2(dy, dx);

            switch (_bufParams.EndCapStyle)
            {
                case  BufferParameters.BufferEndCapStyle.CapRound:
                    // add offset seg points with a fillet between them
                    _vertexList.Add(offsetL.P1);
                    AddFillet(p1, angle + Math.PI / 2, angle - Math.PI / 2, Orientation.Clockwise, _distance);
                    _vertexList.Add(offsetR.P1);
                    break;
                case BufferParameters.BufferEndCapStyle.CapFlat:
                    // only offset segment points are added
                    _vertexList.Add(offsetL.P1);
                    _vertexList.Add(offsetR.P1);
                    break;
                case BufferParameters.BufferEndCapStyle.CapSquare:
                    // add a square defined by extensions of the offset segment endpoints
                    TCoordinate squareCapSideOffset = _coordFact.Create(
                        Math.Abs(_distance) * Math.Cos(angle),
                        Math.Abs(_distance) * Math.Sin(angle));

                    TCoordinate squareCapLOffset = _coordFact.Create(
                        offsetL.P1[Ordinates.X] + squareCapSideOffset[Ordinates.X],
                        offsetL.P1[Ordinates.Y] + squareCapSideOffset[Ordinates.Y]);
                    TCoordinate squareCapROffset = _coordFact.Create(
                        offsetR.P1[Ordinates.X] + squareCapSideOffset[Ordinates.X],
                        offsetR.P1[Ordinates.Y] + squareCapSideOffset[Ordinates.Y]);
                    _vertexList.Add(squareCapLOffset);
                    _vertexList.Add(squareCapROffset);
                    break;

            }
        }
        /**
         * Adds a mitre join connecting the two reflex offset segments.
         * The mitre will be beveled if it exceeds the mitre ratio limit.
         * 
         * @param offset0 the first offset segment
         * @param offset1 the second offset segment
         * @param distance the offset distance
         */
        ///<summary>
        /// Adds a mitre join connecting the two reflex offset segments.
        /// The mitre will be beveled if it exceeds the mitre ratio limit.
        ///</summary>
        /// <param name="p"></param>
        /// <param name="offset0"></param>
        /// <param name="offset1"></param>
        /// <param name="distance"></param>
        private void AddMitreJoin(TCoordinate p,
              LineSegment<TCoordinate> offset0,
              LineSegment<TCoordinate> offset1,
              double distance)
        {
            Boolean isMitreWithinLimit = true;
            TCoordinate intPt = default(TCoordinate);

            /*
             * This computation is unstable if the offset segments are nearly collinear, 
             * but this case should be eliminated earlier by the check if 
             * the offset segment endpoints are almost coincident
             */
            try
            {
                intPt = HCoordinateIntersection(
                            _offset0.P0, _offset0.P1,
                            _offset1.P0, _offset1.P1);

                Double mitreRatio = distance <= 0.0 ? 1.0
                        : intPt.Distance(p) / Math.Abs(distance);

                if (mitreRatio > _bufParams.MitreLimit)
                    isMitreWithinLimit = false;
            }
            catch (NotRepresentableException ex)
            {
                intPt = _coordFact.Create(0, 0);
                isMitreWithinLimit = false;
            }

            if (isMitreWithinLimit)
            {
                _vertexList.Add(intPt);
            }
            else
            {
                AddLimitedMitreJoin(offset0, offset1, distance, _bufParams.MitreLimit);
                //  		addBevelJoin(offset0, offset1);
            }
        }

        // Computes a segment intersection using homogeneous coordinates.
        // Round-off error can cause the raw computation to fail, 
        // (usually due to the segments being approximately parallel).
        // If this happens, a reasonable approximation is computed instead.
        private TCoordinate HCoordinateIntersection(TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2)
        {

            TCoordinate hP1 = _coordFact.Homogenize(p1);
            TCoordinate hP2 = _coordFact.Homogenize(p2);
            TCoordinate hQ1 = _coordFact.Homogenize(q1);
            TCoordinate hQ2 = _coordFact.Homogenize(q2);

            TCoordinate intersectionPoint = IntersectHomogeneous(hP1, hP2, hQ1, hQ2);
            Double x = (Double)intersectionPoint[0];
            Double y = (Double)intersectionPoint[1];
            Double w = (Double)intersectionPoint[2];
            return _coordFact.Create(x / w, y / w);

        }

        private static TCoordinate IntersectHomogeneous(TCoordinate hP1,
                                                TCoordinate hP2,
                                                TCoordinate hQ1,
                                                TCoordinate hQ2)
        {

            // compute cross-products
            TCoordinate p = hP1.Cross(hP2);
            TCoordinate q = hQ1.Cross(hQ2);

            // intersect lines in projective space via homogeneous coordinate cross-product
            TCoordinate intersection = p.Cross(q);
            return intersection;
        }


        /**
         * Adds a limited mitre join connecting the two reflex offset segments.
         * A limited mitre is a mitre which is beveled at the distance
         * determined by the mitre ratio limit.
         * 
         * @param offset0 the first offset segment
         * @param offset1 the second offset segment
         * @param distance the offset distance
         * @param mitreLimit the mitre limit ratio
         */
        private void AddLimitedMitreJoin(
              LineSegment<TCoordinate> offset0,
              LineSegment<TCoordinate> offset1,
              double distance,
              double mitreLimit)
        {
            TCoordinate basePt = _seg0.P1;

            double ang0 = Angle<TCoordinate>.CalculateAngle(basePt, _seg0.P0);
            double ang1 = Angle<TCoordinate>.CalculateAngle(basePt, _seg1.P1);

            // oriented angle between segments
            double angDiff = Angle<TCoordinate>.CalculateAngleBetweenOriented(_seg0.P0, basePt, _seg1.P1);
            // half of the interior angle
            double angDiffHalf = angDiff / 2;

            // angle for bisector of the interior angle between the segments
            double midAng = Angle<TCoordinate>.Normalize(ang0 + angDiffHalf);
            // rotating this by PI gives the bisector of the reflex angle
            double mitreMidAng = Angle<TCoordinate>.Normalize(midAng + Math.PI);

            // the miterLimit determines the distance to the mitre bevel
            double mitreDist = mitreLimit * distance;
            // the bevel delta is the difference between the buffer distance
            // and half of the length of the bevel segment
            double bevelDelta = mitreDist * Math.Abs(Math.Sin(angDiffHalf));
            double bevelHalfLen = distance - bevelDelta;

            // compute the midpoint of the bevel segment
            double bevelMidX = basePt[Ordinates.X] + mitreDist * Math.Cos(mitreMidAng);
            double bevelMidY = basePt[Ordinates.Y] + mitreDist * Math.Sin(mitreMidAng);
            TCoordinate bevelMidPt = _coordFact.Create(bevelMidX, bevelMidY);

            // compute the mitre midline segment from the corner point to the bevel segment midpoint
            LineSegment<TCoordinate> mitreMidLine = new LineSegment<TCoordinate>(basePt, bevelMidPt);

            // finally the bevel segment endpoints are computed as offsets from 
            // the mitre midline
            TCoordinate bevelEndLeft = mitreMidLine.PointAlongOffset(_coordFact, 1.0, bevelHalfLen);
            TCoordinate bevelEndRight = mitreMidLine.PointAlongOffset(_coordFact, 1.0, -bevelHalfLen);

            if (_side == Positions.Left)
            {
                _vertexList.Add(bevelEndLeft);
                _vertexList.Add(bevelEndRight);
            }
            else
            {
                _vertexList.Add(bevelEndRight);
                _vertexList.Add(bevelEndLeft);
            }
        }

        /**
         * Adds a bevel join connecting the two offset segments
         * around a reflex corner.
         * 
         * @param offset0 the first offset segment
         * @param offset1 the second offset segment
         */
        private void AddBevelJoin(
              LineSegment<TCoordinate> offset0,
              LineSegment<TCoordinate> offset1)
        {
            _vertexList.Add(offset0.P1);
            _vertexList.Add(offset1.P0);
        }


        /**
         * Add points for a circular fillet around a reflex corner.
         * Adds the start and end points
         * 
         * @param p base point of curve
         * @param p0 start point of fillet curve
         * @param p1 endpoint of fillet curve
         * @param direction the orientation of the fillet
         * @param radius the radius of the fillet
         */
        private void AddFillet(TCoordinate p, TCoordinate p0, TCoordinate p1, Orientation direction, double radius)
        {
            double dx0 = p0[Ordinates.X] - p[Ordinates.X];
            double dy0 = p0[Ordinates.Y] - p[Ordinates.Y];
            double startAngle = Math.Atan2(dy0, dx0);
            double dx1 = p1[Ordinates.X] - p[Ordinates.X];
            double dy1 = p1[Ordinates.Y] - p[Ordinates.Y];
            double endAngle = Math.Atan2(dy1, dx1);

            if (direction == Orientation.Clockwise)
            {
                if (startAngle <= endAngle) startAngle += 2.0 * Math.PI;
            }
            else
            {    // direction == COUNTERCLOCKWISE
                if (startAngle >= endAngle) startAngle -= 2.0 * Math.PI;
            }
            _vertexList.Add(p0);
            AddFillet(p, startAngle, endAngle, direction, radius);
            _vertexList.Add(p1);
        }

        ///<summary>
        /// Adds points for a circular fillet arc
        /// between two specified angles.
        /// The start and end point for the fillet are not added -
        /// the caller must add them if required.
        ///</summary>
        /// <param name="p">center of arc</param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        /// <param name="direction">is -1 for a CW angle, 1 for a CCW angle</param>
        /// <param name="radius">the radius of the fillet</param>
        private void AddFillet(TCoordinate p, Double startAngle, Double endAngle, Orientation direction, Double radius)
        {
            int directionFactor = direction == Orientation.Clockwise ? -1 : 1;

            Double totalAngle = Math.Abs(startAngle - endAngle);
            int nSegs = (int)(totalAngle / _filletAngleQuantum + 0.5);

            if (nSegs < 1) return;    // no segments because angle is less than increment - nothing to do!


            // choose angle increment so that each segment has equal length
            Double initAngle = 0.0;
            Double currAngleInc = totalAngle / nSegs;

            Double currAngle = initAngle;
            while (currAngle < totalAngle)
            {
                Double angle = startAngle + directionFactor * currAngle;
                TCoordinate pt = _coordFact.Create(
                    p[Ordinates.X] + radius * Math.Cos(angle),
                    p[Ordinates.Y] + radius * Math.Sin(angle));
                _vertexList.Add(pt);
                currAngle += currAngleInc;
            }
        }


        ///<summary>
        /// Adds a CW circle around a point
        ///</summary>
        private void AddCircle(TCoordinate p, double distance)
        {
            // add start point
            TCoordinate pt = _coordFact.Create(p[Ordinates.X] + distance, p[Ordinates.Y]);
            _vertexList.Add(pt);
            AddFillet(p, 0.0, 2.0 * Math.PI, Orientation.Clockwise, distance);
        }

        ///<summary>
        /// Adds a CW square around a point
        ///</summary>
        private void AddSquare(TCoordinate p, double distance)
        {
            // add start point
            _vertexList.Add(_coordFact.Create(p[Ordinates.X] + distance, p[Ordinates.Y] + distance));
            _vertexList.Add(_coordFact.Create(p[Ordinates.X] + distance, p[Ordinates.Y] - distance));
            _vertexList.Add(_coordFact.Create(p[Ordinates.X] - distance, p[Ordinates.Y] - distance));
            _vertexList.Add(_coordFact.Create(p[Ordinates.X] - distance, p[Ordinates.Y] + distance));
            _vertexList.Add(_coordFact.Create(p[Ordinates.X] + distance, p[Ordinates.Y] + distance));
        }
    }
}
