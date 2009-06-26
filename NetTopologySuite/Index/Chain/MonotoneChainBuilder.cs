using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
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
        //    Int32[] array = new Int32[list.TotalItemCount];

        //    for (Int32 i = 0; i < array.Length; i++)
        //    {
        //        array[i] = (Int32) list[i];
        //    }

        //    return array;
        //}

        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(
                                            IGeometryFactory<TCoordinate> geoFactory, 
                                            ICoordinateSequence<TCoordinate> coordinates)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            return GetChains(geoFactory, coordinates, null);
        }

        /// <summary>
        /// Return a list of the <see cref="MonotoneChain{TCoordinate}"/>s
        /// for the given list of coordinates.
        /// </summary>
        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(
                                            IGeometryFactory<TCoordinate> geoFactory, 
                                            ICoordinateSequence<TCoordinate> coordinates, 
                                            Object context)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IEnumerable<Int32> startIndicies = GetChainStartIndices(coordinates);

            foreach (Pair<Int32> indexPair in Slice.GetOverlappingPairs(startIndicies))
            {
                MonotoneChain<TCoordinate> mc = new MonotoneChain<TCoordinate>(
                                                                geoFactory, 
                                                                coordinates, 
                                                                indexPair.First, 
                                                                indexPair.Second, 
                                                                context);

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
        /// Return an enumeration containing of start / end indexes of the 
        /// monotone chains for the given set of coordinates.
        /// The last entry in the enumeration is the index to the end point 
        /// of the point set, for use as a sentinel.
        /// </summary>
        public static IEnumerable<Int32> GetChainStartIndices<TCoordinate>(
                                            ICoordinateSequence<TCoordinate> points)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            Int32 start = 0;

            yield return start;

            Int32 last = start;

            while (last < points.LastIndex)
            {
                last = findChainEnd(points, start);
                yield return last;
                // the next monotone chain starts where the current one ends
                start = last;
            }
        }

        // Returns the index of the last point in the monotone chain starting at 'start'.
        private static Int32 findChainEnd<TCoordinate>(ICoordinateSequence<TCoordinate> points, 
                                                       Int32 start)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Int32 lastSegmentStartIndex = points.LastIndex - 1;

            if (start == lastSegmentStartIndex)
            {
                return start + 1;
            }

            Pair<TCoordinate> segment = points.SegmentAt(start);

            Quadrants chainQuad = QuadrantOp<TCoordinate>.Quadrant(segment.First, 
                                                                   segment.Second);

            Quadrants quad = chainQuad;

            //while (++start <= lastSegmentStartIndex && quad == chainQuad)
            //{
            //    segment = points.SegmentAt(start);
            //    quad = QuadrantOp<TCoordinate>.Quadrant(segment.First, segment.Second);
            //}

            do
            {
                segment = points.SegmentAt(++start);
                quad = QuadrantOp<TCoordinate>.Quadrant(segment.First, segment.Second);
            } while (quad == chainQuad && start < lastSegmentStartIndex);

            return quad == chainQuad ? start + 1 : start;
        }
    }
}