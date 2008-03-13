using System;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary> 
    /// The root node of a single <see cref="BinTree{TItem}"/>.
    /// It is centered at the origin, and does not have a defined extent.
    /// </summary>
    public class Root<TBoundable> : BaseBinNode<TBoundable>
        where TBoundable : IBoundable<Interval>
    {
        // the singleton root node is centered at the origin.
        private static readonly Double Origin = 0.0;

        public Root() : base(Interval.Infinite, -1)
        {
            
        }

        /// <summary> 
        /// Insert an item into the tree this is the root of.
        /// </summary>
        public void Insert(TBoundable item)
        {
            Interval bounds = item.Bounds;
            Int32 index = GetSubNodeIndex(bounds, Origin);
            Node<TBoundable> subNode = GetSubNode(bounds, Origin);

            // if index is -1, itemInterval must contain the origin.
            if (subNode == null)
            {
                AddItem(item);
                return;
            }

            // If the subnode doesn't exist or this item is not contained in it,
            // have to expand the tree upward to contain the item.
            if (!subNode.Interval.Contains(bounds))
            {
                Node<TBoundable> largerNode = Node<TBoundable>.CreateExpanded(subNode, bounds);
                SetSubNode(index, largerNode);
            }

            // At this point we have a subnode which exists and must contain
            // contains the extents for the item.  Insert the item into the tree.
            subNode = GetSubNode(index);
            insertContained(subNode, bounds, item);
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
        private static void insertContained(Node<TBoundable> tree, Interval itemInterval, TBoundable item)
        {
            Assert.IsTrue(tree.Interval.Contains(itemInterval));
            
            /*
            * Do NOT create a new node for zero-area intervals - this would lead
            * to infinite recursion. Instead, use a heuristic of simply returning
            * the smallest existing node containing the query
            */
            Boolean isZeroArea = IntervalSize.IsZeroWidth(itemInterval.Min, itemInterval.Max);
            BaseBinNode<TBoundable> node;

            if (isZeroArea)
            {
                node = tree.Find(itemInterval);
            }
            else
            {
                node = tree.GetNode(itemInterval);
            }

            node.AddItem(item);
        }

        public override Boolean Intersects(Interval bounds)
        {
            return true;
        }

        protected override Interval ComputeBounds()
        {
            return Interval.Infinite;
        }
    }
}