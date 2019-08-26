using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// QuadRoot is the root of a single Quadtree.
    /// It is centred at the origin,
    /// and does not have a defined extent.
    /// </summary>
    [Serializable]
    public class Root<T> : NodeBase<T>
    {
        // the singleton root quad is centred at the origin.
        private static readonly Coordinate Origin = new Coordinate(0.0, 0.0);

        ///// <summary>
        /////
        ///// </summary>
        //public Root() { }

        /// <summary>
        /// Insert an item into the quadtree this is the root of.
        /// </summary>
        public void Insert(Envelope itemEnv, T item)
        {
            int index = GetSubnodeIndex(itemEnv, Origin.X, Origin.Y);
            // if index is -1, itemEnv must cross the X or Y axis.
            if (index == -1)
            {
                Add(item);
                return;
            }
            /*
            * the item must be contained in one quadrant, so insert it into the
            * tree for that quadrant (which may not yet exist)
            */
            var node = Subnode[index];
            /*
            *  If the subquad doesn't exist or this item is not contained in it,
            *  have to expand the tree upward to contain the item.
            */
            if (node == null || ! node.Envelope.Contains(itemEnv))
            {
                var largerNode = Node<T>.CreateExpanded(node, itemEnv);
                Subnode[index] = largerNode;
            }
            /*
            * At this point we have a subquad which exists and must contain
            * contains the env for the item.  Insert the item into the tree.
            */
            InsertContained(Subnode[index], itemEnv, item);
        }

        /// <summary>
        /// Insert an item which is known to be contained in the tree rooted at
        /// the given QuadNode root.  Lower levels of the tree will be created
        /// if necessary to hold the item.
        /// </summary>
        private static void InsertContained(Node<T> tree, Envelope itemEnv, T item)
        {
            Assert.IsTrue(tree.Envelope.Contains(itemEnv));
            /*
            * Do NOT create a new quad for zero-area envelopes - this would lead
            * to infinite recursion. Instead, use a heuristic of simply returning
            * the smallest existing quad containing the query
            */
            bool isZeroX = IntervalSize.IsZeroWidth(itemEnv.MinX, itemEnv.MaxX);
            bool isZeroY = IntervalSize.IsZeroWidth(itemEnv.MinY, itemEnv.MaxY);
            NodeBase<T> node;
            if (isZeroX || isZeroY)
                 node = tree.Find(itemEnv);
            else node = tree.GetNode(itemEnv);
            node.Add(item);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <returns></returns>
        protected override bool IsSearchMatch(Envelope searchEnv)
        {
            return true;
        }
    }
}
