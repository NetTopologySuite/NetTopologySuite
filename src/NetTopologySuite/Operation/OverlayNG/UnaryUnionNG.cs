using System;
using System.Collections.Generic;
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
        /// Unions a geometry (which is often a collection)
        /// using a given precision model.
        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The union of the geometry</returns>
        /// <seealso cref="OverlayNGRobust"/>
        public static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            var op = new UnaryUnionOp(geom) { 
                UnionStrategy = CreateUnionStrategy(pm)
            };
            return op.Union();
        }

        /// <summary>
        /// Unions a geometry (which is often a collection)
        /// using a given precision model.
        /// </summary>
        /// <param name="geoms">The geometries to union</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The union of the geometries</returns>
        /// <seealso cref="OverlayNGRobust"/>
        public static Geometry Union(IEnumerable<Geometry> geoms, PrecisionModel pm)
        {
            var op = new UnaryUnionOp(geoms) {
                UnionStrategy = CreateUnionStrategy(pm)
            };
            return op.Union();
        }

        /// <summary>
        /// Unions a geometry (which is often a collection)
        /// using a given precision model.
        /// </summary>
        /// <param name="geoms">The geometries to union</param>
        /// <param name="geomFact">The geometry factory to use</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The union of the geometries</returns>
        /// <seealso cref="OverlayNGRobust"/>
        public static Geometry Union(IEnumerable<Geometry> geoms, GeometryFactory geomFact, PrecisionModel pm)
        {
            var op = new UnaryUnionOp(geoms, geomFact) {
                UnionStrategy = CreateUnionStrategy(pm)
            };
            return op.Union();
        }

        private static UnionStrategy CreateUnionStrategy(PrecisionModel pm)
        {
            bool isFloating = OverlayUtility.IsFloating(pm);
            return new UnionStrategy((g0, g1) => isFloating
                ? OverlayNGRobust.Overlay(g0, g1, SpatialFunction.Union)
                : OverlayNG.Overlay(g0, g1, SpatialFunction.Union, pm), true);
        }
    }
}
