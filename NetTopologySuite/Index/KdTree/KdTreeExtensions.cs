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
        public static KdNode<T> NearestNeighbor<T>(this KdTree<T> self, Coordinate coord) where T : class
        {
            KdNode<T> result = null;
            double closestDistSq = double.MaxValue;
            NearestNeighbor(self.Root, coord, ref result, ref closestDistSq, true);
            return result;
        }

        private static void NearestNeighbor<T>(
            KdNode<T> currentNode, Coordinate queryCoordinate,
            ref KdNode<T> closestNode, ref double closestDistanceSq,
            bool isOddLevel) where T : class
        {
            while (true)
            {
                if (currentNode == null)
                    return;

                double distSq = Math.Pow(currentNode.X - queryCoordinate.X, 2) +
                             Math.Pow(currentNode.Y - queryCoordinate.Y, 2);

                if (distSq < closestDistanceSq)
                {
                    closestNode = currentNode;
                    closestDistanceSq = distSq;
                }

                // Start with the branch that is more likely to produce the final result
                var firstBranch = currentNode.Left;
                var secondBranch = currentNode.Right;
                if (currentNode.Left != null &&
                    (isOddLevel ? queryCoordinate.X >= currentNode.X : queryCoordinate.Y >= currentNode.Y))
                {
                    firstBranch = currentNode.Right;
                    secondBranch = currentNode.Left;
                }

                if (firstBranch != null
                    && NeedsToBeSearched(queryCoordinate, currentNode, closestDistanceSq,
                                         firstBranch == currentNode.Left, isOddLevel))
                {
                    NearestNeighbor(firstBranch, queryCoordinate, ref closestNode, ref closestDistanceSq, !isOddLevel);
                }

                if (secondBranch != null
                    && NeedsToBeSearched(queryCoordinate, currentNode, closestDistanceSq,
                                         secondBranch == currentNode.Left, isOddLevel))
                {
                    currentNode = secondBranch;
                    isOddLevel = !isOddLevel;
                    continue;
                }

                break;

            }
        }

        private static bool NeedsToBeSearched<T>(Coordinate target, KdNode<T> node,
            double closestDistSq, bool left, bool isOddLevel) where T : class
        {
            if (isOddLevel)
                return (left ? target.X <= node.X : target.X >= node.X) || Math.Pow(target.X - node.X, 2) < closestDistSq;
            return (left ? target.Y <= node.Y : target.Y >= node.Y) || Math.Pow(target.Y - node.Y, 2) < closestDistSq;
        }
    }
}
