using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// Computes the <i>intersects</i> spatial relationship predicate
    /// for <see cref="PreparedPolygon"/>s relative to all other <see cref="Geometry"/> classes.
    /// </summary>
    /// <remarks>Uses short-circuit tests and indexing to improve performance.</remarks>
    /// <author>Martin Davis</author>
    internal class PreparedPolygonIntersects : PreparedPolygonPredicate
    {
        /// <summary>
        /// Computes the intersects predicate between a <see cref="PreparedPolygon"/>
        /// and a <see cref="Geometry"/>.
        /// </summary>
        /// <param name="prep">The prepared polygon</param>
        /// <param name="geom">A test geometry</param>
        /// <returns>true if the polygon intersects the geometry</returns>
        public static bool Intersects(PreparedPolygon prep, Geometry geom)
        {
            var polyInt = new PreparedPolygonIntersects(prep);
            return polyInt.Intersects(geom);
        }

        /// <summary>
        /// Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">The prepared polygon</param>
        public PreparedPolygonIntersects(PreparedPolygon prepPoly) :
            base(prepPoly) { }

        /// <summary>
        /// Tests whether this PreparedPolygon intersects a given geometry.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>true if the test geometry intersects</returns>
        public bool Intersects(Geometry geom)
        {
            /*
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick positive result.
             *
             * If a point of any test components lie in target, result is true
             */
            bool isInPrepGeomArea = IsAnyTestComponentInTarget(geom);
            if (isInPrepGeomArea)
                return true;
            /*
             * If input contains only points, then at
             * this point it is known that none of them are contained in the target
             */
            if (geom.Dimension == Dimension.Point)
                return false;
            /*
             * If any segments intersect, result is true
             */
            var lineSegStr = SegmentStringUtil.ExtractSegmentStrings(geom);
            // only request intersection finder if there are segments (ie NOT for point inputs)
            if (lineSegStr.Count > 0)
            {
                bool segsIntersect = prepPoly.IntersectionFinder.Intersects(lineSegStr);
                if (segsIntersect)
                    return true;
            }

            /*
             * If the test has dimension = 2 as well, it is necessary to
             * test for proper inclusion of the target.
             * Since no segments intersect, it is sufficient to test representative points.
             */
            if (geom.Dimension == Dimension.Surface)
            {
                // TODO: generalize this to handle GeometryCollections
                bool isPrepGeomInArea = IsAnyTargetComponentInAreaTest(geom, prepPoly.RepresentativePoints);
                if (isPrepGeomInArea) return true;
            }

            return false;
        }
    }
}
