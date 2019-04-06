using System.Collections.Generic;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Finds all intersections in one or two sets of edges,
    /// using the straightforward method of
    /// comparing all segments.
    /// This algorithm is too slow for production use, but is useful for testing purposes.
    /// </summary>
    public class SimpleEdgeSetIntersector : EdgeSetIntersector
    {
        /*
        /// <summary>
        ///
        /// </summary>
        public SimpleEdgeSetIntersector() { }
        */
        /// <summary>
        ///
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="si"></param>
        /// <param name="testAllSegments"></param>
        public override void ComputeIntersections(IList<Edge> edges, SegmentIntersector si, bool testAllSegments)
        {
            foreach (var edge0 in edges)
            {
                foreach (var edge1 in edges)
                {
                    if (testAllSegments || edge0 != edge1)
                        ComputeIntersects(edge0, edge1, si);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edges0"></param>
        /// <param name="edges1"></param>
        /// <param name="si"></param>
        public override void ComputeIntersections(IList<Edge> edges0, IList<Edge> edges1, SegmentIntersector si)
        {
            foreach (var edge0 in edges0)
            {
                foreach (var edge1 in edges1)
                    ComputeIntersects(edge0, edge1, si);
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each Edge.
        /// This has n^2 performance, and is about 100 times slower than using
        /// monotone chains.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        /// <param name="si"></param>
        private static void ComputeIntersects(Edge e0, Edge e1, SegmentIntersector si)
        {
            var pts0 = e0.Coordinates;
            var pts1 = e1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                    si.AddIntersections(e0, i0, e1, i1);
        }
    }
}
