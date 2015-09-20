using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Operation;

namespace Open.Topology.TestRunner.Functions
{
    public class BoundaryFunctions
    {
        public static IGeometry boundary(IGeometry g) { return g.Boundary; }

        public static IGeometry boundaryMod2(IGeometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.Mod2BoundaryRule);
        }
        public static IGeometry boundaryEndpoint(IGeometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.EndpointBoundaryRule);
        }
        public static IGeometry boundaryMonoValentEnd(IGeometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.MonoValentEndpointBoundaryRule);
        }
        public static IGeometry boundaryMultiValentEnd(IGeometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.MultivalentEndpointBoundaryRule);
        }

    }
}