using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Hull
{
    public class ConcaveHull
    {
        private readonly IGeometry _geom;
        private readonly double _tolerance;

        public ConcaveHull(IGeometry geom, double tolerance)
        {
            _geom = geom;
            _tolerance = tolerance;
        }

        public IGeometry GetResult()
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

        private static Geometry ComputeHull(IList<QuadEdgeTriangle> tris)
        {
            return null;

        }

        private QuadEdgeSubdivision BuildDelaunay()
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(_geom);
            var subDiv = builder.GetSubdivision();
            return subDiv;
        }
    }
}