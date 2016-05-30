using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// NTS specific <see cref="KdTree{T}"/> implementation, that
    /// fixes NearestNeighbor behavior in some cases (see #97 and #105).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso href="https://github.com/NetTopologySuite/NetTopologySuite/pull/105"/>
    public class KdTreeNTS<T> : KdTree<T>
        where T : class
    {
        public KdTreeNTS() { }

        public KdTreeNTS(double tolerance) : base(tolerance) { }

        public override KdNode<T> NearestNeighbor(Coordinate coord)
        {
            KdNode<T> result = null;
            var closestDistSq = double.MaxValue;
            NearestNeighbor(_root, coord, ref result, ref closestDistSq, true);
            return result;
        }

        private static void NearestNeighbor(KdNode<T> currentNode,
            Coordinate queryCoordinate, ref KdNode<T> closestNode, ref double closestDistanceSq,
            bool isOddLevel)
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

        private static bool NeedsToBeSearched(Coordinate target,
            KdNode<T> node, double closestDistSq,
            bool left, bool isOddLevel)
        {
            if (isOddLevel)
                return (left ? target.X <= node.X : target.X >= node.X) || Math.Pow(target.X - node.X, 2) < closestDistSq;
            return (left ? target.Y <= node.Y : target.Y >= node.Y) || Math.Pow(target.Y - node.Y, 2) < closestDistSq;
        }
    }
}
