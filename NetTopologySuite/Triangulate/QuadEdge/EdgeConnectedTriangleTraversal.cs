using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// A framework to visit sets of edge-connected <see cref="QuadEdgeTriangle"/>s in breadth-first order
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public class EdgeConnectedTriangleTraversal
    {
        private readonly LinkedList<QuadEdgeTriangle> _triQueue = new LinkedList<QuadEdgeTriangle>();

        public void Init(QuadEdgeTriangle tri)
        {
            _triQueue.AddLast(tri);
        }

        /// <summary>
        /// Called to initialize the traversal queue with a given set of <see cref="QuadEdgeTriangle"/>s
        /// </summary>
        /// <param name="tris">a collection of QuadEdgeTriangle</param>
        public void Init(IEnumerable<QuadEdgeTriangle> tris)
        {
            foreach(var tri in tris)
            _triQueue.AddLast(tri);
        }

        /* <summary>
        // Subclasses can call this method to add a triangle to the end of the queue. This is useful for
        // initializing the queue to a chosen set of triangles.
        // </summary>
        // <param name="tri">a triangle</param>
        /*
         * protected void addLast(QuadEdgeTriangle tri) { triQueue.addLast(tri); }
         */

        /// <summary>
        /// Subclasses call this method to perform the visiting process.
        /// </summary>
        public void VisitAll(ITraversalVisitor visitor)
        {
            while (_triQueue.Count > 0)
            {
                var tri = _triQueue.First.Value;
                _triQueue.RemoveFirst();
                Process(tri, visitor);
            }
        }

        private void Process(QuadEdgeTriangle currTri, ITraversalVisitor visitor)
        {
            currTri.GetNeighbours();
            for (int i = 0; i < 3; i++)
            {
                var neighTri = (QuadEdgeTriangle)currTri.GetEdge(i).Sym.Data;
                if (neighTri == null)
                    continue;
                if (visitor.Visit(currTri, i, neighTri))
                    _triQueue.AddLast(neighTri);
            }
        }

    }
}