using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// Extensions methods for the <see cref="KdTree{T}"/>.
    /// </summary>
    public static class KdTreeExtensions
    {
        /// <summary>
        /// Performs a nearest neighbor search of the points in the index.
        /// </summary>
        /// <param name="self">The KdTree to look for the nearest neighbor</param>
        /// <param name="coord">The point to search the nearset neighbor for</param>
        /// <remarks>
        /// Equivalent to <see cref="KdTree{T}.NearestNeighbor(Coordinate)"/> and retained
        /// for backwards compatibility. Prefer the first-class method on <see cref="KdTree{T}"/>.
        /// </remarks>
        public static KdNode<T> NearestNeighbor<T>(this KdTree<T> self, Coordinate coord) where T : class
        {
            return self.NearestNeighbor(coord);
        }
    }
}
