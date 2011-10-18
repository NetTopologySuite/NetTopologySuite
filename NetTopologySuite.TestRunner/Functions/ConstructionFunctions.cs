using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;

namespace Open.Topology.TestRunner.Functions
{
    public class ConstructionFunctions
    {
        public static IGeometry octagonalEnvelope(IGeometry g)
        {
            var octEnv = new OctagonalEnvelope(g);
            return octEnv.ToGeometry(g.Factory);
        }

        public static IGeometry minimumDiameter(IGeometry g) { return (new MinimumDiameter(g)).Diameter; }
        public static IGeometry minimumRectangle(IGeometry g) { return (new MinimumDiameter(g)).GetMinimumRectangle(); }
        public static IGeometry minimumBoundingCircle(Geometry g) { return (new MinimumBoundingCircle(g)).GetCircle(); }

        public static IGeometry boundary(Geometry g) { return g.Boundary; }
        public static IGeometry convexHull(Geometry g) { return g.ConvexHull(); }
        public static IGeometry centroid(Geometry g) { return g.Centroid; }
        public static IGeometry interiorPoint(Geometry g) { return g.InteriorPoint; }

        public static IGeometry densify(Geometry g, double distance) { return Densifier.Densify(g, distance); }


        public static IGeometry mergeLines(Geometry g)
        {
            var merger = new LineMerger();
            merger.Add(g);
            var lines = merger.GetMergedLineStrings();
            return g.Factory.BuildGeometry(lines);
        }

        public static IGeometry extractLines(IGeometry g)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            return g.Factory.BuildGeometry(NetTopologySuite.Utilities.CollectionUtil.Cast<IGeometry>((ICollection)lines));
        }
    }
}