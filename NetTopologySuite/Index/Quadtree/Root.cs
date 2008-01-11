using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// QuadRoot is the root of a single Quadtree.  
    /// It is centered at the origin,
    /// and does not have a defined extent.
    /// </summary>
    public class Root<TCoordinate, TItem> : BaseQuadNode<TCoordinate, TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
        where TItem : IBoundable<IExtents<TCoordinate>>
    {
        // the singleton root quad is centred at the origin.
        private static readonly TCoordinate _origin 
            = Coordinates<TCoordinate>.DefaultCoordinateFactory.Create(0.0, 0.0);

        public Root() : base(null)
        {
            
        }

        /// <summary> 
        /// Insert an item into the quadtree this is the root of.
        /// </summary>
        public void Insert(TItem item)
        {
            Int32 index = GetSubnodeIndex(item.Bounds, _origin);

            // if index is -1, itemEnv must cross the X or Y axis.
            if (index == -1)
            {
                AddItem(item);
                return;
            }

            /*
            * the item must be contained in one quadrant, so insert it into the
            * tree for that quadrant (which may not yet exist)
            */
            Node<TCoordinate, TItem> node = ChildrenInternal[index] as Node<TCoordinate, TItem>;

            /*
            *  If the subquad doesn't exist or this item is not contained in it,
            *  have to expand the tree upward to contain the item.
            */
            if (node == null || !node.Bounds.Contains(item.Bounds))
            {
                Node<TCoordinate, TItem> largerNode = Node<TCoordinate, TItem>.CreateExpanded(node, item.Bounds);
                ChildrenInternal[index] = largerNode;
            }

            /*
            * At this point we have a subquad which exists and must contain
            * contains the extents for the item.  Insert the item into the tree 
            * at this subnode.
            */
            insertContained(ChildrenInternal[index] as Node<TCoordinate, TItem>, item.Bounds, item);
        }

        public override Boolean Intersects(IExtents<TCoordinate> bounds)
        {
            return true;
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
            Assert.IsTrue(tree.Bounds.Contains(itemExtents));

            /*
            * Do NOT create a new quad for zero-area envelopes - this would lead
            * to infinite recursion. Instead, use a heuristic of simply returning
            * the smallest existing quad containing the query
            */
            Boolean isZeroX = IntervalSize.IsZeroWidth(itemExtents.GetMin(Ordinates.X), itemExtents.GetMax(Ordinates.X));
            Boolean isZeroY = IntervalSize.IsZeroWidth(itemExtents.GetMin(Ordinates.Y), itemExtents.GetMax(Ordinates.Y));

            BaseQuadNode<TCoordinate, TItem> node;

            if (isZeroX || isZeroY)
            {
                node = tree.Find(itemExtents);
            }
            else
            {
                node = tree.GetNode(itemExtents);
            }

            node.AddItem(item);
        }

        protected override IExtents<TCoordinate> ComputeBounds()
        {
            throw new NotImplementedException();
        }
    }
}