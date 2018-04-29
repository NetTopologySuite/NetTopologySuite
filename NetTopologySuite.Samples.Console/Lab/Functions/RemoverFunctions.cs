using GeoAPI.Geometries;
using NetTopologySuite.Samples.Lab.Clean;
namespace Open.Topology.TestRunner.Functions
{
    // NOTE: should be moved to 'NetTopologySuite.TestRunner' project, 'Functions' folder...
    public static class RemoverFunctions
    {
        public static IGeometry RemoveSmallHoles(IGeometry geom, double areaTolerance)
        {
            return SmallHoleRemover.Clean(geom, areaTolerance);
        }
    }
}
