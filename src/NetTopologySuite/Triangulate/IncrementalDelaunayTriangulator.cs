using NetTopologySuite.Algorithm;
using NetTopologySuite.Triangulate.QuadEdge;
using System.Collections.Generic;

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
        private bool _isForceConvex = true;

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
        /// Gets or sets whether the triangulation is forced to have a convex boundary. Because
        /// of the use of a finite-size frame, this condition requires special logic to
        /// enforce.The default is true, since this is a requirement for some uses of
        /// Delaunay Triangulations (such as Concave Hull generation). However, forcing
        /// the triangulation boundary to be convex may cause the overall frame
        /// triangulation to be non-Delaunay.This can cause a problem for Voronoi
        /// generation, so the logic can be disabled via this method.
        /// </summary>
        public bool ForceConvex
        {
            get => _isForceConvex;
            set => _isForceConvex = value;
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

            if (_subdiv.IsVertexOfEdge(e, v))
            {
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
            do
            {
                baseQuadEdge = _subdiv.Connect(e, baseQuadEdge.Sym);
                e = baseQuadEdge.OPrev;
            } while (e.LNext != startEdge);

            /*
             * Examine suspect edges to ensure that the Delaunay condition is satisfied.
             * If it is not, flip the edge and continue scanning.
             * 
             * Since the frame is not infinitely far away,
             * edges which touch the frame or are adjacent to it require special logic
             * to ensure the inner triangulation maintains a convex boundary.
             */
            do
            {
                //-- general case - flip if vertex is in circumcircle
                var t = e.OPrev;
                bool doFlip = t.Dest.RightOf(e) && v.IsInCircle(e.Orig, t.Dest, e.Dest);

                if (_isForceConvex)
                {
                    //-- special cases to ensure triangulation boundary is convex
                    if (IsConcaveBoundary(e))
                    {
                        //-- flip if the triangulation boundary is concave
                        doFlip = true;
                    }
                    else if (IsBetweenFrameAndInserted(e, v))
                    {
                        //-- don't flip if edge lies between the inserted vertex and a frame vertex
                        doFlip = false;
                    }
                }

                if (doFlip)
                {
                    //-- flip the edge within its quadrilateral
                    QuadEdge.QuadEdge.Swap(e);
                    e = e.OPrev;
                    continue;
                }

                if (e.ONext == startEdge)
                {
                    return baseQuadEdge; // no more suspect edges.
                }
                //-- check next edge
                e = e.ONext.LPrev;
            } while (true);
        }

        /// <summary>
        /// Tests if a edge touching a frame vertex
        /// creates a concavity in the triangulation boundary.
        /// </summary>
        /// <param name="e">The edge to test</param>
        /// <returns><c>true</c> if the triangulation boundary is concave at the edge</returns>
        private bool IsConcaveBoundary(QuadEdge.QuadEdge e)
        {
            if (_subdiv.IsFrameVertex(e.Dest))
            {
                return IsConcaveAtOrigin(e);
            }
            if (_subdiv.IsFrameVertex(e.Orig))
            {
                return IsConcaveAtOrigin(e.Sym);
            }
            return false;
        }

        /// <summary>
        /// Tests if the quadrilateral surrounding an edge is concave at the edge origin.
        /// Used to determine if the triangulation boundary has a concavity.
        /// </summary>
        /// <param name="e">The edge to test</param>
        /// <returns><c>true</c> if the quadrilateral surrounding an edge is concave at the edge origin</returns>
        private static bool IsConcaveAtOrigin(QuadEdge.QuadEdge e)
        {
            var p = e.Orig.Coordinate;
            var pp = e.OPrev.Dest.Coordinate;
            var pn = e.ONext.Dest.Coordinate;
            bool isConcave = OrientationIndex.CounterClockwise == Orientation.Index(pp, pn, p);
            return isConcave;
        }

        /// <summary>
        /// Edges whose adjacent triangles contain
        /// a frame vertex and the inserted vertex must not be flipped.
        /// </summary>
        /// <param name="e">The edge to test</param>
        /// <param name="vInsert">The inserted vertex</param>
        /// <returns><c>true</c> if the edge is between the frame and inserted vertex</returns>
        private bool IsBetweenFrameAndInserted(QuadEdge.QuadEdge e, Vertex vInsert)
        {
            var v1 = e.ONext.Dest;
            var v2 = e.OPrev.Dest;
            return (v1 == vInsert && _subdiv.IsFrameVertex(v2))
                || (v2 == vInsert && _subdiv.IsFrameVertex(v1));
        }
    }
}
