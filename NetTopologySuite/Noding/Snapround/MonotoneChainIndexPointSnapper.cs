using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="NodedSegmentString{TCoordinate}" />s in 
    /// a <see cref="ISpatialIndex{TCoordinate,TItem}" /> containing
    /// <see cref="MonotoneChain{TCoordinate}" />s to a given
    /// <see cref="HotPixel{TCoordinate}" />.
    /// </summary>
    public class MonotoneChaintIndexPointSnapper<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        // [codekaizen] This doesn't appear to be used at all...
        // Public in java code... temporary modified for "safe assembly" in Sql2005
        //internal static readonly Int32 numberSnaps = 0;

        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();
        private readonly ISpatialIndex<IExtents<TCoordinate>, MonotoneChain<TCoordinate>> _index = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonotoneChaintIndexPointSnapper{TCoordinate}"/> class.
        /// </summary>
        public MonotoneChaintIndexPointSnapper(IEnumerable<MonotoneChain<TCoordinate>> chains, ISpatialIndex<IExtents<TCoordinate>, MonotoneChain<TCoordinate>> index)
        {
            _monoChains.AddRange(chains);
            _index = index;
        }

        /// <summary>
        /// Snaps (nodes) all interacting segments to this hot pixel.
        /// The hot pixel may represent a vertex of an edge,
        /// in which case this routine uses the optimization
        /// of not noding the vertex itself.
        /// </summary>
        /// <param name="hotPixel">The hot pixel to snap to.</param>
        /// <param name="parentEdge">The edge containing the vertex, if applicable, or <see langword="null" />.</param>
        /// <returns><see langword="true"/> if a node was added for this pixel.</returns>
        public Boolean Snap(HotPixel<TCoordinate> hotPixel, NodedSegmentString<TCoordinate> parentEdge, Int32 vertexIndex)
        {
            IExtents<TCoordinate> pixelExtents = hotPixel.GetSafeExtents();
            //HotPixelSnapAction hotPixelSnapAction = new HotPixelSnapAction(hotPixel, parentEdge, vertexIndex);

            // This used to be in the class HotPixelSnapAction, but was refactored to 
            // move responsibility for visiting index nodes to the querying class
            Boolean isNodeAdded = false;

            foreach (MonotoneChain<TCoordinate> chain in _index.Query(pixelExtents))
            {
                NodedSegmentString<TCoordinate> segmentString = chain.Context as NodedSegmentString<TCoordinate>;
                Debug.Assert(segmentString != null);

                foreach (Int32 startIndex in chain.SelectIndexes(pixelExtents))
                {
                    // don't snap a vertex to itself
                    if (parentEdge != null)
                    {
                        if (segmentString == parentEdge && startIndex == vertexIndex)
                        {
                            continue;
                        }
                    }

                    isNodeAdded = SimpleSnapRounder<TCoordinate>.AddSnappedNode(hotPixel, segmentString, startIndex);
                }
            }

            return isNodeAdded;
        }

        public Boolean Snap(HotPixel<TCoordinate> hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

        //private class QueryVisitor : IItemVisitor
        //{
        //    private readonly IExtents<TCoordinate> _extents = null;
        //    private readonly HotPixelSnapAction _action = null;

        //    public QueryVisitor(IExtents<TCoordinate> extents, HotPixelSnapAction action)
        //    {
        //        _extents = extents;
        //        _action = action;
        //    }

        //    public void VisitItem(object item)
        //    {
        //        MonotoneChain<TCoordinate> testChain = (MonotoneChain<TCoordinate>) item;
        //        testChain.Select(_extents, _action);
        //    }
        //}

        //public class HotPixelSnapAction : MonotoneChainSelectAction<TCoordinate>
        //{
        //    private readonly HotPixel<TCoordinate> _hotPixel;
        //    private readonly SegmentString<TCoordinate> _parentEdge = null;
        //    private readonly Int32 _vertexIndex;
        //    private Boolean _isNodeAdded = false;

        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="HotPixelSnapAction"/> class.
        //    /// </summary>
        //    public HotPixelSnapAction(HotPixel<TCoordinate> hotPixel, SegmentString<TCoordinate> parentEdge, Int32 vertexIndex)
        //    {
        //        _hotPixel = hotPixel;
        //        _parentEdge = parentEdge;
        //        _vertexIndex = vertexIndex;
        //    }

        //    public Boolean IsNodeAdded
        //    {
        //        get { return _isNodeAdded; }
        //    }

        //    public override void Select(MonotoneChain<TCoordinate> mc, Int32 startIndex)
        //    {
        //        SegmentString<TCoordinate> segmentString = mc.Context as SegmentString<TCoordinate>;

        //        Debug.Assert(segmentString != null);
                
        //        // don't snap a vertex to itself
        //        if (_parentEdge != null)
        //        {
        //            if (segmentString == _parentEdge && startIndex == _vertexIndex)
        //            {
        //                return;
        //            }
        //        }

        //        _isNodeAdded = SimpleSnapRounder<TCoordinate>.AddSnappedNode(_hotPixel, segmentString, startIndex);
        //    }
        //}
    }
}