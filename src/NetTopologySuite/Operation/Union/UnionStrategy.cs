using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// An strategy class that allows UnaryUnion to adapt to different
    /// kinds of overlay algorithms.
    /// </summary>
    /// <author>Martin Davis</author>
    public sealed class UnionStrategy
    {
        private readonly Func<Geometry, Geometry, Geometry> _unionFunction;

        internal UnionStrategy(Func<Geometry, Geometry, Geometry> func, bool isFloatingPrecision)
        {
            _unionFunction = func;
            IsFloatingPrecision = isFloatingPrecision;
        }

        /// <summary>
        /// Computes the union of two geometries.
        /// This method may throw a <see cref="TopologyException"/>
        /// if one is encountered.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <returns>The union of the input</returns>
        internal Geometry Union(Geometry g0, Geometry g1) => _unionFunction(g0, g1);

        /// <summary>
        /// Indicates whether the union function operates using
        /// a floating(full) precision model.
        /// If this is the case, then the unary union code
        /// can make use of the { @link OverlapUnion}
        /// performance optimization,
        /// and perhaps other optimizations as well.
        /// Otherwise, the union result extent may not be the same as the extent of the inputs,
        /// which prevents using some optimizations.
        /// </summary>
        internal bool IsFloatingPrecision { get; }
    }
}
