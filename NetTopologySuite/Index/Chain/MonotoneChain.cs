using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Chain
{
    /// <summary>
    /// MonotoneChains are a way of partitioning the segments of a linestring to
    /// allow for fast searching of intersections.
    /// </summary>
    /// <remarks>
    /// <para>
    /// They have the following properties:
    /// <list>
    /// <item>the segments within a monotone chain never intersect each other</item>
    /// <item>the envelope of any contiguous subset of the segments in a monotone chain
    /// is equal to the envelope of the endpoints of the subset.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Property 1 means that there is no need to test pairs of segments from within
    /// the same monotone chain for intersection.</para>
    /// <para>Property 2 allows an efficient
    /// binary search to be used to find the intersection points of two monotone chains.
    /// For many types of real-world data, these properties eliminate a large number of
    /// segment comparisons, producing substantial speed gains.</para>
    /// <para>
    /// One of the goals of this implementation of MonotoneChains is to be
    /// as space and time efficient as possible. One design choice that aids this
    /// is that a MonotoneChain is based on a subarray of a list of points.
    /// This means that new arrays of points (potentially very large) do not
    /// have to be allocated.</para>
    /// <para>
    /// MonotoneChains support the following kinds of queries:
    /// <list type="Table">
    /// <item>Envelope select</item><description>determine all the segments in the chain which
    /// intersect a given envelope.</description>
    /// <item>Overlap</item><description>determine all the pairs of segments in two chains whose
    /// envelopes overlap.</description>
    /// </list>
    /// </para>
    /// <para>
    /// This implementation of MonotoneChains uses the concept of internal iterators
    /// (<see cref="MonotoneChainSelectAction"/> and <see cref="MonotoneChainOverlapAction"/>)
    /// to return the resultsets for the above queries.
    /// This has time and space advantages, since it
    /// is not necessary to build lists of instantiated objects to represent the segments
    /// returned by the query.
    /// Queries made in this manner are thread-safe.
    /// </para>
    ///</remarks>
    public class MonotoneChain
    {
        private readonly Coordinate[] _pts;
        private readonly int _start;
        private readonly int _end;
        private Envelope _env;
        private readonly object _context;  // user-defined information

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="context"></param>
        public MonotoneChain(Coordinate[] pts, int start, int end, object context)
        {
            _pts = pts;
            _start = start;
            _end = end;
            _context = context;
        }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets the chain's context
        /// </summary>
        public object Context => _context;

        /// <summary>
        /// Gets the chain's envelope
        /// </summary>
        public Envelope Envelope
        {
            get
            {
                if (_env == null)
                {
                    var p0 = _pts[_start];
                    var p1 = _pts[_end];
                    _env = new Envelope(p0, p1);
                }
                return _env;
            }
        }

        /// <summary>
        /// Gets the start index
        /// </summary>
        public int StartIndex => _start;

        /// <summary>
        /// Gets the end index of the underlying linestring
        /// </summary>
        public int EndIndex => _end;

        /// <summary>
        /// Gets the line segment starting at <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index of the segment</param>
        /// <param name="ls">The line segment to extract to</param>
        public void GetLineSegment(int index, ref LineSegment ls)
        {
            ls.P0 = _pts[index];
            ls.P1 = _pts[index + 1];
        }

        /// <summary>
        /// Return the subsequence of coordinates forming this chain.
        /// Allocates a new array to hold the Coordinates.
        /// </summary>
        public Coordinate[] Coordinates
        {
            get
            {
                var coord = new Coordinate[_end - _start + 1];
                int index = 0;
                for (int i = _start; i <= _end; i++)
                    coord[index++] = _pts[i];
                return coord;
            }
        }

        /// <summary>
        /// Determine all the line segments in the chain whose envelopes overlap
        /// the searchEnvelope, and process them.
        /// </summary>
        /// <remarks>
        /// The monotone chain search algorithm attempts to optimize
        /// performance by not calling the select action on chain segments
        /// which it can determine are not in the search envelope.
        /// However, it *may* call the select action on segments
        /// which do not intersect the search envelope.
        /// This saves on the overhead of checking envelope intersection
        /// each time, since clients may be able to do this more efficiently.
        /// </remarks>
        /// <param name="searchEnv">The search envelope</param>
        /// <param name="mcs">The select action to execute on selected segments</param>
        public void Select(Envelope searchEnv, MonotoneChainSelectAction mcs)
        {
            ComputeSelect(searchEnv, _start, _end, mcs);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="start0"></param>
        /// <param name="end0"></param>
        /// <param name="mcs"></param>
        private void ComputeSelect(Envelope searchEnv, int start0, int end0, MonotoneChainSelectAction mcs)
        {
            var p0 = _pts[start0];
            var p1 = _pts[end0];

            // terminating condition for the recursion
            if (end0 - start0 == 1)
            {
                mcs.Select(this, start0);
                return;
            }
            // nothing to do if the envelopes don't overlap
            if (!searchEnv.Intersects(p0, p1))
                return;

            // the chains overlap, so split each in half and iterate  (binary search)
            int mid = (start0 + end0) / 2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start0 < mid)
                ComputeSelect(searchEnv, start0, mid, mcs);
            if (mid < end0)
                ComputeSelect(searchEnv, mid, end0, mcs);
        }

        /// <summary>
        /// Determine all the line segments in two chains which may overlap, and process them.
        /// </summary>
        /// <remarks>
        /// The monotone chain search algorithm attempts to optimize
        /// performance by not calling the overlap action on chain segments
        /// which it can determine do not overlap.
        /// However, it *may* call the overlap action on segments
        /// which do not actually interact.
        /// This saves on the overhead of checking intersection
        /// each time, since clients may be able to do this more efficiently.
        /// </remarks>
        /// <param name="mc">The monotone chain</param>
        /// <param name="mco">The overlap action to execute on selected segments</param>
        public  void ComputeOverlaps(MonotoneChain mc, MonotoneChainOverlapAction mco)
        {
            ComputeOverlaps(_start, _end, mc, mc._start, mc._end, mco);
        }

        private void ComputeOverlaps(int start0, int end0, MonotoneChain mc, int start1, int end1, MonotoneChainOverlapAction mco)
        {
            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                mco.Overlap(this, start0, mc, start1);
                return;
            }
            // nothing to do if the envelopes of these sub-chains don't overlap
            if (!Overlaps(start0, end0, mc, start1, end1)) return;

            // the chains overlap, so split each in half and iterate  (binary search)
            int mid0 = (start0 + end0) / 2;
            int mid1 = (start1 + end1) / 2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start0 < mid0)
            {
                if (start1 < mid1)
                    ComputeOverlaps(start0, mid0, mc, start1, mid1, mco);
                if (mid1 < end1)
                    ComputeOverlaps(start0, mid0, mc, mid1, end1, mco);
            }
            if (mid0 < end0)
            {
                if (start1 < mid1)
                    ComputeOverlaps(mid0, end0, mc, start1, mid1, mco);
                if (mid1 < end1)
                    ComputeOverlaps(mid0, end0, mc, mid1, end1, mco);
            }
        }
        /// <summary>
        /// Tests whether the envelopes of two chain sections overlap (intersect).
        /// </summary>
        /// <returns><c>true</c> if the section envelopes overlap</returns>
        private bool Overlaps(
            int start0, int end0,
            MonotoneChain mc,
            int start1, int end1)
        {
            return Envelope.Intersects(_pts[start0], _pts[end0], mc._pts[start1], mc._pts[end1]);
        }
    }
}
