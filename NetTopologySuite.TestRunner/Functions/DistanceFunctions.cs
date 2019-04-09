using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace Open.Topology.TestRunner.Functions
{
    public static class DistanceFunctions
    {
        public static double Distance(Geometry a, Geometry b)
        {
            return a.Distance(b);
        }

        public static bool IsWithinDistance(Geometry a, Geometry b, double dist)
        {
            return a.IsWithinDistance(b, dist);
        }

        public static Geometry NearestPoints(Geometry a, Geometry b)
        {
            var pts = DistanceOp.NearestPoints(a, b);
            return a.Factory.CreateLineString(pts);
        }

        public static Geometry DiscreteHausdorffDistanceLine(Geometry a, Geometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            dist.Distance();
            return a.Factory.CreateLineString(dist.Coordinates);
        }

        public static Geometry DensifiedDiscreteHausdorffDistanceLine(Geometry a, Geometry b, double frac)
        {
            var hausDist = new DiscreteHausdorffDistance(a, b);
            hausDist.DensifyFraction = frac;
            hausDist.Distance();
            return a.Factory.CreateLineString(hausDist.Coordinates);
        }

        public static Geometry DiscreteOrientedHausdorffDistanceLine(Geometry a, Geometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            dist.OrientedDistance();
            return a.Factory.CreateLineString(dist.Coordinates);
        }

        public static double DiscreteHausdorffDistance(Geometry a, Geometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            return dist.Distance();
        }

        public static double DiscreteOrientedHausdorffDistance(Geometry a, Geometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            return dist.OrientedDistance();
        }

        public static double DistanceIndexed(Geometry a, Geometry b)
        {
            return IndexedFacetDistance.Distance(a, b);
        }

        public static Geometry NearestPointsIndexed(Geometry a, Geometry b)
        {
            var pts = IndexedFacetDistance.NearestPoints(a, b);
            return a.Factory.CreateLineString(pts);
        }

        public static Geometry NearestPointsIndexedAll(Geometry a, Geometry b)
        {
            var ifd = new IndexedFacetDistance(a);

            int n = b.NumGeometries;
            var lines = new LineString[n];
            for (int i = 0; i < n; i++)
            {
                var pts = ifd.NearestPoints(b.GetGeometryN(i));
                lines[i] = a.Factory.CreateLineString(pts);
            }

            return a.Factory.CreateMultiLineString(lines);
        }
    }
}
