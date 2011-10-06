using GeoAPI.Geometries;
using NetTopologySuite.Simplify;

namespace Open.Topology.TestRunner.Functions
{
    public class SimplificationFunctions
    {
        public static IGeometry simplifyDP(IGeometry g, double distance)
        { return DouglasPeuckerSimplifier.Simplify(g, distance); }

        public static IGeometry simplifyTP(IGeometry g, double distance)
        { return TopologyPreservingSimplifier.Simplify(g, distance); }


    }
}