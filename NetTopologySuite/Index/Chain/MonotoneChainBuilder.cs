using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary> 
    /// A MonotoneChainBuilder implements static functions
    /// to determine the monotone chains in a sequence of points.
    /// </summary>
    public static class MonotoneChainBuilder
    {
        public static Int32[] ToIntArray(IList list)
        {
            Int32[] array = new Int32[list.Count];
            
            for (Int32 i = 0; i < array.Length; i++)
            {
                array[i] = (Int32) list[i];
            }

            return array;
        }

        public static IList GetChains(ICoordinate[] pts)
        {
            return GetChains(pts, null);
        }

        /// <summary>
        /// Return a list of the <c>MonotoneChain</c>s
        /// for the given list of coordinates.
        /// </summary>
        public static IList GetChains(ICoordinate[] pts, object context)
        {
            IList mcList = new ArrayList();
            Int32[] startIndex = GetChainStartIndices(pts);
            
            for (Int32 i = 0; i < startIndex.Length - 1; i++)
            {
                MonotoneChain mc = new MonotoneChain(pts, startIndex[i], startIndex[i + 1], context);
                mcList.Add(mc);
            }

            return mcList;
        }

        /// <summary>
        /// Return an array containing lists of start/end indexes of the monotone chains
        /// for the given list of coordinates.
        /// The last entry in the array points to the end point of the point array,
        /// for use as a sentinel.
        /// </summary>
        public static Int32[] GetChainStartIndices(ICoordinate[] pts)
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            Int32 start = 0;
            IList startIndexList = new ArrayList();
            startIndexList.Add(start);
            
            do
            {
                Int32 last = FindChainEnd(pts, start);
                startIndexList.Add(last);
                start = last;
            } while (start < pts.Length - 1);

            // copy list to an array of ints, for efficiency
            Int32[] startIndex = ToIntArray(startIndexList);
            return startIndex;
        }

        /// <returns> 
        /// The index of the last point in the monotone chain starting at <c>start</c>.
        /// </returns>
        private static Int32 FindChainEnd(ICoordinate[] pts, Int32 start)
        {
            // determine quadrant for chain
            Int32 chainQuad = QuadrantOp.Quadrant(pts[start], pts[start + 1]);
            Int32 last = start + 1;

            while (last < pts.Length)
            {
                // compute quadrant for next possible segment in chain
                Int32 quad = QuadrantOp.Quadrant(pts[last - 1], pts[last]);
                
                if (quad != chainQuad)
                {
                    break;
                }

                last++;
            }

            return last - 1;
        }
    }
}