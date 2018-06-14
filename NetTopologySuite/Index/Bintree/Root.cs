using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// The root node of a single <c>Bintree</c>.
    /// It is centred at the origin,
    /// and does not have a defined extent.
    /// </summary>
    public class Root<T> : NodeBase<T>
    {
        // the singleton root node is centred at the origin.
        private const double Origin = 0.0;

        /*
        /// <summary>
        ///
        /// </summary>
        public Root() { }
        */
        /// <summary>
        /// Insert an item into the tree this is the root of.
        /// </summary>
        /// <param name="itemInterval"></param>
        /// <param name="item"></param>
        public void Insert(Interval itemInterval, T item)
        {
            int index = GetSubnodeIndex(itemInterval, Origin);
            // if index is -1, itemEnv must contain the origin.
            if (index == -1)
            {
                Add(item);
                return;
            }
            /*
            * the item must be contained in one interval, so insert it into the
            * tree for that interval (which may not yet exist)
            */
            var node = Subnode[index];
            /*
            *  If the subnode doesn't exist or this item is not contained in it,
            *  have to expand the tree upward to contain the item.
            */

            if (node == null || ! node.Interval.Contains(itemInterval))
            {
                var largerNode = Node<T>.CreateExpanded(node, itemInterval);
                Subnode[index] = largerNode;
            }
            /*
            * At this point we have a subnode which exists and must contain
            * contains the env for the item.  Insert the item into the tree.
            */
            InsertContained(Subnode[index], itemInterval, item);
        }

        /// <summary>
        /// Insert an item which is known to be contained in the tree rooted at
        /// the given Node.  Lower levels of the tree will be created
        /// if necessary to hold the item.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="itemInterval"></param>
        /// <param name="item"></param>
        private static void InsertContained(Node<T> tree, Interval itemInterval, T item)
        {
            Assert.IsTrue(tree.Interval.Contains(itemInterval));
            /*
            * Do NOT create a new node for zero-area intervals - this would lead
            * to infinite recursion. Instead, use a heuristic of simply returning
            * the smallest existing node containing the query
            */
            bool isZeroArea = IntervalSize.IsZeroWidth(itemInterval.Min, itemInterval.Max);
            NodeBase<T> node;
            if (isZeroArea)
                node = tree.Find(itemInterval);
            else node = tree.GetNode(itemInterval);
            node.Add(item);
        }

        /// <summary>
        /// The root node matches all searches.
        /// </summary>
        /// <param name="interval"></param>
        protected override bool IsSearchMatch(Interval interval)
        {
            return true;
        }
    }
}
