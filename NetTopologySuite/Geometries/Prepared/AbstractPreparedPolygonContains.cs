using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    public abstract class AbstractPreparedPolygonContains<TCoordinate> : PreparedPolygonPredicate<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /**
         * This flag controls a difference between contains and covers.
         * 
         * For contains the value is true.
         * For covers the value is false.
         */
        private Boolean HasNonProperIntersection;
        private Boolean HasProperIntersection;
        private Boolean HasSegmentIntersection;
        protected Boolean RequireSomePointInInterior = true;

        ///<summary>
        /// Creates an instance of this operation.
        ///</summary>
        ///<param name="prepPoly">the PreparedPolygon to evaluate</param>
        public AbstractPreparedPolygonContains(PreparedPolygon<TCoordinate> prepPoly)
            : base(prepPoly)
        {
        }

        ///<summary>
        /// Evaluate the <see cref="ISpatialRelation{TCoordinate}.Contains(GeoAPI.Geometries.IGeometry{TCoordinate})"/> or <see cref="ISpatialRelation.Covers(GeoAPI.Geometries.IGeometry)"/> relationship for a given geometry
        ///</summary>
        /// <param name="geom">the test geometry</param>
        /// <returns>true if the test geometry is contained/covered</returns>
        protected Boolean Eval(IGeometry<TCoordinate> geom)
        {
            /**
             * Do point-in-poly tests first, since they are cheaper and may result
             * in a quick negative result.
             * 
             * If a point of any test components does not lie in target, result is false
             */
            Boolean isAllInTargetArea = IsAllTestComponentsInTarget(geom);
            if (!isAllInTargetArea) return false;

            /**
             * If the test geometry consists of only Points, 
             * then it is now sufficient to test if any of those
             * points lie in the interior of the target geometry.
             * If so, the test is contained.
             * If not, all points are on the boundary of the area,
             * which implies not contained.
             */
            if (RequireSomePointInInterior
                && geom.Dimension == Dimensions.Point)
            {
                Boolean isAnyInTargetInterior = IsAnyTestComponentInTargetInterior(geom);
                return isAnyInTargetInterior;
            }

            /**
             * Check if there is any intersection between the line segments
             * in target and test.
             * In some important cases, finding a proper interesection implies that the 
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
            Boolean properIntersectionImpliesNotContained = IsProperIntersectionImpliesNotContainedSituation(geom);
            // MD - testing only
            //        properIntersectionImpliesNotContained = true;

            // find all intersection types which exist
            FindAndClassifyIntersections(geom);

            if (properIntersectionImpliesNotContained && HasProperIntersection)
                return false;

            /**
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
            if (HasSegmentIntersection && !HasNonProperIntersection)
                return false;

            /**
             * If there is a segment intersection and the situation is not one
             * of the ones above, the only choice is to compute the full topological
             * relationship.  This is because contains/covers is very sensitive 
             * to the situation along the boundary of the target.
             */
            if (HasSegmentIntersection)
            {
                return FullTopologicalPredicate(geom);
                //            System.out.println(geom);
            }

            /**
             * This tests for the case where a ring of the target lies inside
             * a test polygon - which implies the exterior of the Target
             * intersects the interior of the Test, and hence the result is false
             */
            if (geom is IPolygonal<TCoordinate>)
            {
                // TODO: generalize this to handle GeometryCollections
                Boolean isTargetInTestArea = IsAnyTargetComponentInAreaTest(geom, _prepPoly.RepresentativePoints);
                if (isTargetInTestArea) return false;
            }
            return true;
        }

        private Boolean IsProperIntersectionImpliesNotContainedSituation(IGeometry<TCoordinate> testGeom)
        {
            /**
             * If the test geometry is polygonal we have the A/A situation.
             * In this case, a proper intersection indicates that 
             * the Epsilon-Neighbourhood Exterior Intersection condition exists.
             * This condition means that in some small
             * area around the intersection point, there must exist a situation
             * where the interior of the test intersects the exterior of the target.
             * This implies the test is NOT contained in the target. 
             */
            if (testGeom is IPolygonal<TCoordinate>)
                return true;
            /**
             * A single shell with no holes allows concluding that 
             * a proper intersection implies not contained 
             * (due to the Epsilon-Neighbourhood Exterior Intersection condition) 
             */
            if (IsSingleShell(_prepPoly.Geometry))
                return true;

            return false;
        }

        /**
         * Tests whether a geometry consists of a single polygon with no holes.
         *  
         * @return true if the geometry is a single polygon with no holes
         */

        private Boolean IsSingleShell(IGeometry<TCoordinate> geom)
        {
            IPolygon<TCoordinate> poly = null;
            // handles single-element MultiPolygons, as well as Polygons
            IMultiPolygon<TCoordinate> mpoly = geom as IMultiPolygon<TCoordinate>;
            if (mpoly != null)
            {
                if (mpoly.Count != 1) return false;
                poly = mpoly[0];
            }
            else
                poly = (IPolygon<TCoordinate>) geom;

            return poly.InteriorRingsCount == 0;
        }

        private void FindAndClassifyIntersections(IGeometry<TCoordinate> geom)
        {
            List<NodedSegmentString<TCoordinate>> lineSegStr = SegmentStringUtil<TCoordinate>.ExtractSegmentStrings(geom);

            LineIntersector<TCoordinate> li = new RobustLineIntersector<TCoordinate>(geom.Factory);
            SegmentIntersectionDetector<TCoordinate> intDetector = new SegmentIntersectionDetector<TCoordinate>(li);
            intDetector.FindAllTypes = true;
            _prepPoly.IntersectionFinder.Intersects(lineSegStr, intDetector);

            HasSegmentIntersection = intDetector.HasIntersection;
            HasProperIntersection = intDetector.HasProperIntersection;
            HasNonProperIntersection = intDetector.HasNonProperIntersection;
        }

        /**
         * Computes the full topological predicate.
         * Used when short-circuit tests are not conclusive.
         * 
         * @param geom the test geometry
         * @return true if this prepared polygon has the relationship with the test geometry
         */
        protected abstract Boolean FullTopologicalPredicate(IGeometry<TCoordinate> geom);
    }
}