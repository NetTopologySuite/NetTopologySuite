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
    /// Assumes that the holes and polygon shell do not cross
    /// (are properly nested).
    /// Does not check the case where every vertex of a hole touches another
    /// hole; this is invalid, and must be checked elsewhere. 
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

        public Coordinate NestedPoint { get => _nestedPt; }

        public bool IsNested()
        {
            for (int i = 0; i < _polygon.NumInteriorRings; i++)
            {
                var hole = (LinearRing)_polygon.GetInteriorRingN(i);

                var results = _index.Query(hole.EnvelopeInternal);
                for (int j = 0; j < results.Count; j++)
                {
                    var testHole = results[j];
                    if (hole == testHole)
                        continue;

                    /*
                     * Hole is not covered by in test hole,
                     * so cannot be inside
                     */
                    if (!testHole.EnvelopeInternal.Covers(hole.EnvelopeInternal))
                        continue;

                    if (IsHoleInsideHole(hole, testHole))
                        return true;
                }
            }
            return false;
        }

        private bool IsHoleInsideHole(LinearRing hole, LinearRing testHole)
        {
            var testPts = testHole.CoordinateSequence;
            for (int i = 0; i < hole.NumPoints; i++)
            {
                var holePt = hole.GetCoordinateN(i);
                var loc = PointLocation.LocateInRing(holePt, testPts);
                switch (loc)
                {
                    case Location.Exterior: return false;
                    case Location.Interior:
                        _nestedPt = holePt;
                        return true;
                }
                // location is BOUNDARY, so keep trying points
            }
            return false;
        }


    }
}
