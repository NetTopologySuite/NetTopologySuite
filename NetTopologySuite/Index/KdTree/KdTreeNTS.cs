using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// NTS specific <see cref="KdTree{T}"/> code, that
    /// fixes NearestNeighbor behavior in some cases (see #97 and #105).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso href="https://github.com/NetTopologySuite/NetTopologySuite/pull/105"/>
    internal static class KdTreeNTS
    {
        internal static void NearestNeighbor<T>(KdNode<T> currentNode,
            Coordinate queryCoordinate, ref KdNode<T> closestNode, ref double closestDistanceSq,
            bool isOddLevel)
            where T : class
        {
            while (true)
            {
                if (currentNode == null)
                    return;


                var distSq = Math.Pow(currentNode.X - queryCoordinate.X, 2) +
                             Math.Pow(currentNode.Y - queryCoordinate.Y, 2);

                if (distSq < closestDistanceSq)
                {
                    closestNode = currentNode;
                    closestDistanceSq = distSq;
                }

                // Start with the branch that is more likely to produce the final result
                var firstBranch = currentNode.Left;
                var secondBranch = currentNode.Right;
                if (currentNode.Left != null && (isOddLevel ? queryCoordinate.X >= currentNode.X : queryCoordinate.Y >= currentNode.Y))
                {
                    firstBranch = currentNode.Right;
                    secondBranch = currentNode.Left;
                }

                if (firstBranch != null
                    && NeedsToBeSearched(queryCoordinate, currentNode, closestDistanceSq, firstBranch == currentNode.Left, isOddLevel))
                {
                    NearestNeighbor(firstBranch, queryCoordinate, ref closestNode, ref closestDistanceSq, !isOddLevel);
                }

                if (secondBranch != null
                    && NeedsToBeSearched(queryCoordinate, currentNode, closestDistanceSq, secondBranch == currentNode.Left, isOddLevel))
                {
                    currentNode = secondBranch;
                    isOddLevel = !isOddLevel;
                    continue;
                }

                break;
            }
        }

        private static bool NeedsToBeSearched<T>(Coordinate target,
            KdNode<T> node, double closestDistSq,
            bool left, bool isOddLevel)
            where T : class
        {
            if (isOddLevel)
                return (left ? target.X <= node.X : target.X >= node.X) || Math.Pow(target.X - node.X, 2) < closestDistSq;
            return (left ? target.Y <= node.Y : target.Y >= node.Y) || Math.Pow(target.Y - node.Y, 2) < closestDistSq;
        }
    }
}
