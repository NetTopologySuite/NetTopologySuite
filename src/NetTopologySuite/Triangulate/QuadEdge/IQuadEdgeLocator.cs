namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// An interface for classes which locate an edge in a <see cref="QuadEdgeSubdivision"/>
    /// which either contains a given <see cref="Vertex"/> V
    /// or is an edge of a triangle which contains V.
    /// Implementors may utilized different strategies for
    /// optimizing locating containing edges/triangles.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IQuadEdgeLocator
    {
        QuadEdge Locate(Vertex v);
    }
}