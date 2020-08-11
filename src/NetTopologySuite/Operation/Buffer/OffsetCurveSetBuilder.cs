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
    public class OffsetCurveSetBuilder
    {
        private readonly Geometry _inputGeom;
        private readonly double _distance;
        private readonly OffsetCurveBuilder _curveBuilder;

        private readonly IList<ISegmentString> _curveList = new List<ISegmentString>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="distance"></param>
        /// <param name="curveBuilder"></param>
        public OffsetCurveSetBuilder(Geometry inputGeom, double distance, OffsetCurveBuilder curveBuilder)
        {
            _inputGeom = inputGeom;
            _distance = distance;
            _curveBuilder = curveBuilder;
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
            var curve = _curveBuilder.GetLineCurve(coord, _distance);
            AddCurve(curve, Location.Exterior, Location.Interior);
        }

        private void AddLineString(Geometry line)
        {
            if (_curveBuilder.IsLineOffsetEmpty(_distance)) return;

            var coord = CoordinateArrays.RemoveRepeatedPoints(line.Coordinates);
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
            var shellCoord = CoordinateArrays.RemoveRepeatedPoints(shell.Coordinates);
            // optimization - don't bother computing buffer
            // if the polygon would be completely eroded
            if (_distance < 0.0 && IsErodedCompletely(shellCoord, _distance))
                return;
            // don't attemtp to buffer a polygon with too few distinct vertices
            if (_distance <= 0.0 && shellCoord.Length < 3)
                return;

            AddRingSide(shellCoord, offsetDistance, offsetSide,
                           Location.Exterior, Location.Interior);

            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                var hole = (LinearRing)p.GetInteriorRingN(i);
                var holeCoord = CoordinateArrays.RemoveRepeatedPoints(hole.Coordinates);

                // optimization - don't bother computing buffer for this hole
                // if the hole would be completely covered
                if (_distance > 0.0 && IsErodedCompletely(holeCoord, -_distance))
                    continue;

                // Holes are topologically labelled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CCW)
                AddRingSide(holeCoord, offsetDistance, PositionExtensions.Opposite(offsetSide),
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
            if (coord.Length >= LinearRing.MinimumValidSize
                && Orientation.IsCCW(coord))
            {
                leftLoc = cwRightLoc;
                rightLoc = cwLeftLoc;
                side = PositionExtensions.Opposite(side);
            }
            var curve = _curveBuilder.GetRingCurve(coord, side, offsetDistance);
            AddCurve(curve, leftLoc, rightLoc);
        }

        /// <summary>
        /// The ringCoord is assumed to contain no repeated points.
        /// It may be degenerate (i.e. contain only 1, 2, or 3 points).
        /// In this case it has no area, and hence has a minimum diameter of 0.
        /// </summary>
        /// <param name="ringCoord"></param>
        /// <param name="bufferDistance"></param>
        /// <returns></returns>
        private bool IsErodedCompletely(Coordinate[] ringCoord, double bufferDistance)
        {
            // degenerate ring has no area
            if (ringCoord.Length < 4)
                return bufferDistance < 0;

            // important test to eliminate inverted triangle bug
            // also optimizes erosion test for triangles
            if (ringCoord.Length == 4)
                return IsTriangleErodedCompletely(ringCoord, bufferDistance);

            /*
             * The following is a heuristic test to determine whether an
             * inside buffer will be eroded completely.
             * It is based on the fact that the minimum diameter of the ring pointset
             * provides an upper bound on the buffer distance which would erode the
             * ring.
             * If the buffer distance is less than the minimum diameter, the ring
             * may still be eroded, but this will be determined by
             * a full topological computation.
             *
             */
            var ring = _inputGeom.Factory.CreateLinearRing(ringCoord);
            var md = new MinimumDiameter(ring);
            double minDiam = md.Length;
            return minDiam < 2 * Math.Abs(bufferDistance);
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
