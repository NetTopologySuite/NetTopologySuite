using System;
using System.Collections;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="SegmentString{TCoordinate}" />s in 
    /// a <see cref="ISpatialIndex{TCoordinate,TItem}" /> containing
    /// <see cref="MonotoneChain{TCoordinate}" />s to a given
    /// <see cref="HotPixel{TCoordinate}" />.
    /// </summary>
    public class MCIndexPointSnapper<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        // Public in java code... temporary modified for "safe assembly" in Sql2005
        internal static readonly Int32 numberSnaps = 0;

        private IList monoChains = null;
        private StrTree<TCoordinate, > index = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexPointSnapper"/> class.
        /// </summary>
        public MCIndexPointSnapper(IList monoChains, ISpatialIndex<TCoordinate, > index)
        {
            this.monoChains = monoChains;
            this.index = (STRtree) index;
        }

        private class QueryVisitor : IItemVisitor
        {
            private IExtents<TCoordinate> _extents = null;
            private HotPixelSnapAction _action = null;

            public QueryVisitor(IExtents<TCoordinate> extents, HotPixelSnapAction action)
            {
                _extents = extents;
                _action = action;
            }

            public void VisitItem(object item)
            {
                MonotoneChain<TCoordinate> testChain = (MonotoneChain<TCoordinate>) item;
                testChain.Select(_extents, _action);
            }
        }

        /// <summary>
        /// Snaps (nodes) all interacting segments to this hot pixel.
        /// The hot pixel may represent a vertex of an edge,
        /// in which case this routine uses the optimization
        /// of not noding the vertex itself
        /// </summary>
        /// <param name="hotPixel">The hot pixel to snap to.</param>
        /// <param name="parentEdge">The edge containing the vertex, if applicable, or <see langword="null" />.</param>
        /// <param name="vertexIndex"></param>
        /// <returns><see langword="true"/> if a node was added for this pixel.</returns>
        public Boolean Snap(HotPixel<TCoordinate> hotPixel, SegmentString<TCoordinate> parentEdge, Int32 vertexIndex)
        {
            IExtents<TCoordinate> pixelEnv = hotPixel.GetSafeEnvelope();
            HotPixelSnapAction hotPixelSnapAction = new HotPixelSnapAction(hotPixel, parentEdge, vertexIndex);
            index.Query(pixelEnv, new QueryVisitor(pixelEnv, hotPixelSnapAction));
            return hotPixelSnapAction.IsNodeAdded;
        }

        public Boolean Snap(HotPixel<TCoordinate> hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

        public class HotPixelSnapAction : MonotoneChainSelectAction<TCoordinate>
        {
            private HotPixel<TCoordinate> _hotPixel = null;
            private SegmentString<TCoordinate> _parentEdge = null;
            private Int32 _vertexIndex;
            private Boolean _isNodeAdded = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="HotPixelSnapAction"/> class.
            /// </summary>
            public HotPixelSnapAction(HotPixel<TCoordinate> hotPixel, SegmentString<TCoordinate> parentEdge, Int32 vertexIndex)
            {
                _hotPixel = hotPixel;
                _parentEdge = parentEdge;
                _vertexIndex = vertexIndex;
            }

            public Boolean IsNodeAdded
            {
                get { return _isNodeAdded; }
            }

            public override void Select(MonotoneChain<TCoordinate> mc, Int32 startIndex)
            {
                SegmentString<TCoordinate> ss = (SegmentString<TCoordinate>) mc.Context;
                
                // don't snap a vertex to itself
                if (_parentEdge != null)
                {
                    if (ss == _parentEdge && startIndex == _vertexIndex)
                    {
                        return;
                    }
                }

                _isNodeAdded = SimpleSnapRounder.AddSnappedNode(_hotPixel, ss, startIndex);
            }
        }
    }
}