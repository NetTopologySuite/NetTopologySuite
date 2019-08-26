using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace Open.Topology.TestRunner.Functions
{
    public static class SimplificationFunctions
    {
        public static Geometry SimplifyDp(Geometry g, double distance)
        {
            return DouglasPeuckerSimplifier.Simplify(g, distance);
        }

        public static Geometry SimplifyTp(Geometry g, double distance)
        {
            return TopologyPreservingSimplifier.Simplify(g, distance);
        }

        public static Geometry SimplifyVW(Geometry g, double distance)
        {
            return VWSimplifier.Simplify(g, distance);
        }
    }
}