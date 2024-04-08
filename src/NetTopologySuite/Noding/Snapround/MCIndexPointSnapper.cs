using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="ISegmentString" />s in a <see cref="ISpatialIndex{T}" /> containing
    /// <see cref="MonotoneChain" />s to a given <see cref="HotPixel" />.
    /// </summary>
    public class MCIndexPointSnapper
    {
        //private IList<MonotoneChain> _monoChains;
        private readonly ISpatialIndex<MonotoneChain> _index;

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
            var pixelEnv = GetSafeEnvelope(hotPixel);
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

        private const double SafeEnvExpansionFactor = 0.75;

        /// <summary>
        /// Returns a "safe" envelope that is guaranteed to contain the hot pixel.
        /// The envelope returned is larger than the exact envelope of the
        /// pixel by a safe margin.
        /// </summary>
        /// <returns>An envelope which contains the hot pixel</returns>
        private Envelope GetSafeEnvelope(HotPixel hp)
        {
            double safeTolerance = SafeEnvExpansionFactor / hp.ScaleFactor;
            var safeEnv = new Envelope(hp.Coordinate);
            safeEnv.ExpandBy(safeTolerance);
            return safeEnv;
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
            /// Reports whether the HotPixel caused a node to be added in any target
            /// segmentString(including its own). If so, the HotPixel must be added as a
            /// node as well.
            /// </summary>
            /// <returns>
            /// <c>true</c> if a node was added in any target segmentString.
            /// </returns>
            public bool IsNodeAdded => _isNodeAdded;

            /// <summary>
            /// Check if a segment of the monotone chain intersects
            /// the hot pixel vertex and introduce a snap node if so.
            /// Optimized to avoid noding segments which
            /// contain the vertex (which otherwise
            /// would cause every vertex to be noded).
            /// </summary>
            /// <param name="mc">A monotone chain</param>
            /// <param name="startIndex">A start index</param>
            public override void Select(MonotoneChain mc, int startIndex)
            {
                var ss = (INodableSegmentString) mc.Context;
                /*
                 * Check to avoid snapping a hotPixel vertex to the its orginal vertex.
                 * This method is called on segments which intersect the
                 * hot pixel.
                 * If either end of the segment is equal to the hot pixel
                 * do not snap.
                 */
                if (_parentEdge == ss)
                {
                    // exit if hotpixel is equal to endpoint of target segment
                    if (startIndex == _hotPixelVertexIndex
                        || startIndex + 1 == _hotPixelVertexIndex)
                        return;
                }
                // snap and record if a node was created
                _isNodeAdded |= AddSnappedNode(_hotPixel, ss, startIndex);
            }

            /// <summary>
            /// Adds a new node (equal to the snap pt) to the specified segment
            /// if the segment passes through the hot pixel
            /// </summary>
            /// <returns><see langword="true"/> if a node was added to the segment</returns>
            private bool AddSnappedNode(HotPixel hotPixel,
                INodableSegmentString segStr,
                int segIndex
            )
            {
                var coords = segStr.Coordinates;
                var p0 = coords[segIndex];
                var p1 = coords[segIndex + 1];

                if (hotPixel.Intersects(p0, p1))
                {
                    //System.out.println("snapped: " + snapPt);
                    //System.out.println("POINT (" + snapPt.x + " " + snapPt.y + ")");
                    segStr.AddIntersection(hotPixel.Coordinate, segIndex);

                    return true;
                }
                return false;
            }

        }
    }
}
