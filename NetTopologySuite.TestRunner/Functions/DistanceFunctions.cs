using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Operation.Distance;

namespace Open.Topology.TestRunner.Functions
{
    public static class DistanceFunctions
    {
        public static double Distance(IGeometry a, IGeometry b)
        {
            return a.Distance(b);
        }

        public static bool IsWithinDistance(IGeometry a, IGeometry b, double dist)
        {
            return a.IsWithinDistance(b, dist);
        }

        public static IGeometry NearestPoints(IGeometry a, IGeometry b)
        {
            var pts = DistanceOp.NearestPoints(a, b);
            return a.Factory.CreateLineString(pts);
        }

        public static IGeometry DiscreteHausdorffDistanceLine(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            dist.Distance();
            return a.Factory.CreateLineString(dist.Coordinates);
        }

        public static IGeometry DensifiedDiscreteHausdorffDistanceLine(IGeometry a, IGeometry b, double frac)
        {
            var hausDist = new DiscreteHausdorffDistance(a, b);
            hausDist.DensifyFraction = frac;
            hausDist.Distance();
            return a.Factory.CreateLineString(hausDist.Coordinates);
        }

        public static IGeometry DiscreteOrientedHausdorffDistanceLine(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            dist.OrientedDistance();
            return a.Factory.CreateLineString(dist.Coordinates);
        }

        public static double DiscreteHausdorffDistance(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            return dist.Distance();
        }

        public static double DiscreteOrientedHausdorffDistance(IGeometry a, IGeometry b)
        {
            var dist = new DiscreteHausdorffDistance(a, b);
            return dist.OrientedDistance();
        }

        public static double DistanceIndexed(IGeometry a, IGeometry b)
        {
            return IndexedFacetDistance.Distance(a, b);
        }

        public static IGeometry NearestPointsIndexed(IGeometry a, IGeometry b)
        {
            var pts = IndexedFacetDistance.NearestPoints(a, b);
            return a.Factory.CreateLineString(pts);
        }

        public static IGeometry NearestPointsIndexedAll(IGeometry a, IGeometry b)
        {
            var ifd = new IndexedFacetDistance(a);

            int n = b.NumGeometries;
            var lines = new ILineString[n];
            for (int i = 0; i < n; i++)
            {
                var pts = ifd.NearestPoints(b.GetGeometryN(i));
                lines[i] = a.Factory.CreateLineString(pts);
            }

            return a.Factory.CreateMultiLineString(lines);
        }
    }
}
