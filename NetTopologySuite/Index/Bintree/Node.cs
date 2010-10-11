using System;
using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;
using GeoAPI.Indexing;

namespace NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A node of a <see cref="BinTree{TCoordinates}"/>.
    /// </summary>
    public class Node<TBoundable> : BaseBinNode<TBoundable>
        where TBoundable : IBoundable<Interval>
    {
        //private readonly Interval _interval;
        private readonly Double _center;
        //private readonly Int32 _level;

        protected internal Node(Interval bounds, Int32 level)
            : base(bounds, level)
        {
            //_interval = interval;
            //_level = level;
            _center = (bounds.Min + bounds.Max) / 2;

            if (Double.IsNaN(_center))
            {
                throw new ArgumentException("Invalid interval: ", bounds.ToString());
            }
        }

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
                expanded = expanded.ExpandToInclude(node.Bounds);
            }

            Node<TBoundable> largerNode = CreateNode(expanded);

            if (node != null)
            {
                largerNode.Insert(node);
            }

            return largerNode;
        }

        //public Interval Interval
        //{
        //    get { return _interval; }
        //}

        protected override Boolean IsSearchMatch(Interval itemInterval)
        {
            return itemInterval.Intersects(Bounds);
        }

        /// <summary>
        /// Returns the subnode containing the envelope.
        /// Creates the node if it does not already exist.
        /// </summary>
        public Node<TBoundable> GetNode(Interval searchInterval)
        {
            Int32 subnodeIndex = GetSubNodeIndex(searchInterval, _center);

            // if index is -1 searchInterval is not contained in a subnode
            if (subnodeIndex == -1)
            {
                return this;
            }

            // create the node if it does not exist
            Node<TBoundable> node = GetSubNode(subnodeIndex, true);

            // recursively search the found/created node
            return node.GetNode(searchInterval);
        }

        /// <summary>
        /// Returns the smallest existing
        /// node containing the envelope.
        /// </summary>
        public BaseBinNode<TBoundable> Find(Interval searchInterval)
        {
            Node<TBoundable> subNode = GetSubNode(searchInterval, _center, false);

            if (subNode != null)
            {
                // query lies in subnode, so search it
                return subNode.Find(searchInterval);
            }

            // no existing subnode, so return this one
            return this;
        }

        protected void Insert(Node<TBoundable> node)
        {
            Assert.IsTrue(Bounds.Contains(node.Bounds));
            Int32 subnodeIndex = GetSubNodeIndex(node.Bounds, _center);

            if (!BoundsSet || node.Level == Level - 1)
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

        public override Boolean Intersects(Interval bounds)
        {
            return Bounds.Intersects(bounds);
        }

        protected override Interval ComputeBounds()
        {
            Interval bounds = SubNode1 == null ? Interval.Zero : SubNode1.Bounds;
            bounds = SubNode2 == null ? bounds : bounds.ExpandToInclude(SubNode2.Bounds);
            return bounds;
        }
    }
}