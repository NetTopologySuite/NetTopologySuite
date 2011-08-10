using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /**
     * A framework to visit sets of edge-connected {@link QuadEdgeTriangle}s in breadth-first order
     * 
     * @author Martin Davis
     * @version 1.0
     */

    public class EdgeConnectedTriangleTraversal
    {
        private readonly LinkedList<QuadEdgeTriangle> _triQueue = new LinkedList<QuadEdgeTriangle>();

        public EdgeConnectedTriangleTraversal()
        {
        }

        public void Init(QuadEdgeTriangle tri)
        {
            _triQueue.AddLast(tri);
        }

        /**
         * Called to initialize the traversal queue with a given set of {@link QuadEdgeTriangle}s
         * 
         * @param tris a collection of QuadEdgeTriangle
         */

        public void Init(IEnumerable<QuadEdgeTriangle> tris)
        {
            foreach(var tri in tris)
            _triQueue.AddLast(tri);
        }

        /**
         * Subclasses can call this method to add a triangle to the end of the queue. This is useful for
         * initializing the queue to a chosen set of triangles.
         * 
         * @param tri a triangle
         */
        /*
         * protected void addLast(QuadEdgeTriangle tri) { triQueue.addLast(tri); }
         */

        /**
         * Subclasses call this method to perform the visiting process.
         */

        public void VisitAll(ITraversalVisitor visitor)
        {
            while (!_triQueue.isEmpty())
            {
                QuadEdgeTriangle tri = (QuadEdgeTriangle) _triQueue.removeFirst();
                process(tri, visitor);
            }
        }

        private void process(QuadEdgeTriangle currTri, ITraversalVisitor visitor)
        {
            currTri.getNeighbours();
            for (int i = 0; i < 3; i++)
            {
                QuadEdgeTriangle neighTri = (QuadEdgeTriangle) currTri.getEdge(i).sym().getData();
                if (neighTri == null)
                    continue;
                if (visitor.Visit(currTri, i, neighTri))
                    _triQueue.AddLast(neighTri);
            }
        }

    }
}