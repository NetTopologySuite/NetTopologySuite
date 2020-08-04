using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public class TriangleFunctions
    {

        public static Geometry Centroid(Geometry g)
        {
            return GeometryMapper.Map(g, itm =>
            {
                var pts = TrianglePts(itm);
                var cc = Triangle.Centroid(pts[0], pts[1], pts[2]);
                var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
                return geomFact.CreatePoint(cc);
            });
        }

        public static Geometry Circumcentre(Geometry g)
        {
            return GeometryMapper.Map(g, itm =>
            {
                var pts = TrianglePts(itm);
                var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
                var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
                return geomFact.CreatePoint(cc);
            });
        }

        public static Geometry CircumcentreDD(Geometry g)
        {
            return GeometryMapper.Map(g, itm =>
            {
                var pts = TrianglePts(itm);
                var cc = Triangle.CircumcentreDD(pts[0], pts[1], pts[2]);
                var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
                return geomFact.CreatePoint(cc);
            });
        }

        public static Geometry PerpendicularBisectors(Geometry g)
        {
            var pts = TrianglePts(g);
            var cc = Triangle.Circumcentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            var line = new LineString[3];
            var p0 = (new LineSegment(pts[1], pts[2])).ClosestPoint(cc);
            line[0] = geomFact.CreateLineString(new Coordinate[] {p0, cc});
            var p1 = (new LineSegment(pts[0], pts[2])).ClosestPoint(cc);
            line[1] = geomFact.CreateLineString(new Coordinate[] {p1, cc});
            var p2 = (new LineSegment(pts[0], pts[1])).ClosestPoint(cc);
            line[2] = geomFact.CreateLineString(new Coordinate[] {p2, cc});
            return geomFact.CreateMultiLineString(line);
        }

        public static Geometry InCentre(Geometry g)
        {
            return GeometryMapper.Map(g, itm =>
            {
                var pts = TrianglePts(itm);
                var cc = Triangle.InCentre(pts[0], pts[1], pts[2]);
                var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
                return geomFact.CreatePoint(cc);
            });
        }

        public static Geometry AngleBisectors(Geometry g)
        {
            var pts = TrianglePts(g);
            var cc = Triangle.InCentre(pts[0], pts[1], pts[2]);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);
            var line = new LineString[3];
            line[0] = geomFact.CreateLineString(new Coordinate[] {pts[0], cc});
            line[1] = geomFact.CreateLineString(new Coordinate[] {pts[1], cc});
            line[2] = geomFact.CreateLineString(new Coordinate[] {pts[2], cc});
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

}
