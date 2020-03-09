#nullable disable
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

    public class TriangleFunctions
    {
        public static Geometry Circumcentre(Geometry g)
        {
            var pts = TrianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            return geomFact.CreatePoint(cc);
        }

        public static Geometry PerpendicularBisectors(Geometry g)
        {
            var pts = TrianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            var line = new LineString[3];
            var p0 = (new LineSegment(pts[1], pts[2])).ClosestPoint(cc);
            line[0] = geomFact.CreateLineString(new[] { p0, cc });
            var p1 = (new LineSegment(pts[0], pts[2])).ClosestPoint(cc);
            line[1] = geomFact.CreateLineString(new[] { p1, cc });
            var p2 = (new LineSegment(pts[0], pts[1])).ClosestPoint(cc);
            line[2] = geomFact.CreateLineString(new[] { p2, cc });
            return geomFact.CreateMultiLineString(line);
        }

        public static Geometry Incentre(Geometry g)
        {
            var pts = TrianglePts(g);
            var t = new Triangle(pts[0], pts[1], pts[2]);
            var cc = t.InCentre();
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            return geomFact.CreatePoint(cc);
        }

        public static Geometry AngleBisectors(Geometry g)
        {
            var pts = TrianglePts(g);
            var t = new Triangle(pts[0], pts[1], pts[2]);
            var cc = t.InCentre();
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            var line = new LineString[3];
            line[0] = geomFact.CreateLineString(new[] { pts[0], cc });
            line[1] = geomFact.CreateLineString(new[] { pts[1], cc });
            line[2] = geomFact.CreateLineString(new[] { pts[2], cc });
            return geomFact.CreateMultiLineString(line);
        }

        private static Coordinate[] TrianglePts(Geometry g)
        {
            var pts = g.Coordinates;
            if (pts.Length < 3)
                throw new ArgumentException("Input geometry must have at least 3 points");
            return pts;
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