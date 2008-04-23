using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary> 
    /// <see cref="MonotoneChain{TCoordinate}"/>s are a way of 
    /// partitioning the segments of an edge to allow for fast 
    /// searching of intersections.
    /// </summary>
    /// <remarks>
    /// Monotone chains have the following properties:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// the segments within a monotone chain will never intersect each other, and
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the envelope of any contiguous subset of the segments in a monotone chain
    /// is simply the envelope of the endpoints of the subset.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Property 1 means that there is no need to test pairs of segments from within
    /// the same monotone chain for intersection.
    /// </para>
    /// <para>
    /// Property 2 allows binary search to be used to find the intersection points 
    /// of two monotone chains.
    /// </para>
    /// <para>
    /// For many types of real-world data, these properties eliminate a large number of
    /// segment comparisons, producing substantial speed gains.
    /// </para>
    /// </remarks>
    public class MonotoneChainEdge<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly Edge<TCoordinate> _edge;
        private readonly ICoordinateSequence<TCoordinate> _coordinates;

        // the lists of start/end indexes of the monotone chains.
        // Includes the end point of the edge as a sentinel
        private Int32[] _startIndexes;

        // these envelopes are created once and reused
        private readonly IExtents<TCoordinate> _extents1;
        private readonly IExtents<TCoordinate> _extents2;

        public MonotoneChainEdge(Edge<TCoordinate> edge, 
                                 IGeometryFactory<TCoordinate> geoFactory)
        {
            _edge = edge;
            _extents1 = geoFactory.CreateExtents();
            _extents2 = geoFactory.CreateExtents();
            _coordinates = edge.Coordinates;
        }

        public IEnumerable<TCoordinate> Coordinates
        {
            get { return _coordinates; }
        }

        public Int32[] StartIndexes
        {
            get 
            {
                if(_startIndexes == null)
                {
                    //MonotoneChainIndexer<TCoordinate> mcBuilder
                    //    = new MonotoneChainIndexer<TCoordinate>();

                    // TODO: evaluate if instatiating list here is better than
                    // keeping it lazy
                    IEnumerable<Int32> starts =
                        MonotoneChainBuilder.GetChainStartIndices(_coordinates);
                    _startIndexes = Enumerable.ToArray(starts);
                }

                return _startIndexes;
            }
        }

        public Double GetMinX(Int32 chainIndex)
        {
            Double x1, x2;
            getXOrdinateAtIndex(chainIndex, out x1, out x2);
            return Math.Min(x1, x2);
        }

        public Double GetMaxX(Int32 chainIndex)
        {
            Double x1, x2;
            getXOrdinateAtIndex(chainIndex, out x1, out x2);
            return Math.Max(x1, x2);
        }

        public void ComputeIntersects(MonotoneChainEdge<TCoordinate> other, 
                                      SegmentIntersector<TCoordinate> segmentIntersector)
        {
            if (other == null) throw new ArgumentNullException("other");
            if (segmentIntersector == null) throw new ArgumentNullException("segmentIntersector");

            for (Int32 i = 0; i < StartIndexes.Length - 1; i++)
            {
                for (Int32 j = 0; j < other.StartIndexes.Length - 1; j++)
                {
                    ComputeIntersectsForChain(i, other, j, segmentIntersector);
                }
            }
        }

        public void ComputeIntersectsForChain(Int32 chainIndex0,
                                              MonotoneChainEdge<TCoordinate> other, 
                                              Int32 chainIndex1,
                                              SegmentIntersector<TCoordinate> si)
        {
            Int32[] startIndexes0 = StartIndexes;
            Int32[] startIndexes1 = other.StartIndexes;

            computeIntersectsForChain(startIndexes0[chainIndex0],
                                      startIndexes0[chainIndex0 + 1],
                                      other,
                                      startIndexes1[chainIndex1],
                                      startIndexes1[chainIndex1 + 1], 
                                      si);
        }

        private void getXOrdinateAtIndex(Int32 chainIndex, out Double x1, out Double x2)
        {
            Pair<TCoordinate>? segment = Slice.GetPairAt(_coordinates, 
                                                         _startIndexes[chainIndex]);
            Debug.Assert(segment != null);
            Pair<TCoordinate> segmentValue = segment.Value;
            x1 = segmentValue.First[Ordinates.X];
            x2 = segmentValue.Second[Ordinates.X];
        }

        private void computeIntersectsForChain(Int32 start0, Int32 end0,
                                               MonotoneChainEdge<TCoordinate> other, 
                                               Int32 start1, Int32 end1,
                                               SegmentIntersector<TCoordinate> segmentIntersector)
        {
            TCoordinate p00 = _coordinates[start0];
            TCoordinate p01 = _coordinates[end0];
            TCoordinate p10 = other._coordinates[start1];
            TCoordinate p11 = other._coordinates[end1];

            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                segmentIntersector.AddIntersections(_edge, start0, other._edge, start1);
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
                    computeIntersectsForChain(start0, mid0, 
                                              other, 
                                              start1, mid1, 
                                              segmentIntersector);
                }
                if (mid1 < end1)
                {
                    computeIntersectsForChain(start0, mid0, 
                                              other, 
                                              mid1, end1, 
                                              segmentIntersector);
                }
            }

            if (mid0 < end0)
            {
                if (start1 < mid1)
                {
                    computeIntersectsForChain(mid0, end0, 
                                              other, 
                                              start1, mid1, 
                                              segmentIntersector);
                }
                if (mid1 < end1)
                {
                    computeIntersectsForChain(mid0, end0, 
                                              other, 
                                              mid1, end1, 
                                              segmentIntersector);
                }
            }
        }
    }
}