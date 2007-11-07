using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="SegmentString" />s in a <see cref="ISpatialIndex" /> containing
    /// <see cref="MonotoneChain" />s to a given <see cref="HotPixel" />.
    /// </summary>
    public class MCIndexPointSnapper
    {
        // Public in java code... temporary modified for "safe assembly" in Sql2005
        internal static readonly Int32 numberSnaps = 0;

        private IList monoChains = null;
        private STRtree index = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexPointSnapper"/> class.
        /// </summary>
        public MCIndexPointSnapper(IList monoChains, ISpatialIndex index)
        {
            this.monoChains = monoChains;
            this.index = (STRtree) index;
        }

        private class QueryVisitor : IItemVisitor
        {
            private IExtents env = null;
            private HotPixelSnapAction action = null;

            public QueryVisitor(IExtents env, HotPixelSnapAction action)
            {
                this.env = env;
                this.action = action;
            }

            public void VisitItem(object item)
            {
                MonotoneChain testChain = (MonotoneChain) item;
                testChain.Select(env, action);
            }
        }

        /// <summary>
        /// Snaps (nodes) all interacting segments to this hot pixel.
        /// The hot pixel may represent a vertex of an edge,
        /// in which case this routine uses the optimization
        /// of not noding the vertex itself
        /// </summary>
        /// <param name="hotPixel">The hot pixel to snap to.</param>
        /// <param name="parentEdge">The edge containing the vertex, if applicable, or <c>null</c>.</param>
        /// <param name="vertexIndex"></param>
        /// <returns><c>true</c> if a node was added for this pixel.</returns>
        public Boolean Snap(HotPixel hotPixel, SegmentString parentEdge, Int32 vertexIndex)
        {
            IExtents pixelEnv = hotPixel.GetSafeEnvelope();
            HotPixelSnapAction hotPixelSnapAction = new HotPixelSnapAction(hotPixel, parentEdge, vertexIndex);
            index.Query(pixelEnv, new QueryVisitor(pixelEnv, hotPixelSnapAction));
            return hotPixelSnapAction.IsNodeAdded;
        }

        public Boolean Snap(HotPixel hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

        public class HotPixelSnapAction : MonotoneChainSelectAction
        {
            private HotPixel hotPixel = null;
            private SegmentString parentEdge = null;
            private Int32 vertexIndex;
            private Boolean isNodeAdded = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="HotPixelSnapAction"/> class.
            /// </summary>
            public HotPixelSnapAction(HotPixel hotPixel, SegmentString parentEdge, Int32 vertexIndex)
            {
                this.hotPixel = hotPixel;
                this.parentEdge = parentEdge;
                this.vertexIndex = vertexIndex;
            }

            public Boolean IsNodeAdded
            {
                get { return isNodeAdded; }
            }

            public override void Select(MonotoneChain mc, Int32 startIndex)
            {
                SegmentString ss = (SegmentString) mc.Context;
                // don't snap a vertex to itself
                if (parentEdge != null)
                {
                    if (ss == parentEdge && startIndex == vertexIndex)
                    {
                        return;
                    }
                }
                isNodeAdded = SimpleSnapRounder.AddSnappedNode(hotPixel, ss, startIndex);
            }
        }
    }
}