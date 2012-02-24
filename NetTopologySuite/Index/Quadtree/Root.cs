using System;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NPack.Interfaces;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// QuadRoot is the root of a single Quadtree.
    /// It is centered at the origin,
    /// and does not have a defined extent.
    /// </summary>
    public class Root<TCoordinate, TItem> : BaseQuadNode<TCoordinate, TItem>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
        where TItem : IBoundable<IExtents<TCoordinate>>
    {
        // the singleton root quad is centred at the origin.
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly TCoordinate _origin;

        public Root(IGeometryFactory<TCoordinate> geoFactory)
            : base(null)
        {
            // 3D_UNSAFE
            _geoFactory = geoFactory;
            _origin = _geoFactory.CoordinateFactory.Create(0, 0);
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
                Add(item);
                return;
            }

            /*
            * the item must be contained in one quadrant, so insert it into the
            * tree for that quadrant (which may not yet exist)
            */
            Node<TCoordinate, TItem> node = SubNodesInternal[index] as Node<TCoordinate, TItem>;

            /*
            *  If the subquad doesn't exist or this item is not contained in it,
            *  have to expand the tree upward to contain the item.
            */
            if (node == null || !node.Bounds.Contains(item.Bounds))
            {
                Node<TCoordinate, TItem> largerNode
                    = Node<TCoordinate, TItem>.CreateExpanded(_geoFactory,
                                                              node,
                                                              item.Bounds);
                SubNodesInternal[index] = largerNode;
            }

            /*
            * At this point we have a subquad which exists and must contain
            * contains the extents for the item.  Insert the item into the tree
            * at this subnode.
            */
            Node<TCoordinate, TItem> subQuad = SubNodesInternal[index] as Node<TCoordinate, TItem>;
            insertContained(subQuad, item.Bounds, item);
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
            var minXY = itemExtents.Min.ToArray2D();
            var maxXY = itemExtents.Max.ToArray2D();
            var isZeroX = IntervalSize.IsZeroWidth(minXY[0], maxXY[0]);
            var isZeroY = IntervalSize.IsZeroWidth(minXY[1], maxXY[1]);

            BaseQuadNode<TCoordinate, TItem> node;

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

        protected override IExtents<TCoordinate> ComputeBounds()
        {
            IExtents<TCoordinate> bounds = _geoFactory.CreateExtents();

            foreach (ISpatialIndexNode<IExtents<TCoordinate>, TItem> node in SubNodesInternal)
            {
                if (node != null)
                    bounds.ExpandToInclude(node.Bounds);
            }

            return bounds;
        }
    }
}