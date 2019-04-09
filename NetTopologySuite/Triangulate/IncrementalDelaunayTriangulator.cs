using System.Collections.Generic;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// Computes a Delaunay Triangulation of a set of <see cref="Vertex"/>es, using an
    /// incremental insertion algorithm.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public class IncrementalDelaunayTriangulator
    {
        private readonly QuadEdgeSubdivision _subdiv;
        private bool _isUsingTolerance;

        /// <summary>
        /// Creates a new triangulator using the given <see cref="QuadEdgeSubdivision"/>.
        /// The triangulator uses the tolerance of the supplied subdivision.
        /// </summary>
        /// <param name="subdiv">a subdivision in which to build the TIN</param>
        public IncrementalDelaunayTriangulator(QuadEdgeSubdivision subdiv)
        {
            _subdiv = subdiv;
            _isUsingTolerance = subdiv.Tolerance > 0.0;
        }

        /// <summary>
        /// Inserts all sites in a collection. The inserted vertices <b>MUST</b> be
        /// unique up to the provided tolerance value. (i.e. no two vertices should be
        /// closer than the provided tolerance value). They do not have to be rounded
        /// to the tolerance grid, however.
        /// </summary>
        /// <param name="vertices">a Collection of Vertex</param>
        /// <exception cref="LocateFailureException">if the location algorithm fails to converge in a reasonable number of iterations</exception>
        public void InsertSites(ICollection<Vertex> vertices)
        {
            foreach (var v in vertices)
            {
                InsertSite(v);
            }
        }

        /// <summary>
        /// Inserts a new point into a subdivision representing a Delaunay
        /// triangulation, and fixes the affected edges so that the result is still a
        /// Delaunay triangulation.
        /// </summary>
        /// <returns>a quadedge containing the inserted vertex</returns>
        public QuadEdge.QuadEdge InsertSite(Vertex v)
        {
            /*
             * This code is based on Guibas and Stolfi (1985), with minor modifications
             * and a bug fix from Dani Lischinski (Graphic Gems 1993). (The modification
             * I believe is the test for the inserted site falling exactly on an
             * existing edge. Without this test zero-width triangles have been observed
             * to be created)
             */
            var e = _subdiv.Locate(v);

            if (_subdiv.IsVertexOfEdge(e, v)) {
                // point is already in subdivision.
                return e;
            }
            if (_subdiv.IsOnEdge(e, v.Coordinate))
            {
                // the point lies exactly on an edge, so delete the edge
                // (it will be replaced by a pair of edges which have the point as a vertex)
                e = e.OPrev;
                _subdiv.Delete(e.ONext);
            }

            /*
             * Connect the new point to the vertices of the containing triangle
             * (or quadrilateral, if the new point fell on an existing edge.)
             */
            var baseQuadEdge = _subdiv.MakeEdge(e.Orig, v);
            QuadEdge.QuadEdge.Splice(baseQuadEdge, e);
            var startEdge = baseQuadEdge;
            do {
                baseQuadEdge = _subdiv.Connect(e, baseQuadEdge.Sym);
                e = baseQuadEdge.OPrev;
            } while (e.LNext != startEdge);

            // Examine suspect edges to ensure that the Delaunay condition
            // is satisfied.
            do {
                var t = e.OPrev;
                if (t.Dest.RightOf(e) && v.IsInCircle(e.Orig, t.Dest, e.Dest)) {
                    QuadEdge.QuadEdge.Swap(e);
                    e = e.OPrev;
                } else if (e.ONext == startEdge) {
                    return baseQuadEdge; // no more suspect edges.
                } else {
                    e = e.ONext.LPrev;
                }
            } while (true);
        }
    }
}