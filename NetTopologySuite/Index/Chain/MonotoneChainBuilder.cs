using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
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

        public static IEnumerable<MonotoneChain<TCoordinate>> GetChains<TCoordinate>(
                                            IGeometryFactory<TCoordinate> geoFactory, 
                                            ICoordinateSequence<TCoordinate> coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, 
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
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, 
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
                                            IEnumerable<TCoordinate> points)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, 
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // find the startpoint (and endpoints) of all monotone chains in this edge
            Int32 start = 0;

            yield return start;

            IEnumerator<TCoordinate> pointsEnumerator = points.GetEnumerator();

            while (pointsEnumerator.MoveNext())
            {
                Int32 last = findChainEnd(pointsEnumerator, start);
                yield return last;
                // the next monotone chain starts where the current one ends
                start = last;
            }
        }

        // Returns the index of the last point in the monotone chain starting at 'start'.
        private static Int32 findChainEnd<TCoordinate>(IEnumerator<TCoordinate> points, 
                                                       Int32 start)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, 
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Quadrants chainQuad = Quadrants.None;
            Int32 last = start + 1;

            TCoordinate p0 = points.Current;

            while (points.MoveNext())
            {
                TCoordinate p1 = points.Current;

  	            // skip any zero-length segments at the start of the sequence
  	            // (since they cannot be used to establish a quadrant)
  	            if (p0.Equals(p1))
                {
                    last++;
  		            continue;
  	            }

                // determine quadrant for chain on first segment
                if (chainQuad == Quadrants.None)
                {
                    chainQuad = QuadrantOp<TCoordinate>.Quadrant(p0, p1);
                }
                else
                {
                    Quadrants quad = QuadrantOp<TCoordinate>.Quadrant(p0, p1);

                    if (quad != chainQuad)
                    {
                        break;
                    }

                    last++;
                }

                p0 = p1;
            }

            return last - 1;
        }
    }
}