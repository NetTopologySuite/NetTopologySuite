using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Finds all intersections in one or two sets of edges,
    /// using the straightforward method of
    /// comparing all segments.
    /// This algorithm is too slow for production use, but is useful for testing purposes.
    /// </summary>
    public class SimpleEdgeSetIntersector<TCoordinate> : EdgeSetIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges,
                                                  SegmentIntersector<TCoordinate> si, Boolean testAllSegments)
        {
            foreach (Edge<TCoordinate> edge0 in edges)
            {
                foreach (Edge<TCoordinate> edge1 in edges)
                {
                    if (testAllSegments || edge0 != edge1)
                    {
                        computeIntersects(edge0, edge1, si);
                    }
                }
            }
        }

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges0,
                                                  IEnumerable<Edge<TCoordinate>> edges1,
                                                  SegmentIntersector<TCoordinate> si)
        {
            foreach (Edge<TCoordinate> edge0 in edges0)
            {
                foreach (Edge<TCoordinate> edge1 in edges1)
                {
                    computeIntersects(edge0, edge1, si);
                }
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each Edge.
        /// This has n^2 performance, and is about 100 times slower than using
        /// monotone chains.
        /// </summary>
        private void computeIntersects(Edge<TCoordinate> e0, Edge<TCoordinate> e1, SegmentIntersector<TCoordinate> si)
        {
            IEnumerator<TCoordinate> pts0 = e0.Coordinates.GetEnumerator();
            IEnumerator<TCoordinate> pts1 = e1.Coordinates.GetEnumerator();

            Int32 i0 = 0;
            Int32 i1 = 0;

            while (pts0.MoveNext())
            {
                while (pts1.MoveNext())
                {
                    si.AddIntersections(e0, i0, e1, i1);

                    i1 += 1;
                }

                i0 += 1;
            }
        }
    }
}