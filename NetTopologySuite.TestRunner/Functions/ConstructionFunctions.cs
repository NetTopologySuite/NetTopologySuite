using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;

namespace Open.Topology.TestRunner.Functions
{
    public static class ConstructionFunctions
    {
        public static IGeometry OctagonalEnvelope(IGeometry g)
        {
            return NetTopologySuite.Geometries.OctagonalEnvelope.GetOctagonalEnvelope(g);
        }

        public static IGeometry MinimumDiameter(IGeometry g) { return (new MinimumDiameter(g)).Diameter; }
        public static double MinimumDiameterLength(IGeometry g) { return (new MinimumDiameter(g)).Diameter.Length; }

        public static IGeometry MinimumRectangle(IGeometry g) { return (new MinimumDiameter(g)).GetMinimumRectangle(); }
        public static IGeometry MinimumBoundingCircle(Geometry g) { return (new MinimumBoundingCircle(g)).GetCircle(); }
        public static IGeometry MaximumDiameter(IGeometry g) { return g.Factory.CreateLineString((new MinimumBoundingCircle(g)).GetExtremalPoints()); }
        public static IGeometry FarthestPoints(IGeometry g) { return ((new MinimumBoundingCircle(g)).GetFarthestPoints()); }

        public static double MaximumDiameterLength(IGeometry g) { return 2 * (new MinimumBoundingCircle(g)).GetRadius(); }

        public static IGeometry Boundary(Geometry g) { return g.Boundary; }
        public static IGeometry ConvexHull(Geometry g) { return g.ConvexHull(); }
        public static IGeometry Centroid(Geometry g) { return g.Centroid; }
        public static IGeometry InteriorPoint(Geometry g) { return g.InteriorPoint; }

        public static IGeometry Densify(Geometry g, double distance) { return Densifier.Densify(g, distance); }

        public static IGeometry MergeLines(Geometry g)
        {
            var merger = new LineMerger();
            merger.Add(g);
            var lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry ExtractLines(IGeometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(lines);
        }
    }
}