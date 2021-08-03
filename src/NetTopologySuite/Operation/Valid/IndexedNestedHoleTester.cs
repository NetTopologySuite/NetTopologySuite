using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any holes of a Polygon are
    /// nested inside another hole, using a spatial
    /// index to speed up the comparisons.
    /// <para/>
    /// The logic assumes that the holes do not overlap and have no collinear segments
    /// (so they are properly nested, and there are no duplicate holes).
    /// <para/>
    /// The situation where every vertex of a hole touches another hole
    /// is invalid because either the hole is nested,
    /// or else it disconnects the polygon interior.
    /// This class detects the nested situation.
    /// The disconnected interior situation must be checked elsewhere.
    /// </summary>
    class IndexedNestedHoleTester
    {
        private readonly Polygon _polygon;
        private ISpatialIndex<LinearRing> _index;
        private Coordinate _nestedPt;

        public IndexedNestedHoleTester(Polygon poly)
        {
            _polygon = poly;
            LoadIndex();
        }

        private void LoadIndex()
        {
            _index = new STRtree<LinearRing>();

            for (int i = 0; i < _polygon.NumInteriorRings; i++)
            {
                var hole = (LinearRing)_polygon.GetInteriorRingN(i);
                var env = hole.EnvelopeInternal;
                _index.Insert(env, hole);
            }
        }

        /// <summary>
        /// Gets a value indicating a point on a nested hole, if one exists.
        /// </summary>
        /// <returns>A point on a nested hole, or <c>null</c> if none are nested</returns>
        public Coordinate NestedPoint { get => _nestedPt; }

        /// <summary>
        /// Tests if any hole is nested (contained) within another hole.
        /// <b>This is invalid</b>.
        /// The <see cref="NestedPoint"/> will be set to reflect this.
        /// </summary>
        /// <returns><c>true</c> if some hole is nested.</returns>
        public bool IsNested()
        {
            for (int i = 0; i < _polygon.NumInteriorRings; i++)
            {
                var hole = (LinearRing)_polygon.GetInteriorRingN(i);

                var results = _index.Query(hole.EnvelopeInternal);
                foreach (var testHole in results)
                {
                    if (hole == testHole)
                        continue;

                    /*
                     * Hole is not fully covered by in test hole, so cannot be nested
                     */
                    if (!testHole.EnvelopeInternal.Covers(hole.EnvelopeInternal))
                        continue;

                    /*
                     * Checks nesting via a point-in-polygon test, 
                     * or if the point lies on the boundary via 
                     * the topology of the incident edges.
                     */
                    var holePt0 = hole.GetCoordinateN(0);
                    var holePt1 = hole.GetCoordinateN(1);
                    if (PolygonTopologyAnalyzer.IsSegmentInRing(holePt0, holePt1, testHole))
                    {
                        _nestedPt = holePt0;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
