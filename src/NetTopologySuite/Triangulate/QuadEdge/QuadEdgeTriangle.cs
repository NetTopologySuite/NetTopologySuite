using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /// <summary>
    /// Models a triangle formed from <see cref="QuadEdge"/>s in a <see cref="QuadEdgeSubdivision"/>
    /// which forms a triangulation. The class provides methods to access the
    /// topological and geometric properties of the triangle and its neighbours in
    /// the triangulation. Triangle vertices are ordered in CCW orientation in the
    /// structure.
    /// </summary>
    /// <remarks>
    /// QuadEdgeTriangles support having an external data attribute attached to them.
    /// Alternatively, this class can be subclassed and attributes can
    /// be defined in the subclass.  Subclasses will need to define
    /// their own <c>BuilderVisitor</c> class
    /// and <c>CreateOn</c> method.
    /// </remarks>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public class QuadEdgeTriangle
    {
        /// <summary>
        /// Creates <see cref="QuadEdgeTriangle"/>s for all facets of a
        /// <see cref="QuadEdgeSubdivision"/> representing a triangulation.
        /// The <tt>data</tt> attributes of the <see cref="QuadEdge"/>s in the subdivision
        /// will be set to point to the triangle which contains that edge.
        /// This allows tracing the neighbour triangles of any given triangle.
        /// </summary>
        /// <param name="subdiv">The QuadEdgeSubdivision to create the triangles on.</param>
        /// <returns>A List of the created QuadEdgeTriangles</returns>
        public static IList<QuadEdgeTriangle> CreateOn(QuadEdgeSubdivision subdiv)
        {
            var visitor = new QuadEdgeTriangleBuilderVisitor();
            subdiv.VisitTriangles(visitor, false);
            return visitor.GetTriangles();
        }

        /// <summary>
        /// Tests whether the point pt is contained in the triangle defined by 3 <see cref="Vertex"/>es.
        /// </summary>
        /// <param name="tri">an array containing at least 3 Vertexes</param>
        /// <param name="pt">the point to test</param>
        /// <returns>true if the point is contained in the triangle</returns>
        public static bool Contains(Vertex[] tri, Coordinate pt)
        {
            var ring = new[]
                           {
                               tri[0].Coordinate,
                               tri[1].Coordinate,
                               tri[2].Coordinate,
                               tri[0].Coordinate
                           };
            return PointLocation.IsInRing(pt, ring);
        }

        /// <summary>
        /// Tests whether the point pt is contained in the triangle defined by 3 <see cref="QuadEdge"/>es.
        /// </summary>
        /// <param name="tri">an array containing at least 3 QuadEdges</param>
        /// <param name="pt">the point to test</param>
        /// <returns>true if the point is contained in the triangle</returns>
        public static bool Contains(QuadEdge[] tri, Coordinate pt)
        {
            var ring = new[]
                           {
                               tri[0].Orig.Coordinate,
                               tri[1].Orig.Coordinate,
                               tri[2].Orig.Coordinate,
                               tri[0].Orig.Coordinate
                           };
            return PointLocation.IsInRing(pt, ring);
        }

        public static Geometry ToPolygon(Vertex[] v)
        {
            var ringPts = new[]
                              {
                                  v[0].Coordinate,
                                  v[1].Coordinate,
                                  v[2].Coordinate,
                                  v[0].Coordinate
                              };
            var fact = new GeometryFactory();
            var ring = fact.CreateLinearRing(ringPts);
            var tri = fact.CreatePolygon(ring);
            return tri;
        }

        public static Geometry ToPolygon(QuadEdge[] e)
        {
            var ringPts = new[]
                              {
                                  e[0].Orig.Coordinate,
                                  e[1].Orig.Coordinate,
                                  e[2].Orig.Coordinate,
                                  e[0].Orig.Coordinate
                              };
            var fact = new GeometryFactory();
            var ring = fact.CreateLinearRing(ringPts);
            var tri = fact.CreatePolygon(ring);
            return tri;
        }

        /*
        /// <summary>
        /// Finds the next index around the triangle. Index may be an edge or vertex index.
        /// </summary>
        /// <param name="index" />
        /// <returns>The next index</returns>
        public static int NextIndex(int index)
        {
            return index = (index + 1)%3;
        }
         */
        private QuadEdge[] _edge;

        /// <summary>
        /// Creates a new triangle from the given edges.
        /// </summary>
        /// <param name="edge">An array of the edges of the triangle in CCW order</param>
        public QuadEdgeTriangle(QuadEdge[] edge)
        {
            _edge = CopyOf(edge);
            // link the quadedges back to this triangle
            for (int i = 0; i < 3; i++)
            {
                edge[i].Data = this;
            }
        }

        private static QuadEdge[] CopyOf(QuadEdge[] edge)
        {
            var res = new QuadEdge[edge.Length];
            for (int i = 0; i < edge.Length; i++)
                res[i] = edge[i];
            return res;
        }

        /// <summary>
        /// Gets or sets the external data value for this triangle.
        /// </summary>
        public object Data { get; set; }

        public void Kill()
        {
            _edge = null;
        }

        public bool IsLive()
        {
            return _edge != null;
        }

        public QuadEdge[] GetEdges()
        {
            return _edge;
        }

        public QuadEdge GetEdge(int i)
        {
            return _edge[i];
        }

        public Vertex GetVertex(int i)
        {
            return _edge[i].Orig;
        }

        /// <summary>
        /// Gets the vertices for this triangle.
        /// </summary>
        /// <returns>a new array containing the triangle vertices</returns>
        public Vertex[] GetVertices()
        {
            var vert = new Vertex[3];
            for (int i = 0; i < 3; i++)
            {
                vert[i] = GetVertex(i);
            }
            return vert;
        }

        public Coordinate GetCoordinate(int i)
        {
            return _edge[i].Orig.Coordinate;
        }

        /// <summary>
        /// Gets the index for the given edge of this triangle
        /// </summary>
        /// <param name="e">a QuadEdge</param>
        /// <returns>the index of the edge in this triangle,<br/>
        /// or -1 if the edge is not an edge of this triangle
        /// </returns>
        public int GetEdgeIndex(QuadEdge e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_edge[i] == e)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index for the edge that starts at vertex v.
        /// </summary>
        /// <param name="v">the vertex to find the edge for</param>
        /// <returns>the index of the edge starting at the vertex, <br/>
        /// or -1 if the vertex is not in the triangle
        /// </returns>
        public int GetEdgeIndex(Vertex v)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_edge[i].Orig == v)
                    return i;
            }
            return -1;
        }

        public void GetEdgeSegment(int i, LineSegment seg)
        {
            seg.P0 = _edge[i].Orig.Coordinate;
            int nexti = (i + 1)%3;
            seg.P1 = _edge[nexti].Orig.Coordinate;
        }

        public Coordinate[] GetCoordinates()
        {
            var pts = new Coordinate[4];
            for (int i = 0; i < 3; i++)
            {
                pts[i] = _edge[i].Orig.Coordinate;
            }
            pts[3] = pts[0].Copy();
            return pts;
        }

        public bool Contains(Coordinate pt)
        {
            var ring = GetCoordinates();
            return PointLocation.IsInRing(pt, ring);
        }

        public Geometry GetGeometry(GeometryFactory fact)
        {
            var ring = fact.CreateLinearRing(GetCoordinates());
            var tri = fact.CreatePolygon(ring);
            return tri;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return GetGeometry(new GeometryFactory()).ToString();
        }

        /// <summary>
        /// Tests whether this triangle is adjacent to the outside of the subdivision.
        /// </summary>
        /// <returns>true if the triangle is adjacent to the subdivision exterior</returns>
        public bool IsBorder()
        {
            for (int i = 0; i < 3; i++)
            {
                if (GetAdjacentTriangleAcrossEdge(i) == null)
                    return true;
            }
            return false;
        }

        public bool IsBorder(int i)
        {
            return GetAdjacentTriangleAcrossEdge(i) == null;
        }

        public QuadEdgeTriangle GetAdjacentTriangleAcrossEdge(int edgeIndex)
        {
            return (QuadEdgeTriangle) GetEdge(edgeIndex).Sym.Data;
        }

        public int GetAdjacentTriangleEdgeIndex(int i)
        {
            return GetAdjacentTriangleAcrossEdge(i).GetEdgeIndex(GetEdge(i).Sym);
        }

        /// <summary>
        /// Gets the triangles which are adjacent (include) to a
        /// given vertex of this triangle.
        /// </summary>
        /// <param name="vertexIndex">The vertex to query</param>
        /// <returns>A list of the vertex-adjacent triangles</returns>
        public IList<QuadEdgeTriangle> GetTrianglesAdjacentToVertex(int vertexIndex)
        {
            // Assert: isVertex
            var adjTris = new List<QuadEdgeTriangle>();

            var start = GetEdge(vertexIndex);
            var qe = start;
            do
            {
                var adjTri = (QuadEdgeTriangle) qe.Data;
                if (adjTri != null)
                {
                    adjTris.Add(adjTri);
                }
                qe = qe.ONext;
            } while (qe != start);

            return adjTris;

        }

        /// <summary>
        /// Gets the neighbours of this triangle. If there is no neighbour triangle, the array element is
        /// <c>null</c>
        /// </summary>
        /// <returns>an array containing the 3 neighbours of this triangle</returns>
        public QuadEdgeTriangle[] GetNeighbours()
        {
            var neigh = new QuadEdgeTriangle[3];
            for (int i = 0; i < 3; i++)
            {
                neigh[i] = (QuadEdgeTriangle) GetEdge(i).Sym.Data;
            }
            return neigh;
        }

        private class QuadEdgeTriangleBuilderVisitor : ITriangleVisitor
        {

            private readonly List<QuadEdgeTriangle> _triangles = new List<QuadEdgeTriangle>();

            //public QuadEdgeTriangleBuilderVisitor() { }

            public void Visit(QuadEdge[] edges)
            {
                _triangles.Add(new QuadEdgeTriangle(edges));
            }

            public List<QuadEdgeTriangle> GetTriangles()
            {
                return _triangles;
            }
        }

    }
}
