using System;
using GeoAPI.DataStructures;
using GeoAPI.Indexing;

namespace GisSharpBlog.NetTopologySuite.Index.RTree
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TBounds"></typeparam>
    ///<typeparam name="TItem"></typeparam>
    public class RTreeBranchNode<TBounds, TItem> : RTreeNode<TBounds, TItem>
        where TBounds : IContainable<TBounds>, IIntersectable<TBounds>, IComparable<TBounds>
        where TItem : IBoundable<TBounds>
    {
        private readonly RTreeNode<TBounds, TItem> _node1;
        private readonly RTreeNode<TBounds, TItem> _node2;

        ///<summary>
        ///</summary>
        ///<param name="level"></param>
        ///<param name="node1"></param>
        ///<param name="node2"></param>
        public RTreeBranchNode(IBoundsFactory<TBounds> boundsFactory, int level, RTreeNode<TBounds, TItem> node1,
                               RTreeNode<TBounds, TItem> node2)
            : base(
                boundsFactory,
                boundsFactory.CreateMinimumSpanningBounds(new[]
                                                              {
                                                                  node1 == null ? default(TBounds) : node1.Bounds,
                                                                  node2 == null ? default(TBounds) : node2.Bounds
                                                              }), level)
        {
            base.Add(node1);
            base.Add(node2);

            _node1 = node1;
            _node2 = node2;
        }

        protected override TBounds ComputeBounds()
        {
            return BuildBounds(_node1, _node2);
        }

        private TBounds BuildBounds(RTreeNode<TBounds, TItem> node1, RTreeNode<TBounds, TItem> node2)
        {
            TBounds bounds = node1 == null ? BoundsFactory.CreateNullBounds() : node1.Bounds;
            bounds = node2 == null ? bounds : BoundsFactory.CreateMinimumSpanningBounds(new[] {bounds, node2.Bounds});
            return bounds;
        }
    }
}