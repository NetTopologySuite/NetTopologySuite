using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary> 
    /// A MonotoneChainBuilder implements static functions
    /// to determine the monotone chains in a sequence of points.
    /// </summary>
    public static class MonotoneChainBuilder
    {
        //public static Int32[] ToIntArray(IList list)
        //{
        //    Int32[] array = new Int32[list.Count];

        //    for (Int32 i = 0; i < array.Length; i++)
        //    {
        //        array[i] = (Int32) list[i];
        //    }

        //    return array;
        //}

        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(
            IEnumerable<TCoordinate> coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            return GetChains(coordinates, null);
        }

        /// <summary>
        /// Return a list of the <see cref="MonotoneChain{TCoordinate}"/>s
        /// for the given list of coordinates.
        /// </summary>
        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(
            IEnumerable<TCoordinate> coordinates, Object context)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            Int32[] startIndex = GetChainStartIndices(coordinates);

            for (Int32 i = 0; i < startIndex.Length - 1; i++)
            {
                MonotoneChain<TCoordinate> mc =
                    new MonotoneChain<TCoordinate>(coordinates, startIndex[i], startIndex[i + 1], context);
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
        public static Int32[] GetChainStartIndices<TCoordinate>(IEnumerable<TCoordinate> coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            Int32 start = 0;
            IList startIndexList = new ArrayList();
            startIndexList.Add(start);

            do
            {
                Int32 last = FindChainEnd(coordinates, start);
                startIndexList.Add(last);
                start = last;
            } while (start < coordinates.Length - 1);

            // copy list to an array of ints, for efficiency
            Int32[] startIndex = ToIntArray(startIndexList);
            return startIndex;
        }

        /// <returns> 
        /// The index of the last point in the monotone chain starting at <c>start</c>.
        /// </returns>
        private static Int32 FindChainEnd<TCoordinate>(IEnumerable<TCoordinate> coordinates, Int32 start)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            // determine quadrant for chain
            Int32 chainQuad = QuadrantOp.Quadrant(coordinates[start], coordinates[start + 1]);
            Int32 last = start + 1;

            while (last < coordinates.Length)
            {
                // compute quadrant for next possible segment in chain
                Int32 quad = QuadrantOp.Quadrant(coordinates[last - 1], coordinates[last]);

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