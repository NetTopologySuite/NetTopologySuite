using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Hull
{
    public class ConcaveHull
    {
        private readonly Geometry _geom;
        private readonly double _tolerance;

        public ConcaveHull(Geometry geom, double tolerance)
        {
            _geom = geom;
            _tolerance = tolerance;
        }

        /// <summary>
        /// Generate the Delaunay triangulation of the set of input points P;
        /// (2) Remove the longest exterior edge from the triangulation such that:
        /// (a) the edge to be removed is longer than the tolerance parameter;
        /// and (b) the exterior edges of the resulting triangulation form the boundary of a simple polygon;
        /// (3) Repeat 2. as long as there are more edges to be removed (4) Return the polygon formed by the exterior edges of the triangulation
        /// </summary>
        /// <returns>Convex hull if tolerance is higher than the maximum edge length, simple polygon if tolerance is low.</returns>
        /// <exception>If no concavehull can be generated throws not implemented exception.</exception>
        public Geometry GetResult()
        {
            var subdiv = BuildDelaunay();
            var tris = ExtractTriangles(subdiv);
            var hull = ComputeHull(tris);
            return hull;
        }

        private static IList<QuadEdgeTriangle> ExtractTriangles(QuadEdgeSubdivision subdiv)
        {
            var qeTris = QuadEdgeTriangle.CreateOn(subdiv);
            return qeTris;
        }


        private Geometry ComputeHull(IList<QuadEdgeTriangle> tris)
        {
            var quadEdgeTriangleList = new SortedList<QuadEdge, QuadEdgeTriangle>(new QuadEdgeComparer());
            double maxEdgeLength = double.NegativeInfinity;
            foreach (var triangle in tris)
            {
                maxEdgeLength = CheckMaxEdgeAndListBorderTriangles(quadEdgeTriangleList, maxEdgeLength, triangle);
            }

            foreach (var triangle in quadEdgeTriangleList)
            {
                if (_tolerance.CompareTo(maxEdgeLength) <= 0)
                {
                    break;
                }
                var edge = triangle.Key;
                if (edge.Length == maxEdgeLength)
                {
                    var neighbours = triangle.Value.GetNeighbours();
                    triangle.Value.Kill();
                    quadEdgeTriangleList.Remove(edge);
                    maxEdgeLength = quadEdgeTriangleList.Keys.Last().Length;
                    foreach (var neighbour in from n in neighbours
                                              where n != null
                                              select n)
                    {
                        maxEdgeLength = CheckMaxEdgeAndListBorderTriangles(quadEdgeTriangleList, maxEdgeLength, triangle.Value);
                    }
                }
            }
            Geometry geometry = null;
            if (quadEdgeTriangleList.Count == 0)
            {
                throw new NotImplementedException("Simple geometry generate convexhull");
            }
            else
            {
                //Generate from edges
                geometry = QuadEdgeTriangle.ToPolygon(quadEdgeTriangleList.Keys.ToArray());
            }
            return geometry;
        }

        private static double CheckMaxEdgeAndListBorderTriangles(SortedList<QuadEdge, QuadEdgeTriangle> quadEdgeTriangleStack, double maxEdgeLength, QuadEdgeTriangle triangle)
        {
            int neighbourCount = 0;
            var neighbours = triangle.GetNeighbours();
            for (int i = 0; i < 3; i++)
            {
                if (neighbours[i] != null)
                {
                    neighbourCount++;
                }
            }
            if (neighbourCount == 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (triangle.GetAdjacentTriangleAcrossEdge(i) == null)
                    {
                        var edge = triangle.GetEdge(i);
                        if (!(edge.Length < maxEdgeLength))
                        {
                            maxEdgeLength = edge.Length;
                        }
                        if (!quadEdgeTriangleStack.ContainsValue(triangle))
                        {
                            quadEdgeTriangleStack.Add(edge, triangle);
                        }
                    }
                }
            }
            return maxEdgeLength;
        }

        private QuadEdgeSubdivision BuildDelaunay()
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(_geom);
            var subDiv = builder.GetSubdivision();
            return subDiv;
        }
    }

    internal class QuadEdgeComparer : Comparer<QuadEdge>
    {
        public override int Compare(QuadEdge x, QuadEdge y)
        {
            return x.Length.CompareTo(y.Length);
        }
    }
}
