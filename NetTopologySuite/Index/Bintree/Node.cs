using System;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A node of a <see cref="BinTree{TCoordinates}"/>.
    /// </summary>
    public class Node<TBoundable> : BaseBinNode<TBoundable>
        where TBoundable : IBoundable<Interval>
    {
        public static Node<TBoundable> CreateNode(Interval itemInterval)
        {
            BinTreeKey key = new BinTreeKey(itemInterval);

            Node<TBoundable> node = new Node<TBoundable>(key.Bounds, key.Level);
            return node;
        }

        public static Node<TBoundable> CreateExpanded(Node<TBoundable> node, Interval addInterval)
        {
            Interval expanded = new Interval(addInterval);

            if (node != null)
            {
                expanded.ExpandToInclude(node._interval);
            }

            Node<TBoundable> largerNode = CreateNode(expanded);

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
            : base(interval, level)
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
        public Node<TBoundable> GetNode(Interval searchInterval)
        {
            Int32 subnodeIndex = GetSubNodeIndex(searchInterval, _center);

            // if index is -1 searchEnv is not contained in a subnode
            if (subnodeIndex != -1)
            {
                // create the node if it does not exist
                Node<TBoundable> node = GetSubNode(subnodeIndex);

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
        public BaseBinNode<TBoundable> Find(Interval searchInterval)
        {
            Node<TBoundable> subNode = GetSubNode(searchInterval, _center);

            if (subNode != null)
            {
                // query lies in subnode, so search it
                return subNode.Find(searchInterval);
            }

            // no existing subnode, so return this one
            return this;
        }

        public void Insert(Node<TBoundable> node)
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
                Node<TBoundable> childNode = CreateSubNode(subnodeIndex);
                childNode.Insert(node);
                SetSubNode(subnodeIndex, childNode);
            }
        }

        public override bool Intersects(Interval bounds)
        {
            return Bounds.Intersects(bounds);
        }

        protected override Interval ComputeBounds()
        {
            throw new NotImplementedException();
        }
    }
}