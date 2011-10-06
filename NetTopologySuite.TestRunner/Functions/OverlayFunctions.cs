using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class OverlayFunctions
    {
        public static IGeometry intersection(IGeometry a, IGeometry b)
        {
            return a.Intersection(b);
        }

        public static IGeometry union(IGeometry a, IGeometry b)
        {
            return a.Union(b);
        }

        public static IGeometry symDifference(IGeometry a, IGeometry b)
        {
            return a.SymmetricDifference(b);
        }

        public static IGeometry difference(IGeometry a, IGeometry b)
        {
            return a.Difference(b);
        }

        public static IGeometry differenceBA(IGeometry a, IGeometry b)
        {
            return b.Difference(a);
        }

        public static IGeometry unaryUnion(IGeometry a)
        {
            return a.Union(a);
        }
    }
}