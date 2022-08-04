using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Coverage
{

    /// <summary>
    /// Validates that a polygon forms a valid polygonal coverage
    /// with the set of polygons surrounding it.
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
    /// <item><description>a polygon boundary segment is collinear with an adjacent segment but not equal to it</description></item>
    /// <item><description>a polygon boundary segment touches an adjacent segment at a non-vertex point</description></item>
    /// <item><description>a polygon boundary segment crosses into an adjacent polygon</description></item>
    /// <item><description>a polygon boundary segment is in the interior of an adjacent polygon</description></item>
    /// </list>
    /// <para/>
    ///
    /// If any of these errors is present, the target polygon
    /// does not form a valid coverage with the adjacent polygons.
    /// <para/>
    /// The validity rules does not preclude gaps between coverage polygons.
    /// However, this class can detect narrow gaps,
    /// by specifying a maximum gap width using {@link #setGapWidth(double)}.
    /// Note that this will also identify narrow gaps separating disjoint coverage regions,
    /// and narrow gores.
    /// In some situations it may also produce false positives
    /// (i.e.linework identified as part of a gap which is wider than the given width).
    /// <para/>
    /// A polygon may be coverage-valid with respect to
    /// a set of surrounding polygons, but the collection as a whole may not
    /// form a clean coverage.For example, the target polygon boundary may be fully matched
    /// by adjacent boundary segments, but the adjacent set contains polygons
    /// which are not coverage - valid relative to other ones in the set.
    /// A coverage is valid only if every polygon in the coverage is coverage - valid.
    /// Use { @link CoverageValidator} to validate an entire set of polygons.
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
        private IndexedPointInAreaLocator[] _adjPolygonLocators;
        private readonly Geometry[] _adjGeoms;

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
            var adjPolygons = ExtractPolygons(_adjGeoms);
            _adjPolygonLocators = new IndexedPointInAreaLocator[adjPolygons.Count];

            if (HasDuplicateGeom(_targetGeom, adjPolygons))
            {
                //TODO: convert to LineString copies
                return _targetGeom.Boundary;
            }

            var targetRings = CoverageRing.CreateRings(_targetGeom);
            var adjRings = CoverageRing.CreateRings(adjPolygons);

            /*
             * Mark matching segments as valid first.
             * Valid segments are not considered for further checks. 
             * This improves performance substantially for mostly-valid coverages.
             */
            var targetEnv = _targetGeom.EnvelopeInternal.Copy();
            targetEnv.ExpandBy(_gapWidth);
            MarkMatchedSegments(targetRings, adjRings, targetEnv);

            //-- check if target is fully matched and thus forms a clean coverage 
            if (CoverageRing.AllRingsValid(targetRings))
                return CreateEmptyResult();

            FindInvalidInteractingSegments(targetRings, adjRings, _gapWidth);

            FindInteriorSegments(targetRings, adjPolygons);

            return CreateInvalidLines(targetRings);
        }

        private static IList<Polygon> ExtractPolygons(Geometry[] geoms)
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
        /// Check if adjacent geoms contains a duplicate of the target.
        /// This situation is not detected by segment alignment checking,
        /// since all segments are matches.
        /// </summary>
        private bool HasDuplicateGeom(Geometry geom, IList<Polygon> adjPolygons)
        {
            foreach (var adjPoly in adjPolygons)
            {
                if (adjPoly.EnvelopeInternal.Equals(geom.EnvelopeInternal))
                {
                    if (adjPoly.EqualsTopologically(geom))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Marks matched segments as valid.
        /// This improves the efficiency of validity testing, since in valid coverages
        /// all segments (except exterior ones) will be matched,
        /// and hence do not need to be tested further.
        /// In fact, the entire target polygon may be marked valid,
        /// which allows avoiding all further tests.
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

        /**
         * Adds ring segments to the segment map, 
         * and detects if they match an existing segment.
         * Matched segments are marked as coverage-valid.
         * 
         * @param rings
         * @param envLimit
         * @param segMap
         */
        private void MarkMatchedSegments(IList<CoverageRing> rings, Envelope envLimit,
            IDictionary<CoverageRingSegment, CoverageRingSegment> segmentMap)
        {
            foreach (var ring in rings)
            {
                for (int i = 0; i < ring.Count - 1; i++)
                {
                    var seg = CoverageRingSegment.Create(ring, i);
                    //-- skip segments which lie outside the limit envelope
                    if (!envLimit.Intersects(seg.P0, seg.P1))
                    {
                        continue;
                    }
                    //-- if segments match, mark them valid
                    if (segmentMap.TryGetValue(seg, out var segMatch))
                    {
                        segMatch.MarkValid();
                        seg.MarkValid();
                    }
                    else
                    {
                        segmentMap[seg] = seg;
                    }
                }
            }
        }

        private class CoverageRingSegment : LineSegment
        {
            public static CoverageRingSegment Create(CoverageRing ring, int index)
            {
                var p0 = ring.Coordinates[index];
                var p1 = ring.Coordinates[index + 1];
                return new CoverageRingSegment(p0, p1, ring, index);
            }

            private readonly CoverageRing _ring;
            private readonly int _index;

            private CoverageRingSegment(Coordinate p0, Coordinate p1, CoverageRing ring, int index)
                    : base(p0, p1)
            {
                Normalize();
                _ring = ring;
                _index = index;
            }

            public void MarkValid()
            {
                _ring.MarkValid(_index);
            }
        }

    //--------------------------------------------------


    private void FindInvalidInteractingSegments(List<CoverageRing> targetRings, List<CoverageRing> adjRings,
        double distanceTolerance)
    {
        var detector = new InvalidSegmentDetector(distanceTolerance);
        var segSetMutInt = new MCIndexSegmentSetMutualIntersector(targetRings, distanceTolerance);
        segSetMutInt.Process(adjRings, detector);
    }

    private void FindInteriorSegments(List<CoverageRing> targetRings, IList<Polygon> adjPolygons)
    {
        foreach (var ring in targetRings)
        {
            for (int i = 0; i < ring.Count - 1; i++)
            {
                //-- skip check for segments with known state. 
                if (ring.IsKnown(i))
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
                    if (!ring.IsKnown(iPrev))
                        ring.MarkInvalid(iPrev);
                }
            }
        }
    }

    /**
     * Tests if a coordinate is in the interior of some adjacent polygon.
     * Uses the cached Point-In-Polygon indexed locators, for performance.
     * 
     * @param p the coordinate to test
     * @param adjPolygons the list of polygons
     * @return true if the point is in the interior
     */
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
            if (!adjPoly.EnvelopeInternal.Intersects(p))
                continue;

            if (PolygonContainsPoint(i, adjPoly, p))
                return true;
        }
        return false;
    }

    private bool PolygonContainsPoint(int index, Polygon poly, Coordinate pt)
    {
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
