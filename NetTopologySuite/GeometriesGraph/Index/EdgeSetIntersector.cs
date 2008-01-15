using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
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
    public abstract class EdgeSetIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Computes all self-intersections between edges in a set of edges,
        /// allowing client to choose whether self-intersections are computed.
        /// </summary>
        /// <param name="edges">A list of edges to test for intersections.</param>
        /// <param name="si">The SegmentIntersector to use.</param>
        /// <param name="testAllSegments"><see langword="true"/> if self-intersections are to be tested as well.</param>
        public abstract void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges, SegmentIntersector<TCoordinate> si, Boolean testAllSegments);

        /// <summary> 
        /// Computes all mutual intersections between two sets of edges.
        /// </summary>
        public abstract void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges0, IEnumerable<Edge<TCoordinate>> edges1, SegmentIntersector<TCoordinate> si);
    }
}