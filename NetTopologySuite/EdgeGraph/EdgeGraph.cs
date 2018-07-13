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
            var e0 = CreateEdge(p0);
            var e1 = CreateEdge(p1);
            HalfEdge.Init(e0, e1);
            return e0;
        }

        /// <summary>
        /// Adds an edge between the coordinates orig and dest
        /// to this graph.
        /// </summary>
        /// <remarks>
        /// Only valid edges can be added (in particular, zero-length segments cannot be added)
        /// </remarks>
        /// <param name="orig">the edge origin location</param>
        /// <param name="dest">the edge destination location</param>
        /// <returns>The created edge</returns>
        /// <returns><value>null</value> if the edge was invalid and not added</returns>
        /// <seealso cref="IsValidEdge(Coordinate,Coordinate)"/>
        public virtual HalfEdge AddEdge(Coordinate orig, Coordinate dest)
        {
            if (!IsValidEdge(orig, dest)) return null;

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

            var e = Insert(orig, dest, eAdj);
            return e;
        }

        /// <summary>
        /// Test if an the coordinates for an edge form a valid edge (with non-zero length)
        /// </summary>
        /// <param name="orig">The start coordinate</param>
        /// <param name="dest">The end coordinate</param>
        /// <returns><value>true</value> of the edge formed is valid</returns>
        public static bool IsValidEdge(Coordinate orig, Coordinate dest)
        {
            int cmp = dest.CompareTo(orig);
            return cmp != 0;
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
            var e = Create(orig, dest);
            if (eAdj != null)
                eAdj.Insert(e);
            else vertexMap.Add(orig, e);

            HalfEdge eAdjDest;
            bool eAdjDestFound = vertexMap.TryGetValue(dest, out eAdjDest);
            var sym = e.Sym;
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
            var e = vertexMap[orig];
            return e == null ? null : e.Find(dest);
        }
    }
}
