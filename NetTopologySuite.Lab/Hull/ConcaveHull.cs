using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;

namespace NetTopologySuite.Hull
{
    public class ConcaveHull
    {
        private IGeometry geom;
        private double tolerance;

        public ConcaveHull(IGeometry geom, double tolerance)
        {
            this.geom = geom;
            this.tolerance = tolerance;
        }

        public IGeometry GetResult()
        {
            var subdiv = BuildDelaunay();
            var tris = ExtractTriangles(subdiv);
            var hull = ComputeHull(tris);
            return hull;
        }

        private IList<QuadEdgeTriangle> ExtractTriangles(QuadEdgeSubdivision subdiv)
        {
            return QuadEdgeTriangle.CreateOn(subdiv);
        }

        private static Geometry ComputeHull(IList<QuadEdgeTriangle> tris)
        {
            return null;

        }

        private QuadEdgeSubdivision BuildDelaunay()
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            return builder.GetSubdivision();
        }
    }
}