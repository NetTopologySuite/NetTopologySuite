using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// An implementation of a 2-D KD-Tree. KD-trees provide fast range searching on point data.
    /// </summary>
    /// <remarks>
    /// This implementation supports detecting and snapping points which are closer
    /// than a given distance tolerance.
    /// If the same point (up to tolerance) is inserted
    /// more than once , it is snapped to the existing node.
    /// In other words, if a point is inserted which lies within the tolerance of a node already in the index,
    /// it is snapped to that node.
    /// When a point is snapped to a node then a new node is not created but the count of the existing node
    /// is incremented.
    /// If more than one node in the tree is within tolerance of an inserted point,
    /// the closest and then lowest node is snapped to.
    /// </remarks>
    /// <typeparam name="T">The type of the user data object</typeparam>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    public partial class KdTree<T>
        where T : class
    {
        ///<summary>
        /// Converts a collection of<see cref= "KdNode{T}" /> s to an array of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="kdnodes">A collection of nodes</param>
        /// <returns>An array of the coordinates represented by the nodes</returns>
        public static Coordinate[] ToCoordinates(ICollection<KdNode<T>> kdnodes)
        {
            return ToCoordinates(kdnodes, false);
        }

        ///<summary>
        /// Converts a collection of <see cref="KdNode{T}"/>{@link KdNode}s
        /// to an array of <see cref="Coordinate"/>s,
        /// specifying whether repeated nodes should be represented
        /// by multiple coordinates.
        /// </summary>
        /// <param name="kdnodes">a collection of nodes</param>
        /// <param name="includeRepeated">true if repeated nodes should
        /// be included multiple times</param>
        /// <returns>An array of the coordinates represented by the nodes</returns>
        public static Coordinate[] ToCoordinates(ICollection<KdNode<T>> kdnodes, bool includeRepeated)
        {
            var coord = new CoordinateList();
            foreach (var node in kdnodes)
            {
                int count = includeRepeated ? node.Count : 1;
                for (int i = 0; i < count; i++)
                {
                    coord.Add(node.Coordinate, true);
                }
            }
            return coord.ToCoordinateArray();
        }

        private KdNode<T> _root;
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
        /// Creates a new instance of a KdTree with a snapping distance
        /// tolerance. Points which lie closer than the tolerance to a point already
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
        /// Gets a value indicating the root node of the tree
        /// </summary>
        internal KdNode<T> Root => _root;

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

            /**
             * Check if the point is already in the tree, up to tolerance.
             * If tolerance is zero, this phase of the insertion can be skipped.
             */
            if (_tolerance > 0)
            {
                var matchNode = FindBestMatchNode(p);
                if (matchNode != null)
                {
                    // point already in index - increment counter
                    matchNode.Increment();
                    return matchNode;
                }
            }

            return InsertExact(p, data);
        }

        /// <summary>
        /// Finds the node in the tree which is the best match for a point
        /// being inserted.
        /// The match is made deterministic by returning the lowest of any nodes which
        /// lie the same distance from the point.
        /// There may be no match if the point is not within the distance tolerance of any
        /// existing node.
        /// </summary>
        /// <param name="p">The point being inserted</param>
        /// <returns>
        /// <list type="Bullet">
        /// <item>the best matching node</item>
        /// <item>null if no match was found</item>
        /// </list>
        /// </returns>
        private KdNode<T> FindBestMatchNode(Coordinate p)
        {
            var visitor = new BestMatchVisitor<T>(p, _tolerance);

            Query(visitor.QueryEnvelope(), visitor);
            return visitor.Node;

        }

        /// <summary>
        /// Inserts a point known to be beyond the distance tolerance of any existing node.
        /// The point is inserted at the bottom of the exact splitting path,
        /// so that tree shape is deterministic.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <returns>
        /// <list type="Bullet">
        /// <item>The data for the point</item>
        /// <item>The created node</item>
        /// </list>
        /// </returns>
        public KdNode<T> InsertExact(Coordinate p, T data)
        {
            var currentNode = _root;
            var leafNode = _root;
            bool isOddLevel = true;
            bool isLessThan = true;

            /**
             * Traverse the tree, first cutting the plane left-right (by X ordinate)
             * then top-bottom (by Y ordinate)
             */
            while (currentNode != null)
            {
                // test if point is already a node (not strictly necessary)
                if (currentNode != null)
                {
                    bool isInTolerance = p.Distance(currentNode.Coordinate) <= _tolerance;

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

        private static void QueryNode(KdNode<T> currentNode,
            Envelope queryEnv, bool odd, IKdNodeVisitor<T> visitor)
        {
            if (currentNode == null)
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

            // search is computed via in-order traversal
            if (searchLeft)
            {
                QueryNode(currentNode.Left, queryEnv, !odd, visitor);
            }
            if (queryEnv.Contains(currentNode.Coordinate))
            {
                visitor.Visit(currentNode);
            }
            if (searchRight)
            {
                QueryNode(currentNode.Right, queryEnv, !odd, visitor);
            }

        }

        /// <summary>
        /// Performs a range search of the points in the index.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <param name="visitor"></param>
        public void Query(Envelope queryEnv, IKdNodeVisitor<T> visitor)
        {
            QueryNode(_root, queryEnv, true, visitor);
        }

        /// <summary>
        /// Performs a range search of the points in the index.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <returns>A collection of the KdNodes found</returns>
        public IList<KdNode<T>> Query(Envelope queryEnv)
        {
            var result = new List<KdNode<T>>();
            QueryNode(_root, queryEnv, true, new KdNodeVisitor<T>(result));
            return result;
        }

        /// <summary>
        /// Performs a range search of the points in the index.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <param name="result">A collection to accumulate the result nodes into</param>
        public void Query(Envelope queryEnv, IList<KdNode<T>> result)
        {
            QueryNode(_root, queryEnv, true, new KdNodeVisitor<T>(result));
        }

        private class KdNodeVisitor<T> : IKdNodeVisitor<T> where T : class
        {
            private readonly IList<KdNode<T>> _result;

            public KdNodeVisitor(IList<KdNode<T>> result)
            {
                _result = result;
            }

            public void Visit(KdNode<T> node)
            {
                _result.Add(node);
            }
        }

        private class BestMatchVisitor<T> : IKdNodeVisitor<T> where T : class
        {

            private readonly double tolerance;
            private KdNode<T> matchNode = null;
            private double matchDist = 0.0;
            private Coordinate p;

            public BestMatchVisitor(Coordinate p, double tolerance)
            {
                this.p = p;
                this.tolerance = tolerance;
            }

            public KdNode<T> Node => matchNode;

            public Envelope QueryEnvelope()
            {
                var queryEnv = new Envelope(p);
                queryEnv.ExpandBy(tolerance);
                return queryEnv;
            }

            public void Visit(KdNode<T> node)
            {
                double dist = p.Distance(node.Coordinate);
                bool isInTolerance = dist <= tolerance;
                if (!isInTolerance) return;
                bool update = false;
                if (matchNode == null
                    || dist < matchDist
                    // if distances are the same, record the lesser coordinate
                    || (matchNode != null && dist == matchDist
                        && node.Coordinate.CompareTo(matchNode.Coordinate) < 1))
                {
                    update = true;
                }

                if (update)
                {
                    matchNode = node;
                    matchDist = dist;
                }
            }
        }
    }
}