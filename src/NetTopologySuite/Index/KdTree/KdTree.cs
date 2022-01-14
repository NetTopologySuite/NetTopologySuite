using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// An implementation of a
    /// <a href='https://en.wikipedia.org/wiki/K-d_tree'> KD - Tree </a>
    /// over two dimensions(X and Y).
    /// KD-trees provide fast range searching and fast lookup for point data.
    /// The tree is built dynamically by inserting points.
    /// The tree supports queries by range and for point equality.
    /// For querying an internal stack is used instead of recursion to avoid overflow.
    /// </summary>
    /// <remarks>
    /// This implementation supports detecting and snapping points which are closer
    /// than a given distance tolerance.
    /// If the same point (up to tolerance) is inserted
    /// more than once , it is snapped to the existing node.
    /// In other words, if a point is inserted which lies
    /// within the tolerance of a node already in the index,
    /// it is snapped to that node.
    /// When an inserted point is snapped to a node then a new node is not created
    /// but the count of the existing node is incremented.
    /// If more than one node in the tree is within tolerance of an inserted point,
    /// the closest and then lowest node is snapped to.
    /// <para/>
    /// The structure of a KD-Tree depends on the order of insertion of the points.
    /// A tree may become umbalanced if the inserted points are coherent
    /// (e.g.monotonic in one or both dimensions).
    /// A perfectly balanced tree has depth of only log2(N),
    /// but an umbalanced tree may be much deeper.
    /// This has a serious impact on query efficiency.
    /// One solution to this is to randomize the order of points before insertion
    /// (e.g. by using <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle"> Fisher - Yates shuffling</a>).
    /// </remarks>
    /// <typeparam name="T">The type of the user data object</typeparam>
    /// <author>David Skea</author>
    /// <author>Martin Davis</author>
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class KdTree<T>
        where T : class
    {
        /// <summary>
        /// Converts a collection of<see cref= "KdNode{T}" /> s to an array of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="kdnodes">A collection of nodes</param>
        /// <returns>An array of the coordinates represented by the nodes</returns>
        public static Coordinate[] ToCoordinates(IEnumerable<KdNode<T>> kdnodes)
        {
            return ToCoordinates(kdnodes, false);
        }

        /// <summary>
        /// Converts a collection of <see cref="KdNode{T}"/>{@link KdNode}s
        /// to an array of <see cref="Coordinate"/>s,
        /// specifying whether repeated nodes should be represented
        /// by multiple coordinates.
        /// </summary>
        /// <param name="kdnodes">a collection of nodes</param>
        /// <param name="includeRepeated">true if repeated nodes should
        /// be included multiple times</param>
        /// <returns>An array of the coordinates represented by the nodes</returns>
        public static Coordinate[] ToCoordinates(IEnumerable<KdNode<T>> kdnodes, bool includeRepeated)
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
        // ReSharper disable once NotAccessedField.Local
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
        /// <returns>The root node of the tree</returns>
        public KdNode<T> Root => _root;

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

            /*
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
        /// <list type="bullet">
        /// <item><description>the best matching node</description></item>
        /// <item><description>null if no match was found</description></item>
        /// </list>
        /// </returns>
        private KdNode<T> FindBestMatchNode(Coordinate p)
        {
            var visitor = new BestMatchVisitor(p, _tolerance);

            Query(visitor.QueryEnvelope(), visitor);
            return visitor.Node;

        }

        /// <summary>
        /// Inserts a point known to be beyond the distance tolerance of any existing node.
        /// The point is inserted at the bottom of the exact splitting path,
        /// so that tree shape is deterministic.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <param name="data">The data associated with <paramref name="p"/></param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>The data for the point</description></item>
        /// <item><description>The created node</description></item>
        /// </list>
        /// </returns>
        public KdNode<T> InsertExact(Coordinate p, T data)
        {
            var currentNode = _root;
            var leafNode = _root;
            bool isXLevel = true;
            bool isLessThan = true;

            /*
             * Traverse the tree, first cutting the plane left-right (by X ordinate)
             * then top-bottom (by Y ordinate)
             */
            while (currentNode != null)
            {
                bool isInTolerance = p.Distance(currentNode.Coordinate) <= _tolerance;

                // check if point is already in tree (up to tolerance) and if so simply
                // return existing node
                if (isInTolerance)
                {
                    currentNode.Increment();
                    return currentNode;
                }

                double splitValue = currentNode.SplitValue(isXLevel);
                if (isXLevel)
                {
                    isLessThan = p.X < splitValue;
                }
                else
                {
                    isLessThan = p.Y < splitValue;
                }
                leafNode = currentNode;
                currentNode = isLessThan
                    ? currentNode.Left
                    : currentNode.Right;

                isXLevel = !isXLevel;
            }

            // no node found, add new leaf node to tree
            _numberOfNodes += 1;
            var node = new KdNode<T>(p, data) {
                Left = null,
                Right = null
            };
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

        /// <summary>
        /// Performs a range search of the points in the index and visits all nodes found.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <param name="visitor">A visitor to visit all nodes found by the search</param>
        public void Query(Envelope queryEnv, IKdNodeVisitor<T> visitor)
        {

            var queryStack = new Stack<QueryStackFrame>();
            var currentNode = Root;
            bool isXLevel = true;

            // search is computed via in-order traversal
            while (true)
            {
                if (currentNode != null)
                {
                    queryStack.Push(new QueryStackFrame(currentNode, isXLevel));

                    bool searchLeft = currentNode.IsRangeOverLeft(isXLevel, queryEnv);
                    if (searchLeft)
                    {
                        currentNode = currentNode.Left;
                        if (currentNode != null)
                        {
                            isXLevel = !isXLevel;
                        }
                    }
                    else
                    {
                        currentNode = null;
                    }
                }
                else if (queryStack.Count > 0)
                {
                    // currentNode is empty, so pop stack
                    var frame = queryStack.Pop();
                    currentNode = frame.Node;
                    isXLevel = frame.IsXLevel;

                    //-- check if search matches current node
                    if (queryEnv.Contains(currentNode.Coordinate))
                    {
                        visitor.Visit(currentNode);
                    }

                    bool searchRight = currentNode.IsRangeOverRight(isXLevel, queryEnv);
                    if (searchRight)
                    {
                        currentNode = currentNode.Right;
                        if (currentNode != null)
                        {
                            isXLevel = !isXLevel;
                        }
                    }
                    else
                    {
                        currentNode = null;
                    }
                }
                else
                {
                    //-- stack is empty and no current node
                    return;
                }
            }
        }

        class QueryStackFrame
        {
            public QueryStackFrame(KdNode<T> node, bool isXLevel)
            {
                Node = node;
                IsXLevel = isXLevel;
            }

            public KdNode<T> Node { get; }

            public bool IsXLevel { get; } = false;
        }

        /// <summary>
        /// Performs a range search of the points in the index.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <returns>A collection of the KdNodes found</returns>
        public IList<KdNode<T>> Query(Envelope queryEnv)
        {
            var result = new List<KdNode<T>>();
            Query(queryEnv, result);
            return result;
        }

        /// <summary>
        /// Performs a range search of the points in the index.
        /// </summary>
        /// <param name="queryEnv">The range rectangle to query</param>
        /// <param name="result">A collection to accumulate the result nodes into</param>
        public void Query(Envelope queryEnv, IList<KdNode<T>> result)
        {
            Query(queryEnv, new KdNodeVisitor(result));
        }

        /// <summary>
        /// Searches for a given point in the index and returns its node if found.
        /// </summary>
        /// <param name="queryPt">the point to query</param>
        /// <returns>the point node, if it is found in the index, or <see langword="null"/> if not</returns>
        public KdNode<T> Query(Coordinate queryPt)
        {
            var currentNode = Root;
            bool isXLevel = true;

            while (currentNode != null)
            {
                if (currentNode.Coordinate.Equals2D(queryPt))
                    return currentNode;

                bool searchLeft = currentNode.IsPointOnLeft(isXLevel, queryPt);
                if (searchLeft)
                {
                    currentNode = currentNode.Left;
                }
                else
                {
                    currentNode = currentNode.Right;
                }
                isXLevel = !isXLevel;
            }
            //-- point not found
            return null;
        }

        /// <summary>
        /// Gets a value indicating the depth of the tree
        /// </summary>
        /// <returns>The depth of the tree</returns>
        public int Depth
        {
            get => DepthNode(Root);
        }

        private int DepthNode(KdNode<T> currentNode)
        {
            if (currentNode == null)
                return 0;

            int dL = DepthNode(currentNode.Left);
            int dR = DepthNode(currentNode.Right);
            return 1 + (dL > dR ? dL : dR);
        }

        /// <summary>
        /// Gets a value indicating the number of items in the tree.
        /// </summary>
        /// <returns>The number of items in the tree.</returns>
        public int Count 
        {
            get => CountNode(Root);
        }

        private static int CountNode(KdNode<T> currentNode)
        {
            if (currentNode == null)
                return 0;

            int sizeL = CountNode(currentNode.Left);
            int sizeR = CountNode(currentNode.Right);

            return 1 + sizeL + sizeR;
        }



        private class KdNodeVisitor : IKdNodeVisitor<T>
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

        private class BestMatchVisitor : IKdNodeVisitor<T>
        {

            private readonly double _tolerance;
            private KdNode<T> _matchNode;
            private double _matchDist;
            private readonly Coordinate _p;

            public BestMatchVisitor(Coordinate p, double tolerance)
            {
                _p = p;
                _tolerance = tolerance;
            }

            public KdNode<T> Node => _matchNode;

            public Envelope QueryEnvelope()
            {
                var queryEnv = new Envelope(_p);
                queryEnv.ExpandBy(_tolerance);
                return queryEnv;
            }

            public void Visit(KdNode<T> node)
            {
                double dist = _p.Distance(node.Coordinate);
                bool isInTolerance = dist <= _tolerance;
                if (!isInTolerance) return;
                bool update = false;
                if (_matchNode == null
                    || dist < _matchDist
                    // if distances are the same, record the lesser coordinate
                    || (_matchNode != null && dist == _matchDist
                        && node.Coordinate.CompareTo(_matchNode.Coordinate) < 1))
                {
                    update = true;
                }

                if (update)
                {
                    _matchNode = node;
                    _matchDist = dist;
                }
            }
        }
    }
}
