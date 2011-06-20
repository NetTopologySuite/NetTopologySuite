using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// Experimental class which behaves like an STR-Tree before being built and a Dynamic R-Tree for further inserts
    /// </summary>
    /// <typeparam name="TCoordinate"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class DynamicSTRtree<TCoordinate, TItem> : StrTree<TCoordinate, TItem>,
                                                      IUpdatableSpatialIndex<IExtents<TCoordinate>, TItem>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<double, TCoordinate>
        where TItem : IBoundable<IExtents<TCoordinate>>
    {
        private readonly IndexBalanceHeuristic _indexBalanceHeuristic;
        private readonly IItemInsertStrategy<IExtents<TCoordinate>, TItem> _insertStrategy;
        private readonly INodeSplitStrategy<IExtents<TCoordinate>, TItem> _nodeSplitStrategy;
        private readonly IIndexRestructureStrategy<IExtents<TCoordinate>, TItem> _restructureStrategy;


        public DynamicSTRtree(IGeometryFactory<TCoordinate> geometryFactory,
                              Int32 nodeCount,
                              IItemInsertStrategy<IExtents<TCoordinate>, TItem> insertStrategy,
                              IIndexRestructureStrategy<IExtents<TCoordinate>, TItem> restructureStrategy,
                              INodeSplitStrategy<IExtents<TCoordinate>, TItem> nodeSplitStrategy,
                              IndexBalanceHeuristic indexBalanceHeuristic)
            : base(geometryFactory, nodeCount)
        {
            _insertStrategy = insertStrategy;
            _restructureStrategy = restructureStrategy;
            _nodeSplitStrategy = nodeSplitStrategy;
            _indexBalanceHeuristic = indexBalanceHeuristic;

            _nodeSplitStrategy.NodeFactory = this;
            _insertStrategy.NodeFactory = this;
        }

        #region IUpdatableSpatialIndex<IExtents<TCoordinate>,TItem> Members

        public override void Insert(TItem item)
        {
            if (!IsBuilt)
            {
                base.Insert(item);
                return;
            }

            ISpatialIndexNode<IExtents<TCoordinate>, TItem> target = findTargetNode(item.Bounds, Root);

            ISpatialIndexNode<IExtents<TCoordinate>, TItem> newSiblingFromSplit;
            _insertStrategy.Insert(
                item.Bounds,
                item,
                target,
                _nodeSplitStrategy,
                _indexBalanceHeuristic,
                out newSiblingFromSplit);

            if (newSiblingFromSplit == null)
            {
                return;
            }

            //Debug.Print("Node was split.");

            // Add the newly split sibling
            if (newSiblingFromSplit.IsLeaf)
            {
                if (Root.IsLeaf) // handle the first splitting of the root node.
                {
                    ISpatialIndexNode<IExtents<TCoordinate>, TItem> oldRoot = Root;
                    Root = CreateNode(newSiblingFromSplit.Level + 1);
                    Root.Add(oldRoot);
                }

                Root.Add(newSiblingFromSplit);
            }
            else // Came from a root split
            {
                ISpatialIndexNode<IExtents<TCoordinate>, TItem> oldRoot = Root;
                Root = CreateNode(newSiblingFromSplit.Level + 1);
                Root.Add(oldRoot);
                Root.Add(newSiblingFromSplit);
            }
        }

        /// <summary>
        /// Removes an item from the index.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public override Boolean Remove(TItem item)
        {
            if (!IsBuilt)
                return BulkLoadStorage.Remove(item);

            ISpatialIndexNode<IExtents<TCoordinate>, TItem> itemNode = findNodeForItem(item, Root);

            bool removed = itemNode != null && itemNode.Remove(item);
            if (itemNode.IsPrunable)
            {
                ISpatialIndexNode<IExtents<TCoordinate>, TItem> parent = findParentNode(itemNode, Root);
                parent.Remove(itemNode);
            }

            return removed;
        }

        public void Clear()
        {
            BulkLoadStorage.Clear();
            IsBuilt = false;
            NullifyRoot();
            IsBuilt = false;
        }

        #endregion

        private static ISpatialIndexNode<IExtents<TCoordinate>, TItem> findTargetNode(IExtents<TCoordinate> bounds,
                                                                                      ISpatialIndexNode
                                                                                          <IExtents<TCoordinate>, TItem>
                                                                                          searchNode)
        {
            if (!searchNode.Bounds.Intersects(bounds))
                return null;

            ISpatialIndexNode<IExtents<TCoordinate>, TItem> node;

            foreach (ISpatialIndexNode<IExtents<TCoordinate>, TItem> nd in searchNode.SubNodes)
            {
                node = findTargetNode(bounds, nd);
                if (node != null)
                {
                    return node;
                }
            }

            return searchNode;
        }


        private static ISpatialIndexNode<IExtents<TCoordinate>, TItem> findParentNode(
            ISpatialIndexNode<IExtents<TCoordinate>, TItem> lookFor,
            ISpatialIndexNode<IExtents<TCoordinate>, TItem> searchNode)
        {
            foreach (ISpatialIndexNode<IExtents<TCoordinate>, TItem> node in searchNode.SubNodes)
            {
                if (node.Equals(lookFor))
                {
                    return searchNode;
                }
            }

            IExtents<TCoordinate> itemBounds = lookFor.Bounds;
            // TODO: I think a breadth-first search would be more efficient here
            foreach (ISpatialIndexNode<IExtents<TCoordinate>, TItem> child in searchNode.SubNodes)
            {
                if (child.Intersects(itemBounds))
                {
                    ISpatialIndexNode<IExtents<TCoordinate>, TItem> found = findParentNode(lookFor, child);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        private static ISpatialIndexNode<IExtents<TCoordinate>, TItem> findNodeForItem(TItem item,
                                                                                       ISpatialIndexNode
                                                                                           <IExtents<TCoordinate>, TItem
                                                                                           > node)
        {
            foreach (TItem nodeItem in node.Items)
            {
                if (item.Equals(nodeItem))
                {
                    return node;
                }
            }

            IExtents<TCoordinate> itemBounds = item.Bounds;

            // TODO: I think a breadth-first search would be more efficient here
            foreach (ISpatialIndexNode<IExtents<TCoordinate>, TItem> child in node.SubNodes)
            {
                if (child.Intersects(itemBounds))
                {
                    ISpatialIndexNode<IExtents<TCoordinate>, TItem> found = findNodeForItem(item, child);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }
}