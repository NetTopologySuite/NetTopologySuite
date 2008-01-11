using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary> 
    /// A <see cref="MonotoneChainBuilder"/> implements static functions
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

        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(ICoordinateSequence<TCoordinate> coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            return GetChains(coordinates, null);
        }

        /// <summary>
        /// Return a list of the <see cref="MonotoneChain{TCoordinate}"/>s
        /// for the given list of coordinates.
        /// </summary>
        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(ICoordinateSequence<TCoordinate> coordinates, Object context)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            IEnumerable<Int32> startIndicies = GetChainStartIndices(coordinates);

            foreach (Pair<Int32> indexPair in Slice.GetOverlappingPairs(startIndicies))
            {
                MonotoneChain<TCoordinate> mc = new MonotoneChain<TCoordinate>(
                    coordinates, indexPair.First, indexPair.Second, context);

                yield return mc;
            }

            //for (Int32 i = 0; i < startIndicies.Length - 1; i++)
            //{
            //    MonotoneChain<TCoordinate> mc =
            //        new MonotoneChain<TCoordinate>(coordinates, startIndicies[i], startIndicies[i + 1], context);

            //    mcList.Add(mc);
            //}

            //return mcList;
        }

        /// <summary>
        /// Return an array containing lists of start/end indexes of the monotone chains
        /// for the given list of coordinates.
        /// The last entry in the array points to the end point of the point array,
        /// for use as a sentinel.
        /// </summary>
        public static IEnumerable<Int32> GetChainStartIndices<TCoordinate>(ICoordinateSequence<TCoordinate> coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            Int32 start = 0;

            yield return start;

            do
            {
                Int32 last = findChainEnd(coordinates, start);
                yield return last;
                start = last;
            } while (start < coordinates.Count - 1);
        }

        // Returns the index of the last point in the monotone chain starting at 'start'.
        private static Int32 findChainEnd<TCoordinate>(IEnumerable<TCoordinate> coordinates, Int32 start)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            // determine quadrant for chain
            Pair<TCoordinate> startPair = Slice.GetPair(coordinates).Value;

            Quadrants chainQuad = QuadrantOp<TCoordinate>.Quadrant(startPair.First, startPair.Second);
            Int32 end = start;

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(coordinates))
            { 
                // compute quadrant for next possible segment in chain
                Quadrants quad = QuadrantOp<TCoordinate>.Quadrant(pair.First, pair.Second);

                if (quad != chainQuad)
                {
                    break;
                }

                end++;
            }

            return end;

            //while (end < coordinates.Length)
            //{
            //    // compute quadrant for next possible segment in chain
            //    Int32 quad = QuadrantOp<TCoordinate>.Quadrant(coordinates[end - 1], coordinates[end]);

            //    if (quad != chainQuad)
            //    {
            //        break;
            //    }

            //    end++;
            //}

            //return end - 1;
        }
    }
}