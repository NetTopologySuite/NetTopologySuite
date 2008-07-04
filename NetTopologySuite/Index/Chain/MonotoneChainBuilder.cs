using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary> 
    /// A MonotoneChainBuilder implements static functions
    /// to determine the monotone chains in a sequence of points.
    /// </summary>
    public class MonotoneChainBuilder
    {
        /// <summary>
        /// Only static methods!
        /// </summary>
        private MonotoneChainBuilder() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int[] ToIntArray(IList list)
        {
            int[] array = new int[list.Count];
            for (int i = 0; i < array.Length; i++)            
                array[i] = (int)list[i];            
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static IList GetChains(ICoordinate[] pts)
        {
            return GetChains(pts, null);
        }

        /// <summary>
        /// Return a list of the <c>MonotoneChain</c>s
        /// for the given list of coordinates.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="context"></param>
        public static IList GetChains(ICoordinate[] pts, object context)
        {
            IList mcList = new ArrayList();
            int[] startIndex = GetChainStartIndices(pts);
            for (int i = 0; i < startIndex.Length - 1; i++)
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
        /// <param name="pts"></param>
        public static int[] GetChainStartIndices(ICoordinate[] pts)
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            int start = 0;
            IList startIndexList = new ArrayList();
            startIndexList.Add(start);
            do
            {
                int last = FindChainEnd(pts, start);
                startIndexList.Add(last);
                start = last;
            } 
            while (start < pts.Length - 1);

            // copy list to an array of ints, for efficiency
            int[] startIndex = ToIntArray(startIndexList);
            return startIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="start"></param>
        /// <returns> 
        /// The index of the last point in the monotone chain starting at <c>start</c>.
        /// </returns>
        private static int FindChainEnd(ICoordinate[] pts, int start)
        {
            // determine quadrant for chain
            int chainQuad = QuadrantOp.Quadrant(pts[start], pts[start + 1]);
            int last = start + 1;
            while (last < pts.Length)
            {
                // compute quadrant for next possible segment in chain
                int quad = QuadrantOp.Quadrant(pts[last - 1], pts[last]);
                if (quad != chainQuad) 
                    break;
                last++;
            }
            return last - 1;
        }           
    }
}
