namespace NetTopologySuite.Triangulate.QuadEdge
{
    /**
     * An interface for algorithms which process the triangles in a {@link QuadEdgeSubdivision}.
     * 
     * @author Martin Davis
     * @version 1.0
     */

    public interface ITriangleVisitor
    {
        /**
         * Visits the {@link QuadEdge}s of a triangle.
         * 
         * @param triEdges an array of the 3 quad edges in a triangle (in CCW order)
         */
        void Visit(QuadEdge[] triEdges);
    }
}