using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Unions a collection of geometries in an
    /// efficient way, using {@link OverlayNG}
    /// to ensure robust computation.
    /// </summary>
    /// <author>Martin Davis</author>
    public class UnaryUnionNG
    {
        /// <summary>
        /// Unions a collection of geometries
        /// using a given precision model.
        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The union of the geometries</returns>
        public static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            var unionSRFun = new UnionStrategy((g0, g1) => OverlayNG.Overlay(g0, g1, SpatialFunction.Union, pm), pm.IsFloating);
            var op = new UnaryUnionOp(geom)
            {
                UnionStrategy = unionSRFun
            };

            return op.Union();
        }

        /// <summary>
        /// Unions a collection of geometries
        /// using a precision model optimized to provide maximum
        /// precision while ensuring robust computation.
        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <returns>The union of the geometries</returns>
        public static Geometry Union(Geometry geom)
        {
            var pm = PrecisionUtility.RobustPM(geom);
            return Union(geom, pm);
        }

        private UnaryUnionNG()
        {
            // no instantiation for now
        }
    }
}
