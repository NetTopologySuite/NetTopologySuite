using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Triangulate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NetTopologySuite.Coverage
{

    /// <summary>
    /// Validates that a polygon forms a valid polygonal coverage
    /// with the set of polygons adjacent to it.
    /// If the polygon is coverage-valid an empty { @link LineString} is returned.
    /// Otherwise, the result is a linear geometry containing
    /// the polygon boundary linework causing the invalidity.
    /// <para/>
    /// A polygon is coverage-valid if:
    /// <list type="number">
    /// <item><description>The polygon interior does not intersect the interior of other polygons.</description></item>
    /// <item><description>If the polygon boundary intersects another polygon boundary, the vertices
    /// and line segments of the intersection match exactly.</description></item>
    /// </list>
    /// <para/>
    /// The algorithm detects the following coverage errors:
    /// <list type="number">
    /// <item><description>the polygon is a duplicate of another one</description></item>
    /// <item><description>a polygon boundary segment equals an adjacent segment (with same orientation).
    /// This determines that the polygons overlap</description></item>
    /// <item><description>a polygon boundary segment is collinear and overlaps an adjacent segment
    /// but is not equal to it
    /// </description></item>
    /// <item><description>a polygon boundary segment touches an adjacent segment at a non-vertex point</description></item>
    /// <item><description>a polygon boundary segment crosses into an adjacent polygon</description></item>
    /// <item><description>a polygon boundary segment is in the interior of an adjacent polygon</description></item>
    /// </list>
    /// <para/>
    ///
    /// If any of these errors is present, the target polygon
    /// does not form a valid coverage with the adjacent polygons.
    /// <para/>
    /// The validity rules do not preclude properly noded gaps between coverage polygons.
    /// However, this class can detect narrow gaps,
    /// by specifying a maximum gap width using {@link #setGapWidth(double)}.
    /// Note that this will also identify narrow gaps separating disjoint coverage regions,
    /// and narrow gores.
    /// In some situations it may also produce false positives
    /// (i.e.linework identified as part of a gap which is wider than the given width).
    /// To fully identify gaps it maybe necessary to use <see cref="CoverageUnion"/> and analyze
    /// the holes in the result to see if they are acceptable.
    /// <para/>
    /// A polygon may be coverage-valid with respect to
    /// a set of surrounding polygons, but the collection as a whole may not
    /// form a clean coverage.For example, the target polygon boundary may be fully matched
    /// by adjacent boundary segments, but the adjacent set contains polygons
    /// which are not coverage - valid relative to other ones in the set.
    /// A coverage is valid only if every polygon in the coverage is coverage - valid.
    /// Use <see cref="CoverageValidator"/> to validate an entire set of polygons.
    /// <para/>
    /// The adjacent set may contain polygons which do not intersect the target polygon.
    /// These are effectively ignored during validation (but may decrease performance).
    /// </summary>
    /// <seealso cref="CoverageValidator"/>
    /// <author>Martin Davis</author>
    public class CoveragePolygonValidator
    {
        /// <summary>
        /// Validates that a polygon is coverage-valid  against the
        /// surrounding polygons in a polygonal coverage.
        /// </summary>
        /// <param name="targetPolygon">The polygon to validate</param>
        /// <param name="adjPolygons">The adjacent polygons</param>
        /// <returns>A linear geometry containing the segments causing invalidity (if any)</returns>
        public static Geometry Validate(Geometry targetPolygon, Geometry[] adjPolygons)
        {
            var v = new CoveragePolygonValidator(targetPolygon, adjPolygons);
            return v.Validate();
        }

        /// <summary>
        /// Validates that a polygon is coverage-valid against the
        /// surrounding polygons in a polygonal coverage,
        /// and forms no gaps narrower than a specified width.
        /// <para/>
        /// The set of surrounding polygons should include all polygons which
        /// are within the gap width distance of the target polygon.
        /// </summary>
        /// <param name="targetPolygon">The polygon to validate</param>
        /// <param name="adjPolygons">The adjacent polygons</param>
        /// <param name="gapWidth">The maximum width of invalid gaps</param>
        /// <returns>A linear geometry containing the segments causing invalidity (if any)/// </returns>
        public static Geometry Validate(Geometry targetPolygon, Geometry[] adjPolygons, double gapWidth)
        {
            var v = new CoveragePolygonValidator(targetPolygon, adjPolygons);
            v.GapWidth = gapWidth;
            return v.Validate();
        }

        private readonly Geometry _targetGeom;
        private double _gapWidth = 0.0;
        private GeometryFactory _geomFactory;
        private readonly Geometry[] _adjGeoms;
        private List<Polygon> _adjPolygons;
        private IndexedPointInAreaLocator[] _adjPolygonLocators;

        /// <summary>
        /// Create a new validator.
        /// <para/>
        /// If the gap width is specified, the set of surrounding polygons
        /// should include all polygons which
        /// are within the gap width distance of the target polygon.
        /// </summary>
        public CoveragePolygonValidator(Geometry geom, Geometry[] adjGeoms)
        {
            _targetGeom = geom;
            _adjGeoms = adjGeoms;
            _geomFactory = _targetGeom.Factory;
        }

        /// <summary>
        /// Gets or sets the maximum gap width, if narrow gaps are to be detected.
        /// </summary>
        public double GapWidth { get => _gapWidth; set { _gapWidth = value; } }

        /// <summary>
        /// Validates the coverage polygon against the set of adjacent polygons
        /// in the coverage.
        /// </summary>
        /// <returns>A linear geometry containing the segments causing invalidity (if any)</returns>
        public Geometry Validate()
        {
            _adjPolygons = ExtractPolygons(_adjGeoms);
            _adjPolygonLocators = new IndexedPointInAreaLocator[_adjPolygons.Count];

            var targetRings = CoverageRing.CreateRings(_targetGeom);
            var adjRings = CoverageRing.CreateRings(_adjPolygons);

            /*
             * Mark matching segments as valid first.
             * Matched segments are not considered for further checks. 
             * This improves performance substantially for mostly-valid coverages.
             */
            var targetEnv = _targetGeom.EnvelopeInternal.Copy();
            targetEnv.ExpandBy(_gapWidth);

            CheckTargetRings(targetRings, adjRings, targetEnv);

            return CreateInvalidLines(targetRings);
        }

        private void CheckTargetRings(List<CoverageRing> targetRings, List<CoverageRing> adjRings, Envelope targetEnv)
        {
            MarkMatchedSegments(targetRings, adjRings, targetEnv);

            /**
             * Short-circuit if target is fully known (matched or invalid).
             * This often happens in clean coverages,
             * when the target is surrounded by matching polygons.
             * It can also happen in invalid coverages 
             * which have polygons which are duplicates, 
             * or perfectly overlap other polygons.
             * 
             */
            if (CoverageRing.AllRingsKnown(targetRings))
                return;

            /**
             * Here target has at least one unmatched segment.
             * Do further checks to see if any of them are are invalid.
             */
            MarkInvalidInteractingSegments(targetRings, adjRings, _gapWidth);
            MarkInvalidInteriorSegments(targetRings, _adjPolygons);
        }

        private static List<Polygon> ExtractPolygons(Geometry[] geoms)
        {
            var polygons = new List<Polygon>();
            foreach (var geom in geoms)
            {
                Extracter.GetPolygons(geom, polygons);
            }
            return polygons;
        }

        private Geometry CreateEmptyResult()
        {
            return _geomFactory.CreateLineString();
        }

        /// <summary>
        /// Marks matched segments.
        /// This improves the efficiency of validity testing, since in valid coverages
        /// all segments (except exterior ones) are matched,
        /// and hence do not need to be tested further.
        /// Segments which are equal and have same orientation
        /// are detected and marked invalid.
        /// In fact, the entire target polygon may be matched and valid,
        /// which allows avoiding further tests.
        /// Segments matched between adjacent polygons are also marked valid,
        /// since this prevents them from being detected as misaligned,
        /// if this is being done.
        /// </summary>
        /// <param name="targetRings">The target rings</param>
        /// <param name="adjRngs">The adjacent rings</param>
        /// <param name="targetEnv">The tolerance envelope of the target</param>
        private void MarkMatchedSegments(List<CoverageRing> targetRings,
            List<CoverageRing> adjRngs, Envelope targetEnv)
        {
            var segmentMap = new Dictionary<CoverageRingSegment, CoverageRingSegment>();
            MarkMatchedSegments(targetRings, targetEnv, segmentMap);
            MarkMatchedSegments(adjRngs, targetEnv, segmentMap);
        }

        /// <summary>
        /// Adds ring segments to the segment map,
        /// and detects if they match an existing segment.
        /// Matched segments are marked.
        /// </summary>
        private void MarkMatchedSegments(IList<CoverageRing> rings, Envelope envLimit,
            IDictionary<CoverageRingSegment, CoverageRingSegment> segmentMap)
        {
            foreach (var ring in rings)
            {
                for (int i = 0; i < ring.Count - 1; i++)
                {
                    var p0 = ring.Coordinates[i];
                    var p1 = ring.Coordinates[i + 1];

                    //-- skip segments which lie outside the limit envelope
                    if (!envLimit.Intersects(p0, p1))
                    {
                        continue;
                    }
                    //-- if segment keys match, mark them as matched (or invalid)
                    var seg = CoverageRingSegment.Create(ring, i);
                    if (segmentMap.TryGetValue(seg, out var segMatch))
                    {
                        /*
                         * Since inputs should be valid, 
                         * the segments are assumed to be in different rings.
                         */
                        seg.Match(segMatch);
                    }
                    else
                    {
                        //-- store the segment as key and value, to allow retrieving when matched
                        segmentMap[seg] = seg;
                    }
                }
            }
        }

        /// <summary>
        ///  Models a segment in a CoverageRing.
        ///  The segment is normalized so it can be compared with segments
        ///  in any orientation.
        ///  Records valid matching segments in a coverage,
        ///  which must have opposite orientations.
        ///  Also detects equal segments with identical
        ///  orientation, and marks them as coverage-invalid.
        /// </summary>
        private class CoverageRingSegment : LineSegment
        {
            public static CoverageRingSegment Create(CoverageRing ring, int index)
            {
                var p0 = ring.Coordinates[index];
                var p1 = ring.Coordinates[index + 1];
                //-- orient segment as if ring is in canonical orientation
                if (ring.IsInteriorOnRight)
                {
                    return new CoverageRingSegment(p0, p1, ring, index);
                }
                else
                {
                    return new CoverageRingSegment(p1, p0, ring, index);
                }
            }

            private CoverageRing _ringForward;
            private int _indexForward = -1;
            private CoverageRing _ringOpp = null;
            private int _indexOpp = -1;

            private CoverageRingSegment(Coordinate p0, Coordinate p1, CoverageRing ring, int index)
                    : base(p0, p1)
            {
                if (p1.CompareTo(p0) < 0)
                {
                    Reverse();
                    _ringOpp = ring;
                    _indexOpp = index;
                }
                else
                {
                    _ringForward = ring;
                    _indexForward = index;
                }
            }


            public void Match(CoverageRingSegment seg)
            {
                bool isInvalid = CheckInvalid(seg);
                if (isInvalid)
                {
                    return;
                }
                //-- record the match
                if (_ringForward == null)
                {
                    _ringForward = seg._ringForward;
                    _indexForward = seg._indexForward;
                }
                else
                {
                    _ringOpp = seg._ringOpp;
                    _indexOpp = seg._indexOpp;
                }
                //-- mark ring segments as matched
                _ringForward.MarkMatched(_indexForward);
                _ringOpp.MarkMatched(_indexOpp);
            }

            private bool CheckInvalid(CoverageRingSegment seg)
            {
                if (_ringForward != null && seg._ringForward != null)
                {
                    _ringForward.MarkInvalid(_indexForward);
                    seg._ringForward.MarkInvalid(seg._indexForward);
                    return true;
                }
                if (_ringOpp != null && seg._ringOpp != null)
                {
                    _ringOpp.MarkInvalid(_indexOpp);
                    seg._ringOpp.MarkInvalid(seg._indexOpp);
                    return true;
                }
                return false;
            }
        }

        //--------------------------------------------------

        /// <summary>
        /// Marks invalid target segments which cross an adjacent ring segment,
        /// lie partially in the interior of an adjacent ring,
        /// or are nearly collinear with an adjacent ring segment up to the distance tolerance
        /// </summary>
        /// <param name="targetRings">The rings with segments to test</param>
        /// <param name="adjRings">The adjacent rings</param>
        /// <param name="distanceTolerance">The gap distance tolerance, if any</param>
        private void MarkInvalidInteractingSegments(List<CoverageRing> targetRings, List<CoverageRing> adjRings,
            double distanceTolerance)
        {
            var detector = new InvalidSegmentDetector(distanceTolerance);
            var segSetMutInt = new MCIndexSegmentSetMutualIntersector(targetRings, distanceTolerance);
            segSetMutInt.Process(adjRings, detector);
        }

        /// <summary>
        /// Marks invalid target segments which are fully interior
        /// to an adjacent polygon.
        /// </summary>
        /// <param name="targetRings">The rings with segments to test</param>
        /// <param name="adjPolygons">The adjacent polygons</param>
        private void MarkInvalidInteriorSegments(List<CoverageRing> targetRings, List<Polygon> adjPolygons)
        {
            foreach (var ring in targetRings)
            {
                for (int i = 0; i < ring.Count - 1; i++)
                {
                    //-- skip check for segments with known state. 
                    if (ring.IsKnownAt(i))
                        continue;

                    /*
                     * Check if vertex is in interior of an adjacent polygon.
                     * If so, the segments on either side are in the interior.
                     * Mark them invalid, unless they are already matched.
                     */
                    var p = ring.Coordinates[i];
                    if (IsInteriorVertex(p, adjPolygons))
                    {
                        ring.MarkInvalid(i);
                        //-- previous segment may be interior (but may also be matched)
                        int iPrev = i == 0 ? ring.Count - 2 : i - 1;
                        if (!ring.IsKnownAt(iPrev))
                            ring.MarkInvalid(iPrev);
                    }
                }
            }
        }

        /// <summary>
        /// Tests if a coordinate is in the interior of some adjacent polygon.
        /// Uses the cached Point-In-Polygon indexed locators, for performance.
        /// </summary>
        /// <param name="p">The coordinate to test</param>
        /// <param name="adjPolygons">A list of polygons</param>
        /// <returns><c>true</c> if the point is in the interior</returns>
        private bool IsInteriorVertex(Coordinate p, IList<Polygon> adjPolygons)
        {
            /*
             * There should not be too many adjacent polygons, 
             * and hopefully not too many segments with unknown status
             * so a linear scan should not be too inefficient
             */
            //TODO: try a spatial index?
            for (int i = 0; i < adjPolygons.Count; i++)
            {
                var adjPoly = adjPolygons[i];

                if (PolygonContainsPoint(i, adjPoly, p))
                    return true;
            }
            return false;
        }

        private bool PolygonContainsPoint(int index, Polygon poly, Coordinate pt)
        {
            if (!poly.EnvelopeInternal.Intersects(pt))
                return false;

            var pia = GetLocator(index, poly);
            return Location.Interior == pia.Locate(pt);
        }

        private IPointOnGeometryLocator GetLocator(int index, Polygon poly)
        {
            var loc = _adjPolygonLocators[index];
            if (loc == null)
            {
                loc = new IndexedPointInAreaLocator(poly);
                _adjPolygonLocators[index] = loc;
            }
            return loc;
        }

        private Geometry CreateInvalidLines(List<CoverageRing> rings)
        {
            var lines = new List<LineString>();
            foreach (var ring in rings)
            {
                ring.CreateInvalidLines(_geomFactory, lines);
            }

            if (lines.Count == 0)
            {
                return CreateEmptyResult();
            }
            else if (lines.Count == 1)
            {
                return lines[0];
            }
            return _geomFactory.CreateMultiLineString(lines.ToArray());
        }

    }
}
