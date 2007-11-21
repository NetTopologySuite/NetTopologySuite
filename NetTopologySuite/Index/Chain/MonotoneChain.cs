using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary> 
    /// A <see cref="MonotoneChain{TCoordinate}"/> is a way of partitioning 
    /// the segments of a linestring to allow for fast searching of intersections.
    /// </summary>
    /// <remarks>
    /// <see cref="MonotoneChain{TCoordinate}"/>s have the following properties:
    /// the segments within a monotone chain will never intersect each other
    /// the envelope of any contiguous subset of the segments in a monotone chain
    /// is equal to the envelope of the endpoints of the subset.
    /// Property 1 means that there is no need to test pairs of segments from within
    /// the same monotone chain for intersection.
    /// Property 2 allows
    /// binary search to be used to find the intersection points of two monotone chains.
    /// For many types of real-world data, these properties eliminate a large number of
    /// segment comparisons, producing substantial speed gains.
    /// One of the goals of this implementation of MonotoneChains is to be
    /// as space and time efficient as possible. One design choice that aids this
    /// is that a MonotoneChain is based on a subarray of a list of points.
    /// This means that new arrays of points (potentially very large) do not
    /// have to be allocated.
    /// MonotoneChains support the following kinds of queries:
    /// Envelope select: determine all the segments in the chain which
    /// intersect a given envelope.
    /// Overlap: determine all the pairs of segments in two chains whose
    /// envelopes overlap.
    /// This implementation of MonotoneChains uses the concept of internal iterators
    /// to return the resultsets for the above queries.
    /// This has time and space advantages, since it
    /// is not necessary to build lists of instantiated objects to represent the segments
    /// returned by the query.
    /// However, it does mean that the queries are not thread-safe.
    /// </remarks>
    public class MonotoneChain<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly IEnumerable<TCoordinate> _coordinates;
        private readonly Int32 _start;
        private readonly Int32 _end;
        private IExtents<TCoordinate> _extents = null;
        private readonly object _context = null; // user-defined information
        private Int32 id; // useful for optimizing chain comparisons

        public MonotoneChain(IEnumerable<TCoordinate> pts, Int32 start, Int32 end, object context)
        {
            _coordinates = pts;
            _start = start;
            _end = end;
            _context = context;
        }

        public Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        public object Context
        {
            get { return _context; }
        }

        public IExtents<TCoordinate> Extents
        {
            get
            {
                if (_extents == null)
                {
                    TCoordinate p0 = Slice.GetAt(_coordinates, _start);
                    TCoordinate p1 = Slice.GetAt(_coordinates, _end);
                    _extents = new Extents<TCoordinate>(p0, p1);
                }

                return _extents;
            }
        }

        public Int32 StartIndex
        {
            get { return _start; }
        }

        public Int32 EndIndex
        {
            get { return _end; }
        }

        public void GetLineSegment(Int32 index, ref LineSegment<TCoordinate> ls)
        {
            Pair<TCoordinate> pair = Slice.GetPairAt(_coordinates, index);
            ls.P0 = pair.First;
            ls.P1 = pair.Second;
        }

        /// <summary>
        /// Return the subsequence of coordinates forming this chain.
        /// Allocates a new array to hold the Coordinates.
        /// </summary>
        public IEnumerable<TCoordinate> Coordinates
        {
            get
            {
                foreach (TCoordinate coordinate in Slice.GetRange(_coordinates, _start, _end))
                {
                    yield return coordinate;
                }
            }
        }

        /// <summary> 
        /// Determine all the line segments in the chain whose envelopes overlap
        /// the searchEnvelope, and process them.
        /// </summary>
        public void Select(IExtents<TCoordinate> searchExtents, MonotoneChainSelectAction<TCoordinate> mcs)
        {
            ComputeSelect(searchExtents, _start, _end, mcs);
        }

        private void ComputeSelect(IExtents<TCoordinate> searchExtents, Int32 start0, Int32 end0, MonotoneChainSelectAction<TCoordinate> mcs)
        {
            TCoordinate p0 = Slice.GetAt(_coordinates, start0);
            TCoordinate p1 = Slice.GetAt(_coordinates, end0);

            mcs.SearchExtents.ExpandToInclude(p0, p1);

            // terminating condition for the recursion
            if (end0 - start0 == 1)
            {
                mcs.Select(this, start0);
                return;
            }

            // nothing to do if the envelopes don't overlap
            if (!searchExtents.Intersects(mcs.SearchExtents))
            {
                return;
            }

            // the chains overlap, so split each in half and iterate  (binary search)
            Int32 mid = (start0 + end0)/2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start0 < mid)
            {
                ComputeSelect(searchExtents, start0, mid, mcs);
            }

            if (mid < end0)
            {
                ComputeSelect(searchExtents, mid, end0, mcs);
            }
        }

        public void ComputeOverlaps(MonotoneChain<TCoordinate> mc, MonotoneChainOverlapAction<TCoordinate> mco)
        {
            ComputeOverlaps(_start, _end, mc, mc._start, mc._end, mco);
        }

        private void ComputeOverlaps(Int32 start0, Int32 end0, MonotoneChain<TCoordinate> mc, 
            Int32 start1, Int32 end1, MonotoneChainOverlapAction<TCoordinate> mco)
        {
            TCoordinate p00 = Slice.GetAt(_coordinates, start0);
            TCoordinate p01 = Slice.GetAt(_coordinates, end0);
            TCoordinate p10 = Slice.GetAt(mc._coordinates, start1);
            TCoordinate p11 = Slice.GetAt(mc._coordinates, end1);

            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                mco.Overlap(this, start0, mc, start1);
                return;
            }

            // nothing to do if the envelopes of these chains don't overlap
            mco.SearchExtents1.ExpandToInclude(p00, p01);
            mco.SearchExtents2.ExpandToInclude(p10, p11);
            
            if (! mco.SearchExtents1.Intersects(mco.SearchExtents2))
            {
                return;
            }

            // the chains overlap, so split each in half and iterate  (binary search)
            Int32 mid0 = (start0 + end0)/2;
            Int32 mid1 = (start1 + end1)/2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start0 < mid0)
            {
                if (start1 < mid1)
                {
                    ComputeOverlaps(start0, mid0, mc, start1, mid1, mco);
                }

                if (mid1 < end1)
                {
                    ComputeOverlaps(start0, mid0, mc, mid1, end1, mco);
                }
            }

            if (mid0 < end0)
            {
                if (start1 < mid1)
                {
                    ComputeOverlaps(mid0, end0, mc, start1, mid1, mco);
                }

                if (mid1 < end1)
                {
                    ComputeOverlaps(mid0, end0, mc, mid1, end1, mco);
                }
            }
        }
    }
}