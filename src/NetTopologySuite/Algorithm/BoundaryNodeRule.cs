using NetTopologySuite.Geometries;
using NetTopologySuite.Operation;
using NetTopologySuite.Operation.Relate;
using System;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// An interface for rules which determine whether node points
    /// which are in boundaries of <see cref="ILineal"/> geometry components
    /// are in the boundary of the parent geometry collection.
    /// The SFS specifies a single kind of boundary node rule,
    /// the <see cref="BoundaryNodeRules.Mod2BoundaryNodeRule"/> rule.
    /// However, other kinds of Boundary Node Rules are appropriate
    /// in specific situations (for instance, linear network topology
    /// usually follows the <see cref="BoundaryNodeRules.EndPointBoundaryNodeRule"/>.)
    /// Some JTS operations
    /// (such as <see cref="RelateOp"/>, <see cref="BoundaryOp"/> and <see cref="Operation.Valid.IsSimpleOp"/>)
    /// allow the BoundaryNodeRule to be specified,
    /// and respect the supplied rule when computing the results of the operation.
    /// <para/>
    /// An example use case for a non-SFS-standard Boundary Node Rule is
    /// that of checking that a set of <see cref="LineString"/>s have
    /// valid linear network topology, when turn-arounds are represented
    /// as closed rings.  In this situation, the entry road to the
    /// turn-around is only valid when it touches the turn-around ring
    /// at the single (common) endpoint.  This is equivalent
    /// to requiring the set of <tt>LineString</tt>s to be
    /// <b>simple</b> under the <see cref="BoundaryNodeRules.EndPointBoundaryNodeRule"/>.
    /// The SFS-standard <see cref="BoundaryNodeRules.Mod2BoundaryNodeRule"/> is not
    /// sufficient to perform this test, since it
    /// states that closed rings have <b>no</b> boundary points.
    /// <para/>
    /// This interface and its subclasses follow the <tt>Strategy</tt> design pattern.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="RelateOp"/>
    /// <seealso cref="BoundaryOp"/>
    /// <seealso cref="Operation.Valid.IsSimpleOp"/>
    /// <seealso cref="PointLocator"/>
    public interface IBoundaryNodeRule
    {
        /// <summary>
        /// Tests whether a point that lies in <c>boundaryCount</c>
        /// geometry component boundaries is considered to form part of the boundary
        /// of the parent geometry.
        /// </summary>
        /// <param name="boundaryCount">boundaryCount the number of component boundaries that this point occurs in</param>
        /// <returns>true if points in this number of boundaries lie in the parent boundary</returns>
        bool IsInBoundary(int boundaryCount);
    }

    /// <summary>
    /// Provides access to static instances of common <see cref="IBoundaryNodeRule"/>s.
    /// </summary>
    public static class BoundaryNodeRules
    {
        /// <summary>
        /// The Mod-2 Boundary Node Rule (which is the rule specified in the OGC SFS).
        /// </summary>
        /// <see cref="Mod2BoundaryNodeRule"/>
        public static readonly IBoundaryNodeRule Mod2BoundaryRule = new Mod2BoundaryNodeRule();

        /// <summary>The Endpoint Boundary Node Rule.</summary>
        /// <see cref="EndPointBoundaryNodeRule"/>
        public static readonly IBoundaryNodeRule EndpointBoundaryRule = new EndPointBoundaryNodeRule();

        /// <summary>The MultiValent Endpoint Boundary Node Rule.</summary>
        /// <see cref="MultiValentEndPointBoundaryNodeRule"/>
        public static readonly IBoundaryNodeRule MultivalentEndpointBoundaryRule = new MultiValentEndPointBoundaryNodeRule();

        /// <summary>The Monovalent Endpoint Boundary Node Rule.</summary>
        /// <see cref="MonoValentEndPointBoundaryNodeRule"/>
        public static readonly IBoundaryNodeRule MonoValentEndpointBoundaryRule = new MonoValentEndPointBoundaryNodeRule();

        /// <summary>
        /// The Boundary Node Rule specified by the OGC Simple Features Specification,
        /// which is the same as the Mod-2 rule.
        /// </summary>
        /// <see cref="Mod2BoundaryNodeRule"/>
        public static readonly IBoundaryNodeRule OgcSfsBoundaryRule = Mod2BoundaryRule;

        /// <summary>
        /// A <see cref="IBoundaryNodeRule"/> specifies that points are in the
        /// boundary of a lineal geometry if
        /// the point lies on the boundary of an odd number
        /// of components.
        /// Under this rule <see cref="LinearRing"/>s and closed
        /// <see cref="LineString"/>s have an empty boundary.
        /// </summary>
        /// <remarks>
        /// This is the rule specified by the <i>OGC SFS</i>,
        /// and is the default rule used in JTS.
        /// </remarks>
        /// <author>Martin Davis</author>
        private class Mod2BoundaryNodeRule : IBoundaryNodeRule
        {
            public bool IsInBoundary(int boundaryCount)
            {
                // the "Mod-2 Rule"
                return boundaryCount % 2 == 1;
            }

            public override string ToString()
            {
                return "Mod2 Boundary Node Rule";
            }
        }

        /// <summary>
        /// A <see cref="IBoundaryNodeRule" /> which specifies that any points which are endpoints
        /// of lineal components are in the boundary of the
        /// parent geometry.
        /// This corresponds to the "intuitive" topological definition
        /// of boundary.
        /// Under this rule <see cref="LinearRing" />s have a non-empty boundary
        /// (the common endpoint of the underlying LineString).
        /// </summary>
        /// <remarks>
        /// This rule is useful when dealing with linear networks.
        /// For example, it can be used to check
        /// whether linear networks are correctly noded.
        /// The usual network topology constraint is that linear segments may touch only at endpoints.
        /// In the case of a segment touching a closed segment (ring) at one point,
        /// the Mod2 rule cannot distinguish between the permitted case of touching at the
        /// node point and the invalid case of touching at some other interior (non-node) point.
        /// The EndPoint rule does distinguish between these cases,
        /// so is more appropriate for use.
        /// </remarks>
        /// <author>Martin Davis</author>
        private class EndPointBoundaryNodeRule : IBoundaryNodeRule
        {
            public bool IsInBoundary(int boundaryCount)
            {
                return boundaryCount > 0;
            }

            public override string ToString()
            {
                return "EndPoint Boundary Node Rule";
            }
        }

        /// <summary>
        /// A <see cref="IBoundaryNodeRule"/> which determines that only
        /// endpoints with valency greater than 1 are on the boundary.
        /// This corresponds to the boundary of a <see cref="MultiLineString"/>
        /// being all the "attached" endpoints, but not
        /// the "unattached" ones.
        /// </summary>
        /// <author>Martin Davis</author>
        private class MultiValentEndPointBoundaryNodeRule : IBoundaryNodeRule
        {
            public bool IsInBoundary(int boundaryCount)
            {
                return boundaryCount > 1;
            }

            public override string ToString()
            {
                return "MultiValent EndPoint Boundary Node Rule";
            }
        }

        /// <summary>
        /// A <see cref="IBoundaryNodeRule"/> which determines that only
        /// endpoints with valency of exactly 1 are on the boundary.
        /// This corresponds to the boundary of a <see cref="MultiLineString"/>
        /// being all the "unattached" endpoints.
        /// </summary>
        /// <author>Martin Davis</author>
        private class MonoValentEndPointBoundaryNodeRule : IBoundaryNodeRule
        {
            public bool IsInBoundary(int boundaryCount)
            {
                return boundaryCount == 1;
            }

            public override string ToString()
            {
                return "MonoValent EndPoint Boundary Node Rule";
            }

        }

    }
}
