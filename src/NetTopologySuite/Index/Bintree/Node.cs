using System;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A node of a <c>Bintree</c>.
    /// </summary>
    [Serializable]
    public class Node<T> : NodeBase<T>
    {
        /// <summary>
        /// Creates a node
        /// </summary>
        /// <param name="itemInterval">The interval of the node item</param>
        /// <returns>A new node</returns>
        public static Node<T> CreateNode(Interval itemInterval)
        {
            var key = new Key(itemInterval);
            var node = new Node<T>(key.Interval, key.Level);
            return node;
        }

        /// <summary>
        /// Creates a larger node, that contains both <paramref name="node.Interval"/> and <paramref name="addInterval"/>
        /// If <paramref name="node"/> is <c>null</c>, a node for <paramref name="addInterval"/> is created.
        /// </summary>
        /// <param name="node">The original node</param>
        /// <param name="addInterval">The additional interval</param>
        /// <returns>A new node</returns>
        public static Node<T> CreateExpanded(Node<T> node, Interval addInterval)
        {
            var expandInt = new Interval(addInterval);
            if (node != null) expandInt.ExpandToInclude(node._interval);
            /*
            var expandInt = node != null
                            ? addInterval.ExpandedByInterval(node._interval)
                            : addInterval;
             */
            var largerNode = CreateNode(expandInt);
            if (node != null) largerNode.Insert(node);

            return largerNode;
        }

        private readonly Interval _interval; //= Interval.Create();
        private readonly double _centre;
        private readonly int _level;

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        /// <param name="interval">The node's interval</param>
        /// <param name="level">The node's level</param>
        public Node(Interval interval, int level)
        {
            _interval = interval;
            _level = level;
            _centre = (interval.Min + interval.Max) / 2;
        }

        /// <summary>
        /// Gets the node's <see cref="Interval"/>
        /// </summary>
        public  Interval Interval => _interval;

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemInterval"></param>
        /// <returns></returns>
        protected override bool IsSearchMatch(Interval itemInterval)
        {
            return itemInterval.Overlaps(_interval);
        }

        /// <summary>
        /// Returns the subnode containing the envelope.
        /// Creates the node if
        /// it does not already exist.
        /// </summary>
        /// <param name="searchInterval"></param>
        public  Node<T> GetNode(Interval searchInterval)
        {
            int subnodeIndex = GetSubnodeIndex(searchInterval, _centre);
            // if index is -1 searchEnv is not contained in a subnode
            if (subnodeIndex != -1)
            {
                // create the node if it does not exist
                var node = GetSubnode(subnodeIndex);
                // recursively search the found/created node
                return node.GetNode(searchInterval);
            }
            return this;
        }

        /// <summary>
        /// Returns the smallest existing
        /// node containing the envelope.
        /// </summary>
        /// <param name="searchInterval"></param>
        public  NodeBase<T> Find(Interval searchInterval)
        {
            int subnodeIndex = GetSubnodeIndex(searchInterval, _centre);
            if (subnodeIndex == -1)
                return this;
            if (Subnode[subnodeIndex] != null)
            {
                // query lies in subnode, so search it
                var node = Subnode[subnodeIndex];
                return node.Find(searchInterval);
            }
            // no existing subnode, so return this one anyway
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        public  void Insert(Node<T> node)
        {
            Assert.IsTrue(_interval == null || _interval.Contains(node.Interval));
            int index = GetSubnodeIndex(node._interval, _centre);
            if (node._level == _level - 1)
                Subnode[index] = node;
            else
            {
                // the node is not a direct child, so make a new child node to contain it
                // and recursively insert the node
                var childNode = CreateSubnode(index);
                childNode.Insert(node);
                Subnode[index] = childNode;
            }
        }

        /// <summary>
        /// Get the subnode for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        private Node<T> GetSubnode(int index)
        {
            if (Subnode[index] == null)
                Subnode[index] = CreateSubnode(index);
            return Subnode[index];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Node<T> CreateSubnode(int index)
        {
            // create a new subnode in the appropriate interval
            double min = 0.0;
            double max = 0.0;

            switch (index)
            {
                case 0:
                    min = _interval.Min;
                    max = _centre;
                    break;
                case 1:
                    min = _centre;
                    max = _interval.Max;
                    break;
                    /*
                default:
                    break;
                     */
            }
            var subInt = new Interval(min, max);
            //var subInt = Interval.Create(min, max);
            var node = new Node<T>(subInt, _level - 1);
            return node;
        }
    }
}
