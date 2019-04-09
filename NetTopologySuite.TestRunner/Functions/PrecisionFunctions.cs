using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;

namespace Open.Topology.TestRunner.Functions
{
    public static class PrecisionFunctions
    {
        /*
        public static Geometry OLDReducePrecisionPointwise(Geometry geom, double scaleFactor)
        {
            PrecisionModel pm = new PrecisionModel(scaleFactor);
            Geometry reducedGeom = SimpleGeometryPrecisionReducer.Reduce(geom, pm);
            return reducedGeom;
        }
        */

        public static Geometry ReducePrecisionPointwise(Geometry geom, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            var reducedGeom = GeometryPrecisionReducer.Reduce(geom, pm);
            return reducedGeom;
        }
    }
}