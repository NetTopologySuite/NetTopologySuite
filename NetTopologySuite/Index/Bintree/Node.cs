using System;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A node of a <see cref="BinTree{TCoordinates}"/>.
    /// </summary>
    public class Node<TItem> : BaseBinNode<TItem>
    {
        public static Node<TItem> CreateNode(Interval itemInterval)
        {
            BinTreeKey key = new BinTreeKey(itemInterval);

            Node<TItem> node = new Node<TItem>(key.Interval, key.Level);
            return node;
        }

        public static Node<TItem> CreateExpanded(Node<TItem> node, Interval addInterval)
        {
            Interval expanded = new Interval(addInterval);

            if (node != null)
            {
                expanded.ExpandToInclude(node._interval);
            }

            Node<TItem> largerNode = CreateNode(expanded);

            if (node != null)
            {
                largerNode.Insert(node);
            }

            return largerNode;
        }

        private readonly Interval _interval;
        private readonly Double _center;
        private readonly Int32 _level;

        public Node(Interval interval, Int32 level)
        {
            _interval = interval;
            _level = level;
            _center = (interval.Min + interval.Max) / 2;
        }

        public Interval Interval
        {
            get { return _interval; }
        }

        protected override Boolean IsSearchMatch(Interval itemInterval)
        {
            return itemInterval.Overlaps(_interval);
        }

        /// <summary>
        /// Returns the subnode containing the envelope.
        /// Creates the node if
        /// it does not already exist.
        /// </summary>
        public Node<TItem> GetNode(Interval searchInterval)
        {
            Int32 subnodeIndex = GetSubNodeIndex(searchInterval, _center);

            // if index is -1 searchEnv is not contained in a subnode
            if (subnodeIndex != -1)
            {
                // create the node if it does not exist
                Node<TItem> node = GetSubNode(subnodeIndex);

                // recursively search the found/created node
                return node.GetNode(searchInterval);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Returns the smallest existing
        /// node containing the envelope.
        /// </summary>
        public BaseBinNode<TItem> Find(Interval searchInterval)
        {
            Node<TItem> subNode = GetSubNode(searchInterval, _center);

            if (subNode != null)
            {
                // query lies in subnode, so search it
                return subNode.Find(searchInterval);
            }

            // no existing subnode, so return this one
            return this;
        }

        public void Insert(Node<TItem> node)
        {
            Assert.IsTrue(_interval.Contains(node.Interval));
            Int32 subnodeIndex = GetSubNodeIndex(node.Interval, _center);

            if (node.Level == _level - 1)
            {
                SetSubNode(subnodeIndex, node);
            }
            else
            {
                // the node is not a direct child, so make a new child node to contain it
                // and recursively insert the node
                Node<TItem> childNode = CreateSubNode(subnodeIndex);
                childNode.Insert(node);
                SetSubNode(subnodeIndex, childNode);
            }
        }
    }
}