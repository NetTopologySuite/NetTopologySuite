using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary> 
    /// MonotoneChains are a way of partitioning the segments of an edge to
    /// allow for fast searching of intersections.
    /// </summary>
    /// <remarks>
    /// They have the following properties:
    /// the segments within a monotone chain will never intersect each other, and 
    /// the envelope of any contiguous subset of the segments in a monotone chain
    /// is simply the envelope of the endpoints of the subset.
    /// Property 1 means that there is no need to test pairs of segments from within
    /// the same monotone chain for intersection.
    /// Property 2 allows
    /// binary search to be used to find the intersection points of two monotone chains.
    /// For many types of real-world data, these properties eliminate a large number of
    /// segment comparisons, producing substantial speed gains.
    /// </remarks>
    public class MonotoneChainEdge<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly Edge<TCoordinate> _edge;
        private readonly List<TCoordinate> _coordinates = new List<TCoordinate>();

        // the lists of start/end indexes of the monotone chains.
        // Includes the end point of the edge as a sentinel
        private readonly IList<Int32> _startIndex;

        // these envelopes are created once and reused
        private readonly IExtents<TCoordinate> _extents1 = new Extents<TCoordinate>();
        private readonly IExtents<TCoordinate> _extents2 = new Extents<TCoordinate>();

        public MonotoneChainEdge(Edge<TCoordinate> edge)
        {
            _edge = edge;
            _coordinates.AddRange(edge.Coordinates);
            MonotoneChainIndexer<TCoordinate> mcb = new MonotoneChainIndexer<TCoordinate>();
            _startIndex = mcb.GetChainStartIndices(_coordinates);
        }

        public IEnumerable<TCoordinate> Coordinates
        {
            get { return _coordinates; }
        }

        public IList<Int32> StartIndexes
        {
            get { return _startIndex; }
        }

        public Double GetMinX(Int32 chainIndex)
        {
            Double x1, x2;
            getXOrdinateAtIndex(chainIndex, out x1, out x2);
            return (x1 < x2) ? x1 : x2;
        }

        public Double GetMaxX(Int32 chainIndex)
        {
            Double x1, x2;
            getXOrdinateAtIndex(chainIndex, out x1, out x2);
            return (x1 > x2) ? x1 : x2;
        }

        public void ComputeIntersects(MonotoneChainEdge<TCoordinate> monotoneChainEdge, SegmentIntersector<TCoordinate> segmentIntersector)
        {
            if (monotoneChainEdge == null)
            {
                throw new ArgumentNullException("monotoneChainEdge");
            }

            if (segmentIntersector == null)
            {
                throw new ArgumentNullException("segmentIntersector");
            }

            for (Int32 i = 0; i < _startIndex.Count - 1; i++)
            {
                for (Int32 j = 0; j < monotoneChainEdge.StartIndexes.Count - 1; j++)
                {
                    ComputeIntersectsForChain(i, monotoneChainEdge, j, segmentIntersector);
                }
            }
        }

        public void ComputeIntersectsForChain(Int32 chainIndex0,
            MonotoneChainEdge<TCoordinate> monotoneChainEdge, Int32 chainIndex1,
            SegmentIntersector<TCoordinate> si)
        {
            computeIntersectsForChain(_startIndex[chainIndex0], _startIndex[chainIndex0 + 1],
                monotoneChainEdge, monotoneChainEdge._startIndex[chainIndex1],
                monotoneChainEdge._startIndex[chainIndex1 + 1], si);
        }

        private void getXOrdinateAtIndex(Int32 chainIndex, out Double x1, out Double x2)
        {
            Pair<TCoordinate> pair = Slice.GetPairAt(_coordinates, _startIndex[chainIndex]).Value;
            x1 = pair.First[Ordinates.X];
            x2 = pair.Second[Ordinates.X];
        }

        private void computeIntersectsForChain(Int32 start0, Int32 end0,
            MonotoneChainEdge<TCoordinate> monotoneChainEdge, Int32 start1, Int32 end1,
            SegmentIntersector<TCoordinate> segmentIntersector)
        {
            TCoordinate p00 = _coordinates[start0];
            TCoordinate p01 = _coordinates[end0];
            TCoordinate p10 = monotoneChainEdge._coordinates[start1];
            TCoordinate p11 = monotoneChainEdge._coordinates[end1];

            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                segmentIntersector.AddIntersections(_edge, start0, monotoneChainEdge._edge, start1);
                return;
            }

            // nothing to do if the envelopes of these chains don't overlap
            _extents1.ExpandToInclude(p00, p01);
            _extents2.ExpandToInclude(p10, p11);

            if (!_extents1.Intersects(_extents2))
            {
                return;
            }

            // the chains overlap, so split each in half and iterate  (binary search)
            Int32 mid0 = (start0 + end0) / 2;
            Int32 mid1 = (start1 + end1) / 2;

            // check terminating conditions before recursing
            if (start0 < mid0)
            {
                if (start1 < mid1)
                {
                    computeIntersectsForChain(start0, mid0, monotoneChainEdge, start1, mid1, segmentIntersector);
                }
                if (mid1 < end1)
                {
                    computeIntersectsForChain(start0, mid0, monotoneChainEdge, mid1, end1, segmentIntersector);
                }
            }

            if (mid0 < end0)
            {
                if (start1 < mid1)
                {
                    computeIntersectsForChain(mid0, end0, monotoneChainEdge, start1, mid1, segmentIntersector);
                }
                if (mid1 < end1)
                {
                    computeIntersectsForChain(mid0, end0, monotoneChainEdge, mid1, end1, segmentIntersector);
                }
            }
        }
    }
}