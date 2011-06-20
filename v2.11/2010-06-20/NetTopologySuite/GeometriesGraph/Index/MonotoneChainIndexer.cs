using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Partitions a set of coordinates into monotone chains.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate.</typeparam>
    public class MonotoneChainIndexer<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private Int32 _pointCount;

        /// <summary>
        /// Enumerates the given coordinates to compute the starting points of 
        /// monotone chains within them.
        /// </summary>
        /// <param name="points">A set of coordinates to partition.</param>
        /// <returns>
        /// An enumeration of the positions within the given coordinates
        /// where a monotone chain begins.
        /// </returns>
        public IEnumerable<Int32> GetChainStartIndices(IEnumerable<TCoordinate> points)
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

        // Returns the index of the last point in the monotone chain.
        private Int32 findChainEnd(IEnumerator<TCoordinate> points, Int32 start)
        {
            Quadrants chainQuad = Quadrants.None;
            Int32 last = start + 1;

            TCoordinate p0 = points.Current;

            while(points.MoveNext())
            {
                TCoordinate p1 = points.Current;

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