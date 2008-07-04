using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary> 
    /// MonotoneChains are a way of partitioning the segments of a linestring to
    /// allow for fast searching of intersections.
    /// They have the following properties:
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
    /// </summary>
    public class MonotoneChain
    {
        private ICoordinate[] pts;
        private int start, end;
        private IEnvelope env = null;
        private object context = null;  // user-defined information
        private int id;                 // useful for optimizing chain comparisons

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="context"></param>
        public MonotoneChain(ICoordinate[] pts, int start, int end, object context)
        {
            this.pts = pts;
            this.start = start;
            this.end = end;
            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object Context
        {
            get
            {
                return context;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                if (env == null)
                {
                    ICoordinate p0 = pts[start];
                    ICoordinate p1 = pts[end];
                    env = new Envelope(p0, p1);
                }
                return env;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int StartIndex
        {
            get
            {
                return start;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int EndIndex
        {
            get
            {
                return end;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ls"></param>
        public void GetLineSegment(int index, ref LineSegment ls)
        {
            ls.P0 = pts[index];
            ls.P1 = pts[index + 1];
        }

        /// <summary>
        /// Return the subsequence of coordinates forming this chain.
        /// Allocates a new array to hold the Coordinates.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get
            {
                ICoordinate[] coord = new ICoordinate[end - start + 1];
                int index = 0;
                for (int i = start; i <= end; i++) 
                    coord[index++] = pts[i];                
                return coord;
            }
        }

        /// <summary> 
        /// Determine all the line segments in the chain whose envelopes overlap
        /// the searchEnvelope, and process them.
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="mcs"></param>
        public void Select(IEnvelope searchEnv, MonotoneChainSelectAction mcs)
        {
            ComputeSelect(searchEnv, start, end, mcs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="start0"></param>
        /// <param name="end0"></param>
        /// <param name="mcs"></param>
        private void ComputeSelect(IEnvelope searchEnv, int start0, int end0, MonotoneChainSelectAction mcs)
        {
            ICoordinate p0 = pts[start0];
            ICoordinate p1 = pts[end0];
            mcs.TempEnv1.Init(p0, p1);
            
            // terminating condition for the recursion
            if (end0 - start0 == 1)
            {
                mcs.Select(this, start0);
                return;
            }
            // nothing to do if the envelopes don't overlap
            if (!searchEnv.Intersects(mcs.TempEnv1))
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
        /// 
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="mco"></param>
        public  void ComputeOverlaps(MonotoneChain mc, MonotoneChainOverlapAction mco)
        {
            ComputeOverlaps(start, end, mc, mc.start, mc.end, mco);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start0"></param>
        /// <param name="end0"></param>
        /// <param name="mc"></param>
        /// <param name="start1"></param>
        /// <param name="end1"></param>
        /// <param name="mco"></param>
        private void ComputeOverlaps(int start0, int end0, MonotoneChain mc, int start1, int end1, MonotoneChainOverlapAction mco)
        {
            ICoordinate p00 = pts[start0];
            ICoordinate p01 = pts[end0];
            ICoordinate p10 = mc.pts[start1];
            ICoordinate p11 = mc.pts[end1];
            
            // terminating condition for the recursion
            if (end0 - start0 == 1 && end1 - start1 == 1)
            {
                mco.Overlap(this, start0, mc, start1);
                return;
            }
            // nothing to do if the envelopes of these chains don't overlap
            mco.TempEnv1.Init(p00, p01);
            mco.TempEnv2.Init(p10, p11);
            if (! mco.TempEnv1.Intersects(mco.TempEnv2)) 
                return;

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
    }
}
