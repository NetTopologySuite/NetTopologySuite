using NetTopologySuite.Algorithm;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;

namespace Open.Topology.TestRunner.Functions
{
    public static class ConstructionFunctions
    {
        public static Geometry OctagonalEnvelope(Geometry g)
        {
            return NetTopologySuite.Geometries.OctagonalEnvelope.GetOctagonalEnvelope(g);
        }

        public static Geometry MinimumDiameter(Geometry g) { return (new MinimumDiameter(g)).Diameter; }
        public static double MinimumDiameterLength(Geometry g) { return (new MinimumDiameter(g)).Diameter.Length; }

        public static Geometry MinimumRectangle(Geometry g) { return (new MinimumDiameter(g)).GetMinimumRectangle(); }

        public static Geometry MinimumBoundingCircle(Geometry g) { return (new MinimumBoundingCircle(g)).GetCircle(); }
        public static double MinimumBoundingCircleDiameterLength(Geometry g) { return 2 * (new MinimumBoundingCircle(g)).GetRadius(); }

        public static Geometry MaximumDiameter(Geometry g) { return new MinimumBoundingCircle(g).GetMaximumDiameter(); ; }
        public static double MaximumDiameterLength(Geometry g) { return MaximumDiameter(g).Length; }

        public static Geometry Boundary(Geometry g) { return g.Boundary; }
        public static Geometry ConvexHull(Geometry g) { return g.ConvexHull(); }
        public static Geometry Centroid(Geometry g) { return g.Centroid; }
        public static Geometry InteriorPoint(Geometry g) { return g.InteriorPoint; }

        public static Geometry Densify(Geometry g, double distance) { return Densifier.Densify(g, distance); }

        public static Geometry MergeLines(Geometry g)
        {
            var merger = new LineMerger();
            merger.Add(g);
            var lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static Geometry ExtractLines(Geometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(lines);
        }
    }
}
