using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// Computes the <c>containsProperly</c> spatial relationship predicate for <see cref="PreparedPolygon" />s relative to all other {@link Geometry} classes.<br/>
    /// Uses short-circuit tests and indexing to improve performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Geometry A <c>containsProperly</c> another Geometry B if
    /// all points of B are contained in the Interior of A.
    /// Equivalently, B is contained in A AND B does not intersect
    /// the Boundary of A.
    /// </para>
    /// <para>
    /// The advantage to using this predicate is that it can be computed
    /// efficiently, with no need to compute topology at individual points.
    /// In a situation with many geometries intersecting the boundary
    /// of the target geometry, this can make a performance difference.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    internal class PreparedPolygonContainsProperly : PreparedPolygonPredicate
    {
        /// <summary>Computes the <c>containsProperly</c> predicate between a <see cref="PreparedPolygon"/> and a <see cref="Geometry"/>.
        /// </summary>
        /// <param name="prep">The prepared polygon</param>
        /// <param name="geom">A test geometry</param>
        /// <returns>true if the polygon properly contains the geometry</returns>
        public static bool ContainsProperly(PreparedPolygon prep, Geometry geom)
        {
            var polyInt = new PreparedPolygonContainsProperly(prep);
            return polyInt.ContainsProperly(geom);
        }

        /// <summary>
        /// Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">The PreparedPolygon to evaluate</param>
        public PreparedPolygonContainsProperly(PreparedPolygon prepPoly)
            : base(prepPoly)
        {
        }

        /// <summary>
        /// Tests whether this PreparedPolygon containsProperly a given geometry.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>true if the polygon properly contains the geometry</returns>
        public bool ContainsProperly(Geometry geom)
        {
            /*
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick negative result.
             *
             * If a point of any test components does not lie in the target interior, result is false
             */
            bool isAllInPrepGeomAreaInterior = IsAllTestComponentsInTargetInterior(geom);
            if (!isAllInPrepGeomAreaInterior) return false;

            /*
             * If any segments intersect, result is false.
             */
            var lineSegStr = SegmentStringUtil.ExtractSegmentStrings(geom);
            bool segsIntersect = prepPoly.IntersectionFinder.Intersects(lineSegStr);
            if (segsIntersect)
                return false;

            /*
             * Given that no segments intersect, if any vertex of the target
             * is contained in some test component.
             * the test is NOT properly contained.
             */
            if (geom is IPolygonal)
            {
                // TODO: generalize this to handle GeometryCollections
                bool isTargetGeomInTestArea = IsAnyTargetComponentInAreaTest(geom, prepPoly.RepresentativePoints);
                if (isTargetGeomInTestArea) return false;
            }

            return true;
        }
    }
}
