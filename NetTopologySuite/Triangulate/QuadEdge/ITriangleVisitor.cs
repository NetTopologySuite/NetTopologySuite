namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// An interface for algorithms which process the triangles in a <see cref="QuadEdgeSubdivision"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public interface ITriangleVisitor
    {
        /// <summary>
        /// Visits the <see cref="QuadEdge"/>s of a triangle.
        /// </summary>
        /// <param name="triEdges">an array of the 3 quad edges in a triangle (in CCW order)</param>
        void Visit(QuadEdge[] triEdges);
    }
}