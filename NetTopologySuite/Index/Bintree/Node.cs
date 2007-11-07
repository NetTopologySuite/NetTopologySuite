using System;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A node of a <c>Bintree</c>.
    /// </summary>
    public class Node : NodeBase
    {
        public static Node CreateNode(Interval itemInterval)
        {
            Key key = new Key(itemInterval);

            Node node = new Node(key.Interval, key.Level);
            return node;
        }

        public static Node CreateExpanded(Node node, Interval addInterval)
        {
            Interval expandInt = new Interval(addInterval);

            if (node != null)
            {
                expandInt.ExpandToInclude(node.interval);
            }

            Node largerNode = CreateNode(expandInt);

            if (node != null)
            {
                largerNode.Insert(node);
            }

            return largerNode;
        }

        private Interval interval;
        private Double centre;
        private Int32 level;

        public Node(Interval interval, Int32 level)
        {
            this.interval = interval;
            this.level = level;
            centre = (interval.Min + interval.Max)/2;
        }

        public Interval Interval
        {
            get { return interval; }
        }

        protected override Boolean IsSearchMatch(Interval itemInterval)
        {
            return itemInterval.Overlaps(interval);
        }

        /// <summary>
        /// Returns the subnode containing the envelope.
        /// Creates the node if
        /// it does not already exist.
        /// </summary>
        public Node GetNode(Interval searchInterval)
        {
            Int32 subnodeIndex = GetSubnodeIndex(searchInterval, centre);

            // if index is -1 searchEnv is not contained in a subnode
            if (subnodeIndex != -1)
            {
                // create the node if it does not exist
                Node node = GetSubnode(subnodeIndex);
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
        public NodeBase Find(Interval searchInterval)
        {
            Int32 subnodeIndex = GetSubnodeIndex(searchInterval, centre);

            if (subnodeIndex == -1)
            {
                return this;
            }

            if (subnode[subnodeIndex] != null)
            {
                // query lies in subnode, so search it
                Node node = subnode[subnodeIndex];
                return node.Find(searchInterval);
            }

            // no existing subnode, so return this one anyway
            return this;
        }

        public void Insert(Node node)
        {
            Assert.IsTrue(interval == null || interval.Contains(node.Interval));
            Int32 index = GetSubnodeIndex(node.interval, centre);

            if (node.level == level - 1)
            {
                subnode[index] = node;
            }
            else
            {
                // the node is not a direct child, so make a new child node to contain it
                // and recursively insert the node
                Node childNode = CreateSubnode(index);
                childNode.Insert(node);
                subnode[index] = childNode;
            }
        }

        /// <summary>
        /// Get the subnode for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        private Node GetSubnode(Int32 index)
        {
            if (subnode[index] == null)
            {
                subnode[index] = CreateSubnode(index);
            }
            return subnode[index];
        }

        private Node CreateSubnode(Int32 index)
        {
            // create a new subnode in the appropriate interval
            Double min = 0.0;
            Double max = 0.0;

            switch (index)
            {
                case 0:
                    min = interval.Min;
                    max = centre;
                    break;
                case 1:
                    min = centre;
                    max = interval.Max;
                    break;
                default:
                    break;
            }

            Interval subInt = new Interval(min, max);
            Node node = new Node(subInt, level - 1);
            return node;
        }
    }
}