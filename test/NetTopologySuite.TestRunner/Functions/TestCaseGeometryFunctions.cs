using System;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Utilities;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class TestCaseGeometryFunctions
    {
        public static Geometry BufferMitredJoin(Geometry g, double distance)
        {
            var bufParams = new BufferParameters();
            bufParams.JoinStyle = JoinStyle.Mitre;

            return BufferOp.Buffer(g, distance, bufParams);
        }

        public static Geometry Densify(Geometry g, double distance)
        {
            return Densifier.Densify(g, distance);
        }
    }

    public class TriangulationFunctions
    {
        private const double TriangulationTolerance = 0.0;

        public static Geometry DelaunayEdges(Geometry geom)
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            builder.Tolerance = TriangulationTolerance;
            var edges = builder.GetEdges(geom.Factory);
            return edges;
        }

        public static Geometry DelaunayTriangles(Geometry geom)
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            builder.Tolerance = TriangulationTolerance;
            var tris = builder.GetTriangles(geom.Factory);
            return tris;
        }

        public static Geometry VoronoiDiagram(Geometry geom, Geometry g2)
        {
            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(geom);
            if (g2 != null)
                builder.ClipEnvelope = g2.EnvelopeInternal;
            builder.Tolerance = TriangulationTolerance;
            Geometry diagram = builder.GetDiagram(geom.Factory);
            return diagram;
        }

        public static Geometry VoronoiDiagramWithData(Geometry geom, Geometry g2)
        {
            GeometryDataUtil.SetComponentDataToIndex(geom);

            var mapper = new VertexTaggedGeometryDataMapper();
            mapper.LoadSourceGeometries(geom);

            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(mapper.Coordinates);
            if (g2 != null)
                builder.ClipEnvelope = g2.EnvelopeInternal;
            builder.Tolerance = TriangulationTolerance;
            Geometry diagram = builder.GetDiagram(geom.Factory);
            mapper.TransferData(diagram);
            return diagram;
        }

        public static Geometry VoronoiRelaxation(Geometry sitesGeom, Geometry clipGeom, int nIter)
        {
            Geometry voronoiPolys = null;
            for (int i = 0; i < nIter; i++)
            {
                voronoiPolys = VoronoiDiagram(sitesGeom, clipGeom);
                sitesGeom = Centroids(voronoiPolys);
            }
            return voronoiPolys;
        }

        private static Geometry Centroids(Geometry polygons)
        {
            int npolys = polygons.NumGeometries;
            var centroids = new Point[npolys];
            for (int i = 0; i < npolys; i++)
            {
                centroids[i] = polygons.GetGeometryN(i).Centroid;
            }
            return polygons.Factory.CreateMultiPoint(centroids);
        }


        public static Geometry ConformingDelaunayEdges(Geometry sites, Geometry constraints)
        {
            return ConformingDelaunayEdgesWithTolerance(sites, constraints, TriangulationTolerance);
        }

        public static Geometry ConformingDelaunayEdgesWithTolerance(Geometry sites, Geometry constraints, double tol)
        {
            var builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(sites);
            builder.Constraints = constraints;
            builder.Tolerance = tol;

            var geomFact = sites != null ? sites.Factory : constraints.Factory;
            Geometry tris = builder.GetEdges(geomFact);
            return tris;
        }

        public static Geometry ConformingDelaunayTriangles(Geometry sites, Geometry constraints)
        {
            return ConformingDelaunayTrianglesWithTolerance(sites, constraints, TriangulationTolerance);
        }

        private static Geometry ConformingDelaunayTrianglesWithTolerance(Geometry sites, Geometry constraints, double tol)
        {
            var builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(sites);
            builder.Constraints = constraints;
            builder.Tolerance = tol;

            var geomFact = sites != null ? sites.Factory : constraints.Factory;
            Geometry tris = builder.GetTriangles(geomFact);
            return tris;
        }
    }
}
