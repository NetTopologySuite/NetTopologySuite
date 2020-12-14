using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Distance
{
    internal class CachedFastDistance
    {
        private static Geometry _cacheGeom;
        private static IndexedFacetDistance _fastDistanceOp;


        public static double Distance(Geometry g1, Geometry g2)
        {
            if (_cacheGeom != g1)
            {
                _fastDistanceOp = new IndexedFacetDistance(g1);
                _cacheGeom = g1;
            }

            return _fastDistanceOp.Distance(g2);
        }
    }
}
