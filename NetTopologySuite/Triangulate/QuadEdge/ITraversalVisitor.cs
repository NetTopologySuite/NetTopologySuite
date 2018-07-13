namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// Interface for classes which process triangles visited during traversals of a
    /// <see cref="QuadEdgeSubdivision"/>
    /// </summary>
    /// <author>Martin Davis</author>
    public interface ITraversalVisitor
    {
        /// <summary>
        /// Visits a triangle during a traversal of a <see cref="QuadEdgeSubdivision"/>. An implementation of
        /// this method may perform processing on the current triangle. It must also decide whether a
        /// neighbouring triangle should be added to the queue so its neighbours are visited. Often it
        /// will perform processing on the neighbour triangle as well, in order to mark it as processed
        /// (visited) and/or to determine if it should be visited. Note that choosing <b>not</b> to
        /// visit the neighbouring triangle is the terminating condition for many traversal algorithms.
        /// In particular, if the neighbour triangle has already been visited, it should not be visited
        /// again.
        /// </summary>
        /// <param name="currTri">the current triangle being processed</param>
        /// <param name="edgeIndex">the index of the edge in the current triangle being traversed</param>
        /// <param name="neighbTri">a neighbouring triangle next in line to visit</param>
        /// <returns>true if the neighbour triangle should be visited</returns>
        bool Visit(QuadEdgeTriangle currTri, int edgeIndex, QuadEdgeTriangle neighbTri);
    }
}