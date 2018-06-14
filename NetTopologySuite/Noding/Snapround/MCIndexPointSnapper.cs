using GeoAPI.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="ISegmentString" />s in a <see cref="ISpatialIndex" /> containing
    /// <see cref="MonotoneChain" />s to a given <see cref="HotPixel" />.
    /// </summary>
    public class MCIndexPointSnapper
    {
        //private IList<MonotoneChain> _monoChains;
        private readonly STRtree<MonotoneChain> _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexPointSnapper"/> class.
        /// </summary>
        /// <param name="index"></param>
        public MCIndexPointSnapper(ISpatialIndex<MonotoneChain> index)
        {
            //_monoChains = monoChains;
            _index = (STRtree<MonotoneChain>)index;
        }

        /// <summary>
        ///
        /// </summary>
        private class QueryVisitor : IItemVisitor<MonotoneChain>
        {
            readonly Envelope _env;
            readonly HotPixelSnapAction _action;

            /// <summary>
            ///
            /// </summary>
            /// <param name="env"></param>
            /// <param name="action"></param>
            public QueryVisitor(Envelope env, HotPixelSnapAction action)
            {
                _env = env;
                _action = action;
            }

            /// <summary>
            /// </summary>
            /// <param name="item"></param>
            public void VisitItem(MonotoneChain item)
            {
                var testChain = item;
                testChain.Select(_env, _action);
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
        /// <param name="hotPixelVertexIndex"></param>
        /// <returns><c>true</c> if a node was added for this pixel.</returns>
        public bool Snap(HotPixel hotPixel, ISegmentString parentEdge, int hotPixelVertexIndex)
        {
            var pixelEnv = hotPixel.GetSafeEnvelope();
            var hotPixelSnapAction = new HotPixelSnapAction(hotPixel, parentEdge, hotPixelVertexIndex);
            _index.Query(pixelEnv, new QueryVisitor(pixelEnv, hotPixelSnapAction));
            return hotPixelSnapAction.IsNodeAdded;
        }

        /// <summary>
        /// Snaps (nodes) all interacting segments to this hot pixel.
        /// The hot pixel may represent a vertex of an edge,
        /// in which case this routine uses the optimization
        /// of not noding the vertex itself
        /// </summary>
        /// <param name="hotPixel">The hot pixel to snap to.</param>
        /// <returns><c>true</c> if a node was added for this pixel.</returns>
        public bool Snap(HotPixel hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

        /// <summary>
        ///
        /// </summary>
        public class HotPixelSnapAction : MonotoneChainSelectAction
        {
            private readonly HotPixel _hotPixel;
            private readonly ISegmentString _parentEdge;
            // is -1 if hotPixel is not a vertex
            private readonly int _hotPixelVertexIndex;
            private bool _isNodeAdded;

            /// <summary>
            /// Initializes a new instance of the <see cref="HotPixelSnapAction"/> class.
            /// </summary>
            /// <param name="hotPixel"></param>
            /// <param name="parentEdge"></param>
            /// <param name="hotPixelVertexIndex"></param>
            public HotPixelSnapAction(HotPixel hotPixel, ISegmentString parentEdge, int hotPixelVertexIndex)
            {
                _hotPixel = hotPixel;
                _parentEdge = parentEdge;
                _hotPixelVertexIndex = hotPixelVertexIndex;
            }

            /// <summary>
            ///
            /// </summary>
            public bool IsNodeAdded => _isNodeAdded;

            /// <summary>
            ///
            /// </summary>
            /// <param name="mc"></param>
            /// <param name="startIndex"></param>
            public override void Select(MonotoneChain mc, int startIndex)
            {
                var ss = (INodableSegmentString) mc.Context;
                /**
                 * Check to avoid snapping a hotPixel vertex to the same vertex.
                 * This method is called for segments which intersects the
                 * hot pixel,
                 * so need to check if either end of the segment is equal to the hot pixel
                 * and if so, do not snap.
                 *
                 * Sep 22 2012 - MD - currently do need to snap to every vertex,
                 * since otherwise the testCollapse1 test in SnapRoundingTest fails.
                 */
                if (_parentEdge != null)
                {
                    if (ss == _parentEdge &&
                        (startIndex == _hotPixelVertexIndex)
                        )
                        return;
                }
                _isNodeAdded = _hotPixel.AddSnappedNode(ss, startIndex);
            }
        }
    }
}
