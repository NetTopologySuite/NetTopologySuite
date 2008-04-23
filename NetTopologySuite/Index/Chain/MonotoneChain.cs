using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
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
    /// <para>
    /// One of the goals of this implementation of MonotoneChains is to be
    /// as space and time efficient as possible. One design choice that aids this
    /// is that a MonotoneChain is based on a subset of a list of points.
    /// This means that new arrays of points (potentially very large) do not
    /// have to be allocated.
    /// </para>
    /// MonotoneChains support the following kinds of queries:
    /// <list type="table">
    /// <item>
    /// <term>Envelope select</term>
    /// <description>
    /// Determines all the segments in the chain which
    /// intersect a given envelope.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Overlap</term>
    /// <description>
    /// Determines all the pairs of segments in two chains whose
    /// envelopes overlap.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class MonotoneChain<TCoordinate> : IBoundable<IExtents<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly ICoordinateSequence<TCoordinate> _coordinates;
        private readonly Int32 _start;
        private readonly Int32 _end;
        private IExtents<TCoordinate> _extents;
        private readonly Object _context; // user-defined information
        private Int32 _id; // useful for optimizing chain comparisons

        public MonotoneChain(IGeometryFactory<TCoordinate> geoFactory, 
                             ICoordinateSequence<TCoordinate> pts, 
                             Int32 start, Int32 end, Object context)
        {
            if (pts == null) throw new ArgumentNullException("pts");

            _geoFactory = geoFactory;
            _coordinates = pts;
            _start = start;
            _end = end;
            _context = context;
        }

        public Int32 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Object Context
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
                    _extents = _geoFactory.CreateExtents(p0, p1);
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

        public LineSegment<TCoordinate> GetLineSegment(Int32 index)
        {
            Pair<TCoordinate> pair = Slice.GetPairAt(_coordinates, index).Value;
            return new LineSegment<TCoordinate>(pair);
        }

        /// <summary>
        /// Return the subsequence of coordinates forming this chain.
        /// </summary>
        public ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                return _coordinates.Slice(_start, _end);
            }
        }

        /// <summary> 
        /// Determine all the line segments in the chain whose envelopes intersect
        /// the <paramref name="searchExtents"/>, and return them.
        /// </summary>
        public IEnumerable<LineSegment<TCoordinate>> Select(IExtents<TCoordinate> searchExtents)
        {
            return computeSelect(searchExtents, _start, _end);
        }

        public IEnumerable<Int32> SelectIndexes(IExtents<TCoordinate> searchExtents)
        {
            return computeSelectIndexes(searchExtents, _start, _end);
        }

        public IEnumerable<Pair<LineSegment<TCoordinate>>> Overlap(MonotoneChain<TCoordinate> other)
        {
            return computeOverlaps(_start, _end, other, other._start, other._end);
        }

        public IEnumerable<Pair<Int32>> OverlapIndexes(MonotoneChain<TCoordinate> other)
        {
            return computeOverlapIndexes(_start, _end, other, other._start, other._end);
        }

        #region IBoundable<IExtents<TCoordinate>> Members

        IExtents<TCoordinate> IBoundable<IExtents<TCoordinate>>.Bounds
        {
            get { return Extents; }
        }

        Boolean IBoundable<IExtents<TCoordinate>>.Intersects(IExtents<TCoordinate> bounds)
        {
            return Extents.Intersects(bounds);
        }

        #endregion

        #region Private helper routines

        private IEnumerable<LineSegment<TCoordinate>> computeSelect(
                                                IExtents<TCoordinate> searchExtents, 
                                                Int32 start, Int32 end)
        {
            foreach (KeyValuePair<Int32, LineSegment<TCoordinate>> pair in computeSelectWithIndexes(searchExtents, start, end))
            {
                yield return pair.Value;
            }
        }

        private IEnumerable<Int32> computeSelectIndexes(IExtents<TCoordinate> searchExtents, 
                                                        Int32 start, Int32 end)
        {
            foreach (KeyValuePair<Int32, LineSegment<TCoordinate>> pair in computeSelectWithIndexes(searchExtents, start, end))
            {
                yield return pair.Key;
            }
        }

        private IEnumerable<KeyValuePair<Int32, LineSegment<TCoordinate>>> computeSelectWithIndexes(
                                                    IExtents<TCoordinate> searchExtents, 
                                                    Int32 start, Int32 end)
        {
            TCoordinate p0 = Slice.GetAt(_coordinates, start);
            TCoordinate p1 = Slice.GetAt(_coordinates, end);

            IExtents<TCoordinate> currentExtents = _geoFactory.CreateExtents(p0, p1);

            // terminating condition for the recursion
            if (end - start == 1)
            {
                LineSegment<TCoordinate> selectedSegment = GetLineSegment(start);
                yield return new KeyValuePair<Int32, LineSegment<TCoordinate>>(start, selectedSegment);
            }

            // nothing to do if the envelopes don't overlap
            if (!searchExtents.Intersects(currentExtents))
            {
                yield break;
            }

            // the chains overlap, so split each in half and iterate  (binary search)
            Int32 mid = (start + end) / 2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start < mid)
            {
                computeSelect(searchExtents, start, mid);
            }

            if (mid < end)
            {
                computeSelect(searchExtents, mid, end);
            }
        }

        private IEnumerable<Pair<LineSegment<TCoordinate>>> computeOverlaps(
                                                        Int32 start0, Int32 end0, 
                                                        MonotoneChain<TCoordinate> other, 
                                                        Int32 start1, Int32 end1)
        {
            IEnumerable<Pair<Pair<Int32, LineSegment<TCoordinate>>>> overlaps 
                = computeOverlapsWithIndexes(start0, end0, other, start1, end1);

            foreach (Pair<Pair<Int32, LineSegment<TCoordinate>>> pair in overlaps)
            {
                yield return new Pair<LineSegment<TCoordinate>>(pair.First.Second, 
                                                                pair.Second.Second);
            }
        }

        private IEnumerable<Pair<Int32>> computeOverlapIndexes(
                                                        Int32 start0, Int32 end0, 
                                                        MonotoneChain<TCoordinate> other, 
                                                        Int32 start1, Int32 end1)
        {
            // this is starting to look like Lisp, except with angle brackets...
            IEnumerable<Pair<Pair<Int32, LineSegment<TCoordinate>>>> overlaps
                = computeOverlapsWithIndexes(start0, end0, other, start1, end1);

            foreach (Pair<Pair<Int32, LineSegment<TCoordinate>>> pair in overlaps)
            {
                yield return new Pair<Int32>(pair.First.First, pair.Second.First);
            }
        }

        private IEnumerable<Pair<Pair<Int32, LineSegment<TCoordinate>>>> computeOverlapsWithIndexes(
                                                        Int32 start0, Int32 end0, 
                                                        MonotoneChain<TCoordinate> other, 
                                                        Int32 start1, Int32 end1)
        {
            TCoordinate p00 = Slice.GetAt(_coordinates, start0);
            TCoordinate p01 = Slice.GetAt(_coordinates, end0);

            TCoordinate p10 = Slice.GetAt(other._coordinates, start1);
            TCoordinate p11 = Slice.GetAt(other._coordinates, end1);

            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                LineSegment<TCoordinate> s0 = GetLineSegment(start0);
                LineSegment<TCoordinate> s1 = other.GetLineSegment(start1);

                Pair<Int32, LineSegment<TCoordinate>> first
                    = new Pair<Int32, LineSegment<TCoordinate>>(start0, s0);
                Pair<Int32, LineSegment<TCoordinate>> second
                    = new Pair<Int32, LineSegment<TCoordinate>>(start1, s1);

                yield return new Pair<Pair<Int32, LineSegment<TCoordinate>>>(first, second);
            }

            // nothing to do if the envelopes of these chains don't overlap
            if (!Extents<TCoordinate>.Intersects(p00, p01, p10, p11))
            {
                yield break;
            }

            // the chains overlap, so split each in half and iterate  (binary search)
            Int32 mid0 = (start0 + end0) / 2;
            Int32 mid1 = (start1 + end1) / 2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start0 < mid0)
            {
                if (start1 < mid1)
                {
                    computeOverlaps(start0, mid0, other, start1, mid1);
                }

                if (mid1 < end1)
                {
                    computeOverlaps(start0, mid0, other, mid1, end1);
                }
            }

            if (mid0 < end0)
            {
                if (start1 < mid1)
                {
                    computeOverlaps(mid0, end0, other, start1, mid1);
                }

                if (mid1 < end1)
                {
                    computeOverlaps(mid0, end0, other, mid1, end1);
                }
            }
        }
        #endregion
    }
}