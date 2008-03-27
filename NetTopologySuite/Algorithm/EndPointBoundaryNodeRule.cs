using System;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// A <see cref="IBoundaryNodeRule" /> which specifies that any points 
    /// which are endpoints of lineal components are in the boundary of the
    /// parent geometry. This corresponds to the "intuitive" topological definition
    /// of boundary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Under this rule <see cref="LinearRing{TCoordinate}"/>s have a non-empty boundary
    /// (the common endpoint of the underlying <see cref="LineString{TCoordinate}"/>).
    /// </para>
    /// <para>
    /// This rule is useful when dealing with linear networks.
    /// For example, it can be used to check
    /// whether linear networks are correctly noded.
    /// The usual network topology constraint is that linear segments may touch 
    /// only at endpoints.
    /// In the case of a segment touching a closed segment (ring) at one point,
    /// the Mod2 rule cannot distinguish between the permitted case of touching at the
    /// node point and the invalid case of touching at some other interior (non-node) point.
    /// The EndPoint rule does distinguish between these cases,
    /// so is more appropriate for use.
    /// </para>
    /// </remarks>
    public class EndPointBoundaryNodeRule : IBoundaryNodeRule
    {
        #region IBoundaryNodeRule Members

        public Boolean IsInBoundary(Int32 boundaryCount)
        {
            return boundaryCount > 0;
        }

        #endregion
    }
}
