using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Strtree;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="SegmentString" />s in a <see cref="ISpatialIndex" /> containing
    /// <see cref="MonotoneChain" />s to a given <see cref="HotPixel" />.
    /// </summary>
    public class MCIndexPointSnapper
    {
        /// <summary>
        /// 
        /// </summary>
        // Public in java code... temporary modified for "safe assembly" in Sql2005
		internal static readonly int numberSnaps = 0;        

        private IList monoChains = null;
        private STRtree index = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexPointSnapper"/> class.
        /// </summary>
        /// <param name="monoChains"></param>
        /// <param name="index"></param>
        public MCIndexPointSnapper(IList monoChains, ISpatialIndex index)
        {
            this.monoChains = monoChains;
            this.index = (STRtree) index;
        }

        /// <summary>
        /// 
        /// </summary>
        private class QueryVisitor : IItemVisitor
        {
            IEnvelope env = null;
            HotPixelSnapAction action = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="env"></param>
            /// <param name="action"></param>
            public QueryVisitor(IEnvelope env, HotPixelSnapAction action)
            {
                this.env = env;
                this.action = action;
            }

            /// <summary>
            /// </summary>
            /// <param name="item"></param>
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
        public bool Snap(HotPixel hotPixel, SegmentString parentEdge, int vertexIndex)
        {
            IEnvelope pixelEnv = hotPixel.GetSafeEnvelope();
            HotPixelSnapAction hotPixelSnapAction = new HotPixelSnapAction(hotPixel, parentEdge, vertexIndex);
            index.Query(pixelEnv, new QueryVisitor(pixelEnv, hotPixelSnapAction));
            return hotPixelSnapAction.IsNodeAdded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hotPixel"></param>
        /// <returns></returns>
        public bool Snap(HotPixel hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

        /// <summary>
        /// 
        /// </summary>
        public class HotPixelSnapAction : MonotoneChainSelectAction
        {
            private HotPixel hotPixel = null;
            private SegmentString parentEdge = null;
            private int vertexIndex;
            private bool isNodeAdded = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="HotPixelSnapAction"/> class.
            /// </summary>
            /// <param name="hotPixel"></param>
            /// <param name="parentEdge"></param>
            /// <param name="vertexIndex"></param>
            public HotPixelSnapAction(HotPixel hotPixel, SegmentString parentEdge, int vertexIndex)
            {
                this.hotPixel = hotPixel;
                this.parentEdge = parentEdge;
                this.vertexIndex = vertexIndex;
            }

            /// <summary>
            /// 
            /// </summary>
            public bool IsNodeAdded
            {
                get
                {
                    return isNodeAdded;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mc"></param>
            /// <param name="startIndex"></param>
            public override void Select(MonotoneChain mc, int startIndex)
            {
                SegmentString ss = (SegmentString) mc.Context;
                // don't snap a vertex to itself
                if (parentEdge != null) 
                    if (ss == parentEdge && startIndex == vertexIndex)
                        return;
                isNodeAdded = SimpleSnapRounder.AddSnappedNode(hotPixel, ss, startIndex);
            }
        }
    }
}
