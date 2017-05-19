using GeoAPI.Geometries;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    ///     MonotoneChains are a way of partitioning the segments of an edge to
    ///     allow for fast searching of intersections.
    ///     They have the following properties:
    ///     the segments within a monotone chain will never intersect each other, and
    ///     the envelope of any contiguous subset of the segments in a monotone chain
    ///     is simply the envelope of the endpoints of the subset.
    ///     Property 1 means that there is no need to test pairs of segments from within
    ///     the same monotone chain for intersection.
    ///     Property 2 allows
    ///     binary search to be used to find the intersection points of two monotone chains.
    ///     For many types of real-world data, these properties eliminate a large number of
    ///     segment comparisons, producing substantial speed gains.
    /// </summary>
    public class MonotoneChainEdge
    {
        private readonly Edge e;
        // the lists of start/end indexes of the monotone chains.
        // Includes the end point of the edge as a sentinel
        // these envelopes are created once and reused
        private readonly Envelope env1 = new Envelope();
        private readonly Envelope env2 = new Envelope();

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        public MonotoneChainEdge(Edge e)
        {
            this.e = e;
            Coordinates = e.Coordinates;
            var mcb = new MonotoneChainIndexer();
            StartIndexes = mcb.GetChainStartIndices(Coordinates);
        }

        /// <summary>
        /// </summary>
        public Coordinate[] Coordinates { get; }

        /// <summary>
        /// </summary>
        public int[] StartIndexes { get; }

        /// <summary>
        /// </summary>
        /// <param name="chainIndex"></param>
        /// <returns></returns>
        public double GetMinX(int chainIndex)
        {
            var x1 = Coordinates[StartIndexes[chainIndex]].X;
            var x2 = Coordinates[StartIndexes[chainIndex + 1]].X;
            return x1 < x2 ? x1 : x2;
        }

        /// <summary>
        /// </summary>
        /// <param name="chainIndex"></param>
        /// <returns></returns>
        public double GetMaxX(int chainIndex)
        {
            var x1 = Coordinates[StartIndexes[chainIndex]].X;
            var x2 = Coordinates[StartIndexes[chainIndex + 1]].X;
            return x1 > x2 ? x1 : x2;
        }

        /// <summary>
        /// </summary>
        /// <param name="mce"></param>
        /// <param name="si"></param>
        public void ComputeIntersects(MonotoneChainEdge mce, SegmentIntersector si)
        {
            for (var i = 0; i < StartIndexes.Length - 1; i++)
                for (var j = 0; j < mce.StartIndexes.Length - 1; j++)
                    ComputeIntersectsForChain(i, mce, j, si);
        }

        /// <summary>
        /// </summary>
        /// <param name="chainIndex0"></param>
        /// <param name="mce"></param>
        /// <param name="chainIndex1"></param>
        /// <param name="si"></param>
        public void ComputeIntersectsForChain(int chainIndex0, MonotoneChainEdge mce, int chainIndex1,
            SegmentIntersector si)
        {
            ComputeIntersectsForChain(StartIndexes[chainIndex0], StartIndexes[chainIndex0 + 1], mce,
                mce.StartIndexes[chainIndex1], mce.StartIndexes[chainIndex1 + 1], si);
        }

        /// <summary>
        /// </summary>
        /// <param name="start0"></param>
        /// <param name="end0"></param>
        /// <param name="mce"></param>
        /// <param name="start1"></param>
        /// <param name="end1"></param>
        /// <param name="ei"></param>
        private void ComputeIntersectsForChain(int start0, int end0, MonotoneChainEdge mce, int start1, int end1,
            SegmentIntersector ei)
        {
            var p00 = Coordinates[start0];
            var p01 = Coordinates[end0];
            var p10 = mce.Coordinates[start1];
            var p11 = mce.Coordinates[end1];

            // terminating condition for the recursion
            if ((end0 - start0 == 1) && (end1 - start1 == 1))
            {
                ei.AddIntersections(e, start0, mce.e, start1);
                return;
            }

            // nothing to do if the envelopes of these chains don't overlap
            env1.Init(p00, p01);
            env2.Init(p10, p11);
            if (!env1.Intersects(env2))
                return;

            // the chains overlap, so split each in half and iterate  (binary search)
            var mid0 = (start0 + end0)/2;
            var mid1 = (start1 + end1)/2;

            // check terminating conditions before recursing
            if (start0 < mid0)
            {
                if (start1 < mid1)
                    ComputeIntersectsForChain(start0, mid0, mce, start1, mid1, ei);
                if (mid1 < end1)
                    ComputeIntersectsForChain(start0, mid0, mce, mid1, end1, ei);
            }
            if (mid0 < end0)
            {
                if (start1 < mid1)
                    ComputeIntersectsForChain(mid0, end0, mce, start1, mid1, ei);
                if (mid1 < end1)
                    ComputeIntersectsForChain(mid0, end0, mce, mid1, end1, ei);
            }
        }
    }
}