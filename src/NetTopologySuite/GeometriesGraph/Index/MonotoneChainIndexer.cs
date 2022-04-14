using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// MonotoneChains are a way of partitioning the segments of an edge to
    /// allow for fast searching of intersections.
    /// <para>
    /// Specifically, a sequence of contiguous line segments
    /// is a monotone chain if all the vectors defined by the oriented segments
    /// lies in the same quadrant.
    /// </para><para>
    /// Monotone Chains have the following useful properties:
    /// the segments within a monotone chain will never intersect each other, and
    /// the envelope of any contiguous subset of the segments in a monotone chain
    /// is simply the envelope of the endpoints of the subset.
    /// Property 1 means that there is no need to test pairs of segments from within
    /// the same monotone chain for intersection.
    /// Property 2 allows
    /// binary search to be used to find the intersection points of two monotone chains.
    /// For many types of real-world data, these properties eliminate a large number of
    /// segment comparisons, producing substantial speed gains.
    /// </para>
    /// <para>
    /// Note that due to the efficient intersection test, there is no need to limit the size
    /// of chains to obtain fast performance.
    /// </para>
    /// </summary>
    public class MonotoneChainIndexer
    {
        /*
        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public MonotoneChainIndexer() { }
        */
        /// <summary>
        /// Computes the startpoints (and endpoints) of all in monotone chains in this edge
        /// </summary>
        /// <param name="pts">An array of points</param>
        /// <returns>An array of startpoints (and endpoints) of monotone chains</returns>
        public int[] GetChainStartIndices(Coordinate[] pts)
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            int start = 0;
            var startIndexList = new List<int>(pts.Length);
            // use heuristic to size initial array
            //startIndexList.ensureCapacity(pts.length / 4);
            startIndexList.Add(start);
            do
            {
                int last = FindChainEnd(pts, start);
                startIndexList.Add(last);
                start = last;
            } while (start < pts.Length - 1);
            // copy list to an array of ints, for efficiency
            return startIndexList.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public int[] OLDGetChainStartIndices(Coordinate[] pts)
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            int start = 0;
            var startIndexList = new List<int>();
            startIndexList.Add(start);
            do
            {
                int last = FindChainEnd(pts, start);
                startIndexList.Add(last);
                start = last;
            }
            while (start < pts.Length - 1);
            // copy list to an array of ints, for efficiency
            int[] startIndex = startIndexList.ToArray(); /*ToIntArray(startIndexList);*/
            return startIndex;
        }

        /// <summary>
        /// Searches for the end of a <c>MonotoneChain</c>
        /// </summary>
        /// <param name="pts">An array of <c>Coordinate</c>s</param>
        /// <param name="start">The start index of the chain to find the end for</param>
        /// <returns>
        /// The index of the last point in the monotone chain.
        /// </returns>
        private static int FindChainEnd(Coordinate[] pts, int start)
        {
            // determine quadrant for chain
            var chainQuad = new Quadrant(pts[start], pts[start + 1]);
            int last = start + 1;
            while (last < pts.Length)
            {
                //if (last - start > 100) break;
                // compute quadrant for next possible segment in chain
                var quad = new Quadrant(pts[last - 1], pts[last]);
                if (quad != chainQuad)
                    break;
                last++;
            }
            return last - 1;
        }
    }
}
