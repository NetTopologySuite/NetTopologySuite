namespace NetTopologySuite.Triangulate.QuadEdge
{
    /**
     * Interface for classes which process triangles visited during travesals of a
     * {@link QuadEdgeSubdivision}
     * 
     * @author Martin Davis
     */

    public interface ITraversalVisitor
    {
        /**
         * Visits a triangle during a traversal of a {@link QuadEdgeSubdivision}. An implementation of
         * this method may perform processing on the current triangle. It must also decide whether a
         * neighbouring triangle should be added to the queue so its neighbours are visited. Often it
         * will perform processing on the neighbour triangle as well, in order to mark it as processed
         * (visited) and/or to determine if it should be visited. Note that choosing <b>not</b> to
         * visit the neighbouring triangle is the terminating condition for many traversal algorithms.
         * In particular, if the neighbour triangle has already been visited, it should not be visited
         * again.
         * 
         * @param currTri the current triangle being processed
         * @param edgeIndex the index of the edge in the current triangle being traversed
         * @param neighbTri a neighbouring triangle next in line to visit
         * @return true if the neighbour triangle should be visited
         */
        bool Visit(QuadEdgeTriangle currTri, int edgeIndex, QuadEdgeTriangle neighbTri);
    }
}