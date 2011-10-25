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
        public static IGeometry bufferMitredJoin(IGeometry g, double distance)
        {
            var bufParams = new BufferParameters();
            bufParams.JoinStyle = JoinStyle.Mitre;

            return BufferOp.Buffer(g, distance, bufParams);
        }

        public static IGeometry densify(IGeometry g, double distance)
        {
            return Densifier.Densify(g, distance);
        }

    }

    public class TriangleFunctions
    {

        public static IGeometry circumcentre(IGeometry g)
        {
            var pts = trianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.getFactoryOrDefault(g);
            return geomFact.CreatePoint(cc);
        }

        public static IGeometry perpendicularBisectors(IGeometry g)
        {
            var pts = trianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.getFactoryOrDefault(g);
            var line = new ILineString[3];
            var p0 = (new LineSegment(pts[1], pts[2])).ClosestPoint(cc);
            line[0] = geomFact.CreateLineString(new Coordinate[] {p0, cc});
            var p1 = (new LineSegment(pts[0], pts[2])).ClosestPoint(cc);
            line[1] = geomFact.CreateLineString(new Coordinate[] {p1, cc});
            var p2 = (new LineSegment(pts[0], pts[1])).ClosestPoint(cc);
            line[2] = geomFact.CreateLineString(new Coordinate[] {p2, cc});
            return geomFact.CreateMultiLineString(line);
        }

        public static IGeometry incentre(IGeometry g)
        {
            var pts = trianglePts(g);
            var cc = Triangle.InCentreFn(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.getFactoryOrDefault(g);
            return geomFact.CreatePoint(cc);
        }

        public static IGeometry angleBisectors(IGeometry g)
        {
            var pts = trianglePts(g);
            var cc = Triangle.InCentreFn(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.getFactoryOrDefault(g);
            var line = new ILineString[3];
            line[0] = geomFact.CreateLineString(new Coordinate[] {pts[0], cc});
            line[1] = geomFact.CreateLineString(new Coordinate[] {pts[1], cc});
            line[2] = geomFact.CreateLineString(new Coordinate[] {pts[2], cc});
            return geomFact.CreateMultiLineString(line);
        }


        private static Coordinate[] trianglePts(IGeometry g)
        {
            var pts = g.Coordinates;
            if (pts.Length < 3)
                throw new ArgumentException("Input geometry must have at least 3 points");
            return pts;
        }
    }

    public class TriangulationFunctions
    {
        private static readonly double TRIANGULATION_TOLERANCE = 0.0;

        public static IGeometry delaunayEdges(IGeometry geom)
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            builder.Tolerance = TRIANGULATION_TOLERANCE;
            var edges = builder.GetEdges(geom.Factory);
            return edges;
        }

        public static IGeometry delaunayTriangles(IGeometry geom)
        {
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(geom);
            builder.Tolerance = TRIANGULATION_TOLERANCE;
            var tris = builder.GetTriangles(geom.Factory);
            return tris;
        }

        public static IGeometry voronoiDiagram(IGeometry geom, IGeometry g2)
        {
            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(geom);
            if (g2 != null)
                builder.ClipEnvelope = g2.EnvelopeInternal;
            builder.Tolerance = TRIANGULATION_TOLERANCE;
            IGeometry diagram = builder.GetDiagram(geom.Factory);
            return diagram;
        }

        public static IGeometry voronoiDiagramWithData(IGeometry geom, IGeometry g2)
        {
            GeometryDataUtil.setComponentDataToIndex(geom);

            var mapper = new VertexTaggedGeometryDataMapper();
            mapper.LoadSourceGeometries(geom);

            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(mapper.Coordinates);
            if (g2 != null)
                builder.ClipEnvelope = g2.EnvelopeInternal;
            builder.Tolerance = TRIANGULATION_TOLERANCE;
            IGeometry diagram = builder.GetDiagram(geom.Factory);
            mapper.TransferData(diagram);
            return diagram;
        }


        public static IGeometry conformingDelaunayEdges(IGeometry sites, IGeometry constraints)
        {
            ConformingDelaunayTriangulationBuilder builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(sites);
            builder.Constraints = constraints;
            builder.Tolerance = TRIANGULATION_TOLERANCE;

            var geomFact = sites != null ? sites.Factory : constraints.Factory;
            IGeometry tris = builder.GetEdges(geomFact);
            return tris;
        }

        public static IGeometry conformingDelaunayTriangles(IGeometry sites, IGeometry constraints)
        {
            ConformingDelaunayTriangulationBuilder builder = new ConformingDelaunayTriangulationBuilder();
            builder.SetSites(sites);
            builder.Constraints =constraints;
            builder.Tolerance=TRIANGULATION_TOLERANCE;

            var geomFact = sites != null ? sites.Factory : constraints.Factory;
            IGeometry tris = builder.GetTriangles(geomFact);
            return tris;
        }

    }

}