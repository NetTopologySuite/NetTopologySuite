using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// "Snaps" all <see cref="NodedSegmentString{TCoordinate}" />s in 
    /// a <see cref="ISpatialIndex{TBounds,TItem}" /> containing
    /// <see cref="MonotoneChain{TCoordinate}" />s to a given
    /// <see cref="HotPixel{TCoordinate}" />.
    /// </summary>
    public class MonotoneChainIndexPointSnapper<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        // [codekaizen] This doesn't appear to be used at all...
        // Public in java code... temporary modified for "safe assembly" in Sql2005
        //internal static readonly Int32 numberSnaps = 0;

        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly ISpatialIndex<IExtents<TCoordinate>, MonotoneChain<TCoordinate>> _index;
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MonotoneChainIndexPointSnapper{TCoordinate}"/> class.
        /// </summary>
        public MonotoneChainIndexPointSnapper(
            IGeometryFactory<TCoordinate> geoFactory,
            IEnumerable<MonotoneChain<TCoordinate>> chains,
            ISpatialIndex<IExtents<TCoordinate>,
                MonotoneChain<TCoordinate>> index)
        {
            _geoFactory = geoFactory;
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
        /// <param name="vertexIndex"></param>
        /// <returns><see langword="true"/> if a node was added for this pixel.</returns>
        public Boolean Snap(HotPixel<TCoordinate> hotPixel, ISegmentString<TCoordinate> parentEdge,
                            Int32 vertexIndex)
        {
            IExtents<TCoordinate> pixelExtents = hotPixel.GetSafeExtents();

            // This used to be in the class HotPixelSnapAction, but was refactored to 
            // move responsibility for visiting index nodes to the querying class
            foreach (MonotoneChain<TCoordinate> chain in _index.Query(pixelExtents))
            {
                NodedSegmentString<TCoordinate> segmentString = chain.Context as NodedSegmentString<TCoordinate>;
                Debug.Assert(segmentString != null);

                foreach (Int32 startIndex in chain.SelectIndexes(pixelExtents))
                {
                    // don't snap a vertex to itself
                    if (parentEdge != null)
                        if (segmentString == parentEdge && startIndex == vertexIndex)
                            continue;

                    if (SimpleSnapRounder<TCoordinate>.AddSnappedNode(hotPixel, segmentString, startIndex))
                        return true;
                }
            }

            return false;
        }

        public Boolean Snap(HotPixel<TCoordinate> hotPixel)
        {
            return Snap(hotPixel, null, -1);
        }

    }
}