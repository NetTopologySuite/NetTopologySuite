using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// A framework to visit sets of edge-connected <see cref="QuadEdgeTriangle{TCoordinate}"/>s in breadth-first order
    ///</summary>
    public class EdgeConnectedTriangleTraversal<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly Queue<QuadEdgeTriangle<TCoordinate>> _triQueue = new Queue<QuadEdgeTriangle<TCoordinate>>();

        ///<summary>
        /// Called to initialize the traversal queue with a given <see cref="QuadEdgeTriangle{TCoordinate}"/>
        ///</summary>
        ///<param name="tri"></param>
        public void Init(QuadEdgeTriangle<TCoordinate> tri)
        {        

            _triQueue.Enqueue(tri);
        }

        public void Init(IEnumerable<QuadEdgeTriangle<TCoordinate>> tris)
        {
            foreach (QuadEdgeTriangle<TCoordinate> tri in tris)
                Init(tri);
        }

        /**
         * Subclasses can call this method to add a triangle to the end of the queue. This is useful for
         * initializing the queue to a chosen set of triangles.
         * 
         * @param tri a triangle
         */
        /*
         * protected void addLast(QuadEdgeTriangle tri) { _triQueue.addLast(tri); }
         */

        ///<summary>
        /// Subclasses call this method to perform the visiting process.
        ///</summary>
        ///<param name="visitor"></param>
        public void VisitAll(ITraversalVisitor<TCoordinate> visitor)
        {
            while (_triQueue.Count > 0)
            {
                QuadEdgeTriangle<TCoordinate> tri = _triQueue.Dequeue();
                Process(tri, visitor);
            }
        }

        private void Process(QuadEdgeTriangle<TCoordinate> currTri, ITraversalVisitor<TCoordinate> visitor)
        {
            currTri.GetNeighbours();
            for (int i = 0; i < 3; i++)
            {
                QuadEdgeTriangle<TCoordinate> neighTri = (QuadEdgeTriangle<TCoordinate>)currTri.GetEdge(i).Sym().Data;
                if (neighTri == null)
                    continue;
                if (visitor.Visit(currTri, i, neighTri))
                    _triQueue.Enqueue(neighTri);
            }
        }

    }
}
