using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates all the raw offset curves for a buffer of a <c>Geometry</c>.
    /// Raw curves need to be noded together and polygonized to form the final buffer area.
    /// </summary>
    public class OffsetCurveSetBuilder
    {        
        private readonly IGeometry _inputGeom;
        private readonly double _distance;
        private readonly OffsetCurveBuilder _curveBuilder;

        private readonly IList<ISegmentString> _curveList = new List<ISegmentString>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <param name="distance"></param>
        /// <param name="curveBuilder"></param>
        public OffsetCurveSetBuilder(IGeometry inputGeom, double distance, OffsetCurveBuilder curveBuilder)
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
        /// 
        /// </summary>
        /// <param name="lineList"></param>
        /// <param name="leftLoc"></param>
        /// <param name="rightLoc"></param>
        private void AddCurves(IEnumerable<ICoordinate[]> lineList, Locations leftLoc, Locations rightLoc)
        {
            foreach (ICoordinate[] coords in lineList)
                AddCurve(coords, leftLoc, rightLoc);
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
        private void AddCurve(ICoordinate[] coord, Locations leftLoc, Locations rightLoc)
        {
            // don't add null curves!
            if (coord.Length < 2) 
                return;
            // add the edge for a coordinate list which is a raw offset curve
            var e = new NodedSegmentString(coord, new Label(0, Locations.Boundary, leftLoc, rightLoc));
            _curveList.Add(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void Add(IGeometry g)
        {
            if (g.IsEmpty) return;
            if (g is IPolygon)                 
                AddPolygon((IPolygon) g);
            // LineString also handles LinearRings
            else if (g is ILineString)        
                AddLineString(g);
            else if (g is IPoint) 
                AddPoint(g);
            else if (g is IMultiPoint) 
                AddCollection(g);
            else if (g is IMultiLineString) 
                AddCollection(g);
            else if (g is IMultiPolygon)
                AddCollection(g);
            else if (g is IGeometryCollection) 
                AddCollection(g);
            else  throw new NotSupportedException(g.GetType().FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gc"></param>
        private void AddCollection(IGeometry gc)
        {
            for (var i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                Add(g);
            }
        }

        /// <summary>
        /// Add a Point to the graph.
        /// </summary>
        /// <param name="p"></param>
        private void AddPoint(IGeometry p)
        {
            if (_distance <= 0.0) 
                return;
            var coord = p.Coordinates;
            var lineList = _curveBuilder.GetLineCurve(coord, _distance);
            AddCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        private void AddLineString(IGeometry line)
        {
            if (_distance <= 0.0) 
                return;
            var coord = CoordinateArrays.RemoveRepeatedPoints(line.Coordinates);
            var lineList = _curveBuilder.GetLineCurve(coord, _distance);
            AddCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        private void AddPolygon(IPolygon p)
        {
            var offsetDistance = _distance;
            var offsetSide = Positions.Left;
            if (_distance < 0.0)
            {
                offsetDistance = -_distance;
                offsetSide = Positions.Right;
            }

            var shell = p.Shell;
            var shellCoord = CoordinateArrays.RemoveRepeatedPoints(shell.Coordinates);
            // optimization - don't bother computing buffer
            // if the polygon would be completely eroded
            if (_distance < 0.0 && IsErodedCompletely(shellCoord, _distance))
                return;

            AddPolygonRing(shellCoord, offsetDistance, offsetSide, 
                           Locations.Exterior, Locations.Interior);

            for (var i = 0; i < p.NumInteriorRings; i++)
            {
                var hole = (ILinearRing) p.GetInteriorRingN(i);
                var holeCoord = CoordinateArrays.RemoveRepeatedPoints(hole.Coordinates);

                // optimization - don't bother computing buffer for this hole
                // if the hole would be completely covered
                if (_distance > 0.0 && IsErodedCompletely(holeCoord, -_distance))
                    continue;

                // Holes are topologically labelled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CCW)
                AddPolygonRing(holeCoord, offsetDistance, Position.Opposite(offsetSide),
                               Locations.Interior, Locations.Exterior);
            }
        }

        /// <summary>
        /// Add an offset curve for a ring.
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
        private void AddPolygonRing(ICoordinate[] coord, double offsetDistance, 
            Positions side, Locations cwLeftLoc, Locations cwRightLoc)
        {
            var leftLoc = cwLeftLoc;
            var rightLoc = cwRightLoc;
            if (CGAlgorithms.IsCCW(coord))
            {
                leftLoc = cwRightLoc;
                rightLoc = cwLeftLoc;
                side = Position.Opposite(side);
            }
            var lineList = _curveBuilder.GetRingCurve(coord, side, offsetDistance);
            AddCurves(lineList, leftLoc, rightLoc);
        }

        /// <summary>
        /// The ringCoord is assumed to contain no repeated points.
        /// It may be degenerate (i.e. contain only 1, 2, or 3 points).
        /// In this case it has no area, and hence has a minimum diameter of 0.
        /// </summary>
        /// <param name="ringCoord"></param>
        /// <param name="bufferDistance"></param>
        /// <returns></returns>
        private bool IsErodedCompletely(ICoordinate[] ringCoord, double bufferDistance)
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
        private bool IsTriangleErodedCompletely(ICoordinate[] triangleCoord, double bufferDistance)
        {
            var tri = new Triangle(triangleCoord[0], triangleCoord[1], triangleCoord[2]);
            var inCentre = tri.InCentre;
            var distToCentre = CGAlgorithms.DistancePointLine(inCentre, tri.P0, tri.P1);
            return distToCentre < Math.Abs(bufferDistance);
        }
    }
}
