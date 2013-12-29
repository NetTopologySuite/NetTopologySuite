using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;

namespace Open.Topology.TestRunner.Functions
{
    public static class PrecisionFunctions
    {
        public static IGeometry ReducePrecisionPointwise(IGeometry geom, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var reducedGeom = SimpleGeometryPrecisionReducer.Reduce(geom, pm);

            return reducedGeom;
        }
    }
}