using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// JTS <see cref="KdTree{T}"/> implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KdTreeJTS<T> : KdTree<T>
        where T : class
    {
        public KdTreeJTS() { }

        public KdTreeJTS(double tolerance) : base(tolerance) { }

        public override KdNode<T> NearestNeighbor(Coordinate coord)
        {
            KdNode<T> result = null;
            var closestDistSq = double.MaxValue;
            NearestNeighbor(_root, coord, ref result, ref closestDistSq);
            return result;
        }

        private static void NearestNeighbor(KdNode<T> currentNode,
            Coordinate queryCoordinate, ref KdNode<T> closestNode, ref double closestDistanceSq)
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

        private static bool NeedsToBeSearched(Coordinate target, KdNode<T> node, double closestDistSq)
        {
            return node.X >= target.X - closestDistSq && node.X <= target.X + closestDistSq
                   || node.Y >= target.Y - closestDistSq && node.Y <= target.Y + closestDistSq;
        }
    }
}