using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;
using NetTopologySuite.Triangulate.Tri;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Hull
{
    /// <summary>
    /// Functions to operate on triangulations represented as
    /// lists of <see cref="HullTri"/>s.
    /// </summary>
    static class HullTriangulation
    {
        public static IList<Tri> CreateDelaunayTriangulation(Geometry geom)
        {
            //TODO: implement a DT on Tris directly?
            var dt = new DelaunayTriangulationBuilder();
            dt.SetSites(geom);
            var subdiv = dt.GetSubdivision();
            var triList = ToTris(subdiv);
            return triList;
        }

        private static IList<Tri> ToTris(QuadEdgeSubdivision subdiv)
        {
            var visitor = new HullTriVisitor();
            subdiv.VisitTriangles(visitor, false);
            var triList = visitor.GetTriangles();
            TriangulationBuilder.Build(triList);
            return triList;
        }

        class HullTriVisitor : ITriangleVisitor
        {
            private readonly List<Tri> _triList = new List<Tri>();

            public void Visit(QuadEdge[] triEdges)
            {
                var p0 = triEdges[0].Orig.Coordinate;
                var p1 = triEdges[1].Orig.Coordinate;
                var p2 = triEdges[2].Orig.Coordinate;
                HullTri tri;
                if (Triangle.IsCCW(p0, p1, p2))
                {
                    tri = new HullTri(p0, p2, p1);
                }
                else
                {
                    tri = new HullTri(p0, p1, p2);
                }
                _triList.Add(tri);
            }

            public IList<Tri> GetTriangles()
            {
                return _triList;
            }
        }

        /// <summary>
        /// Creates a polygonal geometry representing the area of a triangulation
        /// which may be disconnected or contain holes.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <param name="geomFactory">The geometry factory</param>
        /// <returns>The area polygonal geometry</returns>
        public static Geometry Union(IList<Tri> triList, GeometryFactory geomFactory)
        {
            var polys = new List<Polygon>();
            foreach (var tri in triList)
            {
                var poly = tri.ToPolygon(geomFactory);
                polys.Add(poly);
            }
            return CoverageUnion.Union(geomFactory.BuildGeometry(polys));
        }

        /// <summary>
        /// Creates a Polygon representing the area of a triangulation
        /// which is connected and contains no holes.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <param name="geomFactory">The geometry factory to use</param>
        /// <returns>The area polygon</returns>
        public static Geometry TraceBoundaryPolygon(IList<Tri> triList, GeometryFactory geomFactory)
        {
            if (triList.Count == 1)
            {
                var tri = triList[0];
                return tri.ToPolygon(geomFactory);
            }
            var pts = TraceBoundary(triList);
            return geomFactory.CreatePolygon(pts);
        }
        /// <summary>
        /// Extracts the coordinates of the edgees along the boundary of a triangulation,
        /// by tracing CW around the border triangles.<br/>
        /// Assumption: there are at least 2 tris, they are connected,
        /// and there are no holes.
        /// So each tri has at least one non-border edge, and there is only one border.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <returns>The border of the triangulation</returns>
        private static Coordinate[] TraceBoundary(IList<Tri> triList)
        {
            var triStart = FindBorderTri(triList);
            var coordList = new CoordinateList();
            var tri = triStart;
            do
            {
                int borderIndex = tri.BoundaryIndexCCW;
                //-- add border vertex
                coordList.Add(tri.GetCoordinate(borderIndex).Copy(), false);
                int nextIndex = Tri.Next(borderIndex);
                //-- if next edge is also boundary, add it and move to next
                if (tri.IsBoundary(nextIndex))
                {
                    coordList.Add(tri.GetCoordinate(nextIndex).Copy(), false);
                    //borderIndex = nextIndex;
                }
                //-- find next border tri CCW around non-border edge
                tri = NextBorderTri(tri);
            } while (tri != triStart);
            coordList.CloseRing();
            return coordList.ToCoordinateArray();
        }

        private static HullTri FindBorderTri(IList<Tri> triList)
        {
            foreach (HullTri tri in triList)
            {
                if (tri.IsBorder()) return tri;
            }
            Assert.ShouldNeverReachHere("No border triangles found");
            return null;
        }

        public static HullTri NextBorderTri(Tri triStart)
        {
            var tri = (HullTri)triStart;
            //-- start at first non-border edge CW
            int index = Tri.Next(tri.BoundaryIndexCW);
            //-- scan CCW around vertex for next border tri
            do
            {
                var adjTri = (HullTri)tri.GetAdjacent(index);
                if (adjTri == tri)
                    throw new InvalidOperationException("No outgoing border edge found");
                index = Tri.Next(adjTri.GetIndex(tri));
                tri = adjTri;
            }
            while (!tri.IsBoundary(index));
            return tri;
        }
    }
}
