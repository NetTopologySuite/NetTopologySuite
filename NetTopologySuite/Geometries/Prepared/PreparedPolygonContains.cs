using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
/**
 * Computes the <tt>contains</tt> spatial relationship predicate
 * for a {@link PreparedPolygon} relative to all other {@link Geometry} classes.
 * Uses short-circuit tests and indexing to improve performance. 
 * <p>
 * It is not possible to short-circuit in all cases, in particular
 * in the case where the test geometry touches the polygon linework.
 * In this case full topology must be computed.
 * 
 * @author Martin Davis
 *
 */
public class PreparedPolygonContains : AbstractPreparedPolygonContains
{
	/**
	 * Computes the </tt>contains</tt> predicate between a {@link PreparedPolygon}
	 * and a {@link Geometry}.
	 * 
	 * @param prep the prepared polygon
	 * @param geom a test geometry
	 * @return true if the polygon contains the geometry
	 */
	public static bool Contains(PreparedPolygon prep, IGeometry geom)
	{
    PreparedPolygonContains polyInt = new PreparedPolygonContains(prep);
    return polyInt.Contains(geom);
	}

  /**
   * Creates an instance of this operation.
   * 
   * @param prepPoly the PreparedPolygon to evaluate
   */
	public PreparedPolygonContains(PreparedPolygon prepPoly)
        :base(prepPoly)
	{
	}
		
	/**
	 * Tests whether this PreparedPolygon <tt>contains</tt> a given geometry.
	 * 
	 * @param geom the test geometry
	 * @return true if the test geometry is contained
	 */
	public bool Contains(IGeometry geom)
	{
		return Eval(geom);
	}
	
	/**
	 * Computes the full topological <tt>contains</tt> predicate.
	 * Used when short-circuit tests are not conclusive.
	 * 
	 * @param geom the test geometry
	 * @return true if this prepared polygon contains the test geometry
	 */
	protected override bool FullTopologicalPredicate(IGeometry geom)
	{
		bool isContained = prepPoly.Geometry.Contains(geom);
		return isContained;
	}
	
}
}