using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether a MultiPolygon has any element polygon
    /// nested inside another polygon, using a spatial
    /// index to speed up the comparisons.
    /// <para/>
    /// The logic assumes that the polygons do not overlap and have no collinear segments
    /// (so they are properly nested, and there are no duplicate rings).
    /// </summary>
    class IndexedNestedPolygonTester
    {
        private readonly MultiPolygon _multiPoly;
        private readonly ISpatialIndex<int> _index;
        private readonly IndexedPointInAreaLocator[] _locators;
        private Coordinate _nestedPt;

        public IndexedNestedPolygonTester(MultiPolygon multiPoly)
        {
            _multiPoly = multiPoly;
            _index = LoadIndex();
            _locators = new IndexedPointInAreaLocator[_multiPoly.NumGeometries];
        }

        private STRtree<int> LoadIndex()
        {
            var index = new STRtree<int>();

            for (int i = 0; i < _multiPoly.NumGeometries; i++)
            {
                var poly = (Polygon)_multiPoly.GetGeometryN(i);
                var env = poly.EnvelopeInternal;
                index.Insert(env, i);
            }

            return index;
        }

        private IndexedPointInAreaLocator GetLocator(int polyIndex)
        {
            var locator = _locators[polyIndex];
            if (locator == null)
            {
                locator = new IndexedPointInAreaLocator(_multiPoly.GetGeometryN(polyIndex));
                _locators[polyIndex] = locator;
            }
            return locator;
        }

        /// <summary>
        /// Gets a point on a nested polygon, if one exists.
        /// </summary>
        /// <returns>A point on a nested polygon, or null if none are nested</returns>
        public Coordinate NestedPoint => _nestedPt;

        /// <summary>
        /// Tests if any polygon is nested (contained) within another polygon.
        /// <b>This is invalid.</b>
        /// The nested point will be set to reflect this.
        /// </summary>
        /// <returns><c>true</c> if some polygon is nested</returns>
        public bool IsNested()
        {
            for (int i = 0; i < _multiPoly.NumGeometries; i++)
            {
                var poly = (Polygon)_multiPoly.GetGeometryN(i);
                var shell = poly.ExteriorRing;

                var results = _index.Query(poly.EnvelopeInternal);
                foreach (int polyIndex in results)
                {
                    var possibleOuterPoly = (Polygon)_multiPoly.GetGeometryN(polyIndex);

                    if (poly == possibleOuterPoly)
                        continue;
                    /*
                     * If polygon is not fully covered by candidate polygon it cannot be nested
                     */
                    if (!possibleOuterPoly.EnvelopeInternal.Covers(poly.EnvelopeInternal))
                        continue;

                    _nestedPt = FindNestedPoint(shell, possibleOuterPoly, GetLocator(polyIndex));
                    if (_nestedPt != null)
                        return true;
                }
            }
            return false;
        }

        private Coordinate FindNestedPoint(LineString shell,
            Polygon possibleOuterPoly, IndexedPointInAreaLocator locator)
        {
            /*
             * Try checking two points, since checking point location is fast.
             */
            var shellPt0 = shell.GetCoordinateN(0);
            var loc0 = locator.Locate(shellPt0);
            if (loc0 == Location.Exterior) return null;
            if (loc0 == Location.Interior)
            {
                return shellPt0;
            }

            var shellPt1 = shell.GetCoordinateN(0);
            var loc1 = locator.Locate(shellPt1);
            if (loc1 == Location.Exterior) return null;
            if (loc1 == Location.Interior)
            {
                return shellPt1;
            }

            /*
             * The shell points both lie on the boundary of
             * the polygon.
             * Nesting can be checked via the topology of the incident edges.
             */
            return FindSegmentInPolygon(shell, possibleOuterPoly);
        }

        /// <summary>
        /// Finds a point of a shell segment which lies inside a polygon, if any.
        /// The shell is assume to touch the polyon only at shell vertices,
        /// and does not cross the polygon.
        /// </summary>
        /// <param name="shell">The shell to test</param>
        /// <param name="poly">The polygon to test against</param>
        /// <returns>An interior segment point, or <c>null</c> if the shell is nested correctly</returns>
        private static Coordinate FindSegmentInPolygon(LineString shell, Polygon poly)
        {
            var polyShell = poly.ExteriorRing;
            if (polyShell.IsEmpty) return null;

            var shell0 = shell.GetCoordinateN(0);
            var shell1 = shell.GetCoordinateN(1);

            if (!PolygonTopologyAnalyzer.IsSegmentInRing(shell0, shell1, polyShell))
                return null;

            /*
             * Check if the shell is inside a hole (if there are any). 
             * If so this is valid.
             */
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = poly.GetInteriorRingN(i);
                if (hole.EnvelopeInternal.Covers(shell.EnvelopeInternal)
                    && PolygonTopologyAnalyzer.IsSegmentInRing(shell0, shell1, hole))
                {
                    return null;
                }
            }

            /*
             * The shell is contained in the polygon, but is not contained in a hole.
             * This is invalid.
             */
            return shell0;
        }
    }

}
