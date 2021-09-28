using System;
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
    /// <list type="bullet">
    /// <item><description>the segments within a monotone chain never intersect each other</description></item>
    /// <item><description>the envelope of any contiguous subset of the segments in a monotone chain
    /// is equal to the envelope of the endpoints of the subset.</description></item>
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
    /// <list type="table">
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
    /// <para>
    /// MonotoneChains support being assigned an integer id value
    /// to provide a total ordering for a set of chains.
    /// This can be used during some kinds of processing to
    /// avoid redundant comparisons
    /// (i.e.by comparing only chains where the first id is less than the second).
    /// </para>
    /// <para>
    /// MonotoneChains support using an tolerance distance for overlap tests.
    /// This allows reporting overlap in situations where
    /// intersection snapping is being used.
    /// If this is used the chain envelope must be computed
    /// providing an expansion distance using <see cref="GetEnvelope(double)"/>.
    /// </para>
    /// </remarks>
    public class MonotoneChain
    {
        private readonly Coordinate[] _pts;
        private readonly int _start;
        private readonly int _end;
        private Envelope _env;
        private readonly object _context;  // user-defined information
        //private double _overlapDistance;

        /// <summary>
        /// Creates a new MonotoneChain based on the given array of points.
        /// </summary>
        /// <param name="pts">The points containing the chain</param>
        /// <param name="start">The index of the first coordinate in the chain</param>
        /// <param name="end">The index of the last coordinate in the chain </param>
        /// <param name="context">A user-defined data object</param>
        public MonotoneChain(Coordinate[] pts, int start, int end, object context)
        {
            _pts = pts;
            _start = start;
            _end = end;
            _context = context;
        }

        /// <summary>
        /// Gets or sets the Id of this chain
        /// </summary>
        /// <remarks>
        /// Useful for assigning an ordering to a set of
        /// chains, which can be used to avoid redundant processing.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the overlap distance used in overlap tests
        /// with other chains.
        /// </summary>
        public double OverlapDistance
        {
            get => 0d;//_overlapDistance;
            set { /*_overlapDistance = value;*/ }
        }

        /// <summary>
        /// Gets the chain's user-defined context data value.
        /// </summary>
        public object Context => _context;

        /// <summary>
        /// Gets the envelope of this chain
        /// </summary>
        public Envelope Envelope
        {
            get => GetEnvelope(0.0);
        }

        /// <summary>
        /// Gets the envelope for this chain,
        /// expanded by a given distance.
        /// </summary>
        /// <param name="expansionDistance">Distance to expand the envelope by</param>
        /// <returns>The expanded envelope of the chain</returns>
        public Envelope GetEnvelope(double expansionDistance)
        {

            if (_env == null)
            {
                /*
                 * The monotonicity property allows fast envelope determination
                 */
                var p0 = _pts[_start];
                var p1 = _pts[_end];
                _env = new Envelope(p0, p1);
                if (expansionDistance > 0.0)
                    _env.ExpandBy(expansionDistance);
            }
            return _env;
        }

        /// <summary>
        /// Gets the index of the start of the monotone chain
        /// in the underlying array of points.
        /// </summary>
        public int StartIndex => _start;

        /// <summary>
        /// Gets the index of the end of the monotone chain
        /// in the underlying array of points.
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
        /// Determines the line segments in two chains which may overlap,
        /// and passes them to an overlap action.
        /// </summary>
        /// <remarks>
        /// The monotone chain search algorithm attempts to optimize
        /// performance by not calling the overlap action on chain segments
        /// which it can determine do not overlap.
        /// However, it* may* call the overlap action on segments
        /// which do not actually interact.
        /// This saves on the overhead of checking intersection
        /// each time, since clients may be able to do this more efficiently.
        /// </remarks>
        /// <param name="mc">The chain to compare to</param>
        /// <param name="mco">The overlap action to execute on selected segments</param>
        public void ComputeOverlaps(MonotoneChain mc, MonotoneChainOverlapAction mco)
        {
            ComputeOverlaps(_start, _end, mc, mc._start, mc._end, 0.0, mco);
        }

        /// <summary>
        /// Determines the line segments in two chains which may overlap,
        /// using an overlap distance tolerance,
        /// and passes them to an overlap action.
        /// </summary>
        /// <param name="mc">The chain to compare to</param>
        /// <param name="overlapTolerance">The overlap tolerance distance (may be 0)</param>
        /// <param name="mco">The overlap action to execute on selected segments</param>
        public void ComputeOverlaps(MonotoneChain mc, double overlapTolerance, MonotoneChainOverlapAction mco)
        {
            ComputeOverlaps(_start, _end, mc, mc._start, mc._end, overlapTolerance, mco);
        }

        /// <summary>
        /// Uses an efficient mutual binary search strategy
        /// to determine which pairs of chain segments
        /// may overlap, and calls the given overlap action on them.
        /// </summary>
        /// <param name="start0">The start index of this chain section</param>
        /// <param name="end0">The end index of this chain section</param>
        /// <param name="mc">The target monotone chain</param>
        /// <param name="start1">The start index of the target chain section</param>
        /// <param name="end1">The end index of the target chain section</param>
        /// <param name="overlapTolerance">The overlap tolerance distance (may be 0)</param>
        /// <param name="mco">The overlap action to execute on selected segments</param>
        private void ComputeOverlaps(int start0, int end0, MonotoneChain mc, int start1, int end1, double overlapTolerance, MonotoneChainOverlapAction mco)
        {
            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                mco.Overlap(this, start0, mc, start1);
                return;
            }
            // nothing to do if the envelopes of these sub-chains don't overlap
            if (!Overlaps(start0, end0, mc, start1, end1, overlapTolerance)) return;

            // the chains overlap, so split each in half and iterate  (binary search)
            int mid0 = (start0 + end0) / 2;
            int mid1 = (start1 + end1) / 2;

            // Assert: mid != start or end (since we checked above for end - start <= 1)
            // check terminating conditions before recursing
            if (start0 < mid0)
            {
                if (start1 < mid1)
                    ComputeOverlaps(start0, mid0, mc, start1, mid1, overlapTolerance, mco);
                if (mid1 < end1)
                    ComputeOverlaps(start0, mid0, mc, mid1, end1, overlapTolerance, mco);
            }
            if (mid0 < end0)
            {
                if (start1 < mid1)
                    ComputeOverlaps(mid0, end0, mc, start1, mid1, overlapTolerance, mco);
                if (mid1 < end1)
                    ComputeOverlaps(mid0, end0, mc, mid1, end1, overlapTolerance, mco);
            }
        }

        /// <summary>
        /// Tests whether the envelope of a section of the chain
        /// overlaps(intersects) the envelope of a section of another target chain.
        /// This test is efficient due to the monotonicity property
        /// of the sections(i.e.the envelopes can be are determined
        /// from the section endpoints
        /// rather than a full scan).
        /// </summary>
        /// <param name="start0">The start index of this chain section</param>
        /// <param name="end0">The end index of this chain section</param>
        /// <param name="mc">The target monotone chain</param>
        /// <param name="start1">The start index of the target chain section</param>
        /// <param name="end1">The end index of the target chain section</param>
        /// <param name="overlapTolerance">The overlap tolerance distance (may be 0)</param>
        /// <returns><c>true</c> if the section envelopes overlap</returns>
        private bool Overlaps(
            int start0, int end0,
            MonotoneChain mc,
            int start1, int end1,
            double overlapTolerance)
        {
            if (overlapTolerance > 0.0)
            {
                return Overlaps(_pts[start0], _pts[end0], mc._pts[start1], mc._pts[end1], overlapTolerance);
            }
            return Envelope.Intersects(_pts[start0], _pts[end0], mc._pts[start1], mc._pts[end1]);
        }

        /// <param name="p1">The 1st coordinate of the 1st segment</param>
        /// <param name="p2">The 2nd coordinate of the 1st segment</param>
        /// <param name="q1">The 1st coordinate of the 2nd segment</param>
        /// <param name="q2">The 2nd coordinate of the 2nd segment</param>
        /// <param name="overlapTolerance">The overlap tolerance distance (may be 0)</param>
        private static bool Overlaps(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2, double overlapTolerance)
        {
            double minq = Math.Min(q1.X, q2.X);
            double maxq = Math.Max(q1.X, q2.X);
            double minp = Math.Min(p1.X, p2.X);
            double maxp = Math.Max(p1.X, p2.X);

            if (minp > maxq + overlapTolerance)
                return false;
            if (maxp < minq - overlapTolerance)
                return false;

            minq = Math.Min(q1.Y, q2.Y);
            maxq = Math.Max(q1.Y, q2.Y);
            minp = Math.Min(p1.Y, p2.Y);
            maxp = Math.Max(p1.Y, p2.Y);

            if (minp > maxq + overlapTolerance)
                return false;
            if (maxp < minq - overlapTolerance)
                return false;
            return true;
        }

    }
}
