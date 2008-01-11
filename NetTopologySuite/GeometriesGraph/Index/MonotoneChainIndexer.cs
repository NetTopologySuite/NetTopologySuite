using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    public class MonotoneChainIndexer<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        //public static Int32[] ToIntArray(IList list)
        //{
        //    Int32[] array = new Int32[list.Count];

        //    for (Int32 i = 0; i < array.Length; i++)
        //    {
        //        array[i] = Convert.ToInt32(list[i]);
        //    }

        //    return array;
        //}

        private Int32 _pointCount;

        public IList<Int32> GetChainStartIndices(IEnumerable<TCoordinate> points)
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            Int32 start = 0;
            List<Int32> startIndexList = new List<Int32>();
            startIndexList.Add(start);
            _pointCount = Slice.GetLength(points);

            do
            {
                Int32 last = findChainEnd(points, start);
                startIndexList.Add(last);
                start = last;
            } while (start < _pointCount);

            return startIndexList.AsReadOnly();
        }

        /// <returns> 
        /// The index of the last point in the monotone chain.
        /// </returns>
        private Int32 findChainEnd(IEnumerable<TCoordinate> points, Int32 start)
        {
            // determine quadrant for chain
            Pair<TCoordinate> startPair = Slice.GetPairAt(points, start).Value;
            Quadrants chainQuad = QuadrantOp<TCoordinate>.Quadrant(startPair.First, startPair.Second);
            Int32 last = start + 1;

            while (last < _pointCount)
            {
                // compute quadrant for next possible segment in chain
                Pair<TCoordinate> endPair = Slice.GetPairAt(points, last - 1).Value;
                Quadrants quad = QuadrantOp<TCoordinate>.Quadrant(endPair.First, endPair.Second);

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