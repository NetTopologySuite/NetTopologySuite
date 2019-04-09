using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public static class DissolveFunctions
    {
        public static IGeometry Dissolve(IGeometry geom)
        {
            return LineDissolver.Dissolve(geom);
        }
    }
}
