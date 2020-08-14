using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// A base class containing the logic for computes the <i>contains</i>
    /// and <i>covers</i> spatial relationship predicates
    /// for a <see cref="PreparedPolygon"/> relative to all other <see cref="Geometry"/> classes.
    /// Uses short-circuit tests and indexing to improve performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contains and covers are very similar, and differ only in how certain
    /// cases along the boundary are handled.  These cases require
    /// full topological evaluation to handle, so all the code in
    /// this class is common to both predicates.
    /// </para>
    /// <para>
    /// It is not possible to short-circuit in all cases, in particular
    /// in the case where line segments of the test geometry touches the polygon linework.
    /// In this case full topology must be computed.
    /// (However, if the test geometry consists of only points, this
    /// <i>can</i> be evaluated in an optimized fashion.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    internal abstract class AbstractPreparedPolygonContains : PreparedPolygonPredicate
    {
        /// <summary>
        /// This flag controls a difference between contains and covers.
        /// For contains the value is true.
        /// For covers the value is false.
        /// </summary>
        protected bool RequireSomePointInInterior = true;

        // information about geometric situation
        private bool _hasSegmentIntersection;
        private bool _hasProperIntersection;
        private bool _hasNonProperIntersection;

        /// <summary>
        /// Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">The PreparedPolygon to evaluate</param>
        protected AbstractPreparedPolygonContains(PreparedPolygon prepPoly)
            : base(prepPoly)
        {
        }

        /// <summary>
        /// Evaluate the <i>contains</i> or <i>covers</i> relationship
        /// for the given geometry.
        /// </summary>
        /// <param name="geom">the test geometry</param>
        /// <returns>true if the test geometry is contained</returns>
        protected bool Eval(Geometry geom)
        {
            if (geom.Dimension == Dimension.Point)
            {
                return EvalPoints(geom);
            }

            /*
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick negative result.
             *
             * If a point of any test components does not lie in target, result is false
             */
            bool isAllInTargetArea = IsAllTestComponentsInTarget(geom);
            if (!isAllInTargetArea) return false;

            /*
             * Check if there is any intersection between the line segments
             * in target and test.
             * In some important cases, finding a proper intersection implies that the
             * test geometry is NOT contained.
             * These cases are:
             * <ul>
             * <li>If the test geometry is polygonal
             * <li>If the target geometry is a single polygon with no holes
             * <ul>
             * In both of these cases, a proper intersection implies that there
             * is some portion of the interior of the test geometry lying outside
             * the target, which means that the test is not contained.
             */
            bool properIntersectionImpliesNotContained = IsProperIntersectionImpliesNotContainedSituation(geom);
            // MD - testing only
            // properIntersectionImpliesNotContained = true;

            // find all intersection types which exist
            FindAndClassifyIntersections(geom);

            if (properIntersectionImpliesNotContained && _hasProperIntersection)
                return false;

            /*
             * If all intersections are proper
             * (i.e. no non-proper intersections occur)
             * we can conclude that the test geometry is not contained in the target area,
             * by the Epsilon-Neighbourhood Exterior Intersection condition.
             * In real-world data this is likely to be by far the most common situation,
             * since natural data is unlikely to have many exact vertex segment intersections.
             * Thus this check is very worthwhile, since it avoid having to perform
             * a full topological check.
             *
             * (If non-proper (vertex) intersections ARE found, this may indicate
             * a situation where two shells touch at a single vertex, which admits
             * the case where a line could cross between the shells and still be wholely contained in them.
             */
            if (_hasSegmentIntersection && !_hasNonProperIntersection)
                return false;

            /*
             * If there is a segment intersection and the situation is not one
             * of the ones above, the only choice is to compute the full topological
             * relationship.  This is because contains/covers is very sensitive
             * to the situation along the boundary of the target.
             */
            if (_hasSegmentIntersection)
            {
                return FullTopologicalPredicate(geom);
                // System.out.println(geom);
            }

            /*
             * This tests for the case where a ring of the target lies inside
             * a test polygon - which implies the exterior of the Target
             * intersects the interior of the Test, and hence the result is false
             */
            if (geom is IPolygonal)
            {
                // TODO: generalize this to handle GeometryCollections
                bool isTargetInTestArea = IsAnyTargetComponentInAreaTest(geom, prepPoly.RepresentativePoints);
                if (isTargetInTestArea) return false;
            }
            return true;
        }

        /// <summary>
        /// Evaluation optimized for Point geometries.
        /// This provides about a 2x performance increase, and less memory usage.
        /// </summary>
        /// <param name="geom">A Point or MultiPoint geometry</param>
        /// <returns>The value of the predicate being evaluated</returns>
        private bool EvalPoints(Geometry geom)
        {
            /*
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick negative result.
             * 
             * If a point of any test components does not lie in target, result is false
             */
            bool isAllInTargetArea = IsAllTestPointsInTarget(geom);
            if (!isAllInTargetArea) return false;

            /*
             * If the test geometry consists of only Points, 
             * then it is now sufficient to test if any of those
             * points lie in the interior of the target geometry.
             * If so, the test is contained.
             * If not, all points are on the boundary of the area,
             * which implies not contained.
             */
            if (RequireSomePointInInterior)
            {
                bool isAnyInTargetInterior = IsAnyTestPointInTargetInterior(geom);
                return isAnyInTargetInterior;
            }
            return true;
        }


        private bool IsProperIntersectionImpliesNotContainedSituation(Geometry testGeom)
        {
            /*
             * If the test geometry is polygonal we have the A/A situation.
             * In this case, a proper intersection indicates that
             * the Epsilon-Neighbourhood Exterior Intersection condition exists.
             * This condition means that in some small
             * area around the intersection point, there must exist a situation
             * where the interior of the test intersects the exterior of the target.
             * This implies the test is NOT contained in the target.
             */
            if (testGeom is IPolygonal) return true;
            /*
             * A single shell with no holes allows concluding that
             * a proper intersection implies not contained
             * (due to the Epsilon-Neighbourhood Exterior Intersection condition)
             */
            if (IsSingleShell(prepPoly.Geometry)) return true;
            return false;
        }

        /// <summary>
        /// Tests whether a geometry consists of a single polygon with no holes.
        /// </summary>
        /// <returns>True if the geometry is a single polygon with no holes</returns>
        private static bool IsSingleShell(Geometry geom)
        {
            // handles single-element MultiPolygons, as well as Polygons
            if (geom.NumGeometries != 1) return false;

            var poly = (Polygon)geom.GetGeometryN(0);
            int numHoles = poly.NumInteriorRings;
            if (numHoles == 0) return true;
            return false;
        }

        private void FindAndClassifyIntersections(Geometry geom)
        {
            var lineSegStr = SegmentStringUtil.ExtractSegmentStrings(geom);

            var intDetector = new SegmentIntersectionDetector();
            intDetector.FindAllIntersectionTypes = true;
            prepPoly.IntersectionFinder.Intersects(lineSegStr, intDetector);

            _hasSegmentIntersection = intDetector.HasIntersection;
            _hasProperIntersection = intDetector.HasProperIntersection;
            _hasNonProperIntersection = intDetector.HasNonProperIntersection;
        }

        /// <summary>
        /// Computes the full topological predicate.
        /// Used when short-circuit tests are not conclusive.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>true if this prepared polygon has the relationship with the test geometry</returns>
        protected abstract bool FullTopologicalPredicate(Geometry geom);
    }
}
