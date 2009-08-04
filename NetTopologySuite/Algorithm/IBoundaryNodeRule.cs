using System;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// An interface for rules which determine whether node points
    /// which are in boundaries of linear geometry components
    /// are in the boundary of the parent geometry collection.
    /// </summary>
    /// <remarks>
    /// The SFS specifies a single kind of boundary node rule,
    /// the <see cref="Mod2BoundaryNodeRule"/> rule.
    /// However, other kinds of Boundary Node Rules are appropriate
    /// in specific situations (for instance, linear network topology
    /// usually follows the <see cref="EndPointBoundaryNodeRule"/>.)
    /// Some NTS operations allow the BoundaryNodeRule to be specified,
    /// and respect this rule when computing the results of the operation.
    /// </remarks>
    public interface IBoundaryNodeRule
    {
        /// <summary>
        /// Tests whether a point that lies in <paramref name="boundaryCount"/>
        /// geometry component boundaries is considered to form part of the boundary
        /// of the parent geometry.
        /// </summary>
        /// <param name="boundaryCount">
        /// The number of component boundaries that this point occurs in.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if points in this number of boundaries lie 
        /// in the parent boundary.
        /// </returns>
        Boolean IsInBoundary(Int32 boundaryCount);
    }
}