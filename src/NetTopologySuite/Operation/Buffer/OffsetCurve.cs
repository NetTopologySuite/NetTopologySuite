using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Chain;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes an offset curve from a geometry.
    /// The offset curve is a linear geometry which is offset a specified distance
    /// from the input.
    /// If the offset distance is positive the curve lies on the left side of the input;
    /// if it is negative the curve is on the right side.
    /// <list type="bullet">
    /// <item><description>For a <see cref="LineString"/> the offset curve is a line.</description></item>
    /// <item><description>For a <see cref="Point"/> the offset curve is an empty <see cref="LineString"/>.</description></item>
    /// <item><description>For a <see cref="Polygon"/> the offset curve is the boundary of the polygon buffer (which
    /// may be a <see cref="MultiLineString"/>).</description></item>
    /// <item><description>For a collection the output is a <see cref="MultiLineString"/> containing the element offset curves.</description></item>
    /// </list>
    /// <para/>
    /// The offset curve is computed as a single contiguous section of the geometry buffer boundary.
    /// In some geometric situations this definition is ill-defined.
    /// This algorithm provides a "best-effort" interpretation.
    /// In particular:
    /// <list type="bullet">
    /// <item><description>For self-intersecting lines, the buffer boundary includes
    /// offset lines for both left and right sides of the input line.
    /// Only a single contiguous portion on the specified side is returned.</description></item>
    /// <item><description>If the offset corresponds to buffer holes, only the largest hole is used.</description></item>
    /// </list>
    /// Offset curves support setting the number of quadrant segments,
    /// the join style, and the mitre limit(if applicable) via
    /// the <see cref="BufferParameters"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class OffsetCurve
    {
        /// <summary>
        /// The nearness tolerance between the raw offset linework and the buffer curve.
        /// </summary>
        private const int NearnessFactor = 10000;

        /// <summary>
        /// Computes the offset curve of a geometry at a given distance.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <param name="distance">the offset distance (positive for left, negative for right)</param>
        /// <returns>The offset curve</returns>
        public static Geometry GetCurve(Geometry geom, double distance)
        {
            var oc = new OffsetCurve(geom, distance);
            return oc.GetCurve();
        }

        /// <summary>
        /// Computes the offset curve of a geometry at a given distance,
        /// and for a specified quadrant segments, join style and mitre limit.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <param name="distance">The offset distance (positive for left, negative for right)</param>
        /// <param name="quadSegs">The quadrant segments</param>
        /// <param name="joinStyle">The join style</param>
        /// <param name="mitreLimit">The mitre limit</param>
        /// <returns>The offset curve</returns>
        public static Geometry GetCurve(Geometry geom, double distance, int quadSegs = -1, JoinStyle joinStyle = JoinStyle.Round, double mitreLimit = -1)
        {
            var bufferParams = new BufferParameters();
            if (quadSegs >= 0) bufferParams.QuadrantSegments = quadSegs;
            if (joinStyle >= 0) bufferParams.JoinStyle = joinStyle;
            if (mitreLimit >= 0) bufferParams.MitreLimit = mitreLimit;
            var oc = new OffsetCurve(geom, distance, bufferParams);
            return oc.GetCurve();
        }


        private readonly Geometry _inputGeom;
        private readonly double _distance;
        private readonly BufferParameters _bufferParams;
        private readonly double _matchDistance;
        private readonly GeometryFactory _geomFactory;

        /// <summary>
        /// Creates a new instance for computing an offset curve for a geometryat a given distance.
        /// with default quadrant segments(<see cref="BufferParameters.DefaultQuadrantSegments"/>
        /// and join style (<see cref="BufferParameters.DefaultJoinStyle"/>).
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <param name="distance">The offset distance (positive for left, negative for right)</param>
        public OffsetCurve(Geometry geom, double distance)
            : this(geom, distance, null)
        {
        }

        /// <summary>
        /// Creates a new instance for computing an offset curve for a geometry at a given distance.
        /// allowing the quadrant segments and join style and mitre limit to be set
        /// via <see cref="BufferParameters"/>.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <param name="distance">The offset distance (positive for left, negative for right)</param>
        /// <param name="bufParams">The buffer paramters to use</param>
        public OffsetCurve(Geometry geom, double distance, BufferParameters bufParams)
        {
            _inputGeom = geom;
            _distance = distance;

            _matchDistance = Math.Abs(distance) / NearnessFactor;
            _geomFactory = _inputGeom.Factory;

            //-- make new buffer params since the end cap style must be the default
            _bufferParams = bufParams?.Copy() ?? new BufferParameters();
        }

        /// <summary>
        /// Gets the computed offset curve.
        /// </summary>
        /// <returns>The offset curve geometry</returns>
        public Geometry GetCurve()
        {
            var ocmo = new OffsetCurveMapOp(this);
            return GeometryMapper.FlatMap(_inputGeom, Dimension.Curve, ocmo);
        }

        private class OffsetCurveMapOp : GeometryMapper.IMapOp
        {
            private readonly OffsetCurve _parent;

            public OffsetCurveMapOp(OffsetCurve parent)
            {
                _parent = parent;
            }
            public Geometry Map(Geometry geom)
            {
                if (geom is Point) return null;
                if (geom is Polygon)
                {
                    return ToLineString(geom.Buffer(_parent._distance).Boundary);
                }
                return _parent.ComputeCurve((LineString)geom, _parent._distance);
            }

            /// <summary>
            /// Force LinearRings to be LineStrings.
            /// </summary>
            /// <param name="geom">A geometry, which may be a <c>LinearRing</c></param>
            /// <returns>A geometry which will be a <c>LineString</c> or <c>MulitLineString</c></returns>
            private Geometry ToLineString(Geometry geom)
            {
                if (geom is LinearRing ring)
                {
                    return geom.Factory.CreateLineString(ring.CoordinateSequence);
                }
                return geom;
            }
        }

        /// <summary>
        /// Gets the raw offset curve for a line at a given distance.
        /// The quadrant segments, join style and mitre limit can be specified
        /// via <see cref="BufferParameters"/>.
        /// <para/>
        /// The raw offset line may contain loops and other artifacts which are
        /// not present in the true offset curve.
        /// </summary>
        /// <param name="line">The <c>LineString</c> to offset</param>
        /// <param name="distance">The offset distance (positive for left, negative for right)</param>
        /// <param name="bufParams">The buffer parameters to use</param>
        /// <returns>The raw offset curve points</returns>
        public static Coordinate[] RawOffset(LineString line, double distance, BufferParameters bufParams)
        {
            var ocb = new OffsetCurveBuilder(line.Factory.PrecisionModel, bufParams);
            var pts = ocb.GetOffsetCurve(line.Coordinates, distance);
            return pts;
        }

        /// <summary>
        /// Gets the raw offset curve for a line at a given distance,
        /// with default buffer parameters.
        /// </summary>
        /// <param name="line">The <c>LineString</c> to offset</param>
        /// <param name="distance">The offset distance (positive for left, negative for right)</param>
        /// <returns>The raw offset line</returns>
        public static Coordinate[] RawOffset(LineString line, double distance)
        {
            return RawOffset(line, distance, new BufferParameters());
        }

        private LineString ComputeCurve(LineString lineGeom, double distance)
        {
            //-- first handle special/simple cases
            if (lineGeom.NumPoints < 2 || lineGeom.Length == 0.0)
            {
                return _geomFactory.CreateLineString();
            }
            if (lineGeom.NumPoints == 2)
            {
                return OffsetSegment(lineGeom.Coordinates, distance);
            }

            var rawOffset = RawOffset(lineGeom, distance, _bufferParams);
            if (rawOffset.Length == 0)
            {
                return _geomFactory.CreateLineString();
            }
            /*
             * Note: If the raw offset curve has no
             * narrow concave angles or self-intersections it could be returned as is.
             * However, this is likely to be a less frequent situation, 
             * and testing indicates little performance advantage,
             * so not doing this. 
             */

            var bufferPoly = GetBufferOriented(lineGeom, distance, _bufferParams);

            //-- first try matching shell to raw curve
            var shell = bufferPoly.ExteriorRing.Coordinates;
            var offsetCurve = ComputeCurve(shell, rawOffset);
            if (!offsetCurve.IsEmpty
                || bufferPoly.NumInteriorRings == 0)
                return offsetCurve;

            //-- if shell didn't work, try matching to largest hole 
            var holePts = ExtractLongestHole(bufferPoly).Coordinates;
            offsetCurve = ComputeCurve(holePts, rawOffset);
            return offsetCurve;
        }

        private LineString OffsetSegment(Coordinate[] pts, double distance)
        {
            var offsetSeg = (new LineSegment(pts[0], pts[1])).Offset(distance);
            return _geomFactory.CreateLineString(new Coordinate[] { offsetSeg.P0, offsetSeg.P1 });
        }

        private static Polygon GetBufferOriented(LineString geom, double distance, BufferParameters bufParams)
        {
            var buffer = geom.Buffer(Math.Abs(distance), bufParams);
            var bufferPoly = ExtractMaxAreaPolygon(buffer);
            //-- for negative distances (Right of input) reverse buffer direction to match offset curve
            if (distance < 0)
            {
                bufferPoly = (Polygon)bufferPoly.Reverse();
            }
            return bufferPoly;
        }

        /// <summary>
        /// Extracts the largest polygon by area from a geometry.
        /// Used here to avoid issues with non-robust buffer results which have spurious extra polygons.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <returns>The polygon element of largest area</returns>
        private static Polygon ExtractMaxAreaPolygon(Geometry geom)
        {
            if (geom.NumGeometries == 1)
                return (Polygon)geom;

            double maxArea = 0;
            Polygon maxPoly = null;
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var poly = (Polygon)geom.GetGeometryN(i);
                double area = poly.Area;
                if (maxPoly == null || area > maxArea)
                {
                    maxPoly = poly;
                    maxArea = area;
                }
            }
            return maxPoly;
        }

        private static LinearRing ExtractLongestHole(Polygon poly)
        {
            LinearRing largestHole = null;
            double maxLen = -1;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = (LinearRing)poly.GetInteriorRingN(i);
                double len = hole.Length;
                if (len > maxLen)
                {
                    largestHole = hole;
                    maxLen = len;
                }
            }
            return largestHole;
        }

        private LineString ComputeCurve(Coordinate[] bufferPts, Coordinate[] rawOffset)
        {
            bool[] isInCurve = new bool[bufferPts.Length - 1];
            var segIndex = new SegmentMCIndex(bufferPts);
            int curveStart = -1;
            for (int i = 0; i < rawOffset.Length - 1; i++)
            {
                int index = MarkMatchingSegments(
                                rawOffset[i], rawOffset[i + 1], segIndex, bufferPts, isInCurve);
                if (curveStart < 0)
                {
                    curveStart = index;
                }
            }
            var curvePts = ExtractSection(bufferPts, curveStart, isInCurve);
            return _geomFactory.CreateLineString(curvePts);
        }

        private int MarkMatchingSegments(Coordinate p0, Coordinate p1,
            SegmentMCIndex segIndex, Coordinate[] bufferPts,
            bool[] isInCurve)
        {
            var matchEnv = new Envelope(p0, p1);
            matchEnv.ExpandBy(_matchDistance);
            var action = new MatchCurveSegmentAction(p0, p1, bufferPts, _matchDistance, isInCurve);
            segIndex.Query(matchEnv, action);
            return action.MinCurveIndex;
        }

        /// <summary>
        /// An action to match a raw offset curve segment
        /// to segments in the buffer ring
        /// and mark them as being in the offset curve.
        /// </summary>
        /// <author>Martin Davis</author>
        private class MatchCurveSegmentAction
            : MonotoneChainSelectAction
        {
            private readonly Coordinate _p0;
            private readonly Coordinate _p1;
            private readonly Coordinate[] _bufferPts;
            private readonly double _matchDistance;
            private readonly bool[] _isInCurve;

            private double _minFrac = -1;
            private int _minCurveIndex = -1;

            public MatchCurveSegmentAction(Coordinate p0, Coordinate p1,
                Coordinate[] bufferPts, double matchDistance, bool[] isInCurve)
            {
                _p0 = p0;
                _p1 = p1;
                _bufferPts = bufferPts;
                _matchDistance = matchDistance;
                _isInCurve = isInCurve;
            }

            public override void Select(MonotoneChain mc, int segIndex)
            {
                /*
                 * A curveRingPt segment may match all or only a portion of a single raw segment.
                 * There may be multiple curve ring segs that match along the raw segment.
                 * The one closest to the segment start is recorded as the offset curve start.      
                 */
                double frac = SubsegmentMatchFrac(_bufferPts[segIndex], _bufferPts[segIndex + 1], _p0, _p1, _matchDistance);
                //-- no match
                if (frac < 0) return;

                _isInCurve[segIndex] = true;

                //-- record lowest index
                if (_minFrac < 0 || frac < _minFrac)
                {
                    _minFrac = frac;
                    _minCurveIndex = segIndex;
                }
            }

            public int MinCurveIndex =>  _minCurveIndex;
        }

        /*
        // Slower, non-indexed algorithm.  Left here for future testing.

        private Coordinate[] OLDcomputeCurve(Coordinate[] curveRingPts, Coordinate[] rawOffset) {
          boolean[] isInCurve = new boolean[curveRingPts.length - 1];
          int curveStart = -1;
          for (int i = 0; i < rawOffset.length - 1; i++) {
            int index = markMatchingSegments(
                            rawOffset[i], rawOffset[i + 1], curveRingPts, isInCurve);
            if (curveStart < 0) {
              curveStart = index;
            }
          }
          Coordinate[] curvePts = extractSection(curveRingPts, isInCurve, curveStart);
          return curvePts;
        }

        private int markMatchingSegments(Coordinate p0, Coordinate p1, Coordinate[] curveRingPts, boolean[] isInCurve) {
          double minFrac = -1;
          int minCurveIndex = -1;
          for (int i = 0; i < curveRingPts.length - 1; i++) {
             // A curveRingPt seg will only match a portion of a single raw segment.
             // But there may be multiple curve ring segs that match along that segment.
             // The one closest to the segment start is recorded.
            double frac = subsegmentMatchFrac(curveRingPts[i], curveRingPts[i+1], p0, p1, matchDistance);
            //-- no match
            if (frac < 0) continue;

            isInCurve[i] = true;

            //-- record lowest index
            if (minFrac < 0 || frac < minFrac) {
              minFrac = frac;
              minCurveIndex = i;
            }
          }
          return minCurveIndex;
        }
        */

        private static double SubsegmentMatchFrac(Coordinate p0, Coordinate p1,
            Coordinate seg0, Coordinate seg1, double matchDistance)
        {
            if (matchDistance < DistanceComputer.PointToSegment(p0, seg0, seg1))
                return -1;
            if (matchDistance < DistanceComputer.PointToSegment(p1, seg0, seg1))
                return -1;
            //-- matched - determine position as fraction
            var seg = new LineSegment(seg0, seg1);
            return seg.SegmentFraction(p0);
        }

        /// <summary>
        /// Extracts a section of a ring of coordinates, starting at a given index,
        /// and keeping coordinates which are flagged as being required.
        /// </summary>
        /// <param name="ring">The ring of points</param>
        /// <param name="startIndex">The index of the start coordinate</param>
        /// <param name="isExtracted">A flag indicating if coordinate is to be extracted</param>
        private static Coordinate[] ExtractSection(Coordinate[] ring, int startIndex, bool[] isExtracted)
        {
            if (startIndex < 0)
                return new Coordinate[0];

            var coordList = new CoordinateList();
            int i = startIndex;
            do
            {
                coordList.Add(ring[i], false);
                if (!isExtracted[i])
                {
                    break;
                }
                i = Next(i, ring.Length - 1);
            } while (i != startIndex);
            //-- handle case where every segment is extracted
            if (isExtracted[i])
            {
                coordList.Add(ring[i], false);
            }

            //-- if only one point found return empty LineString
            if (coordList.Count == 1)
                return new Coordinate[0];

            return coordList.ToCoordinateArray();
        }

        private static int Next(int i, int size)
        {
            i += 1;
            return (i < size) ? i : 0;
        }
    }
}
