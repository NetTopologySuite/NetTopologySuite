using GeoAPI.Geometries;
using NetTopologySuite.Simplify;

namespace Open.Topology.TestRunner.Functions
{
    public static class SimplificationFunctions
    {
        public static IGeometry SimplifyDp(IGeometry g, double distance)
        {
            return DouglasPeuckerSimplifier.Simplify(g, distance);
        }

        public static IGeometry SimplifyTp(IGeometry g, double distance)
        {
            return TopologyPreservingSimplifier.Simplify(g, distance);
        }

        public static IGeometry SimplifyVW(IGeometry g, double distance)
        {
            return VWSimplifier.Simplify(g, distance);
        }
    }
}