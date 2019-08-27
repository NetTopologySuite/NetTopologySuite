using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public static class DissolveFunctions
    {
        public static Geometry Dissolve(Geometry geom)
        {
            return LineDissolver.Dissolve(geom);
        }
    }
}
