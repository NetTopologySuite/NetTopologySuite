using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    /**
     * Computes the <tt>covers</tt> spatial relationship predicate
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
    public class PreparedPolygonCovers : AbstractPreparedPolygonContains
    {
        /**
         * Computes the </tt>covers</tt> predicate between a {@link PreparedPolygon}
         * and a {@link Geometry}.
         * 
         * @param prep the prepared polygon
         * @param geom a test geometry
         * @return true if the polygon covers the geometry
         */
        public static bool Covers(PreparedPolygon prep, IGeometry geom)
        {
            PreparedPolygonCovers polyInt = new PreparedPolygonCovers(prep);
            return polyInt.Covers(geom);
        }

        /**
         * Creates an instance of this operation.
         * 
         * @param prepPoly the PreparedPolygon to evaluate
         */
        public PreparedPolygonCovers(PreparedPolygon prepPoly)
            : base(prepPoly)
        {
            RequireSomePointInInterior = false;
        }

        /**
         * Tests whether this PreparedPolygon <tt>covers</tt> a given geometry.
         * 
         * @param geom the test geometry
         * @return true if the test geometry is covered
         */
        public bool Covers(IGeometry geom)
        {
            return Eval(geom);
        }

        /**
         * Computes the full topological <tt>covers</tt> predicate.
         * Used when short-circuit tests are not conclusive.
         * 
         * @param geom the test geometry
         * @return true if this prepared polygon covers the test geometry
         */
        protected override bool FullTopologicalPredicate(IGeometry geom)
        {
            bool result = prepPoly.Geometry.Covers(geom);
            return result;
        }

    }
}