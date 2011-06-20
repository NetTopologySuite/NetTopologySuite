using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// An <see cref="EdgeSetIntersector{TCoordinate}"/> computes all the 
    /// intersections between the edges in the set.  It adds the computed 
    /// intersections to each edge they are found on.
    /// </summary>
    /// <remarks>
    /// An <see cref="EdgeSetIntersector{TCoordinate}"/> may be used in two scenarios:
    /// <list type="bullet">
    /// <item><description>
    /// determining the internal intersections between a single set of edges
    /// </description></item>
    /// <item><description>
    /// determining the mutual intersections between two different sets of edges
    /// </description></item>
    /// </list>
    /// It uses a <see cref="SegmentIntersector{TCoordinate}"/> to compute the 
    /// intersections between segments and to record statistics about what 
    /// kinds of intersections were found.
    /// </remarks>
    public abstract class EdgeSetIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Computes all self-intersections between edges in a set of edges,
        /// allowing client to choose whether self-intersections are computed.
        /// </summary>
        /// <param name="edges">A list of edges to test for intersections.</param>
        /// <param name="si">
        /// The <see cref="SegmentIntersector{TCoordinate}"/> to use.
        /// </param>
        /// <param name="testAllSegments">
        /// <see langword="true"/> if self-intersections are to be tested as well.
        /// </param>
        public abstract void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges,
                                                  SegmentIntersector<TCoordinate> si,
                                                  Boolean testAllSegments);

        /// <summary> 
        /// Computes all mutual intersections between two sets of edges.
        /// </summary>
        /// <param name="edges0">
        /// One set of edges to test for mutual intersections with the other set.
        /// </param>
        /// <param name="edges1">
        /// The other set of edges to test for mutual intersections.
        /// </param>
        /// <param name="si">
        /// The <see cref="SegmentIntersector{TCoordinate}"/> to use.
        /// </param>
        public abstract void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges0,
                                                  IEnumerable<Edge<TCoordinate>> edges1,
                                                  SegmentIntersector<TCoordinate> si);
    }
}