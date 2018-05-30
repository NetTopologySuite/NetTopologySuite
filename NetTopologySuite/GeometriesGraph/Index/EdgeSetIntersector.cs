using System.Collections.Generic;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// An <c>EdgeSetIntersector</c> computes all the intersections between the
    /// edges in the set.  It adds the computed intersections to each edge
    /// they are found on.  It may be used in two scenarios:
    /// determining the internal intersections between a single set of edges
    /// determining the mutual intersections between two different sets of edges
    /// It uses a <c>SegmentIntersector</c> to compute the intersections between
    /// segments and to record statistics about what kinds of intersections were found.
    /// </summary>
    public abstract class EdgeSetIntersector
    {
         /// <summary>
        /// Computes all self-intersections between edges in a set of edges,
        /// allowing client to choose whether self-intersections are computed.
        /// </summary>
        /// <param name="edges">A list of edges to test for intersections.</param>
        /// <param name="si">The SegmentIntersector to use.</param>
        /// <param name="testAllSegments"><c>true</c> if self-intersections are to be tested as well.</param>
        abstract public void ComputeIntersections(IList<Edge> edges, SegmentIntersector si, bool testAllSegments);

        /// <summary>
        /// Computes all mutual intersections between two sets of edges.
        /// </summary>
        abstract public void ComputeIntersections(IList<Edge> edges0, IList<Edge> edges1, SegmentIntersector si);
    }
}
