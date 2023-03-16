using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Simplify;
using NetTopologySuite.Utilities;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Collections.Specialized.BitVector32;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes an offset curve from a geometry.
    /// An offset curve is a linear geometry which is offset a given distance
    /// from the input.
    /// If the offset distance is positive the curve lies on the left side of the input;
    /// if it is negative the curve is on the right side.
    /// The curve(s) have the same direction as the input line(s).
    /// <para/>
    /// The offset curve is based on the boundary of the buffer for the geometry
    /// at the offset distance(see <see cref="BufferOp"/>.
    /// The normal mode of operation is to return the sections of the buffer boundarywhich lie on the raw offset curve
    /// (obtained via <see cref="RawOffset(LineString, double)"/>.
    /// The offset curve will contain multiple sections
    /// if the input self-intersects or has close approaches.The computed sections are ordered along the raw offset curve.
    /// Sections are disjoint.They never self-intersect, but may be rings.
    /// <list type="bullet">
    /// <item><description>For a <see cref="LineString"/> the offset curve is a linear geometry
    /// (<see cref="LineString"/> or <see cref="MultiLineString"/>).</description></item>
    /// <item><description>For a <see cref="Point"/> or <see cref="MultiPoint"/> the offset curve is an empty <see cref="LineString"/>.</description></item>
    /// <item><description>For a <see cref="Polygon"/> the offset curve is the boundary of the polygon buffer (which
    /// may be a <see cref="MultiLineString"/>).</description></item>
    /// <item><description>For a collection the output is a <see cref="MultiLineString"/> containing the offset curves of the elements.</description></item>
    /// </list>
    /// <para/>
    /// In "joined" mode (see {@link #setJoined(boolean)}
    /// the sections computed for each input line are joined into a single offset curve line.
    /// The joined curve may self-intersect.
    /// At larger offset distances the curve may contain "flat-line" artifacts
    /// in places where the input self-intersects.
    /// <para/>
    /// Offset curves support setting the number of quadrant segments,
    /// the join style, and the mitre limit(if applicable) via
    /// the <see cref="BufferParameters"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class OffsetCurve
    {
        /// <summary>
        /// The nearness tolerance for matching the raw offset linework and the buffer curve.
        /// </summary>
        private const int MatchDistanceFactor = 10000;

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
        /// with specified quadrant segments, join style and mitre limit.
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


        /**
         * Computes the offset curve of a geometry at a given distance,
         * joining curve sections into a single line for each input line.
         * 
         * @param geom a geometry
         * @param distance the offset distance (positive for left, negative for right)
         * @return the joined offset curve
         */
        /// <summary>
        /// Computes the offset curve of a geometry at a given distance,
        /// joining curve sections into a single line for each input line.
        /// </summary>
        /// <param name="geom">A geometry</param>
        /// <param name="distance">the offset distance (positive for left, negative for right)</param>
        /// <returns>The joined offset curve</returns>
        public static Geometry GetCurveJoined(Geometry geom, double distance)
        {
            var oc = new OffsetCurve(geom, distance) {
                Joined = true
            };
            return oc.GetCurve();
        }

        private readonly Geometry _inputGeom;
        private readonly double _distance;
        private bool _isJoined;

        private readonly BufferParameters _bufferParams;
        private readonly double _matchDistance;
        private readonly GeometryFactory _geomFactory;

        /// <summary>
        /// Creates a new instance for computing an offset curve for a geometry at a given distance.
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
        /// setting the quadrant segments, join style and mitre limit
        /// via <see cref="BufferParameters"/>.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <param name="distance">The offset distance (positive for left, negative for right)</param>
        /// <param name="bufParams">The buffer paramters to use</param>
        public OffsetCurve(Geometry geom, double distance, BufferParameters bufParams)
        {
            _inputGeom = geom;
            _distance = distance;

            _matchDistance = Math.Abs(distance) / MatchDistanceFactor;
            _geomFactory = _inputGeom.Factory;

            //-- make new buffer params since the end cap style must be the default
            _bufferParams = bufParams?.Copy() ?? new BufferParameters();
        }

        /// <summary>
        /// Gets or sets a value indicating if a single curve line for
        /// each input linear component is computed
        /// by joining curve sections in order along the raw offset curve.
        /// The default mode is to compute separate curve sections.
        /// </summary>
        public bool Joined
        {
            get => _isJoined;
            set => _isJoined = value;
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
            var pts = line.Coordinates;
            var cleanPts = CoordinateArrays.RemoveRepeatedOrInvalidPoints(pts);

            var ocb = new OffsetCurveBuilder(line.Factory.PrecisionModel, bufParams);
            var rawPts = ocb.GetOffsetCurve(cleanPts, distance);
            return rawPts;
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

        private Geometry ComputeCurve(LineString lineGeom, double distance)
        {
            //-- first handle simple cases
            //-- empty or single-point lines
            if (lineGeom.NumPoints < 2 || lineGeom.Length == 0.0)
            {
                return _geomFactory.CreateLineString();
            }
            //-- two-point-line
            if (lineGeom.NumPoints == 2)
            {
                return OffsetSegment(lineGeom.Coordinates, distance);
            }

            var sections = ComputeSections(lineGeom, distance);

            Geometry offsetCurve;
            if (_isJoined)
            {
                offsetCurve = OffsetCurveSection.ToLine(sections, _geomFactory);
            }
            else
            {
                offsetCurve = OffsetCurveSection.ToGeometry(sections, _geomFactory);
            }
            return offsetCurve;
        }

        private List<OffsetCurveSection> ComputeSections(LineString lineGeom, double distance)
        {
            var rawCurve = RawOffset(lineGeom, distance, _bufferParams);
            var sections = new List<OffsetCurveSection>();
            if (rawCurve.Length == 0)
            {
                return sections;
            }

            /*
             * Note: If the raw offset curve has no
             * narrow concave angles or self-intersections it could be returned as is.
             * However, this is likely to be a less frequent situation, 
             * and testing indicates little performance advantage,
             * so not doing this. 
             */

            var bufferPoly = GetBufferOriented(lineGeom, distance, _bufferParams);

            //-- first extract offset curve sections from shell
            var shell = bufferPoly.ExteriorRing.Coordinates;
            ComputeCurveSections(shell, rawCurve, sections);

            //-- extract offset curve sections from holes
            for (int i = 0; i < bufferPoly.NumInteriorRings; i++)
            {
                var hole = bufferPoly.GetInteriorRingN(i).Coordinates;
                ComputeCurveSections(hole, rawCurve, sections);
            }
            return sections;
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
        /// Used here to avoid issues with non-robust buffer results
        /// which have spurious extra polygons.
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


        private const double NOT_IN_CURVE = -1;

        private void ComputeCurveSections(Coordinate[] bufferRingPts,
            Coordinate[] rawCurve, List<OffsetCurveSection> sections)
        {
            double[] rawPosition = new double[bufferRingPts.Length - 1];
            for (int i = 0; i < rawPosition.Length; i++)
            {
                rawPosition[i] = NOT_IN_CURVE;
            }
            var bufferSegIndex = new SegmentMCIndex(bufferRingPts);
            int bufferFirstIndex = -1;
            double minRawPosition = -1;
            for (int i = 0; i < rawCurve.Length - 1; i++)
            {
                int minBufferIndexForSeg = MatchSegments(
                                rawCurve[i], rawCurve[i + 1], i, bufferSegIndex, bufferRingPts, rawPosition);
                if (minBufferIndexForSeg >= 0)
                {
                    double pos = rawPosition[minBufferIndexForSeg];
                    if (bufferFirstIndex < 0 || pos < minRawPosition)
                    {
                        minRawPosition = pos;
                        bufferFirstIndex = minBufferIndexForSeg;
                    }
                }
            }
            //-- no matching sections found in this buffer ring
            if (bufferFirstIndex < 0)
                return;
            ExtractSections(bufferRingPts, rawPosition, bufferFirstIndex, sections);
        }

        /// <summary>
        /// Matches the segments in a buffer ring to the raw offset curve
        /// to obtain their match positions(if any).
        /// </summary>
        /// <param name="raw0">A raw curve segment start point</param>
        /// <param name="raw1">A raw curve segment end point</param>
        /// <param name="rawCurveIndex">The index of the raw curve segment</param>
        /// <param name="bufferSegIndex">The spatial index of the buffer ring segments</param>
        /// <param name="bufferPts">The points of the buffer ring</param>
        /// <param name="rawCurvePos">The raw curve positions of the buffer ring segments</param>
        /// <returns>The index of the minimum matched buffer segment</returns>
        private int MatchSegments(Coordinate raw0, Coordinate raw1, int rawCurveIndex,
            SegmentMCIndex bufferSegIndex, Coordinate[] bufferPts,
            double[] rawCurvePos)
        {
            var matchEnv = new Envelope(raw0, raw1);
            matchEnv.ExpandBy(_matchDistance);
            var matchAction = new MatchCurveSegmentAction(raw0, raw1, rawCurveIndex, _matchDistance, bufferPts, rawCurvePos);
            bufferSegIndex.Query(matchEnv, matchAction);
            return matchAction.BufferMinIndex;
        }

        /// <summary>
        /// An action to match a raw offset curve segment
        /// to segments in a buffer ring
        /// and record the matched segment locations(s) along the raw curve.
        /// </summary>
        /// <author>Martin Davis</author>
        private class MatchCurveSegmentAction
            : MonotoneChainSelectAction
        {
            private readonly Coordinate _p0;
            private readonly Coordinate _p1;
            private readonly int _rawCurveIndex;
            private readonly Coordinate[] _bufferRingPts;
            private readonly double _matchDistance;
            private readonly double[] _rawCurveLoc;
            private double _minRawLocation = -1;
            private int _bufferRingMinIndex = -1;

            public MatchCurveSegmentAction(Coordinate p0, Coordinate p1,
                int rawCurveIndex,
                 double matchDistance, Coordinate[] bufferRingPts, double[] rawCurveLoc)
            {
                _p0 = p0;
                _p1 = p1;
                _rawCurveIndex = rawCurveIndex;
                _bufferRingPts = bufferRingPts;
                _matchDistance = matchDistance;
                _rawCurveLoc = rawCurveLoc;
            }

            public int BufferMinIndex => _bufferRingMinIndex;

            public override void Select(MonotoneChain mc, int segIndex)
            {
                /*
                 * A curveRingPt segment may match all or only a portion of a single raw segment.
                 * There may be multiple curve ring segs that match along the raw segment.
                 * The one closest to the segment start is recorded as the offset curve start.      
                 */
                double frac = SegmentMatchFrac(_bufferRingPts[segIndex], _bufferRingPts[segIndex + 1],
                                               _p0, _p1, _matchDistance);

                //-- no match
                if (frac < 0) return;

                //-- location is used to sort segments along raw curve
                double location = _rawCurveIndex + frac;
                _rawCurveLoc[segIndex] = location;
                //-- record lowest index
                if (_minRawLocation < 0 || location < _minRawLocation)
                {
                    _minRawLocation = location;
                    _bufferRingMinIndex = segIndex;
                }
            }
        }
    
        private static double SegmentMatchFrac(Coordinate p0, Coordinate p1,
    Coordinate seg0, Coordinate seg1, double matchDistance)
        {
            if (matchDistance < DistanceComputer.PointToSegment(p0, seg0, seg1))
                return -1;
            if (matchDistance < DistanceComputer.PointToSegment(p1, seg0, seg1))
                return -1;
            //-- matched - determine position as fraction along segment
            var seg = new LineSegment(seg0, seg1);
            return seg.SegmentFraction(p0);
        }

        /**
         * This is only called when there is at least one ring segment matched
         * (so rawCurvePos has at least one entry != NOT_IN_CURVE).
         * The start index of the first section must be provided.
         * This is intended to be the section with lowest position
         * along the raw curve.
         * @param ringPts the points in a buffer ring
         * @param rawCurveLoc the position of buffer ring segments along the raw curve
         * @param startIndex the index of the start of a section
         * @param sections the list of extracted offset curve sections
         */
        private void ExtractSections(Coordinate[] ringPts, double[] rawCurveLoc,
            int startIndex, List<OffsetCurveSection> sections)
        {
            int sectionStart = startIndex;
            int sectionCount = 0;
            int sectionEnd;
            do
            {
                sectionEnd = FindSectionEnd(rawCurveLoc, sectionStart, startIndex);
                double location = rawCurveLoc[sectionStart];
                int lastIndex = Prev(sectionEnd, rawCurveLoc.Length);
                double lastLoc = rawCurveLoc[lastIndex];
                var section = OffsetCurveSection.Create(ringPts, sectionStart, sectionEnd, location, lastLoc);
                sections.Add(section);
                sectionStart = FindSectionStart(rawCurveLoc, sectionEnd);

                //-- check for an abnormal state
                if (sectionCount++ > ringPts.Length)
                {
                    Assert.ShouldNeverReachHere("Too many sections for ring - probable bug");
                }
            } while (sectionStart != startIndex && sectionEnd != startIndex);
        }

        private int FindSectionStart(double[] loc, int end)
        {
            int start = end;
            do
            {
                int next = Next(start, loc.Length);
                //-- skip ahead if segment is not in raw curve
                if (loc[start] == NOT_IN_CURVE)
                {
                    start = next;
                    continue;
                }
                int prev = Prev(start, loc.Length);
                //-- if prev segment is not in raw curve then have found a start
                if (loc[prev] == NOT_IN_CURVE)
                {
                    return start;
                }
                if (_isJoined)
                {
                    /*
                     *  Start section at next gap in raw curve.
                     *  Only needed for joined curve, since otherwise
                     *  contiguous buffer segments can be in same curve section.
                     */
                    double locDelta = Math.Abs(loc[start] - loc[prev]);
                    if (locDelta > 1)
                        return start;
                }
                start = next;
            } while (start != end);
            return start;
        }

        private int FindSectionEnd(double[] loc, int start, int firstStartIndex)
        {
            // assert: pos[start] is IN CURVE
            int end = start;
            int next;
            do
            {
                next = Next(end, loc.Length);
                if (loc[next] == NOT_IN_CURVE)
                    return next;
                if (_isJoined)
                {
                    /*
                     *  End section at gap in raw curve.
                     *  Only needed for joined curve, since otherwise
                     *  contigous buffer segments can be in same section
                     */
                    double locDelta = Math.Abs(loc[next] - loc[end]);
                    if (locDelta > 1)
                        return next;
                }
                end = next;
            } while (end != start && end != firstStartIndex);
            return end;
        }

        private static int Next(int i, int size)
        {
            i += 1;
            return (i < size) ? i : 0;
        }

        private static int Prev(int i, int size)
        {
            i -= 1;
            return (i < 0) ? size - 1 : i;
        }
    }
}
