using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Hull
{
    public class ChiShape
    {
        private readonly Geometry _geom;
        private readonly double _tolerance;

        public ChiShape(Geometry geom, double tolerance)
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
            foreach (var triangle in tris)
            {
                CheckMaxEdgeAndListBorderTriangles(quadEdgeTriangleList, triangle);
            }

            while (quadEdgeTriangleList.Count != 0)
            {
                var triangle = quadEdgeTriangleList.Last();
                var edge = triangle.Key;
                Debug.WriteLine(triangle.Value);
                quadEdgeTriangleList.Remove(edge);
                if (edge.Length.CompareTo(_tolerance) >= 0 && RemovalGivesRegularPolygon(triangle.Value))
                {
                    var neighbours = triangle.Value.GetNeighbours();
                    triangle.Value.Kill();
                    foreach (var neighbour in from n in neighbours
                                              where n != null && n.IsLive()
                                              select n)
                    {
                        Debug.WriteLine(neighbour);
                        CheckMaxEdgeAndListBorderTriangles(quadEdgeTriangleList, neighbour);
                    }
                }
            }
            var lineMerger = new LineMerger();
            var fact = new GeometryFactory();
            foreach (var triangle in tris)
            {
                if (!triangle.IsLive()) continue;
                for (int i = 0; i < 3; i++)
                {
                    if (triangle.GetAdjacentTriangleAcrossEdge(i) == null || !triangle.GetAdjacentTriangleAcrossEdge(i).IsLive())
                    {
                        lineMerger.Add(triangle.GetEdge(i).ToLineSegment().ToGeometry(fact));
                    }
                }
            }
            var mergedLine = (LineString)lineMerger.GetMergedLineStrings().FirstOrDefault();
            if (mergedLine.IsRing)
            {
                var lr = new LinearRing(mergedLine.CoordinateSequence, fact);

                var concaveHull = new Polygon(lr, null, fact);

                return concaveHull;
            }
            return mergedLine;
        }



        private void CheckMaxEdgeAndListBorderTriangles(SortedList<QuadEdge, QuadEdgeTriangle> quadEdgeTriangleList, QuadEdgeTriangle triangle)
        {
            bool canBeRemoved = RemovalGivesRegularPolygon(triangle);
            if (canBeRemoved)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (triangle.GetAdjacentTriangleAcrossEdge(i) == null)
                    {
                        var edge = triangle.GetEdge(i);
                        if (!quadEdgeTriangleList.ContainsKey(edge))
                        {
                            Debug.WriteLine($"edge: {edge} index: {i} triangle: {triangle}");
                            quadEdgeTriangleList.Add(edge, triangle);
                        }
                    }
                }
            }
        }

        private static bool RemovalGivesRegularPolygon(QuadEdgeTriangle triangle)
        {
            int neighbourCount = 0;
            var neighbours = triangle.GetNeighbours();
            for (int i = 0; i < 3; i++)
            {
                if (neighbours[i] != null && neighbours[i].IsLive())
                {
                    neighbourCount++;
                }
            }
            bool canBeRemoved = neighbourCount == 2;
            return canBeRemoved;
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
