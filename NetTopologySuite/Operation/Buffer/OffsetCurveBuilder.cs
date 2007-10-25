using System;
using System.Collections;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes the raw offset curve for a
    /// single <c>Geometry</c> component (ring, line or point).
    /// A raw offset curve line is not noded -
    /// it may contain self-intersections (and usually will).
    /// The final buffer polygon is computed by forming a topological graph
    /// of all the noded raw curves and tracing outside contours.
    /// The points in the raw curve are rounded to the required precision model.
    /// </summary>
    public class OffsetCurveBuilder 
    {        
        /// <summary>
        /// The default number of facets into which to divide a fillet of 90 degrees.
        /// A value of 8 gives less than 2% max error in the buffer distance.
        /// For a max error smaller of 1%, use QS = 12
        /// </summary>
        public const int DefaultQuadrantSegments = 8;

        private static readonly ICoordinate[] arrayTypeCoordinate = new ICoordinate[0];
        private LineIntersector li;

        /*
        * The angle quantum with which to approximate a fillet curve
        * (based on the input # of quadrant segments)
        */
        private double filletAngleQuantum;

        /*
        * the max error of approximation between a quad segment and the true fillet curve
        */
        private double maxCurveSegmentError = 0.0;

        private ArrayList ptList;
        private double distance = 0.0;
        private IPrecisionModel precisionModel;
        private BufferStyle endCapStyle = BufferStyle.CapRound;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="precisionModel"></param>
        public OffsetCurveBuilder(IPrecisionModel precisionModel) : this(precisionModel, DefaultQuadrantSegments) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="precisionModel"></param>
        /// <param name="quadrantSegments"></param>
        public OffsetCurveBuilder(IPrecisionModel precisionModel, int quadrantSegments)
        {
            this.precisionModel = precisionModel;
            // compute intersections in full precision, to provide accuracy
            // the points are rounded as they are inserted into the curve line
            li = new RobustLineIntersector();
            int limitedQuadSegs = quadrantSegments < 1 ? 1 : quadrantSegments;
            filletAngleQuantum = Math.PI / 2.0 / limitedQuadSegs;
        }

        /// <summary>
        /// 
        /// </summary>
        public BufferStyle EndCapStyle
        {
            get
            {
                return endCapStyle;
            }
            set
            {
                endCapStyle = value;
            }
        }

        /// <summary>
        /// This method handles single points as well as lines.
        /// Lines are assumed to not be closed (the function will not
        /// fail for closed lines, but will generate superfluous line caps).
        /// </summary>
        /// <param name="inputPts"></param>
        /// <param name="distance"></param>
        /// <returns> A List of Coordinate[].</returns>
        public IList GetLineCurve(ICoordinate[] inputPts, double distance)
        {
            IList lineList = new ArrayList();
            // a zero or negative width buffer of a line/point is empty
            if (distance <= 0.0) 
                return lineList;
            Init(distance);
            if (inputPts.Length <= 1)
            {
                switch (endCapStyle) 
                {
                    case BufferStyle.CapRound:
                        AddCircle(inputPts[0], distance);
                        break;
                    case BufferStyle.CapSquare:
                        AddSquare(inputPts[0], distance);
                        break;
	                default:
                        // default is for buffer to be empty (e.g. for a butt line cap);
                        break;                        
                }
            }
            else ComputeLineBufferCurve(inputPts);
            ICoordinate[] lineCoord = Coordinates;           
            lineList.Add(lineCoord);
            return lineList;
        }

        /// <summary>
        /// This method handles the degenerate cases of single points and lines,
        /// as well as rings.
        /// </summary>
        /// <returns>A List of coordinates.</returns>
        public IList GetRingCurve(ICoordinate[] inputPts, Positions side, double distance)
        {
            IList lineList = new ArrayList();
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
            lineList.Add(Coordinates);
            return lineList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private static ICoordinate[] CopyCoordinates(ICoordinate[] pts)
        {
            ICoordinate[] copy = new ICoordinate[pts.Length];
            for (int i = 0; i < copy.Length; i++) 
                copy[i] = new Coordinate(pts[i]);            
            return copy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        private void Init(double distance)
        {
            this.distance = distance;
            maxCurveSegmentError = distance * (1 - Math.Cos(filletAngleQuantum / 2.0));
            ptList = new ArrayList();
        }

        /// <summary>
        /// 
        /// </summary>
        private ICoordinate[] Coordinates
        {
            get
            {
                // check that points are a ring - add the startpoint again if they are not
                if (ptList.Count > 1)
                {
                    ICoordinate start = (ICoordinate) ptList[0];
                    ICoordinate end = (ICoordinate) ptList[1];
                    if (!start.Equals(end)) 
                        AddPt(start);
                }
                ICoordinate[] coord = (ICoordinate[]) ptList.ToArray(typeof(ICoordinate));
                return coord;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPts"></param>
        private void ComputeLineBufferCurve(ICoordinate[] inputPts)
        {
            int n = inputPts.Length - 1;

            // compute points for left side of line
            InitSideSegments(inputPts[0], inputPts[1], Positions.Left);
            for (int i = 2; i <= n; i++) 
                AddNextSegment(inputPts[i], true);            
            AddLastSegment();
            // add line cap for end of line
            AddLineEndCap(inputPts[n - 1], inputPts[n]);

            // compute points for right side of line
            InitSideSegments(inputPts[n], inputPts[n - 1], Positions.Left);
            for (int i = n - 2; i >= 0; i--) 
                AddNextSegment(inputPts[i], true);            
            AddLastSegment();

            // add line cap for start of line
            AddLineEndCap(inputPts[1], inputPts[0]);
            ClosePts();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPts"></param>
        /// <param name="side"></param>
        private void ComputeRingBufferCurve(ICoordinate[] inputPts, Positions side)
        {
            int n = inputPts.Length - 1;
            InitSideSegments(inputPts[n - 1], inputPts[0], side);
            for (int i = 1; i <= n; i++) 
            {
                bool addStartPoint = i != 1;
                AddNextSegment(inputPts[i], addStartPoint);
            }
            ClosePts();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        private void AddPt(ICoordinate pt)
        {
            ICoordinate bufPt = new Coordinate(pt);
            precisionModel.MakePrecise( bufPt);
            // don't add duplicate points
            ICoordinate lastPt = null;
            if (ptList.Count >= 1)
                lastPt = (ICoordinate) ptList[ptList.Count - 1];
            if (lastPt != null && bufPt.Equals(lastPt)) return;            
            ptList.Add(bufPt);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClosePts()
        {
            if (ptList.Count < 1) return;
            ICoordinate startPt = new Coordinate((ICoordinate) ptList[0]);
            ICoordinate lastPt  = (ICoordinate) ptList[ptList.Count - 1];
            ICoordinate last2Pt = null;
            if (ptList.Count >= 2)
                last2Pt = (ICoordinate) ptList[ptList.Count - 2];
            if (startPt.Equals(lastPt)) 
                return;
            ptList.Add(startPt);
        }

        private ICoordinate s0, s1, s2;
        private LineSegment seg0 = new LineSegment();
        private LineSegment seg1 = new LineSegment();
        private LineSegment offset0 = new LineSegment();
        private LineSegment offset1 = new LineSegment();
        private Positions side = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="side"></param>
        private void InitSideSegments(ICoordinate s1, ICoordinate s2, Positions side)
        {
            this.s1 = s1;
            this.s2 = s2;
            this.side = side;
            seg1.SetCoordinates(s1, s2);
            ComputeOffsetSegment(seg1, side, distance, offset1);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="addStartPoint"></param>
        private void AddNextSegment(ICoordinate p, bool addStartPoint)
        {
            // s0-s1-s2 are the coordinates of the previous segment and the current one
            s0 = s1;
            s1 = s2;
            s2 = p;
            seg0.SetCoordinates(s0, s1);
            ComputeOffsetSegment(seg0, side, distance, offset0);
            seg1.SetCoordinates(s1, s2);
            ComputeOffsetSegment(seg1, side, distance, offset1);

            // do nothing if points are equal
            if (s1.Equals(s2)) return;

            int orientation = CGAlgorithms.ComputeOrientation(s0, s1, s2);
            bool outsideTurn =
                (orientation == CGAlgorithms.Clockwise        && side == Positions.Left)
            ||  (orientation == CGAlgorithms.CounterClockwise && side == Positions.Right);

            if (orientation == 0) // lines are collinear
            { 
                li.ComputeIntersection(s0, s1, s1, s2);
                int numInt = li.IntersectionNum;
                /*
                * if numInt is < 2, the lines are parallel and in the same direction.
                * In this case the point can be ignored, since the offset lines will also be
                * parallel.
                */
                if (numInt >= 2)                 
                    /*
                    * segments are collinear but reversing.  Have to add an "end-cap" fillet
                    * all the way around to other direction
                    * This case should ONLY happen for LineStrings, so the orientation is always CW.
                    * (Polygons can never have two consecutive segments which are parallel but reversed,
                    * because that would be a self intersection.
                    */
                    AddFillet(s1, offset0.P1, offset1.P0, CGAlgorithms.Clockwise, distance);                
            }
            else if (outsideTurn)
            {
                // add a fillet to connect the endpoints of the offset segments
                if (addStartPoint) 
                    AddPt(offset0.P1);
                AddFillet(s1, offset0.P1, offset1.P0, orientation, distance);
                AddPt(offset1.P0);
            }
            else // inside turn
            { 
                /*
                 * add intersection point of offset segments (if any)
                 */
                li.ComputeIntersection(offset0.P0, offset0.P1, offset1.P0, offset1.P1);
                if (li.HasIntersection) 
                    AddPt(li.GetIntersection(0));                
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
                    if (offset0.P1.Distance(offset1.P0) < distance / 1000.0) 
                        AddPt(offset0.P1);                    
                    else 
                    {
                        // add endpoint of this segment offset
                        AddPt(offset0.P1);
                        // <FIX> MD - add in centre point of corner, to make sure offset closer lines have correct topology
                        AddPt(s1);
                        AddPt(offset1.P0);
                    }
                }
            }
        }

        /// <summary>
        /// Add last offset point.
        /// </summary>
        private void AddLastSegment()
        {
            AddPt(offset1.P1);
        }

        /// <summary>
        /// Compute an offset segment for an input segment on a given side and at a given distance.
        /// The offset points are computed in full double precision, for accuracy.
        /// </summary>
        /// <param name="seg">The segment to offset.</param>
        /// <param name="side">The side of the segment the offset lies on.</param>
        /// <param name="distance">The offset distance.</param>
        /// <param name="offset">The points computed for the offset segment.</param>
        private void ComputeOffsetSegment(LineSegment seg, Positions side, double distance, LineSegment offset)
        {
            int sideSign = side == Positions.Left ? 1 : -1;
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
        /// Add an end cap around point p1, terminating a line segment coming from p0.
        /// </summary>
        private void AddLineEndCap(ICoordinate p0, ICoordinate p1)
        {
            LineSegment seg = new LineSegment(p0, p1);

            LineSegment offsetL = new LineSegment();
            ComputeOffsetSegment(seg, Positions.Left, distance, offsetL);
            LineSegment offsetR = new LineSegment();
            ComputeOffsetSegment(seg, Positions.Right, distance, offsetR);

            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double angle = Math.Atan2(dy, dx);

            switch (endCapStyle) 
            {
                case BufferStyle.CapRound:
                    // add offset seg points with a fillet between them
                    AddPt(offsetL.P1);
                    AddFillet(p1, angle + Math.PI / 2, angle - Math.PI / 2, CGAlgorithms.Clockwise, distance);
                    AddPt(offsetR.P1);
                    break;

                case BufferStyle.CapButt:
                    // only offset segment points are added
                    AddPt(offsetL.P1);
                    AddPt(offsetR.P1);
                    break;
                
                case BufferStyle.CapSquare:
                    // add a square defined by extensions of the offset segment endpoints
                    ICoordinate squareCapSideOffset = new Coordinate();
                    squareCapSideOffset.X = Math.Abs(distance) * Math.Cos(angle);
                    squareCapSideOffset.Y = Math.Abs(distance) * Math.Sin(angle);

                    ICoordinate squareCapLOffset = new Coordinate(
                        offsetL.P1.X + squareCapSideOffset.X,
                        offsetL.P1.Y + squareCapSideOffset.Y);
                    ICoordinate squareCapROffset = new Coordinate(
                        offsetR.P1.X + squareCapSideOffset.X,
                        offsetR.P1.Y + squareCapSideOffset.Y);
                    AddPt(squareCapLOffset);
                    AddPt(squareCapROffset);
                    break;

	            default:
		            break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p">Base point of curve.</param>
        /// <param name="p0">Start point of fillet curve.</param>
        /// <param name="p1">Endpoint of fillet curve.</param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        private void AddFillet(ICoordinate p, ICoordinate p0, ICoordinate p1, int direction, double distance)
        {
            double dx0 = p0.X - p.X;
            double dy0 = p0.Y - p.Y;
            double startAngle = Math.Atan2(dy0, dx0);
            double dx1 = p1.X - p.X;
            double dy1 = p1.Y - p.Y;
            double endAngle = Math.Atan2(dy1, dx1);

            if (direction == CGAlgorithms.Clockwise)    
            {
                if (startAngle <= endAngle)
                    startAngle += 2.0 * Math.PI;
            }
            else // direction == CounterClockwise
            {                
                if (startAngle >= endAngle)
                    startAngle -= 2.0 * Math.PI;
            }                        
            
            AddPt(p0);
            AddFillet(p, startAngle, endAngle, direction, distance);
            AddPt(p1);
        }

        /// <summary>
        /// Adds points for a fillet.  The start and end point for the fillet are not added -
        /// the caller must add them if required.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        /// <param name="direction">Is -1 for a CW angle, 1 for a CCW angle.</param>
        /// <param name="distance"></param>
        private void AddFillet(ICoordinate p, double startAngle, double endAngle, int direction, double distance)
        {
            int directionFactor = direction == CGAlgorithms.Clockwise ? -1 : 1;

            double totalAngle = Math.Abs(startAngle - endAngle);
            int nSegs = (int) (totalAngle / filletAngleQuantum + 0.5);

            if (nSegs < 1) 
                return;    // no segments because angle is less than increment - nothing to do!

            double initAngle, currAngleInc;

            // choose angle increment so that each segment has equal length
            initAngle = 0.0;
            currAngleInc = totalAngle / nSegs;

            double currAngle = initAngle;
            ICoordinate pt = new Coordinate();            
            while (currAngle < totalAngle) 
            {
                double angle = startAngle + directionFactor * currAngle;
                pt.X = p.X + distance * Math.Cos(angle);
                pt.Y = p.Y + distance * Math.Sin(angle);                             
                AddPt(pt);
                currAngle += currAngleInc;
            }            
        }

        /// <summary>
        /// Adds a CW circle around a point.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distance"></param>
        private void AddCircle(ICoordinate p, double distance)
        {
            // add start point
            ICoordinate pt = new Coordinate(p.X + distance, p.Y);
            AddPt(pt);
            AddFillet(p, 0.0, 2.0 * Math.PI, -1, distance);
        }

        /// <summary>
        /// Adds a CW square around a point
        /// </summary>
        /// <param name="p"></param>
        /// <param name="distance"></param>
        private void AddSquare(ICoordinate p, double distance)
        {
            // add start point
            AddPt(new Coordinate(p.X + distance, p.Y + distance));
            AddPt(new Coordinate(p.X + distance, p.Y - distance));
            AddPt(new Coordinate(p.X - distance, p.Y - distance));
            AddPt(new Coordinate(p.X - distance, p.Y + distance));
            AddPt(new Coordinate(p.X + distance, p.Y + distance));
        }
    }
}
