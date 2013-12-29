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
            var octEnv = new OctagonalEnvelope(g);
            return octEnv.ToGeometry(g.Factory);
        }

        public static IGeometry MinimumDiameterLine(Geometry g) { return (new MinimumDiameter(g)).Diameter; }
        public static double MinimumDiameter(Geometry g) { return (new MinimumDiameter(g)).Diameter.Length; }
        
        public static IGeometry MinimumRectangle(IGeometry g) { return (new MinimumDiameter(g)).GetMinimumRectangle(); }
        public static IGeometry MinimumBoundingCircle(Geometry g) { return (new MinimumBoundingCircle(g)).GetCircle(); }
        public static IGeometry MinimumBoundingCirclePoints(Geometry g) { return g.Factory.CreateLineString((new MinimumBoundingCircle(g)).GetExtremalPoints()); }
        public static double MaximumDiameter(Geometry g) { return 2 * (new MinimumBoundingCircle(g)).GetRadius(); }

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
            return g.Factory.BuildGeometry(NetTopologySuite.Utilities.CollectionUtil.Cast<IGeometry>((ICollection)lines));
        }
    }
}