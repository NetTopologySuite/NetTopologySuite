namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// Locates <see cref="QuadEdge"/>s in a <see cref="QuadEdgeSubdivision"/>,
    /// optimizing the search by starting in the
    /// locality of the last edge found.
    /// </summary>
    /// <author>Martin Davis</author>
    public class LastFoundQuadEdgeLocator : IQuadEdgeLocator
    {
        private readonly QuadEdgeSubdivision _subdiv;
        private QuadEdge _lastEdge;

        public LastFoundQuadEdgeLocator(QuadEdgeSubdivision subdiv)
        {
            _subdiv = subdiv;
            Init();
        }

        private void Init()
        {
            _lastEdge = FindEdge();
        }

        private QuadEdge FindEdge()
        {
            var edges = _subdiv.GetEdges();
            // assume there is an edge - otherwise will get an exception
            var enumerator = edges.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            else
            {
                throw new System.IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Locates an edge e, such that either v is on e, or e is an edge of a triangle containing v.
        /// The search starts from the last located edge and proceeds on the general direction of v.
        /// </summary>
        public QuadEdge Locate(Vertex v)
        {
            if (! _lastEdge.IsLive)
            {
                Init();
            }

            var e = _subdiv.LocateFromEdge(v, _lastEdge);
            _lastEdge = e;
            return e;
        }
    }
}