using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Finds all intersections in one or two sets of edges,
    /// using the straightforward method of
    /// comparing all segments.
    /// This algorithm is too slow for production use, but is useful for testing purposes.
    /// </summary>
    public class SimpleEdgeSetIntersector : EdgeSetIntersector
    {
        public SimpleEdgeSetIntersector() {}

        public override void ComputeIntersections(IList edges, SegmentIntersector si, Boolean testAllSegments)
        {
            for (IEnumerator i0 = edges.GetEnumerator(); i0.MoveNext();)
            {
                Edge edge0 = (Edge) i0.Current;
                for (IEnumerator i1 = edges.GetEnumerator(); i1.MoveNext();)
                {
                    Edge edge1 = (Edge) i1.Current;
                    if (testAllSegments || edge0 != edge1)
                    {
                        ComputeIntersects(edge0, edge1, si);
                    }
                }
            }
        }

        public override void ComputeIntersections(IList edges0, IList edges1, SegmentIntersector si)
        {
            for (IEnumerator i0 = edges0.GetEnumerator(); i0.MoveNext();)
            {
                Edge edge0 = (Edge) i0.Current;
                for (IEnumerator i1 = edges1.GetEnumerator(); i1.MoveNext();)
                {
                    Edge edge1 = (Edge) i1.Current;
                    ComputeIntersects(edge0, edge1, si);
                }
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each Edge.
        /// This has n^2 performance, and is about 100 times slower than using
        /// monotone chains.
        /// </summary>
        private void ComputeIntersects(Edge e0, Edge e1, SegmentIntersector si)
        {
            ICoordinate[] pts0 = e0.Coordinates;
            ICoordinate[] pts1 = e1.Coordinates;
            for (Int32 i0 = 0; i0 < pts0.Length - 1; i0++)
            {
                for (Int32 i1 = 0; i1 < pts1.Length - 1; i1++)
                {
                    si.AddIntersections(e0, i0, e1, i1);
                }
            }
        }
    }
}