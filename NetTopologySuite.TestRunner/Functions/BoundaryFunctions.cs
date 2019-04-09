using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation;

namespace Open.Topology.TestRunner.Functions
{
    public class BoundaryFunctions
    {
        public static Geometry boundary(Geometry g) { return g.Boundary; }

        public static Geometry boundaryMod2(Geometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.Mod2BoundaryRule);
        }
        public static Geometry boundaryEndpoint(Geometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.EndpointBoundaryRule);
        }
        public static Geometry boundaryMonoValentEnd(Geometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.MonoValentEndpointBoundaryRule);
        }
        public static Geometry boundaryMultiValentEnd(Geometry g)
        {
            return BoundaryOp.GetBoundary(g, BoundaryNodeRules.MultivalentEndpointBoundaryRule);
        }

    }
}