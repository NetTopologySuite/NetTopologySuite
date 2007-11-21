using System;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary> 
    /// The root node of a single <see cref="BinTree{TItem}"/>.
    /// It is centred at the origin, and does not have a defined extent.
    /// </summary>
    public class Root<TItem> : NodeBase<TItem>
    {
        // the singleton root node is centred at the origin.
        private static readonly Double Origin = 0.0;

        /// <summary> 
        /// Insert an item into the tree this is the root of.
        /// </summary>
        public void Insert(Interval itemInterval, TItem item)
        {
            Int32 index = GetSubNodeIndex(itemInterval, Origin);
            Node<TItem> subNode = GetSubNode(itemInterval, Origin);

            // if index is -1, itemInterval must contain the origin.
            if (subNode == null)
            {
                Add(item);
                return;
            }

            
            // If the subnode doesn't exist or this item is not contained in it,
            // have to expand the tree upward to contain the item.
            if (!subNode.Interval.Contains(itemInterval))
            {
                Node<TItem> largerNode = Node<TItem>.CreateExpanded(subNode, itemInterval);
                SetSubNode(index, largerNode);
            }

            
            // At this point we have a subnode which exists and must contain
            // contains the extents for the item.  Insert the item into the tree.
            subNode = GetSubNode(index);
            insertContained(subNode, itemInterval, item);
        }

        /// <summary>
        /// The root node matches all searches.
        /// </summary>
        protected override Boolean IsSearchMatch(Interval interval)
        {
            return true;
        }

        /// <summary> 
        /// Insert an item which is known to be contained in the tree rooted at
        /// the given Node.  Lower levels of the tree will be created
        /// if necessary to hold the item.
        /// </summary>
        private static void insertContained(Node<TItem> tree, Interval itemInterval, TItem item)
        {
            Assert.IsTrue(tree.Interval.Contains(itemInterval));
            
            /*
            * Do NOT create a new node for zero-area intervals - this would lead
            * to infinite recursion. Instead, use a heuristic of simply returning
            * the smallest existing node containing the query
            */
            Boolean isZeroArea = IntervalSize.IsZeroWidth(itemInterval.Min, itemInterval.Max);
            NodeBase<TItem> node;

            if (isZeroArea)
            {
                node = tree.Find(itemInterval);
            }
            else
            {
                node = tree.GetNode(itemInterval);
            }

            node.Add(item);
        }
    }
}