using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Operation.Distance;

namespace Open.Topology.TestRunner.Functions
{
    public class DistanceFunctions
    {
        public static double distance(IGeometry a, IGeometry b)
        {
            return a.Distance(b);
        }

        public static bool isWithinDistance(IGeometry a, IGeometry b, double dist)
        {
            return a.IsWithinDistance(b, dist);
        }

        public static IGeometry nearestPoints(IGeometry a, IGeometry b)
        {
            var pts = DistanceOp.NearestPoints(a, b);
            return a.Factory.CreateLineString(pts);
        }

        public static IGeometry discreteHausdorffDistanceLine(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            dist.Distance();
            return a.Factory.CreateLineString(dist.Coordinates);
        }

        public static IGeometry densifiedDiscreteHausdorffDistanceLine(IGeometry a, IGeometry b, double frac)
        {
            var hausDist = new DiscreteHausdorffDistance(a, b);
            hausDist.DensifyFraction = frac;
            hausDist.Distance();
            return a.Factory.CreateLineString(hausDist.Coordinates);
        }

        public static IGeometry discreteOrientedHausdorffDistanceLine(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            dist.OrientedDistance();
            return a.Factory.CreateLineString(dist.Coordinates);
        }

        public static double discreteHausdorffDistance(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            return dist.Distance();
        }

        public static double discreteOrientedHausdorffDistance(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            return dist.OrientedDistance();
        }

    }
}