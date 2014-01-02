using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.EdgeGraph
{
    /// <summary>
    /// A graph comprised of <see cref="HalfEdge"/>s.
    /// It supports tracking the vertices in the graph
    /// via edges incident on them, 
    /// to allow efficient lookup of edges and vertices.
    /// </summary>
    /// <remarks>
    /// This class may be subclassed to use a 
    /// different subclass of HalfEdge,
    /// by overriding <see cref="CreateEdge"/>.
    /// If additional logic is required to initialize
    /// edges then <see cref="AddEdge"/>
    /// can be overridden as well.
    /// </remarks>
    public class EdgeGraph
    {
        private readonly IDictionary<Coordinate, HalfEdge> vertexMap = new Dictionary<Coordinate, HalfEdge>();

        /// <summary>
        /// Creates a single HalfEdge.
        /// Override to use a different HalfEdge subclass.
        /// </summary>
        /// <param name="orig">the origin location</param>
        /// <returns>a new <see cref="HalfEdge"/> with the given origin</returns>
        protected virtual HalfEdge CreateEdge(Coordinate orig)
        {
            return new HalfEdge(orig);
        }

        private HalfEdge Create(Coordinate p0, Coordinate p1)
        {
            HalfEdge e0 = CreateEdge(p0);
            HalfEdge e1 = CreateEdge(p1);
            HalfEdge.Init(e0, e1);
            return e0;
        }

        /// <summary>
        /// Adds an edge between the coordinates orig and dest
        /// to this graph.
        /// </summary>
        /// <param name="orig">the edge origin location</param>
        /// <param name="dest">the edge destination location</param>
        /// <returns>the created edge</returns>
        public virtual HalfEdge AddEdge(Coordinate orig, Coordinate dest)
        {
            int cmp = dest.CompareTo(orig);
            // ignore zero-length edges
            if (cmp == 0)
                return null;

            // Attempt to find the edge already in the graph.
            // Return it if found.
            // Otherwise, use a found edge with same origin (if any) to construct new edge. 
            HalfEdge eAdj;
            bool eAdjFound = vertexMap.TryGetValue(orig, out eAdj);
            HalfEdge eSame = null;
            if (eAdjFound)
                eSame = eAdj.Find(dest);
            if (eSame != null)
                return eSame;

            HalfEdge e = Insert(orig, dest, eAdj);
            return e;
        }

        /// <summary>
        /// Inserts an edge not already present into the graph.
        /// </summary>
        /// <param name="orig">the edge origin location</param>
        /// <param name="dest">the edge destination location</param>
        /// <param name="eAdj">an existing edge with same orig (if any)</param>
        /// <returns>the created edge</returns>
        private HalfEdge Insert(Coordinate orig, Coordinate dest, HalfEdge eAdj)
        {
            // edge does not exist, so create it and insert in graph
            HalfEdge e = Create(orig, dest);
            if (eAdj != null)
                eAdj.Insert(e);
            else vertexMap.Add(orig, e);

            HalfEdge eAdjDest;
            bool eAdjDestFound = vertexMap.TryGetValue(dest, out eAdjDest);
            HalfEdge sym = e.Sym;
            if (eAdjDestFound)
                eAdjDest.Insert(sym);
            else vertexMap.Add(dest, sym);
            return e;
        }

        public IEnumerable<HalfEdge> GetVertexEdges()
        {
            return vertexMap.Values;
        }

        /// <summary>
        /// Finds an edge in this graph with the given origin
        /// and destination, if one exists.
        /// </summary>
        /// <param name="orig">the origin location</param>
        /// <param name="dest">the destination location</param>
        /// <returns>an edge with the given orig and dest, or null if none exists</returns>
        public HalfEdge FindEdge(Coordinate orig, Coordinate dest)
        {
            HalfEdge e = vertexMap[orig];
            return e == null ? null : e.Find(dest);
        }
    }
}
