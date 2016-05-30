using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// standard JTS <see cref="KdTree{T}"/> code.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class KdTreeJTS
    {
        internal static void NearestNeighbor<T>(KdNode<T> currentNode,
            Coordinate queryCoordinate, ref KdNode<T> closestNode, ref double closestDistanceSq)
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


                var searchLeft = false;
                var searchRight = false;
                if (currentNode.Left != null)
                    searchLeft = NeedsToBeSearched(queryCoordinate, currentNode.Left, closestDistanceSq);

                if (currentNode.Right != null)
                    searchRight = NeedsToBeSearched(queryCoordinate, currentNode.Right, closestDistanceSq);

                if (searchLeft)
                {
                    NearestNeighbor(currentNode.Left, queryCoordinate, ref closestNode,
                        ref closestDistanceSq);
                }

                if (searchRight)
                {
                    currentNode = currentNode.Right;
                    continue;
                }
                break;
            }
        }

        private static bool NeedsToBeSearched<T>(Coordinate target, KdNode<T> node, double closestDistSq)
            where T : class
        {
            return node.X >= target.X - closestDistSq && node.X <= target.X + closestDistSq
                   || node.Y >= target.Y - closestDistSq && node.Y <= target.Y + closestDistSq;
        }
    }
}