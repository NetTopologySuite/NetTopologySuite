using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
/**
 * Computes the <tt>containsProperly</tt> spatial relationship predicate
 * for {@link PreparedPolygon}s relative to all other {@link Geometry} classes.
 * Uses short-circuit tests and indexing to improve performance. 
 * <p>
 * A Geometry A <tt>containsProperly</tt> another Geometry B iff
 * all points of B are contained in the Interior of A.
 * Equivalently, B is contained in A AND B does not intersect 
 * the Boundary of A.
 * <p>
 * The advantage to using this predicate is that it can be computed
 * efficiently, with no need to compute topology at individual points.
 * In a situation with many geometries intersecting the boundary 
 * of the target geometry, this can make a performance difference.
 * 
 * @author Martin Davis
 */
public class PreparedPolygonContainsProperly : PreparedPolygonPredicate
{
	/**
	 * Computes the </tt>containsProperly</tt> predicate between a {@link PreparedPolygon}
	 * and a {@link Geometry}.
	 * 
	 * @param prep the prepared polygon
	 * @param geom a test geometry
	 * @return true if the polygon properly contains the geometry
	 */
	public static bool ContainsProperly(PreparedPolygon prep, IGeometry geom)
	{
		PreparedPolygonContainsProperly polyInt = new PreparedPolygonContainsProperly(prep);
    return polyInt.ContainsProperly(geom);
	}

  /**
   * Creates an instance of this operation.
   * 
   * @param prepPoly the PreparedPolygon to evaluate
   */
	public PreparedPolygonContainsProperly(PreparedPolygon prepPoly)
        :base(prepPoly)
	{
	}
	
	/**
	 * Tests whether this PreparedPolygon containsProperly a given geometry.
	 * 
	 * @param geom the test geometry
	 * @return true if the test geometry is contained properly
	 */
	public bool ContainsProperly(IGeometry geom)
	{
		/**
		 * Do point-in-poly tests first, since they are cheaper and may result
		 * in a quick negative result.
		 * 
		 * If a point of any test components does not lie in the target interior, result is false
		 */
		bool isAllInPrepGeomAreaInterior = IsAllTestComponentsInTargetInterior(geom);
		if (! isAllInPrepGeomAreaInterior) return false;
		
		/**
		 * If any segments intersect, result is false.
		 */
    IList<ISegmentString> lineSegStr = SegmentStringUtil.ExtractSegmentStrings(geom);
		bool segsIntersect = prepPoly.IntersectionFinder.Intersects(lineSegStr);
		if (segsIntersect) 
      return false;
		
		/**
		 * Given that no segments intersect, if any vertex of the target
		 * is contained in some test component.
		 * the test is NOT properly contained.
		 */
		if (geom is IPolygonal) {
			// TODO: generalize this to handle GeometryCollections
			bool isTargetGeomInTestArea = IsAnyTargetComponentInAreaTest(geom, prepPoly.RepresentativePoints);
			if (isTargetGeomInTestArea) return false;
		}
		
		return true;
	}
	
}
}