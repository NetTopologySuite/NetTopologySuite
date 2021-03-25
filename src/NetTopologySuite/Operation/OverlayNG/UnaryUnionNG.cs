using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Unions a collection of geometries in an
    /// efficient way, using <see cref="OverlayNG"/>
    /// to ensure robust computation.
    /// <para/>
    /// This class is most useful for performing UnaryUnion using
    /// a fixed-precision model.<br/>
    /// For unary union using floating precision,
    /// <see cref="OverlayNGRobust.Union(Geometry)"/> should be used.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class UnaryUnionNG
    {
        /// <summary>
        /// Unions a collection of geometries
        /// using a given precision model.

        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The union of the geometries</returns>
        /// <seealso cref="OverlayNGRobust"/>
        public static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            if (geom == null)
            {
                throw new ArgumentNullException(nameof(geom));
            }

            var unionSRFun = new UnionStrategy((g0, g1) =>
                OverlayNG.Overlay(g0, g1, SpatialFunction.Union, pm), OverlayUtility.IsFloating(pm));

            var op = new UnaryUnionOp(geom) {
                UnionStrategy = unionSRFun
            };

            return op.Union();
        }
    }
}
