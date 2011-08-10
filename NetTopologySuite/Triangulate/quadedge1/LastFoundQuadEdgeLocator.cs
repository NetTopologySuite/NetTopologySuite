namespace NetTopologySuite.Triangulate.QuadEdge
{
/**
 * Locates {@link QuadEdge}s in a {@link QuadEdgeSubdivision},
 * optimizing the search by starting in the
 * locality of the last edge found.
 * 
 * @author Martin Davis
 */

    public class LastFoundQuadEdgeLocator : IQuadEdgeLocator
    {
    private readonly QuadEdgeSubdivision _subdiv;
    private QuadEdge _lastEdge;

    public LastFoundQuadEdgeLocator(QuadEdgeSubdivision subdiv)
    {
        _subdiv = subdiv;
        init();
    }

    private void init()
    {
        _lastEdge = findEdge();
    }

    private QuadEdge findEdge()
    {
        var edges = _subdiv.getEdges();
        // assume there is an edge - otherwise will get an exception
        return (QuadEdge) edges.iterator().next();
    }

    /**
     * Locates an edge e, such that either v is on e, or e is an edge of a triangle containing v.
     * The search starts from the last located edge amd proceeds on the general direction of v.
     */

    public QuadEdge Locate(Vertex v)
    {
        if (! _lastEdge.IsLive)
        {
            init();
        }

        QuadEdge e = _subdiv.locateFromEdge(v, _lastEdge);
        _lastEdge = e;
        return e;
    }
}
}