using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Functions to reduce the precision of a geometry
     * by rounding it to a given precision model.
     * 
     * @author Martin Davis
     *
     */
    public class PrecisionReducer
    {

        /**
         * Reduces the precision of a geometry by rounding it to the
         * supplied {@link PrecisionModel}.
         * <p> 
         * The output is always a valid geometry.  This implies that input components
         * may be merged if they are closer than the grid precision.
         * if merging is not desired, then the individual geometry components
         * should be processed separately.
         * <p>
         * The output is fully noded.  
         * This provides an effective way to node / snap-round a collection of {@link LineString}s.
         * 
         * @param geom the geometry to reduce
         * @param pm the precision model to use
         * @return the precision-reduced geometry
         */
        public static Geometry ReducePrecision(Geometry geom, PrecisionModel pm)
        {
            var reduced = OverlayNG.Union(geom, pm);
            return reduced;
        }

        private PrecisionReducer()
        {
            // no instantiation for now
        }
    }
}
