using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Triangulate.Quadedge
{
    ///<summary>
    /// Models a triangle formed from <see cref="QuadEdge{TCoordinate, TData}"/>s in a <see cref="QuadEdgeSubdivision{TCoordinate}"/>. Provides methods to
    /// access the topological and geometric properties of the triangle. Triangle edges are oriented CCW.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    ///<typeparam name="TData"></typeparam>
    public class QuadEdgeTriangle<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {

        ///<summary>
        /// Tests whether the point pt is contained in the triangle defined by 3 <see cref="Vertex{TCoordinate}"/>es.
        ///</summary>
        ///<param name="tri"> an array containing at least 3 Vertexes</param>
        ///<param name="pt">the point to test</param>
        ///<returns>true if the point is contained in the triangle</returns>
        public static Boolean Contains(Vertex<TCoordinate>[] tri, TCoordinate pt)
        {
            TCoordinate[] ring = new TCoordinate[]{
            tri[0].Coordinate,
            tri[1].Coordinate,
            tri[2].Coordinate,
            tri[0].Coordinate};
            return CGAlgorithms<TCoordinate>.IsPointInRing(pt, ring);
        }

        ///<summary>
        /// Tests whether the point pt is contained in the triangle defined by 3 <see cref="QuadEdge{TCoordinate}"/>es.
        ///</summary>
        ///<param name="tri">an array containing at least 3 QuadEdges</param>
        ///<param name="pt">the point to test</param>
        ///<returns>true if the point is contained in the triangle</returns>
        public static Boolean Contains(QuadEdge<TCoordinate>[] tri, TCoordinate pt)
        {
            TCoordinate[] ring = new TCoordinate[]{
            tri[0].Origin.Coordinate,
            tri[1].Origin.Coordinate,
            tri[2].Origin.Coordinate,
            tri[0].Origin.Coordinate};
            return CGAlgorithms<TCoordinate>.IsPointInRing(pt, ring);
        }

        public static IPolygon<TCoordinate> ToPolygon(IGeometryFactory<TCoordinate> fact, Vertex<TCoordinate>[] v)
        {
            TCoordinate[] ringPts = new TCoordinate[]{
            v[0].Coordinate,
            v[1].Coordinate,
            v[2].Coordinate,
            v[0].Coordinate};
            ILinearRing<TCoordinate> ring = fact.CreateLinearRing(ringPts);
            return fact.CreatePolygon(ring, null);
        }

        public static IPolygon<TCoordinate> ToPolygon(IGeometryFactory<TCoordinate> fact, QuadEdge<TCoordinate>[] e)
        {
            TCoordinate[] ringPts = new TCoordinate[]{
            e[0].Origin.Coordinate,
            e[1].Origin.Coordinate,
            e[2].Origin.Coordinate,
            e[0].Origin.Coordinate};
            ILinearRing<TCoordinate> ring = fact.CreateLinearRing(ringPts);
            return fact.CreatePolygon(ring, null);
        }


        ///<summary>
        /// Finds the next index around the triangle. Index may be an edge or vertex index.
        ///</summary>
        ///<param name="index"></param>
        ///<returns></returns>
        public static int NextIndex(int index)
        {
            return index = (index + 1) % 3;
        }

        private QuadEdge<TCoordinate>[] _edge;

        public QuadEdgeTriangle(QuadEdge<TCoordinate>[] edge)
        {
            _edge = (QuadEdge<TCoordinate>[])edge.Clone();
        }

        public void Kill()
        {
            _edge = null;
        }

        public Boolean IsLive
        {
            get { return _edge != null; }
        }

        public QuadEdge<TCoordinate>[] Edges
        {
            get { return _edge; }
        }

        public QuadEdge<TCoordinate> GetEdge(int i)
        {
            return _edge[i];
        }

        public Vertex<TCoordinate> GetVertex(int i)
        {
            return _edge[i].Origin;
        }

        ///<summary>
        /// Gets the vertices for this triangle.
        ///</summary>
        ///<returns>a new array containing the triangle vertices</returns>
        public Vertex<TCoordinate>[] GetVertices()
        {
            Vertex<TCoordinate>[] vert = new Vertex<TCoordinate>[3];
            for (int i = 0; i < 3; i++)
            {
                vert[i] = GetVertex(i);
            }
            return vert;
        }

        public TCoordinate GetCoordinate(int i)
        {
            return _edge[i].Origin.Coordinate;
        }


        ///<summary>
        /// Gets the index for the given _edge of this triangle
        ///</summary>
        ///<param name="e">e a QuadEdge</param>
        ///<returns>the index of the _edge in this triangle</returns>
        ///<returns>-1 if the edge is not an _edge of this triangle</returns>
        public int GetEdgeIndex(QuadEdge<TCoordinate> e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_edge[i] == e)
                    return i;
            }
            return -1;
        }


        ///<summary>
        /// Gets the index for the _edge that starts at vertex v.
        ///</summary>
        ///<param name="v">the vertex to find the _edge for</param>
        ///<returns>the index of the _edge starting at the vertex</returns>
        ///<returns>-1 if the vertex is not in the triangle</returns>
        public int GetEdgeIndex(Vertex<TCoordinate> v)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_edge[i].Origin == v)
                    return i;
            }
            return -1;
        }

        public LineSegment<TCoordinate> GetEdgeSegment(int i)
        {
            TCoordinate p0 = _edge[i].Origin.Coordinate;
            int nexti = (i + 1) % 3;
            TCoordinate p1 = _edge[nexti].Origin.Coordinate;
            return new LineSegment<TCoordinate>(p0, p1);
        }

        public TCoordinate[] GetCoordinates()
        {
            TCoordinate[] pts = new TCoordinate[4];
            for (int i = 0; i < 3; i++)
            {
                pts[i] = _edge[i].Origin.Coordinate;
            }
            pts[3] = (TCoordinate) pts[0].Clone();
            
            return pts;
        }

        public Boolean Contains(TCoordinate pt)
        {
            TCoordinate[] ring = GetCoordinates();
            return CGAlgorithms<TCoordinate>.IsPointInRing(pt, ring);
        }

        public IGeometry<TCoordinate> GetGeometry(IGeometryFactory<TCoordinate> fact)
        {
            ILinearRing<TCoordinate> ring = fact.CreateLinearRing(GetCoordinates());
            IPolygon<TCoordinate> tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        public override String ToString()
        {
            Vertex<TCoordinate>[] vertices = GetVertices();
            return string.Format("POLYGON({0}, {1}, {2})", vertices[0].ToString(), vertices[1].ToString(), vertices[2].ToString());
        }

        /**
         * Tests whether this triangle is adjacent to the outside of the subdivision.
         * 
         * @return true if the triangle is adjacent to the subdivision exterior
         */
        public Boolean IsBorder()
        {
            for (int i = 0; i < 3; i++)
            {
                if (GetAdjacentTriangleAcrossEdge(i) == null)
                    return true;
            }
            return false;
        }

        public Boolean IsBorder(int i)
        {
            return GetAdjacentTriangleAcrossEdge(i) == null;
        }

        public QuadEdgeTriangle<TCoordinate> GetAdjacentTriangleAcrossEdge(int edgeIndex)
        {
            return (QuadEdgeTriangle<TCoordinate>)GetEdge(edgeIndex).Sym().Data;
        }

        public int GetAdjacentTriangleEdgeIndex(int i)
        {
            return GetAdjacentTriangleAcrossEdge(i).GetEdgeIndex(GetEdge(i).Sym());
        }

        public List<QuadEdgeTriangle<TCoordinate>> GetTrianglesAdjacentToVertex(Int32 vertexIndex)
        {
            // Assert: isVertex
            List<QuadEdgeTriangle<TCoordinate>> adjTris = new List<QuadEdgeTriangle<TCoordinate>>();

            QuadEdge<TCoordinate> start = GetEdge(vertexIndex);
            QuadEdge<TCoordinate> qe = start;
            do
            {
                QuadEdgeTriangle<TCoordinate> adjTri = (QuadEdgeTriangle<TCoordinate>)qe.Data;
                if (adjTri != null)
                {
                    adjTris.Add(adjTri);
                }
                qe = qe.OriginNext;
            } while (qe != start);

            return adjTris;

        }

        ///<summary>
        /// Gets the neighbours of this triangle. If there is no neighbour triangle, the array element is <value>null</value>
        ///</summary>
        ///<returns>an array containing the 3 neighbours of this triangle</returns>
        public QuadEdgeTriangle<TCoordinate>[] GetNeighbours()
        {
            QuadEdgeTriangle<TCoordinate>[] neigh = new QuadEdgeTriangle<TCoordinate>[3];
            for (int i = 0; i < 3; i++)
            {
                neigh[i] = (QuadEdgeTriangle<TCoordinate>)GetEdge(i).Sym().Data;
            }
            return neigh;
        }

    }
}
