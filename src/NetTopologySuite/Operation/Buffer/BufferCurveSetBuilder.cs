    using System;
    using System.Collections.Generic;
    using NetTopologySuite.Algorithm;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.GeometriesGraph;
    using NetTopologySuite.Noding;
    using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates all the raw offset curves for a buffer of a <c>Geometry</c>.
    /// Raw curves need to be noded together and polygonized to form the final buffer area.
    /// </summary>
    public class BufferCurveSetBuilder
    {
        private readonly Geometry _inputGeom;
        private readonly double _distance;
        private readonly OffsetCurveBuilder _curveBuilder;

        private readonly IList<ISegmentString> _curveList = new List<ISegmentString>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputGeom">The input geometry</param>
        /// <param name="distance">The offset distance</param>
        /// <param name="precisionModel">A precision model</param>
        /// <param name="parameters">The buffer parameters</param>
        public BufferCurveSetBuilder(Geometry inputGeom, double distance,
            PrecisionModel precisionModel, BufferParameters parameters)
        {
            _inputGeom = inputGeom;
            _distance = distance;
            _curveBuilder = new OffsetCurveBuilder(precisionModel, parameters);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the offset curve is generated
        /// using the inverted orientation of input rings.
        /// This allows generating a buffer(0) polygon from the smaller lobes
        /// of self-crossing rings.
        /// </summary>
        public bool InvertOrientation { get; set; }

        /// <summary>
        /// Computes orientation of a ring using a signed-area orientation test.
        /// For invalid (self-crossing) rings this ensures the largest enclosed area
        /// is taken to be the interior of the ring.
        /// This produces a more sensible result when
        /// used for repairing polygonal geometry via buffer-by-zero.
        /// For buffer  use the lower robustness of orientation-by-area
        /// doesn't matter, since narrow or flat rings
        /// produce an acceptable offset curve for either orientation.
        /// </summary>
        /// <param name="coord">The ring coordinates</param>
        /// <returns>true if the ring is CCW</returns>
        private bool IsRingCCW(Coordinate[] coord)
        {
            bool isCCW = Orientation.IsCCWArea(coord);
            //--- invert orientation if required
            if (InvertOrientation) return !isCCW;
            return isCCW;
        }
        /// <summary>
        /// Computes the set of raw offset curves for the buffer.
        /// Each offset curve has an attached {Label} indicating
        /// its left and right location.
        /// </summary>
        /// <returns>A Collection of SegmentStrings representing the raw buffer curves.</returns>
        public IList<ISegmentString> GetCurves()
        {
            Add(_inputGeom);
            return _curveList;
        }

        /// <summary>
        /// Creates a {SegmentString} for a coordinate list which is a raw offset curve,
        /// and adds it to the list of buffer curves.
        /// The SegmentString is tagged with a Label giving the topology of the curve.
        /// The curve may be oriented in either direction.
        /// If the curve is oriented CW, the locations will be:
        /// Left: Location.Exterior.
        /// Right: Location.Interior.
        /// </summary>
        private void AddCurve(Coordinate[] coord, Location leftLoc, Location rightLoc)
        {
            // don't add null or trivial curves!
            if (coord == null || coord.Length < 2)
                return;
            // add the edge for a coordinate list which is a raw offset curve
            var e = new NodedSegmentString(coord, new Label(0, Location.Boundary, leftLoc, rightLoc));
            _curveList.Add(e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        private void Add(Geometry g)
        {
            if (g.IsEmpty) return;
            if (g is Polygon)
                AddPolygon((Polygon)g);
            // LineString also handles LinearRings
            else if (g is LineString)
                AddLineString(g);
            else if (g is Point)
                AddPoint(g);
            else if (g is MultiPoint)
                AddCollection(g);
            else if (g is MultiLineString)
                AddCollection(g);
            else if (g is MultiPolygon)
                AddCollection(g);
            else if (g is GeometryCollection)
                AddCollection(g);
            else throw new NotSupportedException(g.GetType().FullName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        private void AddCollection(Geometry gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                Add(g);
            }
        }

        /// <summary>
        /// Add a Point to the graph.
        /// </summary>
        /// <param name="p"></param>
        private void AddPoint(Geometry p)
        {
            // a zero or negative width buffer of a point is empty
            if (_distance <= 0.0)
                return;
            var coord = p.Coordinates;
            // skip if coordinate is invalid
            if (coord.Length >= 1 && !coord[0].IsValid)
                return;
            var curve = _curveBuilder.GetLineCurve(coord, _distance);
            AddCurve(curve, Location.Exterior, Location.Interior);
        }

        private void AddLineString(Geometry line)
        {
            if (_curveBuilder.IsLineOffsetEmpty(_distance)) return;

            var coord = Clean(line.Coordinates);
            /*
             * Rings (closed lines) are generated with a continuous curve, 
             * with no end arcs. This produces better quality linework, 
             * and avoids noding issues with arcs around almost-parallel end segments.
             * See JTS #523 and #518.
             * 
             * Singled-sided buffers currently treat rings as if they are lines.
             */
            if (CoordinateArrays.IsRing(coord) && !_curveBuilder.BufferParameters.IsSingleSided)
            {
                AddRingBothSides(coord, _distance);
            }
            else
            {
                var curve = _curveBuilder.GetLineCurve(coord, _distance);
                AddCurve(curve, Location.Exterior, Location.Interior);
            }
        }

        /// <summary>
        /// Keeps only valid coordinates, and removes repeated points.
        /// </summary>
        /// <param name="coords">The coordinates to clean</param>
        /// <returns>An array of clean coordinates</returns>
        private static Coordinate[] Clean(Coordinate[] coords)
        {
            return CoordinateArrays.RemoveRepeatedOrInvalidPoints(coords);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        private void AddPolygon(Polygon p)
        {
            double offsetDistance = _distance;
            var offsetSide = Position.Left;
            if (_distance < 0.0)
            {
                offsetDistance = -_distance;
                offsetSide = Position.Right;
            }

            var shell = p.Shell;
            var shellCoord = Clean(shell.Coordinates);
            // optimization - don't bother computing buffer
            // if the polygon would be completely eroded
            if (_distance < 0.0 && IsErodedCompletely(shellCoord, _distance))
                return;
            // don't attempt to buffer a polygon with too few distinct vertices
            if (_distance <= 0.0 && shellCoord.Length < 3)
                return;

            AddRingSide(shellCoord, offsetDistance, offsetSide,
                           Location.Exterior, Location.Interior);

            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                var hole = (LinearRing)p.GetInteriorRingN(i);
                var holeCoord = Clean(hole.Coordinates);

                // optimization - don't bother computing buffer for this hole
                // if the hole would be completely covered
                if (_distance > 0.0 && IsErodedCompletely(holeCoord, -_distance))
                    continue;

                // Holes are topologically labelled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CCW)
                AddRingSide(holeCoord, offsetDistance, offsetSide.Opposite,
                               Location.Interior, Location.Exterior);
            }
        }

        private void AddRingBothSides(Coordinate[] coord, double distance)
        {
            AddRingSide(coord, distance,
                Position.Left,
                Location.Exterior, Location.Interior);
            // Add the opposite side of the ring
            AddRingSide(coord, distance,
                Position.Right,
                Location.Interior, Location.Exterior);
        }


        /// <summary>
        /// Adds an offset curve for a polygon ring.
        /// The side and left and right topological location arguments
        /// assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged and the side flipped.
        /// </summary>
        /// <param name="coord">The coordinates of the ring (must not contain repeated points).</param>
        /// <param name="offsetDistance">The distance at which to create the buffer.</param>
        /// <param name="side">The side of the ring on which to construct the buffer line.</param>
        /// <param name="cwLeftLoc">The location on the L side of the ring (if it is CW).</param>
        /// <param name="cwRightLoc">The location on the R side of the ring (if it is CW).</param>
        private void AddRingSide(Coordinate[] coord, double offsetDistance,
            Position side, Location cwLeftLoc, Location cwRightLoc)
        {
            // don't bother adding ring if it is "flat" and will disappear in the output
            if (offsetDistance == 0.0 && coord.Length < LinearRing.MinimumValidSize)
                return;

            var leftLoc = cwLeftLoc;
            var rightLoc = cwRightLoc;
            bool isCCW = IsRingCCW(coord);
            if (coord.Length >= LinearRing.MinimumValidSize && isCCW)
            {
                leftLoc = cwRightLoc;
                rightLoc = cwLeftLoc;
                side = side.Opposite;
            }
            var curve = _curveBuilder.GetRingCurve(coord, side, offsetDistance);

            /*
             * If the offset curve has inverted completely it will produce
             * an unwanted artifact in the result, so skip it. 
             */
            if (IsRingCurveInverted(coord, offsetDistance, curve))
            {
                return;
            }
            AddCurve(curve, leftLoc, rightLoc);
        }

        private const int MAX_INVERTED_RING_SIZE = 9;
        private const int INVERTED_CURVE_VERTEX_FACTOR = 4;
        private const double NEARNESS_FACTOR = 0.99;

        /// <summary>
        /// Tests whether the offset curve for a ring is fully inverted.
        /// An inverted ("inside-out") curve occurs in some specific situations
        /// involving a buffer distance which should result in a fully-eroded (empty) buffer.
        /// It can happen that the sides of a small, convex polygon
        /// produce offset segments which all cross one another to form
        /// a curve with inverted orientation.<br/>
        /// This happens at buffer distances slightly greater than the distance at
        /// which the buffer should disappear.<br/>
        /// The inverted curve will produce an incorrect non-empty buffer (for a shell)
        /// or an incorrect hole (for a hole).
        /// It must be discarded from the set of offset curves used in the buffer.
        /// Heuristics are used to reduce the number of cases which area checked,
        /// for efficiency and correctness.
        /// <para/>
        /// See <a href="https://github.com/locationtech/jts/issues/472"/>
        /// </summary>
        /// <param name="inputRing">the input ring</param>
        /// <param name="distance">the buffer distance</param>
        /// <param name="curveRing">the generated offset curve</param>
        /// <returns>true if the offset curve is inverted</returns>
        private static bool IsRingCurveInverted(Coordinate[] inputRing, double distance, Coordinate[] curveRing)
        {
            if (distance == 0.0) return false;
            /*
             * Only proper rings can invert.
             */
            if (inputRing.Length <= 3) return false;
            /*
             * Heuristic based on low chance that a ring with many vertices will invert.
             * This low limit ensures this test is fairly efficient.
             */
            if (inputRing.Length >= MAX_INVERTED_RING_SIZE) return false;

            /*
             * Don't check curves which are much larger than the input.
             * This improves performance by avoiding checking some concave inputs 
             * (which can produce fillet arcs with many more vertices)
             */
            if (curveRing.Length > INVERTED_CURVE_VERTEX_FACTOR * inputRing.Length) return false;

            /*
             * If curve contains points which are on the buffer, 
             * it is not inverted and can be included in the raw curves.
             */
            if (hasPointOnBuffer(inputRing, distance, curveRing))
                return false;

            //-- curve is inverted, so discard it
            return true;
        }

        /// <summary>
        /// Tests if there are points on the raw offset curve which may
        /// lie on the final buffer curve
        /// (i.e.they are (approximately) at the buffer distance from the input ring).
        /// For efficiency this only tests a limited set of points on the curve.
        /// </summary>
        /// <param name="inputRing">The input ring</param>
        /// <param name="distance">The distance</param>
        /// <param name="curveRing">The curve ring</param>
        /// <returns><c>true</c> if the curve contains points lying at the required buffer distance</returns>
        private static bool hasPointOnBuffer(Coordinate[] inputRing, double distance, Coordinate[] curveRing)
        {
            double distTol = NEARNESS_FACTOR * Math.Abs(distance);

            for (int i = 0; i < curveRing.Length - 1; i++)
            {
                var v = curveRing[i];

                //-- check curve vertices
                double dist = DistanceComputer.PointToSegmentString(v, inputRing);
                if (dist > distTol)
                {
                    return true;
                }

                //-- check curve segment midpoints
                int iNext = (i < curveRing.Length - 1) ? i + 1 : 0;
                var vnext = curveRing[iNext];
                var midPt = LineSegment.ComputeMidPoint(v, vnext);

                double distMid = DistanceComputer.PointToSegmentString(midPt, inputRing);
                if (distMid > distTol)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether a ring buffer is eroded completely (is empty)
        /// based on simple heuristics.
        /// <para/>
        /// The <paramref name="ringCoord"/> is assumed to contain no repeated points.
        /// It may be degenerate (i.e. contain only 1, 2, or 3 points).
        /// In this case it has no area, and hence has a minimum diameter of 0.
        /// </summary>
        /// <param name="ringCoord"></param>
        /// <param name="bufferDistance"></param>
        /// <returns></returns>
        private static bool IsErodedCompletely(Coordinate[] ringCoord, double bufferDistance)
        {
            // degenerate ring has no area
            if (ringCoord.Length < 4)
                return bufferDistance < 0;

            // important test to eliminate inverted triangle bug
            // also optimizes erosion test for triangles
            if (ringCoord.Length == 4)
                return IsTriangleErodedCompletely(ringCoord, bufferDistance);

            // if envelope is narrower than twice the buffer distance, ring is eroded
            var env = new Envelope(ringCoord);
            double envMinDimension = Math.Min(env.Height, env.Width);
            if (bufferDistance < 0.0
                && 2 * Math.Abs(bufferDistance) > envMinDimension)
                return true;

            return false;
        }

        /// <summary>
        /// Tests whether a triangular ring would be eroded completely by the given
        /// buffer distance.
        /// This is a precise test.  It uses the fact that the inner buffer of a
        /// triangle converges on the inCentre of the triangle (the point
        /// equidistant from all sides).  If the buffer distance is greater than the
        /// distance of the inCentre from a side, the triangle will be eroded completely.
        /// This test is important, since it removes a problematic case where
        /// the buffer distance is slightly larger than the inCentre distance.
        /// In this case the triangle buffer curve "inverts" with incorrect topology,
        /// producing an incorrect hole in the buffer.
        /// </summary>
        /// <param name="triangleCoord"></param>
        /// <param name="bufferDistance"></param>
        /// <returns></returns>
        private static bool IsTriangleErodedCompletely(Coordinate[] triangleCoord, double bufferDistance)
        {
            var tri = new Triangle(triangleCoord[0], triangleCoord[1], triangleCoord[2]);
            var inCentre = tri.InCentre();
            double distToCentre = DistanceComputer.PointToSegment(inCentre, tri.P0, tri.P1);
            return distToCentre < Math.Abs(bufferDistance);
        }
    }
}
