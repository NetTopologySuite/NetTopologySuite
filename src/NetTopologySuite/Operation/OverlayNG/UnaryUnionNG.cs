using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Unions a collection of geometries in an
     * efficient way, using {@link OverlayNG}
     * to ensure robust computation.
     * 
     * @author Martin Davis
     *
     */
    public class UnaryUnionNG
    {

        /**
         * Unions a collection of geometries
         * using a given precision model.
         * 
         * @param geom the geometry to union
         * @param pm the precision model to use
         * @return the union of the geometries
         */
        public static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            var unionSRFun = new UnionFunction((g0, g1) => OverlayNG.overlay(g0, g1, SpatialFunction.Union, pm));
            var op = new UnaryUnionOp(geom);
            op.UnionFunction = unionSRFun;

            return op.Union();
        }

        /**
     * Unions a collection of geometries
     * using a precision model optimized to provide maximum
     * precision while ensuring robust computation.
     * 
     * @param geom the geometry to union
     * @return the union of the geometries
     */
        public static Geometry Union(Geometry geom)
        {
            var pm = PrecisionUtil.RobustPM(geom);
            return Union(geom, pm);
        }

        private UnaryUnionNG()
        {
            // no instantiation for now
        }
    }

}
