using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Triangulate.Quadedge;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate
{
    ///<summary>
    /// Computes a Delauanay Triangulation of a set of {@link Vertex}es, using an incrementatal insertion algorithm.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class IncrementalDelaunayTriangulator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly QuadEdgeSubdivision<TCoordinate> _subdiv;

        ///<summary>
        /// Creates a new triangulator using the given {@link QuadEdgeSubdivision}.
        /// The triangulator uses the tolerance of the supplied subdivision.
        ///</summary>
        ///<param name="subdiv">a subdivision in which to build the TIN</param>
        public IncrementalDelaunayTriangulator(QuadEdgeSubdivision<TCoordinate> subdiv)
        {
            _subdiv = subdiv;

        }

        ///<summary>
        /// Inserts all sites in a collection. The inserted vertices <b>MUST</b> be
        /// unique up to the provided tolerance value. (i.e. no two vertices should be
        /// closer than the provided tolerance value). They do not have to be rounded
        /// to the tolerance grid, however.
        ///</summary>
        ///<param name="vertices">a Collection of Vertex</param>
        /// <exception cref="LocateFailureException{TCoordinate}">if the location algorithm fails to converge in a reasonable number of iterations</exception>
        public void InsertSites(IEnumerable<Vertex<TCoordinate>> vertices)
        {
            foreach (Vertex<TCoordinate> vertex in vertices)
                InsertSite(vertex);
        }

        ///<summary>
        /// Inserts a new point into a subdivision representing a Delaunay
        /// triangulation, and fixes the affected edges so that the result is still a
        /// Delaunay triangulation.
        ///</summary>
        ///<param name="v">vertex to insert</param>
        ///<returns>a quadedge containing the inserted vertex</returns>
        public QuadEdge<TCoordinate> InsertSite(Vertex<TCoordinate> v) {

		/**
		 * This code is based on Guibas and Stolfi (1985), with minor modifications
		 * and a bug fix from Dani Lischinski (Graphic Gems 1993). (The modification
		 * I believe is the test for the inserted site falling exactly on an
		 * existing edge. Without this test zero-width triangles have been observed
		 * to be created)
		 */
		QuadEdge<TCoordinate    > e = _subdiv.Locate(v);

		if (_subdiv.IsVertexOfEdge(e, v))
        {
			// point is already in subdivision.
			return e; 
		} 
		if (_subdiv.IsOnEdge(e, v.Coordinate))
        {
			// the point lies exactly on an edge, so delete the edge 
			// (it will be replaced by a pair of edges which have the point as a vertex)
			e = e.OriginPrev;
			_subdiv.Delete(e.OriginNext);
		}

		/**
		 * Connect the new point to the vertices of the containing triangle 
		 * (or quadrilateral, if the new point fell on an existing edge.)
		 */
		QuadEdge<TCoordinate> baseQe = _subdiv.MakeEdge(e.Origin, v);
        QuadEdge<TCoordinate>.Splice(baseQe, e);
		QuadEdge<TCoordinate> startEdge = baseQe;
		do
        {
			baseQe = _subdiv.Connect(e, baseQe.Sym());
			e = baseQe.OriginPrev;
		} while (e.LeftNext != startEdge);

		// Examine suspect edges to ensure that the Delaunay condition
		// is satisfied.
		do {
			QuadEdge<TCoordinate> t = e.OriginPrev;
			if (t.Destination.RightOf(e) && v.InCircle(e.Origin, t.Destination, e.Destination)) 
            {
				QuadEdge<TCoordinate>.Swap(e);
				e = e.OriginPrev;
			}
            else if (e.OriginNext == startEdge)
            {
				return baseQe; // no more suspect edges.
			}
            else 
            {
				e = e.OriginNext.LeftPrev;
			}
		} while (true);
	}

    }
}
