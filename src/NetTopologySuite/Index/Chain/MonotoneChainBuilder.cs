using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Index.Chain
{
    /// <summary>
    /// Constructs <see cref="MonotoneChain"/>s
    /// for sequences of <see cref="Coordinate"/>s.
    /// </summary>
    public class MonotoneChainBuilder
    {
        /// <summary>
        /// Only static methods!
        /// </summary>
        private MonotoneChainBuilder() { }

        [Obsolete]
        public static int[] ToIntArray(IList<int> list)
        {
            int[] array = new int[list.Count];
            for (int i = 0; i < array.Length; i++)
                array[i] = list[i];
            return array;
        }

        /// <summary>
        /// Computes a list of the <see cref="MonotoneChain"/>s
        /// for a list of coordinates.
        /// </summary>
        /// <param name="pts">The list of points to compute chains for</param>
        /// <returns>A list of the monotone chains for the points</returns>
        public static ReadOnlyCollection<MonotoneChain> GetChains(Coordinate[] pts)
        {
            return GetChains(pts, null);
        }

        /// <summary>
        /// Return a list of the <c>MonotoneChain</c>s
        /// for the given list of coordinates.
        /// </summary>
        /// <param name="pts">The list of points to compute chains for</param>
        /// <param name="context">A data object to attach to each chain</param>
        /// <returns>A list of the monotone chains for the points</returns>
        public static ReadOnlyCollection<MonotoneChain> GetChains(Coordinate[] pts, object context)
        {
            var mcList = new List<MonotoneChain>();
            int chainStart = 0;
            do
            {
                int chainEnd = FindChainEnd(pts, chainStart);
                var mc = new MonotoneChain(pts, chainStart, chainEnd, context);
                mcList.Add(mc);
                chainStart = chainEnd;
            } while (chainStart < pts.Length - 1);
            return mcList.AsReadOnly();
        }

        /// <summary>
        /// Return an array containing lists of start/end indexes of the monotone chains
        /// for the given list of coordinates.
        /// The last entry in the array points to the end point of the point array,
        /// for use as a sentinel.
        /// </summary>
        /// <param name="pts"></param>
        [Obsolete]
        public static int[] GetChainStartIndices(Coordinate[] pts)
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            int start = 0;
            var startIndexList = new List<int>(pts.Length);
            startIndexList.Add(start);
            do
            {
                int last = FindChainEnd(pts, start);
                startIndexList.Add(last);
                start = last;
            }
            while (start < pts.Length - 1);

            // copy list to an array of ints, for efficiency
            //int[] startIndex = ToIntArray(startIndexList);
            return /*startIndex =*/ startIndexList.ToArray();
        }

        /// <summary>
        /// Finds the index of the last point in a monotone chain
        /// starting at a given point.
        /// Any repeated points (0-length segments) will be included
        /// in the monotone chain returned.
        /// </summary>
        /// <param name="pts">The coordinates</param>
        /// <param name="start">The start index</param>
        /// <returns>
        /// The index of the last point in the monotone chain starting at <c>start</c>.
        /// </returns>
        private static int FindChainEnd(Coordinate[] pts, int start)
        {
            int safeStart = start;
            // skip any zero-length segments at the start of the sequence
            // (since they cannot be used to establish a quadrant)
            while (safeStart < pts.Length - 1 && pts[safeStart].Equals2D(pts[safeStart + 1]))
            {
                safeStart++;
            }
            // check if there are NO non-zero-length segments
            if (safeStart >= pts.Length - 1)
            {
                return pts.Length - 1;
            }
            // determine overall quadrant for chain (which is the starting quadrant)
            var chainQuad = new Quadrant(pts[safeStart], pts[safeStart + 1]);
            int last = start + 1;
            while (last < pts.Length)
            {
                // skip zero-length segments, but include them in the chain
                if (!pts[last - 1].Equals2D(pts[last]))
                {
                    // compute quadrant for next possible segment in chain
                    var quad = new Quadrant(pts[last - 1], pts[last]);
                    if (quad != chainQuad) break;
                }
                last++;
            }
            return last - 1;
        }
    }
}
