using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    /**
     * Models a triangle formed from {@link Quadedge}s in a {@link Subdivision}. Provides methods to
     * access the topological and geometric properties of the triangle. Triangle edges are oriented CCW.
     * 
     * @author Martin Davis
     * @version 1.0
     */

    public class QuadEdgeTriangle
    {

        /**
         * Tests whether the point pt is contained in the triangle defined by 3 {@link Vertex}es.
         * 
         * @param tri an array containing at least 3 Vertexes
         * @param pt the point to test
         * @return true if the point is contained in the triangle
         */

        public static bool Contains(Vertex[] tri, ICoordinate pt)
        {
            var ring = new ICoordinate[]
                                    {
                                        tri[0].Coordinate,
                                        tri[1].Coordinate,
                                        tri[2].Coordinate,
                                        tri[0].Coordinate
                                    };
            return CGAlgorithms.IsPointInRing(pt, ring);
        }

        /**
         * Tests whether the point pt is contained in the triangle defined by 3 {@link QuadEdge}es.
         * 
         * @param tri an array containing at least 3 QuadEdges
         * @param pt the point to test
         * @return true if the point is contained in the triangle
         */

        public static bool Contains(QuadEdge[] tri, ICoordinate pt)
        {
            var ring = new []
                                    {
                                        tri[0].Orig.Coordinate,
                                        tri[1].Orig.Coordinate,
                                        tri[2].Orig.Coordinate,
                                        tri[0].Orig.Coordinate
                                    };
            return CGAlgorithms.IsPointInRing(pt, ring);
        }

        public static IGeometry toPolygon(Vertex[] v)
        {
            var ringPts = new []
                                       {
                                           v[0].Coordinate,
                                           v[1].Coordinate,
                                           v[2].Coordinate,
                                           v[0].Coordinate
                                       };
            var fact = new GeometryFactory();
            var ring = fact.CreateLinearRing(ringPts);
            var tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        public static IGeometry toPolygon(QuadEdge[] e)
        {
            var ringPts = new []
                                       {
                                           e[0].Orig.Coordinate,
                                           e[1].Orig.Coordinate,
                                           e[2].Orig.Coordinate,
                                           e[0].Orig.Coordinate
                                       };
            var fact = new GeometryFactory();
            ILinearRing ring = fact.CreateLinearRing(ringPts);
            IPolygon tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        /**
         * Finds the next index around the triangle. Index may be an edge or vertex index.
         * 
         * @param index
         * @return
         */

        public static int nextIndex(int index)
        {
            return index = (index + 1)%3;
        }

        private QuadEdge[] edge;

        public QuadEdgeTriangle(QuadEdge[] edge)
        {
            this.edge = (QuadEdge[]) edge.Clone();
        }

        public void kill()
        {
            edge = null;
        }

        public bool isLive()
        {
            return edge != null;
        }

        public QuadEdge[] getEdges()
        {
            return edge;
        }

        public QuadEdge getEdge(int i)
        {
            return edge[i];
        }

        public Vertex getVertex(int i)
        {
            return edge[i].Orig;
        }

        /**
         * Gets the vertices for this triangle.
         * 
         * @return a new array containing the triangle vertices
         */

        public Vertex[] getVertices()
        {
            Vertex[] vert = new Vertex[3];
            for (int i = 0; i < 3; i++)
            {
                vert[i] = getVertex(i);
            }
            return vert;
        }

        public ICoordinate getCoordinate(int i)
        {
            return edge[i].Orig.Coordinate;
        }

        /**
         * Gets the index for the given edge of this triangle
         * 
         * @param e a QuadEdge
         * @return the index of the edge in this triangle
         * @return -1 if the edge is not an edge of this triangle
         */

        public int getEdgeIndex(QuadEdge e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (edge[i] == e)
                    return i;
            }
            return -1;
        }

        /**
         * Gets the index for the edge that starts at vertex v.
         * 
         * @param v the vertex to find the edge for
         * @return the index of the edge starting at the vertex
         * @return -1 if the vertex is not in the triangle
         */

        public int getEdgeIndex(Vertex v)
        {
            for (int i = 0; i < 3; i++)
            {
                if (edge[i].Orig == v)
                    return i;
            }
            return -1;
        }

        public void getEdgeSegment(int i, LineSegment seg)
        {
            seg.P0 = edge[i].Orig.Coordinate;
            int nexti = (i + 1)%3;
            seg.P1 = edge[nexti].Orig.Coordinate;
        }

        public ICoordinate[] getCoordinates()
        {
            var pts = new ICoordinate[4];
            for (int i = 0; i < 3; i++)
            {
                pts[i] = edge[i].Orig.Coordinate;
            }
            pts[3] = new Coordinate(pts[0]);
            return pts;
        }

        public bool Contains(Coordinate pt)
        {
            var ring = getCoordinates();
            return CGAlgorithms.IsPointInRing(pt, ring);
        }

        public IGeometry getGeometry(GeometryFactory fact)
        {
            ILinearRing ring = fact.CreateLinearRing(getCoordinates());
            IPolygon tri = fact.CreatePolygon(ring, null);
            return tri;
        }

        public override String ToString()
        {
            return getGeometry(new GeometryFactory()).ToString();
        }

        /**
         * Tests whether this triangle is adjacent to the outside of the subdivision.
         * 
         * @return true if the triangle is adjacent to the subdivision exterior
         */

        public bool isBorder()
        {
            for (int i = 0; i < 3; i++)
            {
                if (getAdjacentTriangleAcrossEdge(i) == null)
                    return true;
            }
            return false;
        }

        public bool isBorder(int i)
        {
            return getAdjacentTriangleAcrossEdge(i) == null;
        }

        public QuadEdgeTriangle getAdjacentTriangleAcrossEdge(int edgeIndex)
        {
            return (QuadEdgeTriangle) getEdge(edgeIndex).Sym().Data;
        }

        public int getAdjacentTriangleEdgeIndex(int i)
        {
            return getAdjacentTriangleAcrossEdge(i).getEdgeIndex(getEdge(i).Sym());
        }

        public IList<QuadEdgeTriangle> getTrianglesAdjacentToVertex(int vertexIndex)
        {
            // Assert: isVertex
            var adjTris = new List<QuadEdgeTriangle>();

            QuadEdge start = getEdge(vertexIndex);
            QuadEdge qe = start;
            do
            {
                QuadEdgeTriangle adjTri = (QuadEdgeTriangle) qe.Data;
                if (adjTri != null)
                {
                    adjTris.Add(adjTri);
                }
                qe = qe.oNext();
            } while (qe != start);

            return adjTris;

        }

        /**
         * Gets the neighbours of this triangle. If there is no neighbour triangle, the array element is
         * <code>null</code>
         * 
         * @return an array containing the 3 neighbours of this triangle
         */

        public QuadEdgeTriangle[] getNeighbours()
        {
            QuadEdgeTriangle[] neigh = new QuadEdgeTriangle[3];
            for (int i = 0; i < 3; i++)
            {
                neigh[i] = (QuadEdgeTriangle) getEdge(i).Sym().Data;
            }
            return neigh;
        }

    }
}