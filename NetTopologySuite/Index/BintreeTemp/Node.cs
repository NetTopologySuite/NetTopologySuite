using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;

namespace GisSharpBlog.NetTopologySuite.Index.BintreeTemp
{
    /// <summary>
    /// A node of a <c>Bintree</c>.
    /// </summary>
    public class Node<T> : NodeBase<T>
    {
        private double centre;
        private Interval interval;
        private int level;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="level"></param>
        public Node(Interval interval, int level)
        {
            this.interval = interval;
            this.level = level;
            centre = (interval.Min + interval.Max)/2;
        }

        /// <summary>
        /// 
        /// </summary>
        public Interval Interval
        {
            get { return interval; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInterval"></param>
        /// <returns></returns>
        public static Node<T> CreateNode(Interval itemInterval)
        {
            Key key = new Key(itemInterval);

            Node<T> node = new Node<T>(key.Interval, key.Level);
            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="addInterval"></param>
        /// <returns></returns>
        public static Node<T> CreateExpanded(Node<T> node, Interval addInterval)
        {
            Interval expandInt = new Interval(addInterval);
            if (node != null) expandInt = expandInt.ExpandToInclude(node.interval);
            Node<T> largerNode = CreateNode(expandInt);
            if (node != null) largerNode.Insert(node);
            return largerNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInterval"></param>
        /// <returns></returns>
        protected override bool IsSearchMatch(Interval itemInterval)
        {
            return itemInterval.Overlaps(interval);
        }

        /// <summary>
        /// Returns the subnode containing the envelope.
        /// Creates the node if
        /// it does not already exist.
        /// </summary>
        /// <param name="searchInterval"></param>
        public Node<T> GetNode(Interval searchInterval)
        {
            int subnodeIndex = GetSubnodeIndex(searchInterval, centre);
            // if index is -1 searchEnv is not contained in a subnode
            if (subnodeIndex != -1)
            {
                // create the node if it does not exist
                Node<T> node = GetSubnode(subnodeIndex);
                // recursively search the found/created node
                return node.GetNode(searchInterval);
            }
            else return this;
        }

        /// <summary>
        /// Returns the smallest existing
        /// node containing the envelope.
        /// </summary>
        /// <param name="searchInterval"></param>
        public NodeBase<T> Find(Interval searchInterval)
        {
            int subnodeIndex = GetSubnodeIndex(searchInterval, centre);
            if (subnodeIndex == -1)
                return this;
            if (subnode[subnodeIndex] != null)
            {
                // query lies in subnode, so search it
                Node<T> node = subnode[subnodeIndex];
                return node.Find(searchInterval);
            }
            // no existing subnode, so return this one anyway
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void Insert(Node<T> node)
        {
            Assert.IsTrue(Equals(interval, default(Interval)) || interval.Contains(node.Interval));
            int index = GetSubnodeIndex(node.interval, centre);
            if (node.level == level - 1)
                subnode[index] = node;
            else
            {
                // the node is not a direct child, so make a new child node to contain it
                // and recursively insert the node
                Node<T> childNode = CreateSubnode(index);
                childNode.Insert(node);
                subnode[index] = childNode;
            }
        }

        /// <summary>
        /// Get the subnode for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        private Node<T> GetSubnode(int index)
        {
            if (subnode[index] == null)
                subnode[index] = CreateSubnode(index);
            return subnode[index];
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
            Node<T> node = new Node<T>(subInt, level - 1);
            return node;
        }
    }
}