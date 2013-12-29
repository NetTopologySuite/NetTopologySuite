using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Triangulate;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class TestCaseGeometryFunctions
    {
        public static IGeometry BufferMitredJoin(IGeometry g, double distance)
        {
            var bufParams = new BufferParameters();
            bufParams.JoinStyle = JoinStyle.Mitre;

            return BufferOp.Buffer(g, distance, bufParams);
        }

        public static IGeometry Densify(IGeometry g, double distance)
        {
            return Densifier.Densify(g, distance);
        }
    }

    public class TriangleFunctions
    {
        public static IGeometry Circumcentre(IGeometry g)
        {
            var pts = TrianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            return geomFact.CreatePoint(cc);
        }

        public static IGeometry PerpendicularBisectors(IGeometry g)
        {
            var pts = TrianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            var line = new ILineString[3];
            var p0 = (new LineSegment(pts[1], pts[2])).ClosestPoint(cc);
            line[0] = geomFact.CreateLineString(new Coordinate[] { p0, cc });
            var p1 = (new LineSegment(pts[0], pts[2])).ClosestPoint(cc);
            line[1] = geomFact.CreateLineString(new Coordinate[] { p1, cc });
            var p2 = (new LineSegment(pts[0], pts[1])).ClosestPoint(cc);
            line[2] = geomFact.CreateLineString(new Coordinate[] { p2, cc });
            return geomFact.CreateMultiLineString(line);
        }

        public static IGeometry Incentre(IGeometry g)
        {
            var pts = TrianglePts(g);
            var t = new Triangle(pts[0], pts[1], pts[2]);
            var cc = t.InCentre();
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            return geomFact.CreatePoint(cc);
        }

        public static IGeometry AngleBisectors(IGeometry g)
        {
            var pts = TrianglePts(g);
            var t = new Triangle(pts[0], pts[1], pts[2]);
            var cc = t.InCentre();
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            var line = new ILineString[3];
            line[0] = geomFact.CreateLineString(new Coordinate[] { pts[0], cc });
            line[1] = geomFact.CreateLineString(new Coordinate[] { pts[1], cc });
            line[2] = geomFact.CreateLineString(new Coordinate[] { pts[2], cc });
            return geomFact.CreateMultiLineString(line);
        }

        private static Coordinate[] TrianglePts(IGeometry g)
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

        public static IGeometry DelaunayEdges(IGeometry geom)
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            builder.Tolerance = TriangulationTolerance;
            var edges = builder.GetEdges(geom.Factory);
            return edges;
        }

        public static IGeometry DelaunayTriangles(IGeometry geom)
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            builder.Tolerance = TriangulationTolerance;
            var tris = builder.GetTriangles(geom.Factory);
            return tris;
        }

        public static IGeometry VoronoiDiagram(IGeometry geom, IGeometry g2)
        {
            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(geom);
            if (g2 != null)
                builder.ClipEnvelope = g2.EnvelopeInternal;
            builder.Tolerance = TriangulationTolerance;
            IGeometry diagram = builder.GetDiagram(geom.Factory);
            return diagram;
        }

        public static IGeometry VoronoiDiagramWithData(IGeometry geom, IGeometry g2)
        {
            GeometryDataUtil.SetComponentDataToIndex(geom);

            var mapper = new VertexTaggedGeometryDataMapper();
            mapper.LoadSourceGeometries(geom);

            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(mapper.Coordinates);
            if (g2 != null)
                builder.ClipEnvelope = g2.EnvelopeInternal;
            builder.Tolerance = TriangulationTolerance;
            IGeometry diagram = builder.GetDiagram(geom.Factory);
            mapper.TransferData(diagram);
            return diagram;
        }

        public static IGeometry ConformingDelaunayEdges(IGeometry sites, IGeometry constraints)
        {
            ConformingDelaunayTriangulationBuilder builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(sites);
            builder.Constraints = constraints;
            builder.Tolerance = TriangulationTolerance;

            var geomFact = sites != null ? sites.Factory : constraints.Factory;
            IGeometry tris = builder.GetEdges(geomFact);
            return tris;
        }

        public static IGeometry ConformingDelaunayTriangles(IGeometry sites, IGeometry constraints)
        {
            ConformingDelaunayTriangulationBuilder builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(sites);
            builder.Constraints = constraints;
            builder.Tolerance = TriangulationTolerance;

            var geomFact = sites != null ? sites.Factory : constraints.Factory;
            IGeometry tris = builder.GetTriangles(geomFact);
            return tris;
        }
    }
}