using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// An implementation of a 2-D KD-Tree. KD-trees provide fast range searching on point data.
    /// </summary>
    /// <remarks>
    /// This implementation supports detecting and snapping points which are closer than a given
    /// tolerance value. If the same point (up to tolerance) is inserted more than once a new node is
    /// not created but the count of the existing node is incremented.
    /// </remarks>
    /// <typeparam name="T">The type of the user data object</typeparam>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public class KdTree<T>
        where T : class
    {
        private KdNode<T> _root;
// ReSharper disable once UnusedField.Compiler
        private KdNode<T> _last = null;
        private long _numberOfNodes;
        private readonly double _tolerance;

        /// <summary>
        /// Creates a new instance of a KdTree with a snapping tolerance of 0.0.
        /// (I.e. distinct points will <i>not</i> be snapped)
        /// </summary>
        public KdTree()
            : this(0.0)
        {
        }

        /// <summary>
        /// Creates a new instance of a KdTree, specifying a snapping distance tolerance.
        /// Points which lie closer than the tolerance to a point already 
        /// in the tree will be treated as identical to the existing point.
        /// </summary>
        /// <param name="tolerance">The tolerance distance for considering two points equal</param>
        public KdTree(double tolerance)
        {
            _tolerance = tolerance;
        }

        /// <summary>
        /// Tests whether the index contains any items.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (_root == null) return true;
                return false;
            }
        }



        /// <summary>
        /// Inserts a new point in the kd-tree, with no data.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <returns>The kdnode containing the point</returns>
        public KdNode<T> Insert(Coordinate p)
        {
            return Insert(p, null);
        }

        /// <summary>
        /// Inserts a new point into the kd-tree.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <param name="data">A data item for the point</param>
        /// <returns>
        /// A new KdNode if a new point is inserted, else an existing
        /// node is returned with its counter incremented. This can be checked
        /// by testing returnedNode.getCount() > 1.
        /// </returns>
        public KdNode<T> Insert(Coordinate p, T data)
        {
            if (_root == null)
            {
                _root = new KdNode<T>(p, data);
                return _root;
            }

            var currentNode = _root;
            var leafNode = _root;
            var isOddLevel = true;
            var isLessThan = true;

            /**
             * Traverse the tree,
             * first cutting the plane left-right (by X ordinate)
             * then top-bottom (by Y ordinate)
             */
            while (currentNode != _last)
            {
                // test if point is already a node
                if (currentNode != null)
                {
                    var isInTolerance = p.Distance(currentNode.Coordinate) <= _tolerance;

                    // check if point is already in tree (up to tolerance) and if so simply
                    // return existing node
                    if (isInTolerance)
                    {
                        currentNode.Increment();
                        return currentNode;
                    }
                }

                if (isOddLevel)
                {
// ReSharper disable once PossibleNullReferenceException
                    isLessThan = p.X < currentNode.X;
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    isLessThan = p.Y < currentNode.Y;
                }
                leafNode = currentNode;
                currentNode = isLessThan
                    ? currentNode.Left
                    : currentNode.Right;

                isOddLevel = !isOddLevel;
            }

            // no node found, add new leaf node to tree
            _numberOfNodes = _numberOfNodes + 1;
            var node = new KdNode<T>(p, data);
            node.Left = null;
            node.Right = null;
            if (isLessThan)
            {
                leafNode.Left = node;
            }
            else
            {
                leafNode.Right = node;
            }
            return node;
        }

        private static void QueryNode(KdNode<T> currentNode, KdNode<T> bottomNode,
            Envelope queryEnv, bool odd, ICollection<KdNode<T>> result)
        {
            if (currentNode == null)
                return;
            if (currentNode == bottomNode)
                return;

            double min;
            double max;
            double discriminant;
            if (odd)
            {
                min = queryEnv.MinX;
                max = queryEnv.MaxX;
                discriminant = currentNode.X;
            }
            else
            {
                min = queryEnv.MinY;
                max = queryEnv.MaxY;
                discriminant = currentNode.Y;
            }
            bool searchLeft = min < discriminant;
            bool searchRight = discriminant <= max;

            if (searchLeft)
            {
                QueryNode(currentNode.Left, bottomNode, queryEnv, !odd, result);
            }
            if (queryEnv.Contains(currentNode.Coordinate))
            {
                result.Add(currentNode);
            }
            if (searchRight)
            {
                QueryNode(currentNode.Right, bottomNode, queryEnv, !odd, result);
            }

        }

        /// <summary>
        /// Performs a range search of the points in the index. 
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <returns>A collection of the KdNodes found</returns>
        public ICollection<KdNode<T>> Query(Envelope queryEnv)
        {
            KdNode<T> last = null;
            ICollection<KdNode<T>> result = new Collection<KdNode<T>>();
            QueryNode(_root, _last, queryEnv, true, result);
            return result;
        }

        /// <summary>
        /// Performs a range search of the points in the index.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <param name="result">A collection to accumulate the result nodes into</param>
        public void Query(Envelope queryEnv, ICollection<KdNode<T>> result)
        {
            QueryNode(_root, _last, queryEnv, true, result);
        }

        private static void NearestNeighbor(KdNode<T> currentNode, KdNode<T> bottomNode,
            Coordinate queryCoordinate, ref KdNode<T> closestNode, ref double closestDistanceSq)
        {
            while (true)
            {
                if (currentNode == null)
                    return;
                if (currentNode == bottomNode)
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
                    searchLeft = (Math.Pow(currentNode.Left.X - queryCoordinate.X, 2) +
                                  Math.Pow(currentNode.Left.Y - queryCoordinate.Y, 2)) < closestDistanceSq;

                if (currentNode.Right != null)
                    searchRight = (Math.Pow(currentNode.Right.X - queryCoordinate.X, 2) +
                                   Math.Pow(currentNode.Right.Y - queryCoordinate.Y, 2)) < closestDistanceSq;

                if (searchLeft)
                {
                    NearestNeighbor(currentNode.Left, bottomNode, queryCoordinate, ref closestNode,
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

        /// <summary>
        /// Performs a nearest neighbor search of the points in the index.
        /// </summary>
        /// <param name="coord">The point to search the nearset neighbor for</param>
        public KdNode<T> NearestNeighbor(Coordinate coord)
        {
            KdNode<T> result = null;
            var closestDistSq = double.MaxValue;
            NearestNeighbor(_root, _last, coord, ref result, ref closestDistSq);
            return result;
        }

    }
}