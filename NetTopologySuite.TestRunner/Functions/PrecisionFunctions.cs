using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;
namespace Open.Topology.TestRunner.Functions
{
    public static class PrecisionFunctions
    {
        /*
        public static IGeometry OLDReducePrecisionPointwise(IGeometry geom, double scaleFactor)
        {
            PrecisionModel pm = new PrecisionModel(scaleFactor);
            IGeometry reducedGeom = SimpleGeometryPrecisionReducer.Reduce(geom, pm);
            return reducedGeom;
        }
        */
        public static IGeometry ReducePrecisionPointwise(IGeometry geom, double scaleFactor)
        {
            PrecisionModel pm = new PrecisionModel(scaleFactor);
            IGeometry reducedGeom = GeometryPrecisionReducer.Reduce(geom, pm);
            return reducedGeom;
        }
    }
}
