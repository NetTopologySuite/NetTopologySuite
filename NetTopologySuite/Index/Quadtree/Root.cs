using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// QuadRoot is the root of a single Quadtree.  
    /// It is centered at the origin,
    /// and does not have a defined extent.
    /// </summary>
    public class Root<TCoordinate, TItem> : NodeBase<TCoordinate, TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
    {
        // the singleton root quad is centred at the origin.
        private static readonly TCoordinate _origin = new TCoordinate(0.0, 0.0);

        /// <summary> 
        /// Insert an item into the quadtree this is the root of.
        /// </summary>
        public void Insert(IExtents<TCoordinate> itemExtents, TItem item)
        {
            Int32 index = GetSubnodeIndex(itemExtents, _origin);

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
            Node<TCoordinate, TItem> node = SubNodes[index];

            /*
            *  If the subquad doesn't exist or this item is not contained in it,
            *  have to expand the tree upward to contain the item.
            */
            if (node == null || ! node.Extents.Contains(itemExtents))
            {
                Node<TCoordinate, TItem> largerNode = Node<TCoordinate, TItem>.CreateExpanded(node, itemExtents);
                SubNodes[index] = largerNode;
            }

            /*
            * At this point we have a subquad which exists and must contain
            * contains the env for the item.  Insert the item into the tree.
            */
            insertContained(SubNodes[index], itemExtents, item);
        }

        protected override Boolean IsSearchMatch(IExtents<TCoordinate> query)
        {
            return true;
        }

        /// <summary> 
        /// Insert an item which is known to be contained in the tree rooted at
        /// the given QuadNode root.  Lower levels of the tree will be created
        /// if necessary to hold the item.
        /// </summary>
        private void insertContained(Node<TCoordinate, TItem> tree, IExtents<TCoordinate> itemExtents, TItem item)
        {
            Assert.IsTrue(tree.Extents.Contains(itemExtents));

            /*
            * Do NOT create a new quad for zero-area envelopes - this would lead
            * to infinite recursion. Instead, use a heuristic of simply returning
            * the smallest existing quad containing the query
            */
            Boolean isZeroX = IntervalSize.IsZeroWidth(itemExtents.GetMin(Ordinates.X), itemExtents.GetMax(Ordinates.X));
            Boolean isZeroY = IntervalSize.IsZeroWidth(itemExtents.GetMin(Ordinates.Y), itemExtents.GetMax(Ordinates.Y));

            NodeBase<TCoordinate, TItem> node;

            if (isZeroX || isZeroY)
            {
                node = tree.Find(itemExtents);
            }
            else
            {
                node = tree.GetNode(itemExtents);
            }

            node.Add(item);
        }
    }
}